using Sistemata.Core;
using Sistemata.Spawning;
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

    [Header("Despawn")]
    public float despawnDistance = 55f;


    private SpriteRenderer spriteRenderer;

    private Vector3 movementDirection;

    public Vector2Int spatialGroup = Vector2Int.zero;

    void Start()
    {
        // Se você não quiser arrastar no Inspector, o código busca automaticamente
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // move em direcao ao jogador
        transform.position += movementDirection * Time.deltaTime * movementSpeed;

        // FLIP DO SPRITE
        if (movementDirection.x < 0)
        {
            // O inimigo deve olhar para a esquerda
            spriteRenderer.flipX = true;
        }
        else if (movementDirection.x > 0)
        {
            // O inimigo deve olhar para a direita
            spriteRenderer.flipX = false;
        }
    }

    private void OnDestroy()
    {
        // Quando este inimigo for destruído, ele avisa o Spawner para tirá-lo das listas
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.RemoveFromSpatialGroup(spatialGroup, this);
            EnemySpawner.Instance.UpdateBatchOnUnitDeath("enemy", BatchID);
        }
    }

    public void RunLogic()
    {
        if (GameManager.Instance.player == null)
            return;

        // calcula direcao do movimento
        movementDirection = GameManager.Instance.player.position - transform.position;
        movementDirection.y = 0;

        if (movementDirection.sqrMagnitude > despawnDistance * despawnDistance)
        {
            Destroy(gameObject); // Destrói o inimigo se estiver muito longe do jogador
            return;
        }

        movementDirection.Normalize();

        // Afasta outros inimigos proximos
        PushNearbyEnemies();

        //Atualiza grupo espacial
        Vector2Int newSpatialGroup = EnemySpawner.Instance.GetSpatialGroup(transform.position.x, transform.position.z); // GET spatial group
        if (newSpatialGroup != spatialGroup)
        {
            EnemySpawner.Instance.RemoveFromSpatialGroup(spatialGroup, this);

            spatialGroup = newSpatialGroup; // UPDATE current spatial group
            EnemySpawner.Instance.AddToSpatialGroup(spatialGroup, this); // ADD to new spatial group
        }
    }

    void PushNearbyEnemies()
    {
        Vector3 separationVector = Vector3.zero;
        int pushCount = 0;

        // 1. Usamos o m�todo seguro para n�o dar erro de KeyNotFound e iteramos DIRETO na cole��o, sem .ToList()
        foreach (Enemy otherEnemy in EnemySpawner.Instance.GetEnemiesInSpatialGroup(spatialGroup))
        {
            if (otherEnemy == null || otherEnemy == this) continue;

            // 2. Corrigido para Eixos X e Z (Dist�ncia de Manhattan super r�pida)
            float distance = Mathf.Abs(transform.position.x - otherEnemy.transform.position.x) +
                             Mathf.Abs(transform.position.z - otherEnemy.transform.position.z);

            // Se estiver muito perto (ajuste esse 0.2f se eles forem gordinhos)
            if (distance < 0.2f && distance > 0.001f) // > 0.001f evita divis�o por zero se estiverem no exato mesmo pixel
            {
                // 3. Calculamos a dire��o para fugir do colega
                Vector3 pushDir = transform.position - otherEnemy.transform.position;
                pushDir.y = 0; // Garante que ningu�m vai voar

                // Acumulamos a for�a de repuls�o
                separationVector += pushDir.normalized;
                pushCount++;
            }
        }

        // 4. Se precisamos ser empurrados, ajustamos a dire��o principal do inimigo!
        if (pushCount > 0)
        {
            // Tira a m�dia da repuls�o para n�o ser arremessado longe se tiverem 5 zumbis perto
            separationVector /= pushCount;

            // Misturamos a vontade de "ir at� o player" com a vontade de "afastar dos amigos"
            // Esse 1.5f � a FOR�A do empurr�o. Aumente se quiser que eles espalhem mais r�pido.
            movementDirection += separationVector * 1.5f;
            movementDirection.Normalize();
        }
    }
}