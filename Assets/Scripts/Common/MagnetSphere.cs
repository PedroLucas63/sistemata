using UnityEngine;
using Sistemata.Player;
using Sistemata.Stats;

namespace Sistemata.Common
{
    /// <summary>
    /// Script para ser colocado no objeto filho "Magnet" do Player.
    /// Responsável apenas por detectar e atrair coletáveis no raio de ação.
    /// </summary>
    public class MagnetSphere : MonoBehaviour
    {
        private SphereCollider _magnetCollider;

        private void Awake()
        {
            _magnetCollider = GetComponent<SphereCollider>();
            if (_magnetCollider != null)
                _magnetCollider.isTrigger = true;
        }

        private void Update()
        {
            UpdateRadius();
        }

        private void UpdateRadius()
        {
            if (!_magnetCollider || !PlayerManager.Instance) return;

            var radius = PlayerManager.Instance.GetStat(StatType.PickupRadius)?.Get() ?? 2f;
            
            if (!Mathf.Approximately(_magnetCollider.radius, radius))
                _magnetCollider.radius = radius;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Collectible")) return;
            if (other.TryGetComponent<Collectible>(out var collectible))
                collectible.AttractTo(transform.parent != null ? transform.parent : transform);
        }
    }
}
