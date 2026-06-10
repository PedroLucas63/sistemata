using Sistemata.Stats;
using UnityEngine;

namespace Sistemata.Attack
{
    public class FireballAttack : BaseAttack
    {
        [Header("Configurações da Bola de Fogo")]
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private float fireballSpeed = 8f;

        protected override void ExecuteAttack()
        {
            var amount = Mathf.Max(1, Mathf.FloorToInt(AttackStats.GetStat(StatType.Amount).Get()));
            var damage = Damage;
            var ricochet = AttackStats.GetStat(StatType.Ricochet).Get();
            var size = AttackStats.GetStat(StatType.AreaSize).Get();

            for (var i = 0; i < amount; i++)
            {
                var randomAngle = Random.Range(0f, 360f);
                var spawnDirection = Quaternion.Euler(0, 0, randomAngle) * Vector3.right;

                var projObj = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
                if (projObj.TryGetComponent<Projectile>(out var proj))
                    proj.Setup(spawnDirection, fireballSpeed, damage, ricochet, size);
            }
        }
    }
}