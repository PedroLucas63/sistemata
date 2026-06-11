using System;
using Sistemata.Common;
using Sistemata.Core;
using Sistemata.Spawning;
using Sistemata.Stats;
using UnityEngine;

namespace Sistemata.Enemy
{
    public abstract class EnemyController : MonoBehaviour
    {
        private int batchId;

        public int BatchID
        {
            get => batchId;
            set => batchId = value;
        }
        
        [Header("Despawn")] 
        public float despawnDistance = 55f;
        
        protected SpriteRenderer SpriteRenderer;
        protected Vector3 MovementDirection;
        public Vector2Int spatialGroup = Vector2Int.zero;

        [Header("Stats")] 
        [SerializeField] protected EnemyBaseData baseData;
        
        protected EntityStats Stats;
        protected EntityHealth Health;

        protected float AttackTimer;
        protected float AttackVisualTimer;

        protected Transform CurrentTarget;

        public float MoveSpeed => Stats.GetStat(StatType.MoveSpeed).Get();
        public float BaseMoveSpeed => Stats.GetStat(StatType.MoveSpeed).BaseValue;
        public Vector2 LastMove => new(MovementDirection.x, MovementDirection.z);
        public float AttackCooldown => 1f / Stats.GetStat(StatType.AttackRate).Get();
        public float Damage => Stats.GetStat(StatType.Damage).Get();
        
        public bool IsAttacking => AttackVisualTimer > 0f;

        private void Awake()
        {
            Stats = GetComponent<EntityStats>();
            if (Stats == null) Stats = GetComponentInChildren<EntityStats>();
            
            Health = GetComponent<EntityHealth>();
            if (Health == null) Health = GetComponentInChildren<EntityHealth>();
            
            if (Health != null) ConfigureEntityHealth();
            
            InitializeAllBaseStats();
            OnAwake();
        }

        protected virtual void OnAwake() { }

        protected virtual void Start()
        {
            if (SpriteRenderer == null)
                SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void ConfigureEntityHealth()
        {
            Health.OnDeath += HandleDeath;
        }
        
        protected virtual void HandleDeath()
        {
            Health.OnDeath -= HandleDeath;
            Debug.Log($"[{gameObject.name}] HandleDeath executado. Destruindo objeto.");
            Destroy(gameObject);
        }

        private void InitializeAllBaseStats()
        {
            Stats.InitializeStat(StatType.MaxHealth, baseData.DefaultMaxHealth);
            Stats.InitializeStat(StatType.MoveSpeed, baseData.DefaultMoveSpeed);
            Stats.InitializeStat(StatType.Damage, baseData.DefaultDamage);
            Stats.InitializeStat(StatType.AttackRate, baseData.DefaultAttackRate);
            Stats.InitializeStat(StatType.Armor, baseData.DefaultArmor);
            
            // Forçamos a sincronização da vida com o MaxHealth recém-inicializado
            if (Health != null)
            {
                // Curamos o valor total para garantir que a vida atual suba (ou desça) para o MaxHealth
                Health.Heal(Health.MaxHealth); 
            }
        }

        public virtual void TakeDamage(float damage)
        {
            if (Health == null) Health = GetComponent<EntityHealth>();
            if (Health == null) return;

            var armor = Stats.GetStat(StatType.Armor)?.Get() ?? 0f;
            damage -= armor;
            damage = Mathf.Max(0f, damage);
            
            Health.TakeDamage(damage);
        }

        protected virtual void Update()
        {
            if (AttackTimer > 0) AttackTimer -= Time.deltaTime;
            if (AttackVisualTimer > 0) AttackVisualTimer -= Time.deltaTime;

            if (!IsAttacking)
                transform.position += MovementDirection * (Time.deltaTime * MoveSpeed);
        }

        private void OnDestroy()
        {
            if (EnemySpawner.Instance == null) return;
            EnemySpawner.Instance.RemoveFromSpatialGroup(spatialGroup, this);
            EnemySpawner.Instance.UpdateBatchOnUnitDeath("enemy", BatchID);
        }

        public void RunLogic()
        {
            CurrentTarget = FindNearestTarget();

            if (!CurrentTarget)
            {
                MovementDirection = Vector3.zero;
                return;
            }

            MovementDirection = CurrentTarget.position - transform.position;
            MovementDirection.y = 0;

            if (MovementDirection.sqrMagnitude > despawnDistance * despawnDistance)
            {
                Destroy(gameObject); 
                return;
            }

            var distanceToTarget = MovementDirection.magnitude;
            UpdateCombatBehavior(distanceToTarget);
            PushNearbyEnemies();
            
            var newSpatialGroup = EnemySpawner.Instance.GetSpatialGroup(transform.position.x, transform.position.z);
            
            if (newSpatialGroup == spatialGroup) return;
            EnemySpawner.Instance.RemoveFromSpatialGroup(spatialGroup, this);

            spatialGroup = newSpatialGroup;
            EnemySpawner.Instance.AddToSpatialGroup(spatialGroup, this);
        }

        /// <summary>
        /// Comportamento de combate implementado de formas diferentes para Curta e Longa distância
        /// </summary>
        protected abstract void UpdateCombatBehavior(float distanceToTarget);

        /// <summary>
        /// Varre o jogador e a lista estática de aliados ativos para eleger o alvo mais próximo
        /// </summary>
        private Transform FindNearestTarget()
        {
            Transform closest = null;
            var closestDistSqr = float.MaxValue;

            if (GameManager.Instance && GameManager.Instance.player)
            {
                closest = GameManager.Instance.player;
                closestDistSqr = (closest.position - transform.position).sqrMagnitude;
            }

            var allies = Ally.Ally.ActiveAllies;
            foreach (var ally in allies)
            {
                if (!ally) continue;

                var distSqr = (ally.transform.position - transform.position).sqrMagnitude;
                if (!(distSqr < closestDistSqr)) continue;
                closestDistSqr = distSqr;
                closest = ally.transform;
            }

            return closest;
        }

        private void PushNearbyEnemies()
        {
            var separationVector = Vector3.zero;
            var pushCount = 0;

            foreach (var otherEnemy in EnemySpawner.Instance.GetEnemiesInSpatialGroup(spatialGroup))
            {
                if (!otherEnemy || otherEnemy == this) continue;

                var distance = Mathf.Abs(transform.position.x - otherEnemy.transform.position.x) +
                               Mathf.Abs(transform.position.z - otherEnemy.transform.position.z);

                if (!(distance < 0.2f) || !(distance > 0.001f)) continue;
                
                var pushDir = transform.position - otherEnemy.transform.position;
                pushDir.y = 0;

                separationVector += pushDir.normalized;
                pushCount++;
            }

            if (pushCount <= 0) return;
            separationVector /= pushCount;
            
            MovementDirection += separationVector * 1.5f;
            MovementDirection.Normalize();
        }
    }
}