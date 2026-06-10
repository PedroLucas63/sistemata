using UnityEngine;

namespace Sistemata.Attack
{
    [CreateAssetMenu(fileName = "NewAttackBaseData", menuName = "Stats/Attack Base Data")]
    public class AttackBaseData : ScriptableObject
    {
        [Header("Combate Base")]
        [Tooltip("Dano causado por cada acerto/projétil deste ataque no nível base.")]
        public float DefaultDamage = 10f;

        [Tooltip("Frequência de ataques por segundo. (Ex: 2f significa dois ataques por segundo).")]
        public float DefaultAttackRate = 1f;

        [Header("Mecânicas de Projétil / Escala")]
        [Tooltip("Quantidade de projéteis por execução.")]
        public float DefaultAmount = 1f;

        [Tooltip("Número de vezes que o projétil pode ricochetear nos inimigos antes de sumir.")]
        public float DefaultRicochet = 0f;

        [Header("Área de Efeito")]
        [Tooltip("O raio da aura, tamanho do projétil ou modificador de escala física do ataque.")]
        public float DefaultAreaSize = 1f;
    }
}