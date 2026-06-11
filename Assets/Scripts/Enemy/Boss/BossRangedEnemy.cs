using Sistemata.Core;

namespace Sistemata.Enemy
{
    public class BossRangedEnemy : MeleeEnemy
    {
        protected override void HandleDeath()
        {
            GameManager.Instance.BossDied();
            base.HandleDeath();
        }
    }
}