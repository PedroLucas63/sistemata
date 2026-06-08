using UnityEngine;

namespace Player
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "NewPlayerBaseData", menuName = "Stats/Player Base Data")]
    public class PlayerBaseData : ScriptableObject
    {
        [Header("Sobrevivência")]
        [Tooltip("A vida máxima com a qual o jogador começa a run (nível 0).")]
        public float DefaultMaxHealth = 100f;
    
        [Tooltip("Regeneração de vida por segundo .")]
        public float DefaultHealthRegen = 1f;

        [Header("Movimentação e Coleta")]
        [Tooltip("Velocidade de movimento base do jogador.")]
        public float DefaultMoveSpeed = 5f;

        [Tooltip("O raio inicial do 'Ímã' para coletar orbes de XP e moedas.")]
        public float DefaultPickupRadius = 2f;

        [Header("Combate do Player")]
        [Tooltip("Dano do ataque básico do player .")]
        public float DefaultDamage = 10f;

        [Tooltip("Velocidade do ataque básico do player.")]
        public float DefaultAttackRate = 1f;
        
        [Tooltip("Defesa básica do player.")]
        public float DefaultArmor = 1f;

        [Header("Mecânicas de Deck / Aliados")]
        [Tooltip("Quantidade máxima de monstros que o player pode ter na arena ao mesmo tempo inicialmente.")]
        public int DefaultSummonCap = 3;
    }
}