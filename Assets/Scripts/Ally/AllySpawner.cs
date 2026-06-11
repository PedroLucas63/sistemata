using System.Collections.Generic;
using UnityEngine;
using Sistemata.Core;

namespace Sistemata.Ally
{
    public class AllySpawner : MonoBehaviour
    {
        [Header("Configurações de Spawn")]
        [SerializeField] private int amountToSpawn = 2;
        [SerializeField] private float spawnRadius = 2f;

        [Header("Lista de Aliados Disponíveis")]
        [Tooltip("Arraste para cá os Prefabs configurados de cada aliado diferente.")]
        [SerializeField] private List<Ally> allyPrefabs;

        private void Start()
        {
            SpawnInitialAllies();
        }

        private void SpawnInitialAllies()
        {
            if (GameManager.Instance == null || GameManager.Instance.player == null) return;
            
            if (allyPrefabs == null || allyPrefabs.Count == 0)
                return;

            var playerTransform = GameManager.Instance.player;
            
            for (var i = 0; i < amountToSpawn; i++)
            {
                var randomCircle = Random.insideUnitCircle * spawnRadius;
                var spawnPos = new Vector3(
                    playerTransform.position.x + randomCircle.x,
                    playerTransform.position.y,
                    playerTransform.position.z + randomCircle.y
                );
                
                Ally chosenPrefab = allyPrefabs[i % allyPrefabs.Count];

                if (chosenPrefab == null) continue;

                Ally allyInstance = Instantiate(chosenPrefab, spawnPos, Quaternion.Euler(45f, 0f, 0f));
                allyInstance.transform.SetParent(transform);
            }
        }
    }
}