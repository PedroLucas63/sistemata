using UnityEngine;
using Sistemata.Player;
using Sistemata.Audio;
using Sistemata.Core;

namespace Sistemata.Common
{
    public class MagnetCollectible : Collectible
    {
        [Header("Configurações do Ímã")]
        [SerializeField] private float magnetDuration = 5f;
        [SerializeField] private AudioClip magnetSfx;

        protected override void Awake()
        {
            base.Awake();
            type = CollectibleType.Magnet;
        }

        protected override void Collect()
        {
            if (PlayerManager.Instance != null)
                PlayerManager.Instance.ActivateMagnet(magnetDuration);

            if (magnetSfx != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX2D(magnetSfx);

            if (GameManager.Instance != null) 
                GameManager.Instance.AddCollectible(CollectibleType.Magnet, 1f);

            if (ManagedPool != null)
                ManagedPool.Release(this);
            else
                Destroy(gameObject);
        }
    }
}
