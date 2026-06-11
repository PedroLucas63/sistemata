using UnityEngine;
using Sistemata.Attack;

namespace Sistemata.Enemy
{
    public class RangedEnemy : EnemyController
    {
        [Header("Configurações do Combate Ranged")]
        [Tooltip("O Prefab de ataque herdado de BaseAttack que este inimigo usará.")]
        [SerializeField] private BaseAttack attackPrefab;
        [Tooltip("Ponto de origem dos disparos.")]
        [SerializeField] private Transform firePoint;

        [Header("Configurações de Distanciamento")]
        [Tooltip("Alcance máximo do tiro.")]
        [SerializeField] private float shootRange = 10f;
        [Tooltip("Distância mínima que o inimigo tenta manter do alvo. Se o alvo quebrar essa barreira, o inimigo recua.")]
        [SerializeField] private float minKeepDistance = 4f;

        private BaseAttack _instantiatedAttack;

        protected override void Start()
        {
            base.Start();
            InitializeAttachedAttack();
        }

        private void InitializeAttachedAttack()
        {
            if (attackPrefab == null) return;

            var spawnParent = firePoint != null ? firePoint : transform;
            _instantiatedAttack = Instantiate(attackPrefab, spawnParent.position, spawnParent.rotation, spawnParent);
        }

        protected override void UpdateCombatBehavior(float distanceToTarget)
        {
            if (!CurrentTarget) return;
            
            var toTarget = CurrentTarget.position - transform.position;
            toTarget.y = 0;

            if (distanceToTarget < minKeepDistance)
            {
                MovementDirection = -toTarget.normalized;
            }
            else if (distanceToTarget <= shootRange)
            {
                MovementDirection = Vector3.zero;
                if (toTarget.sqrMagnitude > 0.001f)
                {
                    var lookDir = toTarget.normalized;
                    if (SpriteRenderer)
                    {
                        SpriteRenderer.flipX = lookDir.x switch
                        {
                            < -0.01f => true,
                            > 0.01f => false,
                            _ => SpriteRenderer.flipX
                        };
                    }
                }

                if (!(AttackTimer <= 0f)) return;
                AttackTimer = AttackCooldown;
                AttackVisualTimer = Mathf.Min(0.25f, AttackCooldown * 0.5f);
            }
            else
            {
                MovementDirection = toTarget.normalized;
            }
        }
    }
}