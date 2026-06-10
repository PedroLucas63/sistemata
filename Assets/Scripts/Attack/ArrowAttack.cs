using Sistemata.Player;
using Sistemata.Stats;
using UnityEngine;
using UnityEngine.Pool;

namespace Sistemata.Attack
{
    public class ArrowAttack : BaseAttack
    {
        [Header("Configurações da Flecha")]
        [SerializeField] private Projectile arrowPrefab;
        [SerializeField] private float arrowSpeed = 12f;
        [SerializeField] private float fanAngleSpread = 15f;
        
        private ObjectPool<Projectile> _arrowPool;

        protected override void ExecuteAttack()
        {
            var amount = Mathf.Max(1, Mathf.FloorToInt(AttackStats.GetStat(StatType.Amount).Get()));
            var damage = Damage;
            var ricochet = AttackStats.GetStat(StatType.Ricochet).Get();
            var size = AttackStats.GetStat(StatType.AreaSize).Get();

            var baseDirection = transform.right; 
            if (PlayerManager.Instance)
                baseDirection = PlayerManager.Instance.GetDirection(); 

            var startAngle = -((amount - 1) * fanAngleSpread) / 2f;

            for (var i = 0; i < amount; i++)
            {
                var currentAngle = startAngle + (i * fanAngleSpread);
                var spawnDirection = Quaternion.Euler(0, currentAngle, 0) * baseDirection;
                var spawnPosition = transform.position + 0.5f * baseDirection;

                var projObj = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);
                if (projObj.TryGetComponent<Projectile>(out var proj))
                    proj.Setup(spawnDirection, arrowSpeed, damage, ricochet, size);
            }
        }
    }
}