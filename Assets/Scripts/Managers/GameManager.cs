using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("Configurações Gerais")]
    public bool gunDemoMode;
    public Transform player;
    private CharacterController playerScript;
    public CharacterController PlayerScript { get { return playerScript; } }

    [Header("Spawning de Inimigos")]
    public GameObject enemyPF;
    public Transform enemyHolder;
    private float enemySpawnTimer = 0f;
    private float enemySpawnTimerCD = 0f;
    private int maxEnemyCount = 100;

    [Header("Lógica de Lotes (Batches)")]
    private Dictionary<int, List<Enemy>> enemyBatches = new Dictionary<int, List<Enemy>>();
    private float runLogicTimer = 0f;
    private float runLogicTimerCD = 1f;

    [Header("Particionamento Espacial Infinito")]
    public float cellSize = 20f; // Tamanho de cada célula (equivalente ao tamanho de uma partição local)

    // Dicionários dinâmicos com Vector2Int (suporta coordenadas infinitas)
    [HideInInspector] public Dictionary<Vector2Int, HashSet<Enemy>> enemySpatialGroups = new Dictionary<Vector2Int, HashSet<Enemy>>();
    //[HideInInspector] public Dictionary<Vector2Int, HashSet<Bullet>> bulletSpatialGroups = new Dictionary<Vector2Int, HashSet<Bullet>>();

    //[Header("Experiência")]
    //public GameObject experiencePointPF;
    //public Transform experiencePointHolder;

    // ==========================================
    // MIN HEAP FOR BATCH (Mantido intacto)
    // ==========================================
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

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (player != null)
            playerScript = player.GetComponent<CharacterController>();

        InitializeBatches();

        int initEnemyCount = gunDemoMode ? 100 : 10000;
        maxEnemyCount = gunDemoMode ? 100 : 10000;

        for (int i = 0; i < initEnemyCount; i++)
        {
            SpawnEnemy();
        }
    }

    void FixedUpdate() // 50 FPS
    {
        if (player == null) return;

        runLogicTimer += Time.fixedDeltaTime;

        if (runLogicTimer >= runLogicTimerCD)
        {
            //RunOnceASecondLogicForAllBullets();
            runLogicTimer = 0f;
        }

        SpawnEnemies();
        RunBatchLogic((int)(runLogicTimer * 50));
    }

    // ==========================================
    // GERENCIAMENTO DE LOTES (BATCHES)
    // ==========================================
    void InitializeBatches()
    {
        for (int i = 0; i < 50; i++)
        {
            BatchScore batchScore = new BatchScore(i, 0);
            enemyBatches.Add(i, new List<Enemy>());
            batchScoreMap_Enemy.Add(i, batchScore);
            batchQueue_Enemy.Add(batchScore);
        }
    }

    public void AddToEnemyBatch(int batchId, Enemy enemy)
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
            foreach (Enemy enemy in enemyBatches[batchID].ToList())
            {
                if (enemy) enemy.RunLogic();
            }
        }
    }

    //void RunOnceASecondLogicForAllBullets()
    //{
    //    foreach (Bullet bullet in bulletSpatialGroups.SelectMany(x => x.Value).ToList())
    //    {
    //        if (bullet) bullet.OnceASecondLogic();
    //    }
    //}

    // ==========================================
    // SPAWN INFINTIO RELATIVO AO JOGADOR
    // ==========================================
    void SpawnEnemies()
    {
        enemySpawnTimer += Time.fixedDeltaTime;

        if (enemySpawnTimer > enemySpawnTimerCD && enemyHolder.childCount < maxEnemyCount)
        {
            for (int i = 0; i < 10; i++)
            {
                SpawnEnemy();
            }
            enemySpawnTimer = 0f;
        }
    }

    void SpawnEnemy()
    {
        if (player == null) return;

        int batchToBeAdded = GetBestBatch("enemy");

        // 1. Pega a célula atual do jogador (usando X e Z)
        Vector2Int playerCell = GetSpatialGroup(player.position.x, player.position.z);

        // 2. Escolhe uma célula vizinha para o spawn (ex: raio de 1 a 4 células de distância)
        int offsetX = Random.Range(-4, 5);
        int offsetZ = Random.Range(-4, 5);
        if (offsetX == 0 && offsetZ == 0) offsetX = 2; // Evita spawnar em cima do jogador

        Vector2Int spawnCell = new Vector2Int(playerCell.x + offsetX, playerCell.y + offsetZ);

        // 3. Define posição global aleatória dentro dessa célula selecionada
        float xVal = (spawnCell.x * cellSize) + Random.Range(0, cellSize);
        float zVal = (spawnCell.y * cellSize) + Random.Range(0, cellSize);

        // 4. Instancia o inimigo
        GameObject enemyGO = Instantiate(enemyPF, new Vector3(xVal, enemyPF.transform.position.y, zVal), Quaternion.identity, enemyHolder);
        Enemy enemyScript = enemyGO.GetComponent<Enemy>();

        // 5. Registra o inimigo no grupo espacial e no batch de lógica
        enemyScript.spatialGroup = spawnCell;
        AddToSpatialGroup(spawnCell, enemyScript);

        enemyScript.BatchID = batchToBeAdded;
        enemyBatches[batchToBeAdded].Add(enemyScript);
    }

    // ==========================================
    // SISTEMA DE HASH ESPACIAL (GRID INFINITA)
    // ==========================================

    // Retorna a coordenada do grid com base nas posições reais X e Z
    public Vector2Int GetSpatialGroup(float xPos, float zPos)
    {
        return new Vector2Int(Mathf.FloorToInt(xPos / cellSize), Mathf.FloorToInt(zPos / cellSize));
    }

    public void AddToSpatialGroup(Vector2Int cell, Enemy enemy)
    {
        if (!enemySpatialGroups.ContainsKey(cell))
        {
            enemySpatialGroups[cell] = new HashSet<Enemy>();
        }
        enemySpatialGroups[cell].Add(enemy);
    }

    public void RemoveFromSpatialGroup(Vector2Int cell, Enemy enemy)
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

    public IEnumerable<Enemy> GetEnemiesInSpatialGroup(Vector2Int cell)
    {
        // TryGetValue tenta pegar batch. Se existir, devolve a lista.
        if (enemySpatialGroups.TryGetValue(cell, out HashSet<Enemy> group))
        {
            return group;
        }

        // Se batch não existir (ninguém pisou lá), devolve uma lista vazia segura
        return Enumerable.Empty<Enemy>();
    }

    // ==========================================
    // UTILITÁRIOS E EXPERIÊNCIA
    // ==========================================
    //public void DropExperiencePoint(Vector3 position, int amount)
    //{
    //    GameObject expPointsGO = Instantiate(experiencePointPF, position, Quaternion.identity, experiencePointHolder);
    //    ExperiencePoint xpScript = expPointsGO.GetComponent<ExperiencePoint>();

    //    xpScript.Amount = amount;
    //    xpScript.SpatialGroup = GetSpatialGroup(position.x, position.z); // Atualizado para Z
    //    xpScript.SurroundingSpatialGroups = new HashSet<Vector2Int>(GetExpandedSpatialGroups(xpScript.SpatialGroup));
    //}
}
