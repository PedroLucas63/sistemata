using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sistemata.Stats
{
    [System.Serializable]
    public class Stat
    {
        public float BaseValue;
        private readonly List<StatModifier> _modifiers = new();
        private float _currentValue;
        private bool _dirty = true;

        public Stat(float baseValue)
        {
            this.BaseValue = baseValue;
        }

        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            _dirty = true;
        }

        public void RemoveModifier(StatModifier modifier)
        {
            _modifiers.Remove(modifier);
            _dirty = true;
        }

        public float Get()
        {
            if (_dirty)
                Recalculate();
            return (float) Math.Round(_currentValue, 4);
        }

        private void Recalculate()
        {
            var flatAdd = 0.0f;
            var increasedAdd = 1.0f;
            var moreMultiply = 1.0f;

            foreach (var mod in _modifiers)
            {
                switch (mod.Type)
                {
                    case ModifierType.Flat:
                        flatAdd += mod.Value;
                        break;
                    case ModifierType.Increased:
                        increasedAdd += mod.Value;
                        break;
                    case ModifierType.More:
                        moreMultiply *= (1 + mod.Value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            _currentValue = (BaseValue + flatAdd) * increasedAdd * moreMultiply;
            _dirty = false;
        }
    }
}