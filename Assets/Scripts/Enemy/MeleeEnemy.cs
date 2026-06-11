using Sistemata.Common;
using UnityEngine;
using Sistemata.Core;
using Sistemata.Player;

namespace Sistemata.Enemy
{
    public class MeleeEnemy : EnemyController
    {
        [Header("Configurações Melee")]
        [SerializeField] private float meleeAttackRange = 1.3f;

        protected override void UpdateCombatBehavior(float distanceToTarget)
        {
            if (distanceToTarget <= meleeAttackRange)
            {
                if (AttackTimer <= 0f)
                {
                    AttackTimer = AttackCooldown;
                    // O IsAttacking fica ativo por AttackVisualTimer
                    AttackVisualTimer = Mathf.Min(0.25f, AttackCooldown * 0.5f);
                    
                    // Pequeno atraso para o dano sincronizar com o "swing" da animação
                    Invoke(nameof(ExecuteMeleeDamage), 0.15f);
                }

                MovementDirection = Vector3.zero;
            }
            else
            {
                MovementDirection.Normalize();
            }
        }

        private void ExecuteMeleeDamage()
        {
            if (!CurrentTarget) return;

            // Tentamos obter o componente de saúde diretamente do alvo ou de seus pais
            var health = CurrentTarget.GetComponentInParent<EntityHealth>();
            
            if (health != null)
            {
                health.TakeDamage(Damage);
            }
            else if (CurrentTarget.CompareTag("Player"))
            {
                // Fallback para o PlayerManager caso o alvo seja o Player e não encontramos o EntityHealth (segurança)
                PlayerManager.Instance.TakeDamage(Damage);
            }
        }
    }
}