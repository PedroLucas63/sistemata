using System;
using System.Linq;
using Sistemata.Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Sistemata.Enemy
{
    public class BoxEnemy : EnemyController
    {
        [Serializable]
        public class CollectibleWeight
        {
            public Collectible collectible;
            public int weight;
        }
        
        [SerializeField] private CollectibleWeight[]  collectibleWeights;
        private int _weightSum;

        protected override void OnAwake()
        {
            _weightSum = collectibleWeights.Sum(x => x.weight);
        }

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
            if (!CollectablePoolManager.Instance) return;
            var value = Random.Range(0, _weightSum);
            foreach (var collectibleWeight in collectibleWeights)    
            {
                if (value < collectibleWeight.weight)
                {
                    CollectablePoolManager.Instance.Spawn(collectibleWeight.collectible, transform.position);
                    return;
                }
                
                value -= collectibleWeight.weight; 
            }
        }
    }
}