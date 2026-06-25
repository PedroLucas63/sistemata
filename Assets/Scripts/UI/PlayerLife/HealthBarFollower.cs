using UnityEngine;

namespace Sistemata.UI.PlayerLife
{
    public class HealthBarFollower : MonoBehaviour
    {
        [Tooltip("A criatura que esta barra deve seguir.")]
        public Transform target; 
    
        [Tooltip("Ajuste para posicionar a barra embaixo (valores negativos em Y) ou em cima da criatura.")]
        public Vector3 worldOffset = new Vector3(0f, -1.5f, 0f); 

        private RectTransform _rectTransform;
        private Camera _mainCamera;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (target)
            {
                var targetPosition = target.position + worldOffset;
                var screenPosition = _mainCamera.WorldToScreenPoint(targetPosition);

                if (screenPosition.z > 0)
                {
                    _rectTransform.position = screenPosition;
                    gameObject.SetActive(true);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                Destroy(gameObject); 
            }
        }
    }
}
