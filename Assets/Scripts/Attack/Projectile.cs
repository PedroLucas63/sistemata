using UnityEngine;
using UnityEngine.Pool;

namespace Sistemata.Attack
{
    public class Projectile : MonoBehaviour
    {
        [Header("2.5D Perspectiva")]
        [SerializeField] private Transform visualChild;
        [SerializeField] private float perspectiveYFactor = 0.6f;
        
        private Vector3 _direction;
        private float _speed;
        private float _damage;
        private float _ricochetsLeft;
        private const string EnemyTag = "Enemy";
        
        public IObjectPool<Projectile> ManagedPool { get; set; }

        public void Setup(Vector3 direction, float speed, float damage, float ricochet, float size)
        {
            _direction = new Vector3(direction.x, 0f, direction.z).normalized;
            _speed = speed;
            _damage = damage;
            _ricochetsLeft = ricochet;
            
            if (!visualChild && transform.childCount > 0)
                visualChild = transform.GetChild(0);
            
            transform.localScale = Vector3.one * size;

            if (!visualChild) return;
            
            visualChild.localScale = new Vector3(size, size * perspectiveYFactor, 1f);
            
            var angle = Mathf.Atan2(_direction.z, _direction.x) * Mathf.Rad2Deg;
            visualChild.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        private void Update()
        {
            transform.position += _direction * (_speed * Time.deltaTime);
        }
        
        private void OnTriggerEnter(Collider collision)
        {
            if (!collision.CompareTag(EnemyTag)) return;
            
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

            if (visualChild == null) return;
            var angle = Mathf.Atan2(_direction.z, _direction.x) * Mathf.Rad2Deg;
            visualChild.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}