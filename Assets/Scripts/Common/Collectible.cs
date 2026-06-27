using System;
using Sistemata.Core;
using UnityEngine;

namespace Sistemata.Common
{
    public enum CollectibleType { Coin, Xp, Magnet, Bomb, Life }

    public class Collectible : MonoBehaviour
    {
        [Header("Configurações")]
        [SerializeField] protected CollectibleType type;
        [SerializeField] protected float value = 1f;
        [SerializeField] private float moveSpeed = 4f; // Reduzido para uma atração mais fraca

        [Header("Animação Flutuante")]
        [SerializeField] private float floatAmplitude = 0.2f;
        [SerializeField] private float floatFrequency = 2f;
        
        private Transform _visualChild;
        private Vector3 _visualBasePosition;
        private Transform _targetPlayer;
        private bool _isBeingAttracted;
        private bool _attractedByMagnet;
        private float _floatTimer;
        
        public UnityEngine.Pool.IObjectPool<Collectible> ManagedPool { get; set; }

        protected virtual void Awake()
        {
            if (transform.childCount > 0)
                _visualChild = transform.GetChild(0);
            
            if (_visualChild != null)
                _visualBasePosition = _visualChild.localPosition;
        }

        private void OnEnable()
        {
            transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            _isBeingAttracted = false;
            _attractedByMagnet = false;
            _targetPlayer = null;
            _floatTimer = UnityEngine.Random.Range(0f, 5f);
            
            if (_visualChild != null)
                _visualChild.localPosition = _visualBasePosition;
        }

        private void Update()
        {
            HandleFloatingAnimation();

            if (!_isBeingAttracted && Player.PlayerManager.Instance && Player.PlayerManager.Instance.IsMagnetActive)
            {
                if (type is CollectibleType.Xp or CollectibleType.Coin)
                {
                    _attractedByMagnet = true;
                    AttractTo(Player.PlayerManager.Instance.transform);
                }
            }

            if (_isBeingAttracted && _targetPlayer)
                MoveTowardsPlayer();
        }

        private void HandleFloatingAnimation()
        {
            if (_visualChild == null) return;

            _floatTimer += Time.deltaTime;
            float newY = _visualBasePosition.y + Mathf.Sin(_floatTimer * floatFrequency) * floatAmplitude;
            _visualChild.localPosition = new Vector3(_visualBasePosition.x, newY, _visualBasePosition.z);
        }

        private void MoveTowardsPlayer()
        {
            if (!_targetPlayer) return;

            var manager = Player.PlayerManager.Instance;

            // Tentamos pegar o centro real do Player (CharacterController)
            var targetCenter = _targetPlayer.position;
            if (manager && manager.PlayerScript)
            {
                targetCenter = manager.PlayerScript.bounds.center;
            }

            var direction = targetCenter - transform.position;
            var distance = direction.magnitude;

            if (manager && !manager.IsMagnetActive)
            {
                var normalRadius = manager.GetStat(Stats.StatType.PickupRadius)?.Get() ?? 2f;
                if (distance > normalRadius + 0.5f)
                {
                    _isBeingAttracted = false;
                    _attractedByMagnet = false;
                    _targetPlayer = null;
                    return;
                }
            }
            
            if (distance < 0.5f)
            {
                Collect(null);
                return;
            }

            // Aceleração/velocidade da atração
            var speedMultiplier = 1f;
            var magnetMinSpeed = 0f;

            if (Player.PlayerManager.Instance && Player.PlayerManager.Instance.IsMagnetActive)
            {
                speedMultiplier = 3.5f; // Aumenta a velocidade
                magnetMinSpeed = 12f;   // Velocidade mínima rápida para longo alcance
            }

            var smoothFactor = Mathf.Clamp(2f / distance, 0.5f, 3f);
            var currentSpeed = Mathf.Max(moveSpeed * smoothFactor * speedMultiplier, magnetMinSpeed);
            
            transform.position += direction.normalized * (currentSpeed * Time.deltaTime);
        }

        public void AttractTo(Transform playerTransform)
        {
            if (_isBeingAttracted) return;
            _targetPlayer = playerTransform;
            _isBeingAttracted = true;
        }

        protected virtual void OnTriggerEnter(Collider collision)
        {
            if (collision.CompareTag("Player"))
                Collect(collision);
        }

        /// <summary>
        /// Define o valor deste coletável (XP ou Ouro). Chamado geralmente ao spawnar via pool.
        /// </summary>
        public void SetValue(float newValue)
        {
            value = newValue;
        }

        protected virtual void Collect(Collider collision)
        {
            if (Player.PlayerManager.Instance)
            {
                switch (type)
                {
                    case CollectibleType.Coin:
                        Player.PlayerManager.Instance.AddGold((int)value);
                        break;
                    case CollectibleType.Xp:
                        Player.PlayerManager.Instance.AddXp(value);
                        break;
                    case CollectibleType.Magnet:
                    case CollectibleType.Bomb:
                    case CollectibleType.Life:
                    default:
                        // Magnet and Bomb are handled in their own subclass override of Collect()
                        break;
                }
            }
            
            if (GameManager.Instance) GameManager.Instance.AddCollectible(type, value);

            if (ManagedPool != null)
            {
                ManagedPool.Release(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
