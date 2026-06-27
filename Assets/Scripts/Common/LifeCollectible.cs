using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sistemata.Enemy;
using Sistemata.Audio;
using Sistemata.Core;
using Sistemata.Player;

namespace Sistemata.Common
{
    public class LifeCollectible : Collectible
    {
        [Header("Configurações da Vida")] 
        [Tooltip("Vida compartilhada entre o player e todos os aliados vivos")]
        [SerializeField] private bool shared;

        protected override void Awake()
        {
            base.Awake();
            type = CollectibleType.Life;
        }
        
        protected override void OnTriggerEnter(Collider collision)
        {
            if (collision.CompareTag("Player") || collision.CompareTag("Ally"))
                Collect(collision);
        }

        protected override void Collect(Collider collision)
        {
            Cure(collision);

            if (GameManager.Instance) 
                GameManager.Instance.AddCollectible(CollectibleType.Life, value);

            if (ManagedPool != null)
                ManagedPool.Release(this);
            else
                Destroy(gameObject);
        }
        
        private void Cure(Collider collision)
        {
            if (!shared)
            {
                if (collision && collision.CompareTag("Player") && PlayerManager.Instance)
                {
                    PlayerManager.Instance.TakeHeal(value);
                } 
                else if (collision && collision.CompareTag("Ally"))
                {
                    var ally = collision.GetComponent<Ally.Ally>();
                    if (ally)
                        ally.TakeHeal(value);
                }
                else
                {
                    if (PlayerManager.Instance)
                        PlayerManager.Instance.TakeHeal(value);
                }

                return;
            }

            var quantity = PlayerManager.Instance ? 1 : 0;
            quantity += Ally.Ally.ActiveAllies.Count;
            
            var amountDivided = value / quantity;
            
            if (PlayerManager.Instance)
                PlayerManager.Instance.TakeHeal(amountDivided);
            foreach (var ally in Ally.Ally.ActiveAllies.Where(ally => ally)) ally.TakeHeal(amountDivided);
        }
    }
}
