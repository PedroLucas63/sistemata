using UnityEngine;

namespace Sistemata.Ally
{
    [CreateAssetMenu(fileName = "NewAllyBaseData", menuName = "Stats/Ally Base Data")]
    public class AllyBaseData : ScriptableObject
    {
        [Header("Sobrevivência")]
        [Tooltip("A vida máxima com a qual o aliado começa a run (nível 0).")]
        public float DefaultMaxHealth = 300f;

        [Header("Movimentação")]
        [Tooltip("Velocidade de movimento base do aliado.")]
        public float DefaultMoveSpeed = 5f;

        [Header("Combate do Aliado")]
        [Tooltip("Dano do ataque básico do aliado .")]
        public float DefaultDamage = 10f;

        [Tooltip("Velocidade do ataque básico do aliados.")]
        public float DefaultAttackRate = 1f;
        
        [Tooltip("Defesa básica do aliado.")]
        public float DefaultArmor = 1f;
    }
}