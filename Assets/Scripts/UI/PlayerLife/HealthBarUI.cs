using UnityEngine;
using UnityEngine.UI;

namespace Sistemata.UI.PlayerLife
{
    public class HealthBarUI : MonoBehaviour
    {
        private Slider _slider;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            _slider.transform.SetAsFirstSibling();
        }
        
        public void UpdatePercentage(float percentage)
        {
            if (_slider)
                _slider.value = percentage;
        }
    }
}