using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Sistemata.Core;

namespace Sistemata.Attack
{
    public class ProjectilePoolManager : MonoBehaviour
    {
        public static ProjectilePoolManager Instance { get; private set; }
        private readonly Dictionary<int, IObjectPool<Projectile>> _projectilePools = new();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Obtém um projétil ativo da pool correspondente ao prefab fornecido.
        /// </summary>
        public Projectile GetProjectile(Projectile prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            var prefabInstanceId = prefab.gameObject.GetInstanceID();

            if (!_projectilePools.TryGetValue(prefabInstanceId, out var pool))
            {
                pool = new LinkedPool<Projectile>(
                    createFunc: () => {
                        var parent = GameManager.Instance != null ? GameManager.Instance.ProjectileParent : null;
                        var instance = Instantiate(prefab, parent);
                        instance.ManagedPool = _projectilePools[prefabInstanceId];
                        return instance;
                    },
                    actionOnGet: proj => proj.gameObject.SetActive(true),
                    actionOnRelease: proj => proj.gameObject.SetActive(false),
                    actionOnDestroy: proj => Destroy(proj.gameObject),
                    collectionCheck: true,
                    maxSize: 500
                );

                _projectilePools.Add(prefabInstanceId, pool);
            }

            var activeProj = pool.Get();
            activeProj.transform.SetPositionAndRotation(position, rotation);

            return activeProj;
        }
    }
}