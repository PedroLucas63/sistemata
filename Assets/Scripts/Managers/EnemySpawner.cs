using System;
using Sistemata.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sistemata.Enemy;
using Random = UnityEngine.Random;

namespace Sistemata.Spawning
{
    public class EnemySpawner : MonoBehaviour
    {
        [Serializable]
        public class EnemyWeight
        {
            public EnemyController Enemy;
            public int Weight;
        }
        
        public static EnemySpawner Instance { get; private set; }

        [Header("Prefabs")]
        public List<EnemyWeight> EnemyPrefabs;
        public List<EnemyWeight> BossPrefabs;

        [Header("Configurações de Spawn")]
        public float initialSpawnDelay = 8f;
        public float normalSpawnDelay = 2f;
        public float chaosSpawnDelay = 0.5f;
        [Tooltip("O menor atraso possível que o spawn normal pode atingir ao acelerar.")]
        public float minimumSpawnDelay = 0.4f; 
        public int initialLevel = 1;
        public int maxLevel = 15;
        [Tooltip("Quão rápido a dificuldade cresce. Valores maiores aceleram o spawn mais cedo.")]
        public float difficultyScaleSpeed = 0.005f; 
        
        public int maxEnemyCount = 100;
        public int initEnemyCount = 20;
        public float minSpawnRadius = 25f;
        public float minRespawnRadius = 15f;
        public float maxSpawnRadius = 35f;
        public Transform enemyHolder;

        private float currentSpawnTimer;

        [Header("Lógica de Lotes (Batches)")]
        private Dictionary<int, List<EnemyController>> enemyBatches = new Dictionary<int, List<EnemyController>>();
        private float runLogicTimer = 0f;
        private float runLogicTimerCD = 1f;

        [Header("Particionamento Espacial Infinito")]
        public float cellSize = 20f;

        [HideInInspector] 
        public Dictionary<Vector2Int, HashSet<EnemyController>> enemySpatialGroups = new Dictionary<Vector2Int, HashSet<EnemyController>>();

        private float _initialSpawnTimer;
        private bool _firstSpawn = false;
        private int _enemyWeightSum = 0;
        private int _bossWeightSum = 0;

        // Coleções devidamente inicializadas direto na declaração para evitar falhas de ciclo de vida
        private SortedSet<BatchScore> batchQueue_Enemy = new SortedSet<BatchScore>();
        private Dictionary<int, BatchScore> batchScoreMap_Enemy = new Dictionary<int, BatchScore>();

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
                InitializeBatches(); // Garante que as estruturas do Batch existam antes de qualquer Start
            }
            else {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBossWarning += StopSpawning;
                GameManager.Instance.OnBossSpawn += SpawnTheBoss;
                GameManager.Instance.OnChaosStart += StartChaosMode;
            }

