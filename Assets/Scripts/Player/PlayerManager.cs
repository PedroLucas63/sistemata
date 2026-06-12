using Sistemata.Attack;
using Sistemata.Common;
using Sistemata.Core;
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
        
        [Header("Progressão")]
        public int currentLevel = 1;
        public float currentXP = 0;
        public int gold = 0;

        public event System.Action<int, float, float> OnXPChanged; // level, current, target
        public event System.Action<int> OnGoldChanged;

        [Header("Referências de Coleta")]
        [Tooltip("Referência opcional ao script de imã no objeto filho.")]
        [SerializeField] private CollectibleMagnet magnetScript;

        private EntityStats _stats;
        private EntityHealth _playerHealth;
        private PlayerMovement _playerMovement;
        private int currentAttacks = 0;
        
        public static PlayerManager Instance { get; private set; }
        public CharacterController PlayerScript => _playerMovement != null ? _playerMovement.GetComponent<CharacterController>() : null;

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
            
            // Inicializa a UI
            OnXPChanged?.Invoke(currentLevel, currentXP, GetRequiredXp(currentLevel));
            OnGoldChanged?.Invoke(gold);
        }

        public void AddGold(int amount)
        {
            gold += amount;
            OnGoldChanged?.Invoke(gold);
        }

        public void AddXP(float amount)
        {
            currentXP += amount;
            var targetXP = GetRequiredXp(currentLevel);

            while (currentXP >= targetXP)
            {
                currentXP -= targetXP;
                LevelUp();
                targetXP = GetRequiredXp(currentLevel);
            }

            OnXPChanged?.Invoke(currentLevel, currentXP, targetXP);
        }

        private void LevelUp()
        {
            currentLevel++;
            
            if (UI.LevelUp.LevelUpUIManager.Instance)
                UI.LevelUp.LevelUpUIManager.Instance.TriggerLevelUp();
        }

        public float GetRequiredXp(int level)
        {
            return Mathf.Floor(20f * Mathf.Pow(1.2f, level - 1));
        }

         private void ConfigurePlayerHealth()
        {
            _playerHealth.OnDeath += HandleDeath;
        }
        
        private void HandleDeath()
        {
            _playerHealth.OnDeath -= HandleDeath;
            
            this.enabled = false;
            if (_playerMovement != null) _playerMovement.enabled = false;
            
            var anim = GetComponentInChildren<Animator>();
            if (anim) anim.enabled = false;
            
            StartCoroutine(DeathSequence());
        }

        private System.Collections.IEnumerator DeathSequence()
        {
            var duration = 0.8f;
            var elapsed = 0f;
            
            var sr = GetComponentInChildren<SpriteRenderer>();
            Quaternion startRotation = sr ? sr.transform.localRotation : transform.localRotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, 90f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                if (sr)
                    sr.transform.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
                else
                    transform.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
                    
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            if (GameManager.Instance)
            {
                GameManager.Instance.PlayerDied();
            }
            
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