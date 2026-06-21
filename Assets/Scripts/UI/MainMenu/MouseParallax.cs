using UnityEngine;
using UnityEngine.InputSystem;

namespace Sistemata.UI.MainMenu
{
    public class MouseParallax : MonoBehaviour
    {
        [SerializeField] private float forceOfMotion = 15f;
        [SerializeField] private float smoothness = 5f;

        private Vector2 _initialPosition;
        private RectTransform _rectTransform;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _initialPosition = _rectTransform.anchoredPosition;
        }

        private void Update()
        {
            if (Mouse.current == null) return;
            var mousePosition = Mouse.current.position.ReadValue();
            var mouseX = (mousePosition.x / Screen.width) * 2f - 1f;
            var mouseY = (mousePosition.y / Screen.height) * 2f - 1f;

            var targetPosition = new Vector2(mouseX * -forceOfMotion, mouseY * -forceOfMotion);

            _rectTransform.anchoredPosition = Vector2.Lerp(
                _rectTransform.anchoredPosition, 
                _initialPosition + targetPosition, 
                Time.deltaTime * smoothness
            );
        }
    }
}
