using System.Collections.Generic;
using System.Linq;
using Sistemata.Common;
using Sistemata.Core;
using Sistemata.Spawning;
using Sistemata.Stats;
using UnityEngine;

namespace Sistemata.Enemy
{
    public abstract class EnemyController : MonoBehaviour
    {
        [Header("AI Configs")]
        [SerializeField] protected float aiTickRate = 0.2f;

        private float _targetChanceRoll = -1f;
        private float _predictionFactor = 0f;
        private float _aiTickTimer;
        private float _lastLogicTime;

        [Header("Reposition Transform")] 
        [SerializeField] private float repositionDistance = 20f;
        [SerializeField] private float repositionTime = 10f;
        
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
        public Transform Target => CurrentTarget;

        protected float RepositionTimer;

        public float MoveSpeed => Stats.GetStat(StatType.MoveSpeed).Get();
        public float BaseMoveSpeed => Stats.GetStat(StatType.MoveSpeed).BaseValue;
        public Vector2 LastMove => new(MovementDirection.x, MovementDirection.z);
        public float AttackCooldown => 1f / Stats.GetStat(StatType.AttackRate).Get();
        public float Damage => Stats.GetStat(StatType.Damage).Get();

        protected int Level;
        
        public bool IsAttacking => AttackVisualTimer > 0f;

        private void Awake()
        {
            Stats = GetComponent<EntityStats>();
            if (Stats == null) Stats = GetComponentInChildren<EntityStats>();
            
            Health = GetComponent<EntityHealth>();
            if (Health == null) Health = GetComponentInChildren<EntityHealth>();
            
            if (Health != null) ConfigureEntityHealth();

            RepositionTimer = repositionTime;
            _aiTickTimer = UnityEngine.Random.Range(0f, aiTickRate);
            
            InitializeAllBaseStats();
            OnAwake();
        }

        protected virtual void OnAwake() { }

        protected virtual void Start()
        {
            if (SpriteRenderer == null)
                SpriteRenderer = GetComponentInChildren<SpriteRenderer>();

            _predictionFactor = Random.value < 0.5f ? UnityEngine.Random.Range(0.3f, 1.0f) : 0f;
            _lastLogicTime = Time.time;
        }

        private void ConfigureEntityHealth()
        {
            Health.OnDeath += HandleDeath;
        }
        
        [Header("Loot")]
        [SerializeField] protected List<LootItem> lootTable = new List<LootItem>();

        protected virtual void HandleDeath()
        {
            Health.OnDeath -= HandleDeath;
            
            // Contabiliza a morte no GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddKill();
            }

            // --- Spawn de Loot ---
            SpawnLoot();

            // --- Trava o personagem ---
            this.enabled = false; 
            MovementDirection = Vector3.zero;
            
            var anim = GetComponentInChildren<Animator>();
            if (anim) anim.enabled = false; 

            var collider = GetComponent<Collider>();
            if (collider) collider.enabled = false; 

            StartCoroutine(DeathSequence());
        }

        protected virtual void SpawnLoot()
        {
            if (!CollectablePoolManager.Instance || lootTable == null) return;

            foreach (var loot in lootTable)
            {
                if (!loot.prefab) continue;

                if (Random.value > loot.dropChance) continue;

                var noise = Random.insideUnitSphere;
                noise.y = 0;
                var spawnPosition = transform.position + noise * 0.1f;

                var instance = CollectablePoolManager.Instance.Spawn(loot.prefab, spawnPosition);
                if (!instance) continue;
                
                var min = loot.minValue;
                var max = loot.maxValue;

                if (loot.scaleWithLevel)
                {
                    var multiplier = Mathf.Max(1f, Level / 2f);
                    max *= multiplier;
                }

                var randomValue = UnityEngine.Random.Range(min, max);
                instance.SetValue(randomValue);
            }
        }

        public void DefineLevel(int level)
        {
            var eligibleStats = new[]
            {
                StatType.MaxHealth,
                StatType.Damage,
                StatType.Armor,
                StatType.MoveSpeed,
                StatType.AttackRate
            };

            for (var i = 1; i < level; i++)
            {
                var randomStat = eligibleStats[Random.Range(0, eligibleStats.Length)];
                var upgrade = GenerateUpgradeForStat(randomStat);
                Stats.ApplyUpgrade(randomStat, upgrade);
            }
            
            if (Health)
            {
                Health.Heal(Health.MaxHealth); 
            }

            Level = level;
        }

        /// <summary>
        /// Cria um modificador do tipo 'Increased' (porcentagem) calibrado para não quebrar a física ou animações do jogo.
        /// </summary>
        private StatModifier GenerateUpgradeForStat(StatType stat)
        {
            var percentageBonus = stat switch
            {
                StatType.MaxHealth => 0.04f // +4% de Vida por upgrade
                ,
                StatType.Damage => 0.02f // +2% de Dano por upgrade
                ,
                StatType.Armor => 0.10f // +10% de Armadura por upgrade
                ,
                StatType.MoveSpeed => 0.03f // +3% de Velocidade (Mantém o controle do NavMesh/Transform)
                ,
                StatType.AttackRate => 0.01f // +1% de Velocidade de Ataque
                ,
                _ => 0f
            };

            return new StatModifier
            {
                Type = ModifierType.Increased,
                Value = percentageBonus,
                Source = "EnemyLevelUpScaling"
            };
        }

