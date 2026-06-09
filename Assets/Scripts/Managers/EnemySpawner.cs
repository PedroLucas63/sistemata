using Sistemata.Core;
using UnityEngine;

namespace Sistemata.Spawning
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject enemyPrefab;
        public GameObject bossPrefab;

        [Header("Configurações de Spawn")]
        public float normalSpawnDelay = 2f;
        public float chaosSpawnDelay = 0.5f;

        private float currentSpawnTimer;

        private void Start()
        {
            GameManager.Instance.OnBossWarning += StopSpawning;
            GameManager.Instance.OnBossSpawn += SpawnTheBoss;
            GameManager.Instance.OnChaosStart += StartChaosMode;
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
                if (currentSpawnTimer <= 0)
                {
                    SpawnEnemy();
                    currentSpawnTimer = (state == GameState.Chaos) ? chaosSpawnDelay : normalSpawnDelay;
                }
            }
        }

        private void StopSpawning()
        {
            Debug.Log("O spawn normal parou. Boss se aproximando!");
        }

        private void SpawnTheBoss()
        {
            Debug.Log("Boss Spawnou!");
            Instantiate(bossPrefab, transform.position, Quaternion.identity);
        }

        private void StartChaosMode()
        {
            Debug.Log("Fase Caótica Iniciada! Sobreviva se puder.");
            currentSpawnTimer = chaosSpawnDelay;
        }

        private void SpawnEnemy()
        {
            Instantiate(enemyPrefab, transform.position + (Vector3)Random.insideUnitCircle * 10f, Quaternion.identity);
        }
    }
}
