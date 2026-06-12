using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Sistemata.Core;

namespace Sistemata.Common
{
    public class CollectablePoolManager : MonoBehaviour
    {
        public static CollectablePoolManager Instance { get; private set; }

        [Header("Configurações de Pool")]
        [SerializeField] private int maxPoolSize = 2000;

        private readonly Dictionary<int, IObjectPool<Collectible>> _pools = new();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public Collectible Spawn(Collectible prefab, Vector3 position)
        {
            if (prefab == null) return null;

            var id = prefab.gameObject.GetInstanceID();
            if (!_pools.TryGetValue(id, out var pool))
            {
                pool = new LinkedPool<Collectible>(
                    createFunc: () => {
                        var instance = Instantiate(prefab, transform);
                        instance.ManagedPool = _pools[id];
                        return instance;
                    },
                    actionOnGet: c => c.gameObject.SetActive(true),
                    actionOnRelease: c => c.gameObject.SetActive(false),
                    actionOnDestroy: c => Destroy(c.gameObject),
                    collectionCheck: false, // Melhor performance desativando checagem
                    maxSize: maxPoolSize
                );
                _pools.Add(id, pool);
            }

            var collectible = pool.Get();
            collectible.transform.position = position;
            return collectible;
        }
    }
}
