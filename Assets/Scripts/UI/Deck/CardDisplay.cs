using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [HideInInspector] public CardData cardData;

    [Header("Referências de UI")]
    public RectTransform visualContent;
    public Image imageComponent;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI descriptionText;

    private Vector3 originalScale;

    public delegate void CardChosenAction(CardDisplay card);
    public event CardChosenAction OnCardChosen;

    void Awake()
    {
        originalScale = visualContent.localScale;
    }

    public void Setup(CardData data)
    {
        cardData = data;
        nameText.text = data.cardName;
        imageComponent.sprite ??= data.cardImage;
        categoryText.text = data.category.ToString();
        descriptionText.text = data.description;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        visualContent.DOKill();
        visualContent.DOScale(originalScale * 1.1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        visualContent.DOKill();
        visualContent.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardChosen?.Invoke(this);
    }
}