using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sistemata.Upgrades;

namespace UI.LevelUp
{
    public class UpgradeCardUI : MonoBehaviour
    {
        [Header("Referências Visuais")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundQualityImage; 
        [SerializeField] private Button cardButton;

        private UpgradeData _currentData;
        private Action<UpgradeData> _onSelectedCallback;
        
        public void Setup(UpgradeData data, Action<UpgradeData> onSelected)
        {
            _currentData = data;
            _onSelectedCallback = onSelected;

            nameText.text = data.UpgradeName;
            descriptionText.text = data.Description;
            if (data.Icon != null) iconImage.sprite = data.Icon;

            SetQualityColor(data.Quality);

            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(OnCardClicked);
        }

        private void OnCardClicked()
        {
            _onSelectedCallback?.Invoke(_currentData);
        }

        private void SetQualityColor(UpgradeQuality quality)
        {
            backgroundQualityImage.color = quality switch
            {
                UpgradeQuality.Normal => Color.white,
                UpgradeQuality.Uncommon => Color.green,
                UpgradeQuality.Rare => new Color(0, 0.5f, 1f),
                UpgradeQuality.SuperRare => new Color(0.5f, 0, 0.5f),
                UpgradeQuality.Epic => new Color(1f, 0.5f, 0f),
                UpgradeQuality.Legendary => Color.yellow,
                _ => backgroundQualityImage.color
            };
        }
    }
}