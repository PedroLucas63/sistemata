using UnityEngine;

namespace Sistemata.Enemy
{
    [CreateAssetMenu(fileName = "NewEnemyBaseData", menuName = "Stats/Enemy Base Data")]
    public class EnemyBaseData : ScriptableObject
    {
        [Header("Sobrevivência")]
        [Tooltip("A vida máxima com a qual o inimigo começa a run (nível 0).")]
        public float DefaultMaxHealth = 100f;

        [Header("Movimentação")]
        [Tooltip("Velocidade de movimento base do inimigo.")]
        public float DefaultMoveSpeed = 5f;

        [Header("Combate do Inimigo")]
        [Tooltip("Dano do ataque básico do inimigo .")]
        public float DefaultDamage = 10f;

        [Tooltip("Velocidade do ataque básico do inimigo.")]
        public float DefaultAttackRate = 1f;
        
        [Tooltip("Defesa básica do inimigo.")]
        public float DefaultArmor = 1f;
    }
}