            UpdateWeights();
            _initialSpawnTimer = initialSpawnDelay;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnBossWarning -= StopSpawning;
            GameManager.Instance.OnBossSpawn -= SpawnTheBoss;
            GameManager.Instance.OnChaosStart -= StartChaosMode;
        }

        private void UpdateWeights()
        {
            _enemyWeightSum = EnemyPrefabs?.Sum(e => e.Weight) ?? 0;
            _bossWeightSum = BossPrefabs?.Sum(e => e.Weight) ?? 0;
        }

        private void Update()
        {
            if (!_firstSpawn)
            {
                FirstSpawn();
                return;
            }
            
            var state = GameManager.Instance.currentState;
            if (state != GameState.Normal && state != GameState.Chaos) return;
            
            currentSpawnTimer -= Time.deltaTime;
            if (!(currentSpawnTimer <= 0) || enemyHolder.childCount >= maxEnemyCount) return;

            Spawn();
            currentSpawnTimer = CalculateDynamicSpawnDelay(state);
        }

        public void Spawn()
        {
            var chosenEnemy = SelectRandomEnemy();
            if (chosenEnemy)
            {
                SpawnEnemy(chosenEnemy);
            }
        }

        /// <summary>
        /// Calcula o tempo de espera do spawn reduzindo o delay progressivamente com o tempo
        /// </summary>
        private float CalculateDynamicSpawnDelay(GameState state)
        {
            if (state == GameState.Chaos) return chaosSpawnDelay;

            float timeSurvived = GameManager.Instance.TotalTimeSurvived;
            
            // Aplica uma curva decrescente baseada no tempo. 
            // Quanto maior o totalTimeSurvived, menor e mais frequente o delay se tornará.
            float dynamicDelay = normalSpawnDelay / (1f + (timeSurvived * difficultyScaleSpeed));

            // Impele que o spawn fique rápido demais a ponto de quebrar a CPU
            return Mathf.Max(dynamicDelay, minimumSpawnDelay);
        }
        
        private EnemyController SelectRandomEnemy()
        {
            if (EnemyPrefabs == null || EnemyPrefabs.Count == 0 || _enemyWeightSum <= 0)
                return null;

            var randomValue = Random.Range(0, _enemyWeightSum);
            var currentWeightCounter = 0;

            foreach (var prefabData in EnemyPrefabs)
            {
                currentWeightCounter += prefabData.Weight;
                if (randomValue < currentWeightCounter)
                {
                    return prefabData.Enemy;
                }
            }

            return EnemyPrefabs.FirstOrDefault()?.Enemy;
        }
        
        private EnemyController SelectRandomBoss()
        {
            if (BossPrefabs == null || BossPrefabs.Count == 0 || _bossWeightSum <= 0)
                return null;

            var randomValue = Random.Range(0, _bossWeightSum);
            var currentWeightCounter = 0;

            foreach (var prefabData in BossPrefabs)
            {
                currentWeightCounter += prefabData.Weight;
                if (randomValue < currentWeightCounter)
                {
                    return prefabData.Enemy;
                }
            }

            return BossPrefabs.FirstOrDefault()?.Enemy;
        }

        private void FirstSpawn()
        {
            _initialSpawnTimer -= Time.deltaTime;
            if (_initialSpawnTimer > 0) return;
            
            for (var i = 0; i < initEnemyCount; i++)
            {
                var enemyToSpawn = SelectRandomEnemy();
                if (enemyToSpawn) SpawnEnemy(enemyToSpawn);
            }
            _firstSpawn = true;
            currentSpawnTimer = normalSpawnDelay;
        }

        private void SpawnTheBoss()
        {
            var bossToSpawn = SelectRandomBoss();
            if (bossToSpawn != null) SpawnEnemy(bossToSpawn, true);
        }

        private void FixedUpdate()
        {
            runLogicTimer += Time.fixedDeltaTime;

            if (runLogicTimer >= runLogicTimerCD)
            {
                runLogicTimer = 0f;
            }

            // Clampa a conversão matemática para travar estritamente entre os indexes válidos (0 a 49)
            int targetBatch = Mathf.Clamp((int)(runLogicTimer * 50), 0, 49);
            RunBatchLogic(targetBatch);
        }

        private void StopSpawning()
        {
            Debug.Log("O spawn normal parou. Boss se aproximando!");
        }

        private void StartChaosMode()
        {
            Debug.Log("Fase Caótica Iniciada! Sobreviva se puder.");
            currentSpawnTimer = chaosSpawnDelay;
        }

        private void SpawnEnemy(EnemyController enemy, bool isBoss = false)
        {
            if (!enemy) return;

            var batchToBeAdded = GetBestBatch("enemy");

            var randomDir = Random.insideUnitCircle.normalized;
            var randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);

            var xVal = GameManager.Instance.player.position.x + (randomDir.x * randomDistance);
            var zVal = GameManager.Instance.player.position.z + (randomDir.y * randomDistance);

            var spawnCell = GetSpatialGroup(xVal, zVal);

            var obj = Instantiate(enemy, new Vector3(xVal, enemy.transform.position.y, zVal), Quaternion.Euler(45f, 0f, 0f), enemyHolder);
            if (!isBoss)
                obj.DefineLevel(GetCurrentRandomLevel());
            obj.spatialGroup = spawnCell;
            AddToSpatialGroup(spawnCell, obj);

            obj.BatchID = batchToBeAdded;
            AddToEnemyBatch(batchToBeAdded, obj);
        }

        private int GetCurrentRandomLevel()
        {
            var survivedTime = GameManager.Instance.TotalTimeSurvived;
            var totalTime  = GameManager.Instance.TotalTimeSurvived;

            if (survivedTime > totalTime) return maxLevel;

            var timeToIncrement = (totalTime - 20) / (maxLevel - initialLevel + 1);
            var incrementLevels = Mathf.RoundToInt(survivedTime / timeToIncrement);

            var max = Mathf.Max(maxLevel, initialLevel + incrementLevels);
            var min = Mathf.Max(initialLevel, max - 5);
            return Random.Range(min, max + 1);
        }

        public void RepositionEnemy(EnemyController enemy)
        {
            if (!enemy) return;

            enemy.gameObject.SetActive(false);

            var randomDir = Random.insideUnitCircle.normalized;
            var randomDistance = Random.Range(minRespawnRadius, minSpawnRadius);

            var playerPos = GameManager.Instance.player.position;
            var xVal = playerPos.x + (randomDir.x * randomDistance);
            var zVal = playerPos.z + (randomDir.y * randomDistance);

            var newPosition = new Vector3(xVal, enemy.transform.position.y, zVal);

            var newCell = GetSpatialGroup(xVal, zVal);

            if (enemy.spatialGroup != newCell) 
            {
                RemoveFromSpatialGroup(enemy.spatialGroup, enemy); 
                enemy.spatialGroup = newCell;
                AddToSpatialGroup(newCell, enemy);
            }

            enemy.transform.position = newPosition;

            var trails = enemy.GetComponentsInChildren<TrailRenderer>();
            foreach (var trail in trails)
            {
                trail.Clear();
            }

            enemy.gameObject.SetActive(true);
        }
        
        public class BatchScore : IComparable<BatchScore>
        {
            public int BatchId { get; }
            public int Score { get; private set; }

            public BatchScore(int batchId, int score)
            {
                BatchId = batchId;
                Score = score;
            }

            public void UpdateScore(int delta)
            {
                Score += delta;
            }

            public int CompareTo(BatchScore other)
            {
                if (other == null) return 1;
                int scoreComparison = Score.CompareTo(other.Score);
                if (scoreComparison == 0)
                {
                    return BatchId.CompareTo(other.BatchId);
                }
                return scoreComparison;
            }
        }

        // ==========================================
        // GERENCIAMENTO DE LOTES (BATCHES)
        // ==========================================
        void InitializeBatches()
        {
            enemyBatches.Clear();
            batchScoreMap_Enemy.Clear();
            batchQueue_Enemy.Clear();

            for (int i = 0; i < 50; i++)
            {
                BatchScore batchScore = new BatchScore(i, 0);
                enemyBatches.Add(i, new List<EnemyController>());
                batchScoreMap_Enemy.Add(i, batchScore);
                batchQueue_Enemy.Add(batchScore);
            }
        }

        public void AddToEnemyBatch(int batchId, EnemyController enemy)
        {
            if (enemyBatches.ContainsKey(batchId))
            {
                enemyBatches[batchId].Add(enemy);
            }
        }

        public void UpdateBatchOnUnitDeath(string option, int batchId)
        {
            if (option == "enemy") UpdateBatchOnEnemyDeathRaw(batchQueue_Enemy, batchScoreMap_Enemy, batchId);
        }

        void UpdateBatchOnEnemyDeathRaw(SortedSet<BatchScore> batchQueue, Dictionary<int, BatchScore> batchScoreMap, int batchId)
        {
            if (batchScoreMap.TryGetValue(batchId, out BatchScore batchScore))
            {
                batchQueue.Remove(batchScore);
                batchScore.UpdateScore(-1);
                batchQueue.Add(batchScore);
            }
        }

        public int GetBestBatch(string option)
        {
            if (option == "enemy" && batchQueue_Enemy.Count > 0) return GetBestBatchRaw(batchQueue_Enemy);
            return 0;
        }

        int GetBestBatchRaw(SortedSet<BatchScore> batchQueue)
        {
            if (batchQueue == null || batchQueue.Count == 0) return 0;

            BatchScore leastLoadedBatch = batchQueue.Min;

            if (leastLoadedBatch == null) return 0;

            batchQueue.Remove(leastLoadedBatch);
            leastLoadedBatch.UpdateScore(1);
            batchQueue.Add(leastLoadedBatch);

            return leastLoadedBatch.BatchId;
        }

        void RunBatchLogic(int batchID)
        {
            if (enemyBatches.TryGetValue(batchID, out List<EnemyController> batchList))
            {
                // Cria uma cópia rasa temporária para evitar erros de modificação concorrente se monstros morrerem no loop
                var currentActiveEnemies = batchList.Where(e => e != null).ToList();
                foreach (EnemyController enemy in currentActiveEnemies)
                {
                    enemy.RunLogic();
                }
            }
        }

        // ==========================================
        // SISTEMA DE HASH ESPACIAL (GRID INFINITA)
        // ==========================================

        public Vector2Int GetSpatialGroup(float xPos, float zPos)
        {
            return new Vector2Int(Mathf.FloorToInt(xPos / cellSize), Mathf.FloorToInt(zPos / cellSize));
        }

        public void AddToSpatialGroup(Vector2Int cell, EnemyController enemy)
        {
            if (!enemySpatialGroups.ContainsKey(cell))
            {
                enemySpatialGroups[cell] = new HashSet<EnemyController>();
            }
            enemySpatialGroups[cell].Add(enemy);
        }

        public void RemoveFromSpatialGroup(Vector2Int cell, EnemyController enemy)
        {
            if (enemySpatialGroups.ContainsKey(cell))
            {
                enemySpatialGroups[cell].Remove(enemy);

                if (enemySpatialGroups[cell].Count == 0)
                {
                    enemySpatialGroups.Remove(cell);
                }
            }
        }

        public List<Vector2Int> GetExpandedSpatialGroups(Vector2Int centerCell)
        {
            List<Vector2Int> expandedGroups = new List<Vector2Int>(9);
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    expandedGroups.Add(new Vector2Int(centerCell.x + x, centerCell.y + y));
                }
            }
            return expandedGroups;
        }

        public IEnumerable<EnemyController> GetEnemiesInSpatialGroup(Vector2Int cell)
        {
            // O uso do ToList() aqui blinda o método contra erros de leitura assíncrona/concorrente do Ally.cs
            if (enemySpatialGroups.TryGetValue(cell, out HashSet<EnemyController> group))
            {
                return group.Where(e => e != null).ToList();
            }

            return Enumerable.Empty<EnemyController>();
        }
    }
}