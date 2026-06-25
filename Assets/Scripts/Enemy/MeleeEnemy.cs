using Sistemata.Common;
using UnityEngine;
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
                    AttackVisualTimer = Mathf.Min(0.25f, AttackCooldown * 0.5f);
                }

                MovementDirection = Vector3.zero;
            }
            else
            {
                MovementDirection.Normalize();
            }
        }

        public void OnAnimationAttackEvent()
        {
            ExecuteMeleeDamage();
        }

        private void ExecuteMeleeDamage()
        {
            if (!CurrentTarget) return;

            if (MovementDirection.magnitude > meleeAttackRange) return;

            var health = CurrentTarget.GetComponentInParent<EntityHealth>();
            
            if (health != null)
                health.TakeDamage(Damage);
            else if (CurrentTarget.CompareTag("Player"))
                PlayerManager.Instance.TakeDamage(Damage);
        }
    }
}