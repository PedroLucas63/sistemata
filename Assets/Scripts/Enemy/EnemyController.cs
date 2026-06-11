using System;
using System.Text.RegularExpressions;
using Sistemata.Common;
using Sistemata.Core;
using Sistemata.Spawning;
using Sistemata.Stats;
using UnityEngine;

namespace Sistemata.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        int batchId;

        public int BatchID
        {
            get { return batchId; }
            set { batchId = value; }
        }
        
        [Header("Despawn")] public float despawnDistance = 55f;
        
        protected SpriteRenderer SpriteRenderer;
        protected Vector3 MovementDirection;
        public Vector2Int spatialGroup = Vector2Int.zero;

        [Header("Stats")] [SerializeField] protected EnemyBaseData baseData;
        
        protected EntityStats Stats;
        protected EntityHealth Health;

        public float MoveSpeed => Stats.GetStat(StatType.MoveSpeed).Get() ;
        public float BaseMoveSpeed => Stats.GetStat(StatType.MoveSpeed).BaseValue;
        public Vector2 LastMove => new(MovementDirection.x, MovementDirection.z);

        private void Awake()
        {
            Stats = GetComponent<EntityStats>();
            Health = GetComponent<EntityHealth>();
        }

        protected virtual void Start()
        {
            if (SpriteRenderer == null)
                SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            InitializeAllBaseStats();
            ConfigureEntityHealth();
        }

        private void ConfigureEntityHealth()
        {
            Health.OnDeath += HandleDeath;
        }
        
        protected virtual void HandleDeath()
        {
            Health.OnDeath -= HandleDeath;
            Debug.Log($"{gameObject.name} foi derrotado e removido do mapa.");
            
            // TODO: Ativar animação de morte, dropar XP e Moedas de Ouro, Destruir.
            Destroy(gameObject);
        }

        private void InitializeAllBaseStats()
        {
            Stats.InitializeStat(StatType.MaxHealth, baseData.DefaultMaxHealth);
            Stats.InitializeStat(StatType.MoveSpeed, baseData.DefaultMoveSpeed);
            Stats.InitializeStat(StatType.Damage, baseData.DefaultDamage);
            Stats.InitializeStat(StatType.AttackRate, baseData.DefaultAttackRate);
            Stats.InitializeStat(StatType.Armor, baseData.DefaultArmor);
        }

        public virtual void TakeDamage(float damage)
        {
            damage -= Stats.GetStat(StatType.Armor).Get();
            damage = Mathf.Clamp(damage, 0f, damage);
            Health.TakeDamage(damage);
        }

        protected virtual void Update()
        {
            // move em direcao ao jogador
            transform.position += MovementDirection * (Time.deltaTime * MoveSpeed);

            // FLIP DO SPRITE
            if (MovementDirection.x < 0)
            {
                // O inimigo deve olhar para a esquerda
                SpriteRenderer.flipX = true;
            }
            else if (MovementDirection.x > 0)
            {
                // O inimigo deve olhar para a direita
                SpriteRenderer.flipX = false;
            }
        }

        private void OnDestroy()
        {
            // Quando este inimigo for destruído, ele avisa o Spawner para tirá-lo das listas
            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.RemoveFromSpatialGroup(spatialGroup, this);
                EnemySpawner.Instance.UpdateBatchOnUnitDeath("enemy", BatchID);
            }
        }

        public void RunLogic()
        {
            if (GameManager.Instance.player == null)
                return;

            // calcula direcao do movimento
            MovementDirection = GameManager.Instance.player.position - transform.position;
            MovementDirection.y = 0;

            if (MovementDirection.sqrMagnitude > despawnDistance * despawnDistance)
            {
                Destroy(gameObject); // Destrói o inimigo se estiver muito longe do jogador
                return;
            }

            MovementDirection.Normalize();

            // Afasta outros inimigos proximos
            PushNearbyEnemies();

            //Atualiza grupo espacial
            Vector2Int newSpatialGroup =
                EnemySpawner.Instance.GetSpatialGroup(transform.position.x, transform.position.z); // GET spatial group
            if (newSpatialGroup != spatialGroup)
            {
                EnemySpawner.Instance.RemoveFromSpatialGroup(spatialGroup, this);

                spatialGroup = newSpatialGroup; // UPDATE current spatial group
                EnemySpawner.Instance.AddToSpatialGroup(spatialGroup, this); // ADD to new spatial group
            }
        }

        void PushNearbyEnemies()
        {
            Vector3 separationVector = Vector3.zero;
            int pushCount = 0;

            // 1. Usamos o m�todo seguro para n�o dar erro de KeyNotFound e iteramos DIRETO na cole��o, sem .ToList()
            foreach (EnemyController otherEnemy in EnemySpawner.Instance.GetEnemiesInSpatialGroup(spatialGroup))
            {
                if (otherEnemy == null || otherEnemy == this) continue;

                // 2. Corrigido para Eixos X e Z (Dist�ncia de Manhattan super r�pida)
                float distance = Mathf.Abs(transform.position.x - otherEnemy.transform.position.x) +
                                 Mathf.Abs(transform.position.z - otherEnemy.transform.position.z);

                // Se estiver muito perto (ajuste esse 0.2f se eles forem gordinhos)
                if (distance < 0.2f &&
                    distance > 0.001f) // > 0.001f evita divis�o por zero se estiverem no exato mesmo pixel
                {
                    // 3. Calculamos a dire��o para fugir do colega
                    Vector3 pushDir = transform.position - otherEnemy.transform.position;
                    pushDir.y = 0; // Garante que ningu�m vai voar

                    // Acumulamos a for�a de repuls�o
                    separationVector += pushDir.normalized;
                    pushCount++;
                }
            }

            // 4. Se precisamos ser empurrados, ajustamos a dire��o principal do inimigo!
            if (pushCount > 0)
            {
                // Tira a m�dia da repuls�o para n�o ser arremessado longe se tiverem 5 zumbis perto
                separationVector /= pushCount;

                // Misturamos a vontade de "ir at� o player" com a vontade de "afastar dos amigos"
                // Esse 1.5f � a FOR�A do empurr�o. Aumente se quiser que eles espalhem mais r�pido.
                MovementDirection += separationVector * 1.5f;
                MovementDirection.Normalize();
            }
        }
    }
}