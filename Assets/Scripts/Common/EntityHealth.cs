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
                return _maxHealthStat?.Get() ?? 1f; // Failsafe reduzido para evitar bugs de "imortalidade"
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

        private void Awake()
        {
            if (_stats == null) _stats = GetComponent<EntityStats>();
            if (_stats == null) _stats = GetComponentInParent<EntityStats>();
            if (_stats == null) _stats = GetComponentInChildren<EntityStats>();
        }

        private void Start()
        {
            // Se o CurrentHealth ainda é 0 (ou seja, não foi inicializado por fora), usamos o MaxHealth
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
            
            CurrentHealth -= amount;
            Debug.Log($"[{gameObject.name}] Levando dano: {amount} (HP Restante: {CurrentHealth}/{MaxHealth})");

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Debug.Log($"[{gameObject.name}] Morreu!");
                Die();
            }

            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
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