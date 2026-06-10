using Sistemata.Player;
using Sistemata.Stats;
using Sistemata.Upgrades;
using UnityEngine;

namespace Sistemata.Attack
{
    [RequireComponent(typeof(EntityStats))]
    public abstract class BaseAttack : MonoBehaviour
    {
        [Header("Configurações Básicas do Ataque")]
        [SerializeField] protected string attackID;
        [SerializeField] protected ElementalType elementalType;
        
        [Header("Valores de Ataque")]
        [SerializeField] protected AttackBaseData baseData;

        protected EntityStats AttackStats { get; private set; }
        private float _attackTimer;

        protected virtual void Awake()
        {
            AttackStats = GetComponent<EntityStats>();
        }
        
        protected virtual void Start()
        {
            InitializeAllBaseStats();
            RegisterTag();
            ResetTimer();
        }

        protected virtual void RegisterTag()
        {
            UpgradeRegistry.RegisterAttack(attackID, AttackStats);
            if (UpgradePoolManager.Instance != null)
                UpgradePoolManager.Instance.AddUnlockedTag($"Has_{attackID}");
        }

        protected virtual void InitializeAllBaseStats()
        {
            AttackStats.InitializeStat(StatType.Damage, baseData.DefaultDamage);
            AttackStats.InitializeStat(StatType.AttackRate, baseData.DefaultAttackRate);
            AttackStats.InitializeStat(StatType.Amount, baseData.DefaultAmount);
            AttackStats.InitializeStat(StatType.Ricochet, baseData.DefaultRicochet);
            AttackStats.InitializeStat(StatType.AreaSize, baseData.DefaultAreaSize);
        }
        
        private void Update()
        {
            _attackTimer -= Time.deltaTime;
            if (!(_attackTimer <= 0f)) return;
            ExecuteAttack();
            ResetTimer();
        }

        private void ResetTimer()
        {
            var finalRate = Rate;
            _attackTimer = finalRate > 0 ? (1f / finalRate) : 1f;
        }

        protected abstract void ExecuteAttack();

        protected virtual void OnDestroy()
        {
            UpgradeRegistry.UnregisterAttack(attackID);
        }

        protected float Damage
        {
            get
            {
                var weaponDamage = AttackStats.GetStat(StatType.Damage).Get();
                
                if (!PlayerManager.Instance) return weaponDamage;

                var playerWeight = 1f;
                var playerStat = PlayerManager.Instance.GetStat(StatType.Strength);
                if (playerStat != null) playerWeight = playerStat.Get();

                return weaponDamage * playerWeight;
            }
        }
        
        protected float Rate
        {
            get
            {
                var weaponRate = AttackStats.GetStat(StatType.AttackRate).Get();
                
                if (!PlayerManager.Instance) return weaponRate;

                var playerRate = 1f;
                var playerStat = PlayerManager.Instance.GetStat(StatType.AttackRate);
                if (playerStat != null) playerRate = playerStat.Get();

                return weaponRate * playerRate;
            }
        }

        public string AttackId => attackID;
    }
}