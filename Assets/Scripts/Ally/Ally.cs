using Sistemata.Core;
using Sistemata.Enemy;
using Sistemata.Spawning;
using System.Collections.Generic;
using UnityEngine;

public class Ally : MonoBehaviour
{
    // --- LISTA GLOBAL DE ALIADOS PARA REPULSÃO ---
    public static List<Ally> activeAllies = new List<Ally>();

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    [Header("Configurações de Movimento")]
    public float moveSpeed = 2.5f;
    public float followDistance = 2f;
    public float maxDistance = 6f;
    public float teleportDistance = 15f;
    public float allyRepulsionRadius = 1.2f; // Distância que eles tentam manter um do outro

    [Header("Configurações de Combate")]
    public float attackDamage = 15f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    public float detectionRadius = 5f;

    private Transform player;
    private float attackTimer;

    private EnemyController targetEnemy;

    // Adiciona e remove da lista global automaticamente
    private void OnEnable() { activeAllies.Add(this); }
    private void OnDisable() { activeAllies.Remove(this); }

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            player = GameManager.Instance.player;
        }

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

    }

    void Update()
    {
        if (player == null) return;

        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer > teleportDistance)
        {
            TeleportToPlayer();
            return;
        }

        if (distToPlayer > maxDistance)
        {
            targetEnemy = null; // Solta o alvo e volta correndo
            MoveTowards(player.position);
            return;
        }

        // --- 1. LÓGICA DE FOCO NO ALVO ---
        // Se o inimigo morrer (a Unity destrói o gameObject) ou fugir muito, limpa o alvo
        if (targetEnemy != null)
        {
            float distToTarget = Vector3.Distance(transform.position, targetEnemy.transform.position);
            if (distToTarget > detectionRadius) // Dá uma margem para ele não desistir tão fácil
            {
                targetEnemy = null;
            }
        }

        // Só procura um alvo novo se estiver sem nenhum
        if (targetEnemy == null)
        {
            targetEnemy = FindBestEnemyInGrid();
        }

        // --- MÁQUINA DE ESTADOS ---
        if (targetEnemy != null)
        {
            float distToEnemy = Vector3.Distance(transform.position, targetEnemy.transform.position);

            if (distToEnemy > attackRange)
            {
                MoveTowards(targetEnemy.transform.position);
            }
            else
            {
                if (attackTimer <= 0) AttackTarget();
            }
        }
        else
        {
            if (distToPlayer > followDistance)
            {
                MoveTowards(player.position);
            }
            else
            {
                // Se já chegou perto do player e não tem alvo, só aplica a repulsão para eles se espalharem bonitinho
                ApplyIdleRepulsion();
            }
        }
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0;
        direction.Normalize();

        // --- 2. APLICA REPULSÃO NO MOVIMENTO ---
        direction += GetAllyRepulsion();
        direction.Normalize();

        transform.position += direction * (moveSpeed * Time.deltaTime);

        // FLIP DO SPRITE
        if (direction.x < 0)
        {
            // O inimigo deve olhar para a esquerda
            spriteRenderer.flipX = true;
        }
        else if (direction.x > 0)
        {
            // O inimigo deve olhar para a direita
            spriteRenderer.flipX = false;
        }
    }

    // Usado quando o aliado está parado ao lado do player só esperando, para não ficarem amontoados
    private void ApplyIdleRepulsion()
    {
        Vector3 repulsion = GetAllyRepulsion();
        if (repulsion != Vector3.zero)
        {
            transform.position += repulsion * (moveSpeed * 0.5f * Time.deltaTime);
        }
    }

    // Calcula o vetor de fuga dos outros aliados
    private Vector3 GetAllyRepulsion()
    {
        Vector3 separationVector = Vector3.zero;
        int pushCount = 0;

        foreach (Ally otherAlly in activeAllies)
        {
            if (otherAlly == null || otherAlly == this) continue;

            // Distância de Manhattan (super rápida para CPU)
            float distance = Mathf.Abs(transform.position.x - otherAlly.transform.position.x) +
                             Mathf.Abs(transform.position.z - otherAlly.transform.position.z);

            if (distance < allyRepulsionRadius && distance > 0.001f)
            {
                Vector3 pushDir = transform.position - otherAlly.transform.position;
                pushDir.y = 0;
                separationVector += pushDir.normalized;
                pushCount++;
            }
        }

        if (pushCount > 0)
        {
            separationVector /= pushCount;
            return separationVector * 1.5f; // Força do empurrão
        }

        return Vector3.zero;
    }

    private void TeleportToPlayer()
    {
        Vector2 randomCircle = Random.insideUnitCircle * 2f;
        transform.position = new Vector3(
            player.position.x + randomCircle.x,
            transform.position.y,
            player.position.z + randomCircle.y
        );
    }

    private void AttackTarget()
    {
        attackTimer = attackCooldown;
        targetEnemy.TakeDamage(attackDamage);
    }

    private EnemyController FindBestEnemyInGrid()
    {
        Vector2Int myCell = EnemySpawner.Instance.GetSpatialGroup(transform.position.x, transform.position.z);
        List<Vector2Int> nearbyCells = EnemySpawner.Instance.GetExpandedSpatialGroups(myCell);

        EnemyController bestEnemy = null;
        float bestScore = float.MaxValue;

        // 1. Calculamos o quadrado do raio de detecção (super rápido para a CPU)
        float detectionSqr = detectionRadius * detectionRadius;

        foreach (Vector2Int cell in nearbyCells)
        {
            foreach (EnemyController enemy in EnemySpawner.Instance.GetEnemiesInSpatialGroup(cell))
            {
                if (enemy == null) continue;

                float distSqr = (transform.position - enemy.transform.position).sqrMagnitude;

                // 2. A CORREÇÃO VITAL: Se o inimigo estiver fora do raio de detecção, ignora ele imediatamente!
                if (distSqr > detectionSqr) continue;

                // 3. Ruído muito menor. Serve apenas como critério de desempate para zumbis próximos.
                float randomNoise = Random.Range(0f, 3f);
                float score = distSqr + randomNoise;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestEnemy = enemy;
                }
            }
        }

        return bestEnemy;
    }

    public void SetSprite(Sprite newSprite)
    {
        if (spriteRenderer != null && newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
        }
    }
}