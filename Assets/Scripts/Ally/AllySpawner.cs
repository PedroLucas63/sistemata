using Sistemata.Core;
using System.Collections.Generic;
using UnityEngine;

public class AllySpawner : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private GameObject allyPrefab;
    [SerializeField] private int amountToSpawn = 2;
    [SerializeField] private float spawnRadius = 2f; // Distância do player ao nascer

    [Header("Visuais")]
    [SerializeField] private List<Sprite> allySprites; // Lista de imagens diferentes

    void Start()
    {
        // Executa o spawn assim que a rodada começa
        SpawnInitialAllies();
    }

    void SpawnInitialAllies()
    {
        if (GameManager.Instance == null || GameManager.Instance.player == null) return;

        Transform playerTransform = GameManager.Instance.player;

        for (int i = 0; i < amountToSpawn; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = new Vector3(
                playerTransform.position.x + randomCircle.x,
                playerTransform.position.y,
                playerTransform.position.z + randomCircle.y
            );

            GameObject allyGO = Instantiate(allyPrefab, spawnPos, Quaternion.Euler(45f, 0f, 0f));
            Ally allyScript = allyGO.GetComponent<Ally>();

            // --- TROCA DE SPRITE ---
            if (allySprites.Count > 0)
            {
                Sprite chosenSprite = allySprites[i % allySprites.Count];
                allyScript.SetSprite(chosenSprite);
            }

            allyGO.transform.SetParent(transform);
        }
    }
}