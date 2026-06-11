using UnityEngine;
using Sistemata.Player;
using Sistemata.Stats;

namespace Sistemata.Common
{
    /// <summary>
    /// Script para ser colocado no objeto filho "Magnet" do Player.
    /// Responsável apenas por detectar e atrair coletáveis no raio de ação.
    /// </summary>
    public class CollectibleMagnet : MonoBehaviour
    {
        private SphereCollider _magnetCollider;

        private void Awake()
        {
            _magnetCollider = GetComponent<SphereCollider>();
            if (_magnetCollider != null)
            {
                _magnetCollider.isTrigger = true;
            }
        }

        private void Update()
        {
            UpdateRadius();
        }

        private void UpdateRadius()
        {
            if (_magnetCollider == null || PlayerManager.Instance == null) return;

            // Busca o valor atualizado do stat de PickupRadius
            float radius = PlayerManager.Instance.GetStat(StatType.PickupRadius)?.Get() ?? 2f;
            
            if (!Mathf.Approximately(_magnetCollider.radius, radius))
            {
                _magnetCollider.radius = radius;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Se o que entrou no raio for um coletável, ativa a atração dele
            if (other.CompareTag("Collectible"))
            {
                if (other.TryGetComponent<Collectible>(out var collectible))
                {
                    // Atrai para o transform do PAI (o Player real)
                    collectible.AttractTo(transform.parent != null ? transform.parent : transform);
                }
            }
        }
    }
}
