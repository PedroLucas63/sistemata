using Sistemata.Enemy;
using Sistemata.Stats;
using UnityEngine;

namespace Sistemata.Attack
{
    public class AuraAttack : BaseAttack
    {
        [Header("Configurações da Aura")]
        [Tooltip("Objeto visual que representa a aura (ex: uma esfera ou círculo).")]
        [SerializeField] private GameObject auraVisual;
        
        [Tooltip("Layer que identifica os inimigos.")]
        [SerializeField] private LayerMask enemyLayer;

        protected override void Start()
        {
            base.Start();
            
            // Se o visual não foi atribuído, tenta buscar nos filhos
            if (auraVisual == null)
            {
                auraVisual = transform.Find("AuraVisual")?.gameObject;
                if (auraVisual == null) auraVisual = transform.GetChild(0).gameObject;
            }
            
            UpdateVisualScale();
        }

        private void Update()
        {
            UpdateVisualScale();
        }

        private void UpdateVisualScale()
        {
            if (!auraVisual) return;
            
            var areaSize = AttackStats.GetStat(StatType.AreaSize).Get();
            var finalScale = areaSize * 2f;
            auraVisual.transform.localScale = new Vector3(finalScale, finalScale, finalScale);
        }

        protected override void ExecuteAttack()
        {
            var radius = AttackStats.GetStat(StatType.AreaSize).Get();
            var damage = Damage;

            var hits = Physics.OverlapSphere(transform.position, radius, enemyLayer);

            foreach (var hit in hits)
            {
                var enemy = hit.GetComponentInParent<EnemyController>();
                if (enemy)
                    enemy.TakeDamage(damage);
            }
        }
    }
}
