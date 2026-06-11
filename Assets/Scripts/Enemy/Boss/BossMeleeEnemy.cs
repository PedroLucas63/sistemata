using Sistemata.Core;

namespace Sistemata.Enemy
{
    public class BossMeleeEnemy: MeleeEnemy
    {
        protected override void HandleDeath()
        {
            GameManager.Instance.BossDied();
            base.HandleDeath();
        }
    }
}