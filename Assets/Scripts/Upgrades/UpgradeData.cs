using System.Collections.Generic;
using Sistemata.Stats;
using UnityEngine;

namespace Sistemata.Upgrades
{
    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "Upgrade/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
        [Header("Informações Visuais")]
        public string UpgradeName;
        [TextArea] public string Description;
        public Sprite Icon;
        public UpgradeQuality Quality;

        [Header("Destino do Upgrade")] public TargetEntityType UpgradeType;
        
        [Header("Efeito do Upgrade")]
        public StatType TargetStat;
        public ModifierType ModType;
        public float Amount;
        
        [Header("Para quem vai este upgrade?")]
        [Tooltip("Deixe vazio se for para o Player global. Se for Ataque ou Aliado, coloque o ID (ex: 'Fireball' ou 'ArcherAlly')")]
        public string TargetID;
        
        [Header("Regras de Sorteio")]
        
        [Tooltip("Quanto maior o peso, maior a chance de aparecer.")]
        public int Weight = 100; 
        
        [Tooltip("Se for verdadeiro, esse upgrade sai da pool geral assim que for escolhido.")]
        public bool IsUnique = false;
        
        [Tooltip("Tags que o jogador DEVE TER para este upgrade aparecer na tela (ex: 'Has_Fireball')")]
        public List<string> RequiredTags = new();

        [Tooltip("Tags que o jogador GANHA ao pegar este upgrade (ex: desbloquear a 'Has_Fireball')")]
        public List<string> GrantedTags = new();
    }
}