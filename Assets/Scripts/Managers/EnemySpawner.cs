using Sistemata.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sistemata.Enemy;

namespace Sistemata.Spawning
{
    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner Instance { get; private set; }

        [Header("Prefabs")]
        public GameObject enemyPrefab;
        public GameObject bossPrefab;

        [Header("Configurações de Spawn")]
        public float normalSpawnDelay = 2f;
        public float chaosSpawnDelay = 0.5f;
        public int maxEnemyCount = 100;
        public int initEnemyCount = 20;
        public float minSpawnRadius = 25f;
        public float maxSpawnRadius = 35f;
        public Transform enemyHolder;

        private float currentSpawnTimer;

        [Header("Lógica de Lotes (Batches)")]
        private Dictionary<int, List<EnemyController>> enemyBatches = new Dictionary<int, List<EnemyController>>();
        private float runLogicTimer = 0f;
        private float runLogicTimerCD = 1f;

        [Header("Particionamento Espacial Infinito")]
        public float cellSize = 20f; // Tamanho de cada célula (equivalente ao tamanho de uma partição local)

        [HideInInspector] public Dictionary<Vector2Int, HashSet<EnemyController>> enemySpatialGroups = new Dictionary<Vector2Int, HashSet<EnemyController>>();

        private void Awake()
        {
            if (Instance == null) {
                Instance = this;
            }
            else Destroy(gameObject);
        }

        private void Start()
        {
            GameManager.Instance.OnBossWarning += StopSpawning;
            GameManager.Instance.OnBossSpawn += SpawnTheBoss;
            GameManager.Instance.OnChaosStart += StartChaosMode;

            InitializeBatches();

            for (int i = 0; i < initEnemyCount; i++)
            {
                SpawnEnemy(enemyPrefab);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBossWarning -= StopSpawning;
                GameManager.Instance.OnBossSpawn -= SpawnTheBoss;
                GameManager.Instance.OnChaosStart -= StartChaosMode;
            }
        }

        private void Update()
        {
            GameState state = GameManager.Instance.currentState;

            if (state == GameState.Normal || state == GameState.Chaos)
            {
                currentSpawnTimer -= Time.deltaTime;
                if (currentSpawnTimer <= 0 && enemyHolder.childCount < maxEnemyCount)
                {
                    SpawnEnemy(enemyPrefab);
                    currentSpawnTimer = (state == GameState.Chaos) ? chaosSpawnDelay : normalSpawnDelay;
                }
            }
        }

        private void FixedUpdate()
        {
            // Os batches ficam no FixedUpdate, garantindo que rodem perfeitamente 50 vezes por segundo
            runLogicTimer += Time.fixedDeltaTime;

            if (runLogicTimer >= runLogicTimerCD)
            {
                runLogicTimer = 0f;
            }

            RunBatchLogic((int)(runLogicTimer * 50));
        }

        private void StopSpawning()
        {
            Debug.Log("O spawn normal parou. Boss se aproximando!");
        }

        private void SpawnTheBoss()
        {
            Debug.Log("Boss Spawnou!");
            SpawnEnemy(bossPrefab);
        }

        private void StartChaosMode()
        {
            Debug.Log("Fase Caótica Iniciada! Sobreviva se puder.");
            currentSpawnTimer = chaosSpawnDelay;
        }

        void SpawnEnemy(GameObject enemy)
        {
            int batchToBeAdded = GetBestBatch("enemy");

            // Cria uma direção aleatória em 360 graus
            Vector2 randomDir = Random.insideUnitCircle.normalized;

            // Sorteia uma distância entre o limite mínimo (fora da tela) e máximo
            float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);

            // Calcula a posição exata no mundo (usando X e Z)
            float xVal = GameManager.Instance.player.position.x + (randomDir.x * randomDistance);
            float zVal = GameManager.Instance.player.position.z + (randomDir.y * randomDistance);