        private System.Collections.IEnumerator DeathSequence()
        {
            float duration = 0.5f;
            float elapsed = 0f;
            Quaternion startRotation = SpriteRenderer ? SpriteRenderer.transform.localRotation : transform.localRotation;
            // Rotaciona 90 graus no eixo Z para "cair de lado"
            Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, 90f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                if (SpriteRenderer)
                    SpriteRenderer.transform.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
                else
                    transform.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
                    
                yield return null;
            }

            // Espera um pouco antes de sumir
            yield return new WaitForSeconds(0.5f);
            
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
            if (!Health) Health = GetComponent<EntityHealth>();
            if (!Health) return;

            var armor = Stats.GetStat(StatType.Armor)?.Get() ?? 0f;
            damage -= armor;
            damage = Mathf.Max(1f, damage);
            
            Health.TakeDamage(damage);
        }

        protected virtual void Update()
        {
            if (AttackTimer > 0) AttackTimer -= Time.deltaTime;
            if (AttackVisualTimer > 0) AttackVisualTimer -= Time.deltaTime;

            if (aiTickRate <= 0f)
            {
                RunLogic();
            }
            else
            {
                _aiTickTimer -= Time.deltaTime;
                if (_aiTickTimer <= 0f)
                {
                    RunLogic();
                    _aiTickTimer = aiTickRate;
                }
            }

            if (!IsAttacking)
                transform.position += MovementDirection * (Time.deltaTime * MoveSpeed);
        }

        private void OnDestroy()
        {
            if (EnemySpawner.Instance == null) return;
            EnemySpawner.Instance.RemoveFromSpatialGroup(spatialGroup, this);
        }

        public virtual void RunLogic()
        {
            CurrentTarget = FindTarget();

            if (!CurrentTarget)
            {
                MovementDirection = Vector3.zero;
                return;
            }

            var targetPosition = CurrentTarget.position;
            if (_predictionFactor > 0f && CurrentTarget.CompareTag("Player") && Player.PlayerManager.Instance)
            {
                var controller = Player.PlayerManager.Instance.PlayerScript;
                if (controller)
                    targetPosition += controller.velocity * _predictionFactor;
            }

            MovementDirection = targetPosition - transform.position;
            MovementDirection.y = 0;
            
            float elapsed = Time.time - _lastLogicTime;
            _lastLogicTime = Time.time;

            var distanceToTarget = MovementDirection.magnitude;
            if (distanceToTarget > repositionDistance)
                RepositionTimer -= elapsed;
            else
                RepositionTimer = repositionTime;

            if (RepositionTimer < 0)
            {
                Destroy(gameObject); 
                EnemySpawner.Instance.Spawn();
                return;
            }

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

        private Transform FindTarget()
        {
            if (_targetChanceRoll < 0f)
                _targetChanceRoll = Random.value;

            Transform player = null;
            if (GameManager.Instance && GameManager.Instance.player)
                player = GameManager.Instance.player;

            var allies = Ally.Ally.ActiveAllies
                .Where(a => a)
                .Select(a => a.transform)
                .ToList();

            if (!player && allies.Count == 0)
                return null;

            if (!player)
            {
                var allyIndex = Mathf.FloorToInt(Random.value * allies.Count);
                allyIndex = Mathf.Clamp(allyIndex, 0, allies.Count - 1);
                return allies[allyIndex];
            }

            if (allies.Count == 0  || _targetChanceRoll < 0.5f)
                return player;

            {
                var relativeRoll = _targetChanceRoll - 0.5f;
                var allyIndex = Mathf.FloorToInt((relativeRoll / 0.5f) * allies.Count);
                allyIndex = Mathf.Clamp(allyIndex, 0, allies.Count - 1);
                return allies[allyIndex];
            }
        }
        
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

        protected virtual void PushNearbyEnemies()
        {
            var separationVector = Vector3.zero;
            var pushCount = 0;

            foreach (var otherEnemy in EnemySpawner.Instance.GetEnemiesInSpatialGroup(spatialGroup))
            {
                if (!otherEnemy || otherEnemy == this) continue;

                var pushDir = transform.position - otherEnemy.transform.position;
                pushDir.y = 0;
        
                var distSqr = pushDir.sqrMagnitude;

                switch (distSqr)
                {
                    case > 0.04f:
                        continue;
                    case < 0.0001f:
                        pushDir = new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), 0, UnityEngine.Random.Range(-0.1f, 0.1f));
                        break;
                }

                separationVector += pushDir.normalized;
                pushCount++;
            }

            if (pushCount <= 0) return;
    
            separationVector /= pushCount;
    
            MovementDirection += separationVector * 1.5f;
            MovementDirection.Normalize();
        }
    }

    [System.Serializable]
    public struct LootItem
    {
        [Tooltip("Prefab do item coletável.")]
        public Collectible prefab;
        [Tooltip("Probabilidade de drop do item (de 0 a 1). Ex: 0.1 = 10%")]
        [Range(0f, 1f)] public float dropChance;
        [Tooltip("Valor mínimo que este coletável terá ao dropar.")]
        public float minValue;
        [Tooltip("Valor máximo que este coletável terá ao dropar.")]
        public float maxValue;
        [Tooltip("Se marcado, o valor máximo será multiplicado com base no nível do inimigo (usado para XP).")]
        public bool scaleWithLevel;
    }
}