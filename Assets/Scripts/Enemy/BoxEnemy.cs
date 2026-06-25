using System;
using System.Linq;
using Sistemata.Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Sistemata.Enemy
{
    public class BoxEnemy : EnemyController
    {
        protected override void UpdateCombatBehavior(float distanceToTarget)
        {
            // Box is not a combatant.
        }
        
        protected override void Update()
        {
            // Clear Update
        }
        
        public override void RunLogic()
        {
            // Clear Run Logic
        }
        
        protected override void PushNearbyEnemies()
        {
            // Clear Run Logic
        }
        
        protected override void SpawnLoot()
        {
            if (!CollectablePoolManager.Instance || lootTable == null || lootTable.Count == 0) return;

            var totalWeight = lootTable.Where(loot => loot.prefab).Sum(loot => loot.dropChance);
            if (totalWeight <= 0f) return;

            var randomValue = Random.Range(0f, totalWeight);
            var currentWeightSum = 0f;

            foreach (var loot in lootTable.Where(loot => loot.prefab))
            {
                currentWeightSum += loot.dropChance;
                if (randomValue > currentWeightSum) continue;
                
                var instance = CollectablePoolManager.Instance.Spawn(loot.prefab, transform.position);
                if (!instance) return;
                
                var randomVal = Random.Range(loot.minValue, loot.maxValue);
                instance.SetValue(randomVal);
                return;
            }
        }
    }
}