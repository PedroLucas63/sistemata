using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    int batchId;
    public int BatchID
    {
        get { return batchId; }
        set { batchId = value; }
    }

    [Header("Movement")]
    [SerializeField] private float movementSpeed = 1f;
   
    private Vector3 movementDirection;

    public Vector2Int spatialGroup = Vector2Int.zero;

    void Update()
    {
        // move em direcao ao jogador
        transform.position += movementDirection * Time.deltaTime * movementSpeed;
    }

    public void RunLogic()
    {
        if (GameManager.instance.player == null)
            return;

        // calcula direcao do movimento
        movementDirection = GameManager.instance.player.position - transform.position;
        movementDirection.Normalize();

        // Afasta outros inimigos proximos
        PushNearbyEnemies();

        //Atualiza grupo espacial
        Vector2Int newSpatialGroup = GameManager.instance.GetSpatialGroup(transform.position.x, transform.position.z); // GET spatial group
        if (newSpatialGroup != spatialGroup)
        {
            GameManager.instance.RemoveFromSpatialGroup(spatialGroup, this);

            spatialGroup = newSpatialGroup; // UPDATE current spatial group
            GameManager.instance.AddToSpatialGroup(spatialGroup, this); // ADD to new spatial group
        }
    }

    void PushNearbyEnemies()
    {
        Vector3 separationVector = Vector3.zero;
        int pushCount = 0;

        // 1. Usamos o método seguro para não dar erro de KeyNotFound e iteramos DIRETO na coleção, sem .ToList()
        foreach (Enemy otherEnemy in GameManager.instance.GetEnemiesInSpatialGroup(spatialGroup))
        {
            if (otherEnemy == null || otherEnemy == this) continue;

            // 2. Corrigido para Eixos X e Z (Distância de Manhattan super rápida)
            float distance = Mathf.Abs(transform.position.x - otherEnemy.transform.position.x) +
                             Mathf.Abs(transform.position.z - otherEnemy.transform.position.z);

            // Se estiver muito perto (ajuste esse 0.2f se eles forem gordinhos)
            if (distance < 0.2f && distance > 0.001f) // > 0.001f evita divisão por zero se estiverem no exato mesmo pixel
            {
                // 3. Calculamos a direção para fugir do colega
                Vector3 pushDir = transform.position - otherEnemy.transform.position;
                pushDir.y = 0; // Garante que ninguém vai voar

                // Acumulamos a força de repulsão
                separationVector += pushDir.normalized;
                pushCount++;
            }
        }

        // 4. Se precisamos ser empurrados, ajustamos a direção principal do inimigo!
        if (pushCount > 0)
        {
            // Tira a média da repulsão para não ser arremessado longe se tiverem 5 zumbis perto
            separationVector /= pushCount;

            // Misturamos a vontade de "ir até o player" com a vontade de "afastar dos amigos"
            // Esse 1.5f é a FORÇA do empurrão. Aumente se quiser que eles espalhem mais rápido.
            movementDirection += separationVector * 1.5f;
            movementDirection.Normalize();
        }
    }
}
