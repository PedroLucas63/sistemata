using System;
using System.Collections.Generic;
using System.Linq;
using Sistemata.Player;
using Sistemata.Stats;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Sistemata.Upgrades
{
    public class UpgradePoolManager : MonoBehaviour
    {
        public static UpgradePoolManager Instance { get; private set; }

        [Header("Pool de Upgrades Disponíveis")]
        [SerializeField] private List<UpgradeData> availableUpgradesPool;
        
        private readonly HashSet<string> _unlockedTags = new();
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AddUnlockedTag(string tag)
        {
            _unlockedTags.Add(tag);
        }

        public List<UpgradeData> GetRandomUpgrades(int countToDraw)
        {
            var filteredPool = availableUpgradesPool.Where(u =>
                u.RequiredTags.TrueForAll(PlayerHasTag)
            ).ToList();
            
            var drawnUpgrades = new List<UpgradeData>();
            var tempPool = new List<UpgradeData>(filteredPool);

            for (var i = 0; i < countToDraw; i++)
            {
                if (tempPool.Count == 0) break;

                var selected = DrawSingleUpgrade(tempPool);
                drawnUpgrades.Add(selected);
                tempPool.Remove(selected);
            }

            return drawnUpgrades;
        }
        
        private static UpgradeData DrawSingleUpgrade(List<UpgradeData> poolToDrawFrom)
        {
            var totalWeight = poolToDrawFrom.Sum(upgrade => upgrade.Weight);
            var randomValue = Random.Range(0, totalWeight);
            var accumulatedWeight = 0;

            foreach (var upgrade in poolToDrawFrom)
            {
                accumulatedWeight += upgrade.Weight;
                if (randomValue < accumulatedWeight)
                {
                    return upgrade;
                }
            }

            return poolToDrawFrom[0]; 
        }
        
        public void OnUpgradeChosen(UpgradeData chosenUpgrade)
        {

            if (chosenUpgrade.GrantedTags != null)
            {
                foreach (var newTag in chosenUpgrade.GrantedTags)
                {
                    _unlockedTags.Add(newTag);
                }
            }
            
            if (chosenUpgrade.IsUnique)
            {
                availableUpgradesPool.Remove(chosenUpgrade);
            }
            
            ApplyUpgradeToTarget(chosenUpgrade);
        }

        private static void ApplyUpgradeToTarget(UpgradeData upgrade)
        {
            var modifier = new StatModifier
            {
                Type = upgrade.ModType,
                Value = upgrade.Amount,
                Source = upgrade
            };
            
            switch (upgrade.UpgradeType)
            {
                case TargetEntityType.Player:
                    PlayerManager.Instance.ApplyRunUpgrade(upgrade);
                    break;
                case TargetEntityType.Attack:
                    var attackStats = UpgradeRegistry.GetAttack(upgrade.TargetID);
                    if (attackStats)
                        attackStats.ApplyUpgrade(upgrade.TargetStat, modifier);
                    break;

                case TargetEntityType.Ally:
                    var allyStats = UpgradeRegistry.GetAlly(upgrade.TargetID);
                    if (allyStats)
                        allyStats.ApplyUpgrade(upgrade.TargetStat, modifier);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool PlayerHasTag(string tag) => _unlockedTags.Contains(tag);
    }
}