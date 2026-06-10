using System.Collections.Generic;
using Sistemata.Stats;

namespace Sistemata.Upgrades
{
    public static class UpgradeRegistry
    {
        private static readonly Dictionary<string, EntityStats> _attackRegistry = new();
        private static readonly Dictionary<string, EntityStats> _allyRegistry = new();

        public static void RegisterAttack(string id, EntityStats stats) => _attackRegistry[id] = stats;
        public static void UnregisterAttack(string id) => _attackRegistry.Remove(id);
        public static EntityStats GetAttack(string id) => _attackRegistry.GetValueOrDefault(id);

        public static void RegisterAlly(string id, EntityStats stats) => _allyRegistry[id] = stats;
        public static void UnregisterAlly(string id) => _allyRegistry.Remove(id);
        public static EntityStats GetAlly(string id) => _allyRegistry.GetValueOrDefault(id);
    }
}