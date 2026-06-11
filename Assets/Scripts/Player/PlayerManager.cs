using Sistemata.Attack;
using Sistemata.Common;
using Sistemata.Stats;
using UnityEngine;
using Sistemata.Upgrades;

namespace Sistemata.Player
{
    [RequireComponent(typeof(EntityStats))]
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] private PlayerBaseData baseData;
        
        [Header("Sistema de Armas / Ataques")]
        [Tooltip("O prefab do ataque com o qual o player sempre começa a run.")]
        [SerializeField] private BaseAttack startingAttackPrefab;
        [Tooltip("Objeto de ancoragem opcional para organizar os ataques dentro da hierarquia do Player.")]
        [SerializeField] private Transform attacksContainer;
        
        private EntityStats _stats;
        private EntityHealth _playerHealth;
        private PlayerMovement _playerMovement;
        private int currentAttacks = 0;
        
        public static PlayerManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            
            _stats = GetComponent<EntityStats>();
            _playerMovement = GetComponent<PlayerMovement>();
            _playerHealth = GetComponent<EntityHealth>();
            if (attacksContainer == null) attacksContainer = transform;
        }

        private void Start()
        {
            InitializeAllBaseStats();
            SpawnStartingAttack();
            ConfigurePlayerHealth();
        }

         private void ConfigurePlayerHealth()
        {
            _playerHealth.OnDeath += HandleDeath;
        }
        
        private void HandleDeath()
        {
            _playerHealth.OnDeath -= HandleDeath;
            Destroy(gameObject);
        }

        public void TakeDamage(float damage)
        {
            _playerHealth.TakeDamage(damage);
        }

        private void InitializeAllBaseStats()
        {
            _stats.InitializeStat(StatType.MaxHealth, baseData.DefaultMaxHealth);
            _stats.InitializeStat(StatType.HealthRegen, baseData.DefaultHealthRegen);
        
            _stats.InitializeStat(StatType.MoveSpeed, baseData.DefaultMoveSpeed);
            _stats.InitializeStat(StatType.PickupRadius, baseData.DefaultPickupRadius);
        
            _stats.InitializeStat(StatType.Strength, baseData.DefaultStrength);
            _stats.InitializeStat(StatType.AttackRate, baseData.DefaultAttackRate);
            _stats.InitializeStat(StatType.Armor, baseData.DefaultArmor);
        
            _stats.InitializeStat(StatType.SummonCap, baseData.DefaultSummonCap);
        }

        public void ApplyRunUpgrade(UpgradeData chosenUpgrade)
        {
            var newModifier = new StatModifier()
            {
                Source = chosenUpgrade.UpgradeName,
                Type = chosenUpgrade.ModType,
                Value = chosenUpgrade.Amount
            };
            _stats.ApplyUpgrade(chosenUpgrade.TargetStat, newModifier);
        }
        
        private void SpawnStartingAttack()
        {
            if (startingAttackPrefab != null)
                UnlockNewAttack(startingAttackPrefab);
        }
        
        public void UnlockNewAttack(BaseAttack attackPrefab)
        {
            if (!attackPrefab) return;
            Instantiate(attackPrefab, attacksContainer.position, Quaternion.identity, attacksContainer);
        }
        
        public Stat GetStat(StatType type) =>  _stats.GetStat(type);

        public Vector3 GetDirection()
        {
            Vector3 dir = new(
                _playerMovement.LastMoveInput.x,
                0,
                _playerMovement.LastMoveInput.y
            );
            
            return dir.normalized;
        }
    }
}