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
        
        private Vector3 _direction;
        private float _speed;
        private float _damage;
        private float _ricochetsLeft;
        private float _currentLifetime;
        private const string EnemyTag = "Enemy";
        
        public IObjectPool<Projectile> ManagedPool { get; set; }

        public void Setup(Vector3 direction, float speed, float damage, float ricochet, float size)
        {
            _direction = new Vector3(direction.x, 0f, direction.z).normalized;
            _speed = speed;
            _damage = damage;
            _ricochetsLeft = ricochet;
            _currentLifetime = 0f;
            
            transform.localScale = Vector3.one * size;
            
            if (!visualChild && transform.childCount > 0)
                visualChild = transform.GetChild(0);

            if (visualChild)
            {
                visualChild.localRotation = Quaternion.Euler(90f, 0f, 0f);
                visualChild.localScale = new Vector3(1f, 1f * perspectiveYFactor, 1f);
            }
            
            UpdateDirectionalBehavior();
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
            var angle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
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
            if (!collision.CompareTag(EnemyTag)) return;
            
            // TODO: CHAMAR AÇÃO DE DANO NO INIMIGO
            var enemy = collision.GetComponent<EnemyController>();
            if (!enemy) return;
            
            enemy.TakeDamage(_damage);
            
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