using Stats;
using UnityEngine;

namespace Upgrades
{
    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "Upgrade/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
        [Header("Informações Visuais")]
        public string UpgradeName;
        [TextArea] public string Description;
        public Sprite Icon;
        public UpgradeQuality Quality;

        [Header("Efeito do Upgrade")]
        public StatType TargetStat;
        public ModifierType ModType;
        public float Amount;
        
        [Header("Regras de Sorteio")]
        [Tooltip("Quanto maior o peso, maior a chance de aparecer.")]
        public int Weight = 100; 
        
        [Tooltip("Se for verdadeiro, esse upgrade sai da pool geral assim que for escolhido.")]
        public bool IsUnique = false;
    }
}