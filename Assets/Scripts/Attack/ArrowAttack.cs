using System;
using Sistemata.Core;
using Sistemata.Player;
using Sistemata.Stats;
using Sistemata.Enemy;
using Sistemata.Ally;
using UnityEngine;

namespace Sistemata.Attack
{
    public class ArrowAttack : BaseAttack
    {
        [Header("Configurações da Flecha")]
        [SerializeField] private Projectile arrowPrefab;
        [SerializeField] private float arrowSpeed = 12f;
        [SerializeField] private float fanAngleSpread = 15f;
        
        private Ally.Ally _cachedAllyOwner;
        private EnemyController _cachedEnemyOwner;

        protected override void Start()
        {
            base.Start();
            
            _cachedAllyOwner = GetComponentInParent<Ally.Ally>();
            _cachedEnemyOwner = GetComponentInParent<EnemyController>();
        }

        protected override void ExecuteAttack()
        {
            if (!ProjectilePoolManager.Instance || !arrowPrefab) return;

            var amount = Mathf.Max(1, Mathf.FloorToInt(AttackStats.GetStat(StatType.Amount).Get()));
            var damage = Damage;
            var ricochet = AttackStats.GetStat(StatType.Ricochet).Get();
            var size = AttackStats.GetStat(StatType.AreaSize).Get();

            var baseDirection = GetOwnerForwardDirection(); 

            var targetTag = belongsToPlayer || _cachedAllyOwner ? "Enemy" : "Player";

            var startAngle = -((amount - 1) * fanAngleSpread) / 2f;

            for (var i = 0; i < amount; i++)
            {
                var currentAngle = startAngle + (i * fanAngleSpread);
                var spawnDirection = Quaternion.Euler(0, currentAngle, 0) * baseDirection;
                var spawnPosition = transform.position + 0.5f * baseDirection;

                var proj = ProjectilePoolManager.Instance.GetProjectile(arrowPrefab, spawnPosition, Quaternion.identity);
                
                if (proj)
                    proj.Setup(spawnDirection, arrowSpeed, damage, ricochet, size, targetTag);
            }
        }

        /// <summary>
        /// Calcula matematicamente a "frente" real para onde o dono está apontando no plano XZ
        /// </summary>
        private Vector3 GetOwnerForwardDirection()
        {
            var forwardVector = transform.right; // Failsafe padrão

            if (belongsToPlayer && PlayerManager.Instance)
            {
                forwardVector = PlayerManager.Instance.GetDirection();
            }
            else if (_cachedAllyOwner)
            {
                var allyLook = _cachedAllyOwner.LastMove;
                if (allyLook.sqrMagnitude > 0.001f)
                {
                    forwardVector = new Vector3(allyLook.x, 0f, allyLook.y);
                }
            }
            else if (_cachedEnemyOwner)
            {
                forwardVector = transform.parent ? transform.parent.forward :
                    transform.right;
            }

            forwardVector.y = 0;

            if (forwardVector.sqrMagnitude < 0.001f)
                forwardVector = transform.right;

            return forwardVector.normalized;
        }
    }
}