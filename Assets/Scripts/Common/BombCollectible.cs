using System.Collections.Generic;
using UnityEngine;
using Sistemata.Enemy;
using Sistemata.Audio;
using Sistemata.Core;

namespace Sistemata.Common
{
    public class BombCollectible : Collectible
    {
        [Header("Configurações da Bomba")]
        [SerializeField] private float explosionRadius = 6f;
        [SerializeField] private float explosionDamage = 30f;
        [SerializeField] private GameObject explosionEffectPrefab;
        [SerializeField] private AudioClip explosionSfx;

        protected override void Awake()
        {
            base.Awake();
            type = CollectibleType.Bomb;
        }

        protected override void Collect(Collider collision)
        {
            Explode();

            if (explosionSfx && AudioManager.Instance)
                AudioManager.Instance.PlaySFX2D(explosionSfx);

            if (GameManager.Instance) 
                GameManager.Instance.AddCollectible(CollectibleType.Bomb, 1f);

            if (ManagedPool != null)
                ManagedPool.Release(this);
            else
                Destroy(gameObject);
        }

        private void Explode()
        {
            // Instanciar efeito visual se houver
            if (explosionEffectPrefab)
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

            // Encontrar inimigos no raio e causar dano
            var colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            var damagedEnemies = new HashSet<EnemyController>();

            foreach (var col in colliders)
            {
                if (!col.CompareTag("Enemy")) continue;
                var enemy = col.GetComponentInParent<EnemyController>();
                if (enemy && damagedEnemies.Add(enemy))
                {
                    enemy.TakeDamage(explosionDamage);
                }
            }
        }
    }
}
