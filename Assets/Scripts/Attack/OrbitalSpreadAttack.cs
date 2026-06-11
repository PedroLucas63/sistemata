using Sistemata.Core;
using Sistemata.Player;
using Sistemata.Stats;
using Sistemata.Enemy;
using Sistemata.Ally;
using UnityEngine;

namespace Sistemata.Attack
{
    public class OrbitalSpreadAttack : BaseAttack
    {
        [Header("Configurações do Projétil")]
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private float projectileSpeed = 11f;

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

            var targetTag = _cachedEnemyOwner ? "Player" : "Enemy";

            // Sorteia o ângulo inicial (ponto de partida da distribuição)
            var baseAngle = Random.Range(0f, 360f);
            
            // Divide os 360 graus pelo número total de projéteis
            var angleStep = 360f / amount;

            for (var i = 0; i < amount; i++)
            {
                // Calcula o ângulo exato deste projétil na "roda"
                var currentAngle = baseAngle + (i * angleStep);
                
                var spawnDirection = Quaternion.Euler(0f, currentAngle, 0f) * Vector3.forward;
                var spawnPosition = transform.position + 0.5f * spawnDirection;

                var proj = ProjectilePoolManager.Instance.GetProjectile(projectilePrefab, spawnPosition, Quaternion.identity);
                
                if (proj)
                    proj.Setup(spawnDirection, projectileSpeed, damage, ricochet, size, targetTag);
            }
        }
    }
}