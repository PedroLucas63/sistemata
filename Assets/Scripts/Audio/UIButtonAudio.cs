using UnityEngine;
using UnityEngine.EventSystems;
using Sistemata.Audio; // Para conversar com o seu AudioManager

namespace Sistemata.UI
{
    // Exige os componentes de Interface para funcionar perfeitamente
    public class UIButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [Header("Sons do Botão")]
        [Tooltip("Som ao passar o mouse por cima")]
        [SerializeField] private AudioClip hoverSound;

        [Tooltip("Som ao clicar no botão")]
        [SerializeField] private AudioClip clickSound;

        [Header("Configurações")]
        [Range(0f, 1f)][SerializeField] private float volume = 0.8f;
        [Tooltip("Se marcado, tocará os sons mesmo se o botão estiver desativado/inacessível")]
        [SerializeField] private bool ignoreButtonState = false;

        private UnityEngine.UI.Button _button;

        private void Awake()
        {
            // Tenta achar um componente Button neste objeto
            _button = GetComponent<UnityEngine.UI.Button>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Se o botão existir e não for interativo (e não estivermos ignorando isso), não toca som
            if (!ignoreButtonState && _button != null && !_button.interactable)
            {
                Debug.Log("Button is not interactable, ignoring hover sound.");
                return;
            }

            if (hoverSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUISFX(hoverSound, volume);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!ignoreButtonState && _button != null && !_button.interactable) return;

            if (clickSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUISFX(clickSound, volume);
            }
        }
    }
}