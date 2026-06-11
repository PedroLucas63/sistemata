using Sistemata.Common;
using Sistemata.Enemy;
using Sistemata.Player;
using UnityEngine;
using UnityEngine.Pool;

namespace Sistemata.Attack
{
    public class Projectile : MonoBehaviour
    {
        [Header("2.5D Perspectiva")]
        [Tooltip("Arraste aqui o objeto filho que contém o SpriteRenderer.")]
        [SerializeField] private Transform visualChild;
        [Tooltip("Fator de achatamento visual para casar com a perspectiva do cenário.")]
        [SerializeField] private float perspectiveYFactor = 0.6f;
        
        [Header("Limites de Desativação (Performance)")]
        [Tooltip("Tempo máximo em segundos que o projétil pode existir.")]
        [SerializeField] private float maxLifetime = 3f;
        [Tooltip("Distância máxima do Player antes de ser reciclado.")]
        [SerializeField] private float maxRangeFromPlayer = 20f;
        
        [Header("Materiais de Contorno")]
        [SerializeField] private Material allyOutlineMaterial;
        [SerializeField] private Material enemyOutlineMaterial;

        private Vector3 _direction;
        private float _speed;
        private float _damage;
        private float _ricochetsLeft;
        private float _currentLifetime;
        private SpriteRenderer _spriteRenderer;
        
        private string _targetTag = "Enemy"; 
        
        public IObjectPool<Projectile> ManagedPool { get; set; }

        /// <summary>
        /// Configura o projétil injetando comportamento, física e a tag alvo que ele deve colidir.
        /// </summary>
        public void Setup(Vector3 direction, float speed, float damage, float ricochet, float size, string targetTag = "Enemy")
        {
            _direction = new Vector3(direction.x, 0f, direction.z).normalized;
            _speed = speed;
            _damage = damage;
            _ricochetsLeft = ricochet;
            _currentLifetime = 0f;
            _targetTag = targetTag;
            
            transform.localScale = Vector3.one * size;
            
            // Busca robusta pelo SpriteRenderer
            if (_spriteRenderer == null)
            {
                if (visualChild != null) _spriteRenderer = visualChild.GetComponent<SpriteRenderer>();
                if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (visualChild)
            {
                visualChild.localScale = new Vector3(1f, 1f * perspectiveYFactor, 1f);
            }

            if (_spriteRenderer != null)
            {
                ApplyOutlineMaterial(targetTag);
            }
            
            UpdateDirectionalBehavior();
        }

        private void ApplyOutlineMaterial(string target)
        {
            // Se o alvo for "Enemy", o projétil é do Jogador/Aliado -> Branco
            if (target == "Enemy")
            {
                if (allyOutlineMaterial != null)
                {
                    _spriteRenderer.material = allyOutlineMaterial;
                }
            }
            // Se o alvo for "Player", o projétil é do Inimigo -> Vermelho
            else
            {
                if (enemyOutlineMaterial != null)
                {
                    _spriteRenderer.material = enemyOutlineMaterial;
                }
            }
        }
        
        private void Update()
        {
            transform.position += _direction * (_speed * Time.deltaTime);

            if (CheckLifetime())
                CheckDistanceToPlayer();
        }

        private void UpdateDirectionalBehavior()
        {
            if (_direction == Vector3.zero) return;

            // Calcula o ângulo em graus baseado na direção
            // Atan2 retorna de -180 a 180. Multiplicamos por Rad2Deg para virar graus.
            float angle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg;

            // Normaliza o ângulo para ser sempre positivo (0 a 360) para facilitar a nossa lógica
            if (angle < 0) angle += 360f;

            // --- FATIAMOS O CÍRCULO EM 8 PEDAÇOS (de 45 em 45 graus) ---

            if (angle >= 337.5f || angle < 22.5f)
            {
                // N (Norte) -> A fatia do Norte cruza a linha do 360/0
                transform.rotation = Quaternion.Euler(-135, 0, 180); // Seu valor original
            }
            else if (angle >= 22.5f && angle < 67.5f)
            {
                // NE (Nordeste)
                transform.rotation = Quaternion.Euler(45, 0, -45);
            }
            else if (angle >= 67.5f && angle < 112.5f)
            {
                // E (Leste)
                transform.rotation = Quaternion.Euler(45, 0, -90); // Seu valor original
            }
            else if (angle >= 112.5f && angle < 157.5f)
            {
                // SE (Sudeste)
                transform.rotation = Quaternion.Euler(45, 0, -135);
            }
            else if (angle >= 157.5f && angle < 202.5f)
            {
                // S (Sul)
                transform.rotation = Quaternion.Euler(45, 0, 180); // Seu valor original
            }
            else if (angle >= 202.5f && angle < 247.5f)
            {
                // SW (Sudoeste)
                transform.rotation = Quaternion.Euler(45, 0, 135);
            }
            else if (angle >= 247.5f && angle < 292.5f)
            {
                // W (Oeste)
                transform.rotation = Quaternion.Euler(45, 0, 90); // Seu valor original
            }
            else if (angle >= 292.5f && angle < 337.5f)
            {
                // NW (Noroeste)
                transform.rotation = Quaternion.Euler(45, 0, 45);
            }
        }

        private bool CheckLifetime()
        {
            _currentLifetime += Time.deltaTime;
            if (!(_currentLifetime >= maxLifetime)) return true;
            
            ReleaseProjectile();
            return false;
        }

        private void CheckDistanceToPlayer()
        {
            if (!PlayerManager.Instance) return;
            
            var distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);
            if (!(distance > maxRangeFromPlayer)) return;
                
            ReleaseProjectile();
        }
        
        private void OnTriggerEnter(Collider collision)
        {
            if (!collision.CompareTag(_targetTag)) return;
            
            if (_targetTag == "Enemy")
            {
                var enemy = collision.GetComponentInParent<EnemyController>();
                if (enemy) enemy.TakeDamage(_damage);
            }
            else 
            {
                var health = collision.GetComponentInParent<EntityHealth>();
                if (health) health.TakeDamage(_damage);
            }
            
            if (_ricochetsLeft > 0)
            {
                _ricochetsLeft--;
                DefineNewDirection();
            }
            else
            {
                ReleaseProjectile();
            }
        }
        
        public void ReleaseProjectile()
        {
            if (gameObject.activeSelf)
            {
                ManagedPool?.Release(this);
            }
        }

        private void DefineNewDirection()
        {
            var oppositeDirection = -_direction;
            var lateralAngle = Random.Range(25f, 45f);
    
            if (Random.value > 0.5f) 
                lateralAngle = -lateralAngle;

            _direction = Quaternion.Euler(0f, lateralAngle, 0f) * oppositeDirection;
            UpdateDirectionalBehavior();
        }
    }
}