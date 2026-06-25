using System;
using Sistemata.Core;
using UnityEngine;

namespace Sistemata.Common
{
    public enum CollectibleType { Coin, XP, Magnet, Bomb }

    public class Collectible : MonoBehaviour
    {
        [Header("Configurações")]
        [SerializeField] protected CollectibleType type;
        [SerializeField] private float value = 1f;
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
                if (type is CollectibleType.XP or CollectibleType.Coin)
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

            // Tentamos pegar o centro real do Player (CharacterController)
            Vector3 targetCenter = _targetPlayer.position;
            if (Sistemata.Player.PlayerManager.Instance && Sistemata.Player.PlayerManager.Instance.PlayerScript != null)
            {
                targetCenter = Sistemata.Player.PlayerManager.Instance.PlayerScript.bounds.center;
            }

            Vector3 direction = targetCenter - transform.position;
            float distance = direction.magnitude;

            // Se o imã global não está ativo e o item está fora do raio de coleta, desativa a atração
            if (Player.PlayerManager.Instance && !Player.PlayerManager.Instance.IsMagnetActive)
            {
                float normalRadius = Player.PlayerManager.Instance.GetStat(Stats.StatType.PickupRadius)?.Get() ?? 2f;
                if (distance > normalRadius + 0.5f)
                {
                    _isBeingAttracted = false;
                    _attractedByMagnet = false;
                    _targetPlayer = null;
                    return;
                }
            }
            
            // Se estiver muito perto do centro, coleta
            if (distance < 0.5f)
            {
                Collect();
                return;
            }

            // Aceleração/velocidade da atração
            float speedMultiplier = 1f;
            float magnetMinSpeed = 0f;

            if (Player.PlayerManager.Instance && Player.PlayerManager.Instance.IsMagnetActive)
            {
                speedMultiplier = 3.5f; // Aumenta a velocidade
                magnetMinSpeed = 12f;   // Velocidade mínima rápida para longo alcance
            }

            float smoothFactor = Mathf.Clamp(2f / distance, 0.5f, 3f);
            float currentSpeed = Mathf.Max(moveSpeed * smoothFactor * speedMultiplier, magnetMinSpeed);
            
            transform.position += direction.normalized * (currentSpeed * Time.deltaTime);
        }

        public void AttractTo(Transform playerTransform)
        {
            if (_isBeingAttracted) return;
            _targetPlayer = playerTransform;
            _isBeingAttracted = true;
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (collision.CompareTag("Player"))
                Collect();
        }

        /// <summary>
        /// Define o valor deste coletável (XP ou Ouro). Chamado geralmente ao spawnar via pool.
        /// </summary>
        public void SetValue(float newValue)
        {
            value = newValue;
        }

        protected virtual void Collect()
        {
            if (Player.PlayerManager.Instance)
            {
                switch (type)
                {
                    case CollectibleType.Coin:
                        Player.PlayerManager.Instance.AddGold((int)value);
                        break;
                    case CollectibleType.XP:
                        Player.PlayerManager.Instance.AddXp(value);
                        break;
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
