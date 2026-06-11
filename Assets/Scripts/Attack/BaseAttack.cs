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
        
        [Header("Vínculo do Ataque")]
        [SerializeField] protected bool belongsToPlayer = true;

        [Header("Controle de Sincronização")]
        [Tooltip("Se marcado, o ataque NÃO dispara sozinho pelo timer. Ele esperará uma chamada externa (ex: Evento de Animação).")]
        [SerializeField] protected bool useManualTrigger = false;

        protected EntityStats AttackStats { get; private set; }
        private float _attackTimer;
        private bool _canAttackManual = true;

        protected virtual void Awake()
        {
            AttackStats = GetComponent<EntityStats>();
        }
        
        protected virtual void Start()
        {
            InitializeAllBaseStats();
            
            if (belongsToPlayer)
            {
                RegisterTag();
            }
            else
            {
                var uniqueId = $"{transform.root.name}_{attackID}";
                UpgradeRegistry.RegisterAttack(uniqueId, AttackStats);
            }
            
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
            AttackStats.InitializeStat(StatType.MaxHealth, 100f); // Failsafe para EntityHealth não reclamar
            AttackStats.InitializeStat(StatType.Damage, baseData.DefaultDamage);
            AttackStats.InitializeStat(StatType.AttackRate, baseData.DefaultAttackRate);
            AttackStats.InitializeStat(StatType.Amount, baseData.DefaultAmount);
            AttackStats.InitializeStat(StatType.Ricochet, baseData.DefaultRicochet);
            AttackStats.InitializeStat(StatType.AreaSize, baseData.DefaultAreaSize);
        }
        
        private void Update()
        {
            _attackTimer -= Time.deltaTime;

            if (useManualTrigger)
            {
                if (_attackTimer <= 0f)
                {
                    _canAttackManual = true;
                }
                return;
            }

            if (!(_attackTimer <= 0f)) return;
            ExecuteAttack();
            ResetTimer();
        }

        public void TriggerAttack()
        {
            // Se o timer ainda não resetou, ignoramos o gatilho manual para respeitar o AttackRate
            if (!useManualTrigger || !_canAttackManual) return;

            ExecuteAttack();
            _canAttackManual = false;
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
            if (belongsToPlayer)
            {
                UpgradeRegistry.UnregisterAttack(attackID);
            }
            else
            {
                var uniqueId = $"{transform.root.name}_{attackID}";
                UpgradeRegistry.UnregisterAttack(uniqueId);
            }
        }

        protected float Damage
        {
            get
            {
                var weaponDamage = AttackStats.GetStat(StatType.Damage).Get();
                
                // Se não pertence ao player ou se o PlayerManager não existe, usamos apenas o dano da arma
                if (!belongsToPlayer || !PlayerManager.Instance) return weaponDamage;

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
                
                // Se não pertence ao player ou se o PlayerManager não existe, usamos apenas a cadência da arma
                if (!belongsToPlayer || !PlayerManager.Instance) return weaponRate;

                var playerRate = 1f;
                var playerStat = PlayerManager.Instance.GetStat(StatType.AttackRate);
                if (playerStat != null) playerRate = playerStat.Get();

                return weaponRate * playerRate;
            }
        }

        public string AttackId => attackID;
    }
}