using Sistemata.Core;
using Sistemata.Enemy;
using Sistemata.Spawning;
using System.Collections.Generic;
using Sistemata.Common;
using Sistemata.Stats;
using UnityEngine;

namespace Sistemata.Ally
{
    public abstract class Ally : MonoBehaviour
    {
        public static readonly List<Ally> ActiveAllies = new List<Ally>();

        [Header("Configurações de Movimento")]
        public float followDistance = 2f;
        public float maxDistance = 6f;
        public float teleportDistance = 15f;
        public float allyRepulsionRadius = 1.2f;

        [Header("Configurações de Combate")]
        public float attackRange = 1.5f;
        public float detectionRadius = 5f;
        
        [Header("Stats")] 
        [SerializeField] protected AllyBaseData baseData;

        protected Transform Player;
        protected float AttackTimer;
        protected float AttackVisualTimer;

        // Modificado para protected para que as classes filhas saibam exatamente quem atacar/mirar
        protected EnemyController TargetEnemy;
        protected SpriteRenderer SpriteRenderer;
        
        protected EntityStats Stats;
        protected EntityHealth Health;
        
        protected Vector3 MovementDirection;
        public float MoveSpeed => Stats.GetStat(StatType.MoveSpeed).Get();
        public float BaseMoveSpeed => Stats.GetStat(StatType.MoveSpeed).BaseValue;
        
        public Vector2 LastMove => new(MovementDirection.x, MovementDirection.z);
        public float AttackCooldown => 1 / Stats.GetStat(StatType.AttackRate).Get();
        public float Damage => Stats.GetStat(StatType.Damage).Get();
        
        public bool IsAttacking => AttackVisualTimer > 0f;

        private void OnEnable() { ActiveAllies.Add(this); }
        private void OnDisable() { ActiveAllies.Remove(this); }
        
        private void Awake()
        {
            Stats = GetComponent<EntityStats>();
            if (Stats == null) Stats = GetComponentInChildren<EntityStats>();

            Health = GetComponent<EntityHealth>();
            if (Health == null) Health = GetComponentInChildren<EntityHealth>();

            SpriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (Health != null) ConfigureEntityHealth();
            
            InitializeAllBaseStats();
        }
        
        private void ConfigureEntityHealth()
        {
            Health.OnDeath += HandleDeath;
        }
            
        protected virtual void HandleDeath()
        {
            Health.OnDeath -= HandleDeath;
            Debug.Log($"[{gameObject.name}] Aliado morreu.");
            Destroy(gameObject);
        }
        
        private void InitializeAllBaseStats()
        {
            Stats.InitializeStat(StatType.MaxHealth, baseData.DefaultMaxHealth);
            Stats.InitializeStat(StatType.MoveSpeed, baseData.DefaultMoveSpeed);
            Stats.InitializeStat(StatType.Damage, baseData.DefaultDamage);
            Stats.InitializeStat(StatType.AttackRate, baseData.DefaultAttackRate);
            Stats.InitializeStat(StatType.Armor, baseData.DefaultArmor);

            if (Health != null)
            {
                Health.Heal(Health.MaxHealth);
            }
        }

        protected virtual void Start()
        {
            if (GameManager.Instance != null && GameManager.Instance.player != null)
                Player = GameManager.Instance.player;
        }

