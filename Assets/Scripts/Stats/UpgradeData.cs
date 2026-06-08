using UnityEngine;

namespace Stats
{
    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "Stats/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
        [Header("Informações Visuais")]
        public string UpgradeName;
        [TextArea] public string Description;
        public Sprite Icon;

        [Header("Efeito do Upgrade")]
        public StatType TargetStat;

        public ModifierType ModType;
        public float Amount;
    }
}