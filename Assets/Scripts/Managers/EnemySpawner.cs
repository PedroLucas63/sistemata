using System;
using Sistemata.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sistemata.Enemy;
using Sistemata.Player;
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
        public int chaosSpawnModifier = 1;
        public float invasionSpawnDelay = 0.5f;
        public int invasionSpawnCount = 4;
        public float playerPositionPrediction = 1f;
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

        [Header("Particionamento Espacial Infinito")]
        public float cellSize = 20f;

        [HideInInspector] 
        public Dictionary<Vector2Int, HashSet<EnemyController>> enemySpatialGroups = new Dictionary<Vector2Int, HashSet<EnemyController>>();

        private float _initialSpawnTimer;
        private bool _firstSpawn = false;
        private int _enemyWeightSum = 0;
        private int _bossWeightSum = 0;

        // Coleções devidamente inicializadas direto na declaração para evitar falhas de ciclo de vida
        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
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
            if (SpawnStateBased(state))
                UpdateSpawnDelay(state);
        }

        private bool SpawnStateBased(GameState state)
        {
            currentSpawnTimer -= Time.deltaTime;
            if (!(currentSpawnTimer <= 0) || enemyHolder.childCount >= maxEnemyCount) return false;
            
            switch (state)
            {
                case GameState.Normal:
                    Spawn();
                    break;
                case GameState.Invasion:
                {
                    for (var i = 0; i < invasionSpawnCount; i++)
                        Spawn();
                    break;
                }
                case GameState.Chaos:
                {
                    var timeSurvived = GameManager.Instance.TotalTimeSurvived;
                    var bossTime = GameManager.Instance.timeUntilBoss;
                    var count = Mathf.FloorToInt((timeSurvived - bossTime) / 60) * chaosSpawnModifier;
                    count = Mathf.Max(1, count);

                    for (var i = 0; i < count; i++)
                        Spawn();
                    break;
                }
                case GameState.InvasionTransition:
                case GameState.BossTransition:
                case GameState.Boss:
                case GameState.ChaosTransition:
                case GameState.GameOver:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
            
            return true;
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
        private void UpdateSpawnDelay(GameState state)
        {
            switch (state)
            {
                case GameState.Chaos:
                    currentSpawnTimer = chaosSpawnDelay;
                    break;
                case GameState.Invasion:
                    currentSpawnTimer = invasionSpawnDelay;
                    break;
                case GameState.Normal:
                {
                    var timeSurvived = GameManager.Instance.TotalTimeSurvived;
                    var dynamicDelay = normalSpawnDelay / (1f + (timeSurvived * difficultyScaleSpeed));
                    currentSpawnTimer = Mathf.Max(dynamicDelay, minimumSpawnDelay);
                    break;
                }
                case GameState.InvasionTransition:
                case GameState.BossTransition:
                case GameState.Boss:
                case GameState.ChaosTransition:
                case GameState.GameOver:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
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
            
            Vector2 spawnDir;
            var moveDir = Vector3.zero;
            var player = PlayerManager.Instance;
            if (player)
            {
                var controller = player.PlayerScript;
                if (controller && controller.velocity.sqrMagnitude > 0.01f)
                    moveDir = controller.velocity.normalized;
                else
                    moveDir = player.GetDirection();
            }

            if (moveDir.sqrMagnitude > 0.001f && Random.value < 0.8f)
            {
                var angle = Random.Range(-60f, 60f);
                var spawnDir3D = Quaternion.Euler(0f, angle, 0f) * moveDir;
                spawnDir = new Vector2(spawnDir3D.x, spawnDir3D.z).normalized;
            }
            else
            {
                spawnDir = Random.insideUnitCircle.normalized;
            }

            var randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);

            var playerDir = player ? player.GetDirection() * playerPositionPrediction : Vector3.zero;
            var playerPosition = player ? player.transform.position : GameManager.Instance.player.position;
            playerPosition += playerDir;

            var xVal = playerPosition.x + (spawnDir.x * randomDistance);
            var zVal = playerPosition.z + (spawnDir.y * randomDistance);

            var spawnCell = GetSpatialGroup(xVal, zVal);

            var obj = Instantiate(enemy, new Vector3(xVal, enemy.transform.position.y, zVal), Quaternion.Euler(45f, 0f, 0f), enemyHolder);
            if (!isBoss)
                obj.DefineLevel(GetCurrentRandomLevel());
            obj.spatialGroup = spawnCell;
            AddToSpatialGroup(spawnCell, obj);
        }

        private int GetCurrentRandomLevel()
        {
            var survivedTime = GameManager.Instance.TotalTimeSurvived;
            var totalTime = GameManager.Instance.timeUntilBoss;

            if (survivedTime > totalTime) return maxLevel;

            var timeToIncrement = (totalTime - 20f) / (maxLevel - initialLevel + 1);
            var incrementLevels = Mathf.RoundToInt(survivedTime / timeToIncrement);

            var max = Mathf.Min(maxLevel, initialLevel + incrementLevels);
            var min = Mathf.Max(initialLevel, max - 5);
            return Random.Range(min, max + 1);
        }

        public void RepositionEnemy(EnemyController enemy)
        {
            if (!enemy) return;

            enemy.gameObject.SetActive(false);

            Vector2 spawnDir;
            Vector3 moveDir = Vector3.zero;
            if (Sistemata.Player.PlayerManager.Instance)
            {
                var controller = Sistemata.Player.PlayerManager.Instance.PlayerScript;
                if (controller != null && controller.velocity.sqrMagnitude > 0.01f)
                    moveDir = controller.velocity.normalized;
                else
                    moveDir = Sistemata.Player.PlayerManager.Instance.GetDirection();
            }

            // Se o player estiver se movendo, 80% de chance de spawnar no cone de 120º à sua frente
            if (moveDir.sqrMagnitude > 0.001f && Random.value < 0.8f)
            {
                float angle = Random.Range(-60f, 60f); // Cone de 120 graus à frente
                Vector3 spawnDir3D = Quaternion.Euler(0f, angle, 0f) * moveDir;
                spawnDir = new Vector2(spawnDir3D.x, spawnDir3D.z).normalized;
            }
            else
            {
                // Caso contrário (ou 20% das vezes), spawn aleatório ao redor
                spawnDir = Random.insideUnitCircle.normalized;
            }

            var randomDistance = Random.Range(minRespawnRadius, minSpawnRadius);

            var playerPos = GameManager.Instance.player.position;
            var xVal = playerPos.x + (spawnDir.x * randomDistance);
            var zVal = playerPos.z + (spawnDir.y * randomDistance);

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