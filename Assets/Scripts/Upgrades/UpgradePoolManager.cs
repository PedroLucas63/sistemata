using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Upgrades
{
    public class UpgradePoolManager : MonoBehaviour
    {
        public static UpgradePoolManager Instance { get; private set; }

        [Header("Pool de Upgrades Disponíveis")]
        [SerializeField] private List<UpgradeData> availableUpgradesPool;
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public List<UpgradeData> GetRandomUpgrades(int countToDraw)
        {
            var drawnUpgrades = new List<UpgradeData>();
            var tempPool = new List<UpgradeData>(availableUpgradesPool);

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
            if (chosenUpgrade.IsUnique)
            {
                availableUpgradesPool.Remove(chosenUpgrade);
            }
        }
    }
}