using UnityEngine;
using UnityEngine.Serialization;

namespace Sistemata.Global
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Configurations")]
        [SerializeField] private float velocity = 5f;

        [Header("Pursued")] [SerializeField] private GameObject player;
        private Vector3 _offset;
        private Vector3 _position;
        
        private void Start()
        {
            _offset = transform.position - player.transform.position;
            _position = transform.position;
        }

        private void Update()
        {
            _position = Vector3.Lerp(
                _position,
                player.transform.position + _offset,
                velocity * Time.deltaTime
            );
            
            transform.position = _position;
        }
    }
}
