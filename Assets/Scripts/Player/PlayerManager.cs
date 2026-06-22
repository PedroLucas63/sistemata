using Sistemata.Attack;
using Sistemata.Common;
using Sistemata.Core;
using Sistemata.Stats;
using UnityEngine;
using Sistemata.Upgrades;
using Sistemata.Audio; // <-- Adicionado para acessar o AudioManager

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

        // ==========================================
        // BLOCO DE ÁUDIO
        // ==========================================
        [Header("Áudio (SFX)")]
        [SerializeField] private AudioClip xpSfx;
        [SerializeField] private AudioClip goldSfx;
        [SerializeField] private AudioClip levelUpSfx;
        [SerializeField] private AudioClip[] upgradeSfx;
        [SerializeField] private AudioClip deathSfx;

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
            OnXPChanged?.Invoke(currentLevel, currentXP, GetRequiredXP(currentLevel));
            OnGoldChanged?.Invoke(gold);
        }

        public void AddGold(int amount)
        {
            gold += amount;

            // Som de Ouro
            if (goldSfx != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX2D(goldSfx);

            OnGoldChanged?.Invoke(gold);
        }

        public void AddXP(float amount)
        {
            currentXP += amount;
            Debug.Log($"XP Coletado: +{amount} | Total: {currentXP}/{GetRequiredXP(currentLevel)}");
            float targetXP = GetRequiredXP(currentLevel);

            // Som de Coleta de XP
            if (xpSfx != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX2D(xpSfx, 0.6f);

            while (currentXP >= targetXP)
            {
                currentXP -= targetXP;
                LevelUp();
                targetXP = GetRequiredXP(currentLevel);
            }

            OnXPChanged?.Invoke(currentLevel, currentXP, targetXP);
        }

        private void LevelUp()
        {
            currentLevel++;
            Debug.Log($"<color=cyan><b>SUBIU DE NÍVEL!</b></color> Novo nível: {currentLevel}");

            // Som de Level Up
            if (levelUpSfx != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX2D(levelUpSfx);
            
            if (GameManager.Instance) GameManager.Instance.AddLevel();

            // Ativa a tela de upgrades
            if (UI.LevelUp.LevelUpUIManager.Instance)
            {
                UI.LevelUp.LevelUpUIManager.Instance.TriggerLevelUp();
            }
        }

        public float GetRequiredXP(int level)
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

            // Som de Morte
            if (deathSfx != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX2D(deathSfx);

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

            PlayRandomSFX(upgradeSfx);
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

        public Stat GetStat(StatType type) => _stats.GetStat(type);

        public Vector3 GetDirection()
        {
            Vector3 dir = new(
                _playerMovement.LastMoveInput.x,
                0,
                _playerMovement.LastMoveInput.y
            );

            return dir.normalized;
        }

        /// <summary>
        /// Sorteia e toca um áudio de uma lista. Evita repetição de código.
        /// </summary>
        private void PlayRandomSFX(AudioClip[] audioArray, float volume = 1f)
        {
            if (audioArray == null || audioArray.Length == 0 || AudioManager.Instance == null) return;

            int randomIndex = Random.Range(0, audioArray.Length);
            AudioClip chosenClip = audioArray[randomIndex];

            AudioManager.Instance.PlaySFX2D(chosenClip, volume);
        }
    }
}