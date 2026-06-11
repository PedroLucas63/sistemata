using Sistemata.Core;
using Sistemata.Player;
using Sistemata.Stats;
using Sistemata.Enemy;
using Sistemata.Ally;
using UnityEngine;

namespace Sistemata.Attack
{
    public class RandomDirectionAttack : BaseAttack
    {
        [Header("Configurações do Projétil")]
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private float projectileSpeed = 10f;

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
            if (!ProjectilePoolManager.Instance || !projectilePrefab) return;

            var amount = Mathf.Max(1, Mathf.FloorToInt(AttackStats.GetStat(StatType.Amount).Get()));
            var damage = Damage;
            var ricochet = AttackStats.GetStat(StatType.Ricochet).Get();
            var size = AttackStats.GetStat(StatType.AreaSize).Get();

            var targetTag = BaseAttackOwnerTargetTag();

            for (var i = 0; i < amount; i++)
            {
                var randomAngle = Random.Range(0f, 360f);
                var spawnDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;
                var spawnPosition = transform.position + 0.5f * spawnDirection;

                var proj = ProjectilePoolManager.Instance.GetProjectile(projectilePrefab, spawnPosition, Quaternion.identity);
                
                if (proj)
                    proj.Setup(spawnDirection, projectileSpeed, damage, ricochet, size, targetTag);
            }
        }

        private string BaseAttackOwnerTargetTag()
        {
            return _cachedEnemyOwner ? "Player" : "Enemy";
        }
    }
}