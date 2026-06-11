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
                
                // Rotação visual baseada na direção do alvo
                if (toTarget.sqrMagnitude > 0.001f && SpriteRenderer)
                {
                    SpriteRenderer.flipX = toTarget.x < -0.01f;
                }

                if (AttackTimer <= 0f)
                {
                    AttackTimer = AttackCooldown;
                    // Ativa o estado de IsAttacking para o AnimatorController disparar o Trigger de Attack
                    AttackVisualTimer = Mathf.Min(0.25f, AttackCooldown * 0.5f);
                }
            }
            else
            {
                MovementDirection = toTarget.normalized;
            }
        }

        /// <summary>
        /// Chamado via Animation Event na animação de ataque
        /// </summary>
        public void OnAnimationAttackEvent()
        {
            if (_instantiatedAttack != null)
            {
                _instantiatedAttack.TriggerAttack();
            }
        }
    }
}