using System.Collections.Generic;
using UnityEngine;

namespace Sistemata.Stats
{ 
    public class EntityStats : MonoBehaviour
    {
        private readonly Dictionary<StatType, Stat> _stats = new();

        public void InitializeStat(StatType type, float baseValue)
        {
            if(!_stats.TryGetValue(type, out var stat))
                _stats[type] = new Stat(baseValue);
            else
                stat.BaseValue =  baseValue;
        }

        public Stat GetStat(StatType type)
        {
            return _stats.GetValueOrDefault(type);
        }

        public void ApplyUpgrade(StatType type, StatModifier modifier)
        {
            if (_stats.TryGetValue(type, out var stat))
                stat.AddModifier(modifier);
        }
    }
}