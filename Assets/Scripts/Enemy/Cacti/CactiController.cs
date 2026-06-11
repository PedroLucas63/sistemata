using UnityEngine;

namespace Sistemata.Enemy.Cacti
{
    public class CactiController : EnemyController
    {
        public bool IsAttacking = false;

        protected override void Update()
        {
            transform.position += MovementDirection * (Time.deltaTime * MoveSpeed);
        }
    }
}