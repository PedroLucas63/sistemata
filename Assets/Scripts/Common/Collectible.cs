using System;
using UnityEngine;

namespace Sistemata.Common
{
    public enum CollectibleType { Coin, XP }

    public class Collectible : MonoBehaviour
    {
        [Header("Configurações")]
        [SerializeField] private CollectibleType type;
        [SerializeField] private float value = 1f;
        [SerializeField] private float moveSpeed = 4f; // Reduzido para uma atração mais fraca

        [Header("Animação Flutuante")]
        [SerializeField] private float floatAmplitude = 0.2f;
        [SerializeField] private float floatFrequency = 2f;
        
        private Transform _visualChild;
        private Vector3 _visualBasePosition;
        private Transform _targetPlayer;
        private bool _isBeingAttracted;
        private float _floatTimer;
        
        public UnityEngine.Pool.IObjectPool<Collectible> ManagedPool { get; set; }

        private void Awake()
        {
            if (transform.childCount > 0)
                _visualChild = transform.GetChild(0);
            
            if (_visualChild != null)
                _visualBasePosition = _visualChild.localPosition;
        }

        private void OnEnable()
        {
            _isBeingAttracted = false;
            _targetPlayer = null;
            _floatTimer = UnityEngine.Random.Range(0f, 5f);
            
            if (_visualChild != null)
                _visualChild.localPosition = _visualBasePosition;
        }

        private void Update()
        {
            HandleFloatingAnimation();

            if (_isBeingAttracted && _targetPlayer)
            {
                MoveTowardsPlayer();
            }
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
            if (Sistemata.Player.PlayerManager.Instance != null && Sistemata.Player.PlayerManager.Instance.PlayerScript != null)
            {
                targetCenter = Sistemata.Player.PlayerManager.Instance.PlayerScript.bounds.center;
            }

            Vector3 direction = targetCenter - transform.position;
            float distance = direction.magnitude;
            
            // Se estiver muito perto do centro, coleta
            if (distance < 0.5f)
            {
                Collect();
                return;
            }

            // Suavizamos a atração:
            // Usamos uma velocidade mais baixa que aumenta de forma bem suave conforme chega perto
            float smoothFactor = Mathf.Clamp(2f / distance, 0.5f, 3f);
            float currentSpeed = moveSpeed * smoothFactor;
            
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
            // Se colidir com o Player (o corpo real, não o imã), coleta
            if (collision.CompareTag("Player"))
            {
                Collect();
            }
        }

        /// <summary>
        /// Define o valor deste coletável (XP ou Ouro). Chamado geralmente ao spawnar via pool.
        /// </summary>
        public void SetValue(float newValue)
        {
            value = newValue;
        }

        private void Collect()
        {
            if (Sistemata.Player.PlayerManager.Instance)
            {
                switch (type)
                {
                    case CollectibleType.Coin:
                        Sistemata.Player.PlayerManager.Instance.AddGold((int)value);
                        break;
                    case CollectibleType.XP:
                        Sistemata.Player.PlayerManager.Instance.AddXP(value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

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
