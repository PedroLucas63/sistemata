using Sistemata.Core;
using Sistemata.Player;
using Sistemata.Stats;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace Sistemata.Attack
{
    public class ArrowAttack : BaseAttack
    {
        [Header("Configurações da Flecha")]
        [SerializeField] private Projectile arrowPrefab;
        [SerializeField] private float arrowSpeed = 12f;
        [SerializeField] private float fanAngleSpread = 15f;
        
        private ObjectPool<Projectile> _arrowPool;

        protected override void Start()
        {
            base.Start();

            _arrowPool = new ObjectPool<Projectile>(
                createFunc: CreateProjectile,
                actionOnGet: OnTakeFromPool,
                actionOnRelease: OnReturnedToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: true,
                defaultCapacity: 20,
                maxSize: 100
            );
        }
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

                var proj = _arrowPool.Get();
                proj.transform.position = spawnPosition;
                proj.Setup(spawnDirection, arrowSpeed, damage, ricochet, size);
            }
        }
        
        private Projectile CreateProjectile()
        {
            var parent = GameManager.Instance.ProjectileParent;
            var proj = Instantiate(arrowPrefab, parent);
            proj.ManagedPool = _arrowPool;
            return proj;
        }
        
        private static void OnTakeFromPool(Projectile proj)
        {
            proj.gameObject.SetActive(true);
        }

        private static void OnReturnedToPool(Projectile proj)
        {
            proj.gameObject.SetActive(false);
        }

        private static void OnDestroyPoolObject(Projectile proj)
        {
            Destroy(proj.gameObject);
        }
    }
}