            // Descobre em qual célula do Grid essa posição caiu (para o Hashing)
            Vector2Int spawnCell = GetSpatialGroup(xVal, zVal);

            // Instancia o inimigo
            GameObject enemyGO = Instantiate(enemy, new Vector3(xVal, enemy.transform.position.y, zVal), Quaternion.Euler(45f, 0f, 0f), enemyHolder);
            EnemyController enemyScript = enemyGO.GetComponent<EnemyController>();

            // Registra o inimigo no grupo espacial e no batch de lógica
            enemyScript.spatialGroup = spawnCell;
            AddToSpatialGroup(spawnCell, enemyScript);

            enemyScript.BatchID = batchToBeAdded;
            AddToEnemyBatch(batchToBeAdded, enemyScript);
        }

        public class BatchScore : System.IComparable<BatchScore>
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
                int scoreComparison = Score.CompareTo(other.Score);
                if (scoreComparison == 0)
                {
                    return BatchId.CompareTo(other.BatchId);
                }
                return scoreComparison;
            }
        }

        private SortedSet<BatchScore> batchQueue_Enemy = new SortedSet<BatchScore>();
        private Dictionary<int, BatchScore> batchScoreMap_Enemy = new Dictionary<int, BatchScore>();

        // ==========================================
        // GERENCIAMENTO DE LOTES (BATCHES)
        // ==========================================
        void InitializeBatches()
        {
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
            enemyBatches[batchId].Add(enemy);
        }

        public void UpdateBatchOnUnitDeath(string option, int batchId)
        {
            if (option == "enemy") UpdateBatchOnEnemyDeathRaw(batchQueue_Enemy, batchScoreMap_Enemy, batchId);
        }

        void UpdateBatchOnEnemyDeathRaw(SortedSet<BatchScore> batchQueue, Dictionary<int, BatchScore> batchScoreMap, int batchId)
        {
            if (batchScoreMap.ContainsKey(batchId))
            {
                BatchScore batchScore = batchScoreMap[batchId];
                batchQueue.Remove(batchScore);
                batchScore.UpdateScore(-1);
                batchQueue.Add(batchScore);
            }
        }

        public int GetBestBatch(string option)
        {
            if (option == "enemy") return GetBestBatchRaw(batchQueue_Enemy);
            return -1;
        }

        int GetBestBatchRaw(SortedSet<BatchScore> batchQueue)
        {
            BatchScore leastLoadedBatch = batchQueue.Min;

            if (leastLoadedBatch == null) return 0;

            batchQueue.Remove(leastLoadedBatch);
            leastLoadedBatch.UpdateScore(1);
            batchQueue.Add(leastLoadedBatch);

            return leastLoadedBatch.BatchId;
        }

        void RunBatchLogic(int batchID)
        {
            // Usa ToList() ou iteração segura se houver remoção de inimigos no meio do loop
            if (enemyBatches.ContainsKey(batchID))
            {
                foreach (EnemyController enemy in enemyBatches[batchID].ToList())
                {
                    if (enemy) enemy.RunLogic();
                }
            }
        }

        // ==========================================
        // SISTEMA DE HASH ESPACIAL (GRID INFINITA)
        // ==========================================

        // Retorna a coordenada do grid com base nas posições reais X e Z
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

                // Opcional: Remove a célula do dicionário se ficar vazia (libera memória)
                if (enemySpatialGroups[cell].Count == 0)
                {
                    enemySpatialGroups.Remove(cell);
                }
            }
        }

        // Retorna a célula central + os 8 vizinhos (útil para detecção de colisão/visão)
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
            // TryGetValue tenta pegar batch. Se existir, devolve a lista.
            if (enemySpatialGroups.TryGetValue(cell, out HashSet<EnemyController> group))
            {
                return group;
            }

            // Se batch não existir (ninguém pisou lá), devolve uma lista vazia segura
            return Enumerable.Empty<EnemyController>();
        }
    }
}
