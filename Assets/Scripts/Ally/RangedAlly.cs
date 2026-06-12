using UnityEngine;
using Sistemata.Attack;

namespace Sistemata.Ally
{
    public class RangedAlly : Ally
    {
        [Header("Configurações do Ataque à Distância")]
        [Tooltip("O Prefab de ataque herdado de BaseAttack (ex: ArrowAttack, FireballAttack).")]
        [SerializeField] private BaseAttack attackPrefab;
        
        [Tooltip("O ponto exato de onde os projéteis devem ser gerados (mão, ponta do arco/cajado).")]
        [SerializeField] private Transform firePoint;

        [Header("Configurações de Distanciamento (Kiting)")]
        [Tooltip("Distância mínima que o aliado tenta manter dos monstros. Se o monstro chegar mais perto que isso, o aliado recua.")]
        [SerializeField] private float minKeepDistance = 3.5f;

        private BaseAttack _instantiatedAttack;

        protected override void Start()
        {
            base.Start();
            InitializeAttachedAttack();
        }

        private void InitializeAttachedAttack()
        {
            if (attackPrefab == null)
                return;

            var spawnParent = firePoint != null ? firePoint : transform;
            _instantiatedAttack = Instantiate(attackPrefab, spawnParent.position, spawnParent.rotation, spawnParent);
        }

        protected override void Update()
        {
            base.Update();

            if (!TargetEnemy) return;

            if (AttackTimer <= 0)
            {
                AttackTimer = AttackCooldown;
                AttackVisualTimer = Mathf.Min(0.25f, AttackCooldown * 0.5f);
            }

            var distToEnemy = Vector3.Distance(transform.position, TargetEnemy.transform.position);

            if (distToEnemy < minKeepDistance)
                RecueDoInimigo();
        }

        protected override void ExecuteAttack()
        {
        }

        /// <summary>
        /// Chamado via Animation Event na animação de ataque
        /// </summary>
        public void OnAnimationAttackEvent()
        {
            if (_instantiatedAttack != null)
            {
                Debug.Log("Aliado atacando!");
                _instantiatedAttack.TriggerAttack();
            }
        }

        private void RecueDoInimigo()
        {
            var toEnemy = TargetEnemy.transform.position - transform.position;
            toEnemy.y = 0;
            
            var retreatDirection = -toEnemy.normalized;

            retreatDirection += GetAllyRepulsion();
            retreatDirection.Normalize();

            transform.position += retreatDirection * (MoveSpeed * Time.deltaTime);
            
            MovementDirection = retreatDirection;
        }
    }
}