using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using Sistemata.Ally;
using Sistemata.Audio;

namespace sistemata.UI.Deck
{
    public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [HideInInspector] public AllyBaseData cardData;

        [Header("Referências de UI")]
        [SerializeField] private RectTransform visualContent;
        [SerializeField] private Image imageComponent;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI AttackText;
        [SerializeField] private TextMeshProUGUI lifeText;
        [SerializeField] private Image allyicon;

        [Header("Áudio da Carta")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip selectSound;
        [Range(0f, 1f)][SerializeField] private float cardVolume = 0.8f;

        private Vector3 originalScale;

        public delegate void CardChosenAction(CardDisplay card);
        public event CardChosenAction OnCardChosen;

        void Awake()
        {
            originalScale = visualContent.localScale;
        }

        public void Setup(AllyBaseData data)
        {
            cardData = data;
            nameText.text = data.cardName;
            imageComponent.sprite ??= data.cardImage;
            AttackText.text = data.DefaultDamage.ToString();
            lifeText.text = data.DefaultMaxHealth.ToString();
            allyicon.sprite = data.cardImage;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            visualContent.DOKill();
            visualContent.DOScale(originalScale * 1.1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);

            if (selectSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUISFX(hoverSound, cardVolume);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            visualContent.DOKill();
            visualContent.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (selectSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUISFX(selectSound, cardVolume);
            }
            OnCardChosen?.Invoke(this);
        }
    }
}