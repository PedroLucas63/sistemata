using System;
using Sistemata.Stats;
using UnityEngine;

namespace Sistemata.Common
{
    [RequireComponent(typeof(EntityStats))]
    public class EntityHealth : MonoBehaviour
    {
        private EntityStats _stats;
        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;
        
        private Stat _maxHealthStat;
        private Stat _healthRegenStat;
        
        public float MaxHealth 
        { 
            get
            {
                _maxHealthStat ??= _stats.GetStat(StatType.MaxHealth);
                return _maxHealthStat?.Get() ?? 1f; 
            } 
        }

        public float HealthRegen 
        { 
            get
            {
                _healthRegenStat ??= _stats.GetStat(StatType.HealthRegen);
                return _healthRegenStat?.Get() ?? 0f;
            } 
        }

        [Header("Configurações de Dano")]
        [SerializeField] private float damageCooldown = 0.3f;
        private float _lastDamageTime;
        
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor = Color.white;
        private Coroutine _flashCoroutine;

        private void Awake()
        {
            if (_stats == null) _stats = GetComponent<EntityStats>();
            if (_stats == null) _stats = GetComponentInParent<EntityStats>();
            if (_stats == null) _stats = GetComponentInChildren<EntityStats>();
            
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_spriteRenderer != null) _originalColor = _spriteRenderer.color;
        }

        private void Start()
        {
            if (CurrentHealth <= 0)
            {
                CurrentHealth = MaxHealth;
            }
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void Update()
        {
            if (IsDead) return;

            if (CurrentHealth < MaxHealth && HealthRegen > 0)
            {
                ExecuteRegeneration();
            }
        }

        private void ExecuteRegeneration()
        {
            CurrentHealth += HealthRegen * Time.deltaTime;

            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }

            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;
            
            if (Time.time < _lastDamageTime + damageCooldown) return;
            _lastDamageTime = Time.time;
            
            CurrentHealth -= amount;

            // Feedback visual de dano
            TriggerDamageFlash();

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Die();
            }

            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void TriggerDamageFlash()
        {
            if (_spriteRenderer == null) return;
            
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashRoutine());
        }

        private System.Collections.IEnumerator FlashRoutine()
        {
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.color = _originalColor;
            _flashCoroutine = null;
        }

        public void Heal(float amount)
        {
            if (IsDead) return;

            CurrentHealth += amount;

            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }

            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void Die()
        {
            IsDead = true;
            OnDeath?.Invoke();
        }
    }
}