        protected virtual void Update()
        {
            if (!Player) return;
            
            if (AttackTimer > 0) AttackTimer -= Time.deltaTime;
            if (AttackVisualTimer > 0) AttackVisualTimer -= Time.deltaTime;

            var distToPlayer = Vector3.Distance(transform.position, Player.position);

            if (distToPlayer > teleportDistance)
            {
                TeleportToPlayer();
                return;
            }

            if (distToPlayer > maxDistance)
            {
                TargetEnemy = null;
                MoveTowards(Player.position);
                return;
            }

            if (TargetEnemy)
            {
                var distToTarget = Vector3.Distance(transform.position, TargetEnemy.transform.position);
                if (distToTarget > detectionRadius)
                    TargetEnemy = null;
            }

            if (!TargetEnemy)
            {
                TargetEnemy = FindBestEnemyInGrid();
            }

            if (TargetEnemy)
            {
                var distToEnemy = Vector3.Distance(transform.position, TargetEnemy.transform.position);

                if (distToEnemy > attackRange)
                {
                    MoveTowards(TargetEnemy.transform.position);
                }
                else if (AttackTimer <= 0) 
                {
                    AttackTimer = AttackCooldown;
                    AttackVisualTimer = Mathf.Min(0.25f, AttackCooldown * 0.5f);
                    
                    ExecuteAttack();
                }
                else
                {
                    var lookDir = (TargetEnemy.transform.position - transform.position);
                    lookDir.y = 0;
                    if (lookDir.sqrMagnitude > 0.001f) MovementDirection = lookDir.normalized;
                }
            }
            else
            {
                if (distToPlayer > followDistance)
                    MoveTowards(Player.position);
                else
                    ApplyIdleRepulsion();
            }
        }

        private void MoveTowards(Vector3 targetPosition)
        {
            var direction = (targetPosition - transform.position);
            direction.y = 0;
            direction.Normalize();

            direction += GetAllyRepulsion();
            direction.Normalize();

            transform.position += direction * (MoveSpeed * Time.deltaTime);
            MovementDirection = direction;
        }

        private void ApplyIdleRepulsion()
        {
            var repulsion = GetAllyRepulsion();
            if (repulsion != Vector3.zero)
            {
                transform.position += repulsion * (MoveSpeed * 0.5f * Time.deltaTime);
                MovementDirection = repulsion;
            }
            else
            {
                MovementDirection = new Vector3(MovementDirection.x * 0.001f, 0, MovementDirection.z * 0.001f);
            }
        }

        protected Vector3 GetAllyRepulsion()
        {
            var separationVector = Vector3.zero;
            var pushCount = 0;

            foreach (var otherAlly in ActiveAllies)
            {
                if (!otherAlly || otherAlly == this) continue;

                var distance = Mathf.Abs(transform.position.x - otherAlly.transform.position.x) +
                               Mathf.Abs(transform.position.z - otherAlly.transform.position.z);

                if (distance >= allyRepulsionRadius || distance <= 0.001f) continue;
                
                var pushDir = transform.position - otherAlly.transform.position;
                pushDir.y = 0;
                separationVector += pushDir.normalized;
                pushCount++;
            }

            if (pushCount <= 0) return Vector3.zero;
            separationVector /= pushCount;
            return separationVector * 1.5f;
        }

        private void TeleportToPlayer()
        {
            var randomCircle = Random.insideUnitCircle * 2f;
            transform.position = new Vector3(
                Player.position.x + randomCircle.x,
                transform.position.y,
                Player.position.z + randomCircle.y
            );
        }

        /// <summary>
        /// Método abstrato que força as classes derivadas a implementarem suas próprias mecânicas de ataque (Projéteis, Melee, etc.).
        /// </summary>
        protected abstract void ExecuteAttack();

        private EnemyController FindBestEnemyInGrid()
        {
            if (!EnemySpawner.Instance) return null;

            var myCell = EnemySpawner.Instance.GetSpatialGroup(transform.position.x, transform.position.z);
            var nearbyCells = EnemySpawner.Instance.GetExpandedSpatialGroups(myCell);

            EnemyController bestEnemy = null;
            var bestScore = float.MaxValue;
            var detectionSqr = detectionRadius * detectionRadius;

            foreach (var cell in nearbyCells)
            {
                foreach (var enemy in EnemySpawner.Instance.GetEnemiesInSpatialGroup(cell))
                {
                    if (!enemy) continue;

                    var distSqr = (transform.position - enemy.transform.position).sqrMagnitude;

                    if (distSqr > detectionSqr) continue;

                    var randomNoise = Random.Range(0f, 3f);
                    var score = distSqr + randomNoise;

                    if (!(score < bestScore)) continue;
                    bestScore = score;
                    bestEnemy = enemy;
                }
            }

            return bestEnemy;
        }
    }
}