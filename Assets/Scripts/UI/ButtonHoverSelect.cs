using UnityEngine;
using UnityEngine.EventSystems;

namespace Sistemata.UI
{ 
    public class ButtonHoverSelect : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}