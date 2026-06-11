using UnityEngine;

namespace Sistemata.Ally
{
    public class MeleeAlly : Ally
    {
        [Header("Configurações do Golpe")]
        [Tooltip("Efeito visual opcional para instanciar no momento do impacto (ex: corte/poeira).")]
        [SerializeField] private GameObject hitEffectPrefab;

        protected override void ExecuteAttack()
        {
            if (!TargetEnemy) return;

            TargetEnemy.TakeDamage(Damage);
            if (hitEffectPrefab)
                Instantiate(hitEffectPrefab, TargetEnemy.transform.position, Quaternion.identity);
        }
    }
}