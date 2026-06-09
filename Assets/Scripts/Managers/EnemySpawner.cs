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
        public float chaosSpawnDelay = 0.5f; // Muito mais rápido

        private float currentSpawnTimer;

        private void Start()
        {
            // Se inscreve nos eventos do GameManager
            GameManager.Instance.OnBossWarning += StopSpawning;
            GameManager.Instance.OnBossSpawn += SpawnTheBoss;
            GameManager.Instance.OnChaosStart += StartChaosMode;
        }

        private void OnDestroy()
        {
            // Boa prática: desinscrever de eventos ao ser destruído
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

            // Só spawna monstros normais se estiver na fase Normal ou Chaos
            if (state == GameState.Normal || state == GameState.Chaos)
            {
                currentSpawnTimer -= Time.deltaTime;
                if (currentSpawnTimer <= 0)
                {
                    SpawnEnemy();
                    // Define o delay baseado na fase
                    currentSpawnTimer = (state == GameState.Chaos) ? chaosSpawnDelay : normalSpawnDelay;

                    // DICA: Aqui você pode colocar lógica para diminuir o normalSpawnDelay 
                    // com base no GameManager.Instance.totalTimeSurvived para o jogo ir ficando mais difícil.
                }
            }
        }

        private void StopSpawning()
        {
            // Limpa a tela ou apenas para de spawnar
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
            currentSpawnTimer = chaosSpawnDelay; // Reseta o timer para a velocidade do caos
        }

        private void SpawnEnemy()
        {
            // Sua lógica de escolher pontos aleatórios fora da tela vai aqui
            Instantiate(enemyPrefab, transform.position + (Vector3)Random.insideUnitCircle * 10f, Quaternion.identity);
        }
    }
}
