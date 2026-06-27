using DG.Tweening;
using Sistemata.Common;
using Sistemata.Ally;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameplayCardUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Referências")]
    public Transform visualTransform;
    public Image cardImage;

    [HideInInspector] public AllyBaseData cardData;

    // Variáveis de Estado
    private bool isAllyAlive = false;
    private float specificCooldownTimer = 0f;
    private bool isDragging = false;
    private Vector2 originalPosition;

    //Juice
    private Vector3 gameplayScale = new Vector3(0.5f, 0.5f, 0.5f); //Cartas ficam 30% menores no deck de jogo
    private float originalY;
    private bool isHovered = false;

    public static event Action OnAnyCardUsed;
    private Transform playerTransform;

    public void SetupCard(AllyBaseData data, Transform player)
    {
        cardData = data;
        playerTransform = player;

        visualTransform.localScale = gameplayScale;
        originalY = visualTransform.localPosition.y;
    }

    void Update()
    {
        if (specificCooldownTimer > 0)
        {
            specificCooldownTimer -= Time.deltaTime;
        }

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (isDragging) return;

        bool hasGlobalCooldown = DeckGameplayManager.Instance.GlobalCooldownTimer > 0;
        bool hasSpecificCooldown = specificCooldownTimer > 0 || isAllyAlive;

        cardImage.color = (hasSpecificCooldown || hasGlobalCooldown) ? Color.gray : Color.white;

        if (hasSpecificCooldown)
        {
            visualTransform.DOLocalMoveY(originalY - 30f, 0.2f).SetEase(Ease.OutQuad);
        }
        else if (isHovered && !hasGlobalCooldown)
        {
            visualTransform.DOLocalMoveY(originalY + 25f, 0.2f).SetEase(Ease.OutQuad); 
        }
        else
        {
            visualTransform.DOLocalMoveY(originalY, 0.2f).SetEase(Ease.OutQuad);
        }
    }

    private bool IsCardReady()
    {
        return specificCooldownTimer <= 0 && DeckGameplayManager.Instance.GlobalCooldownTimer <= 0 && !isAllyAlive;
    }

    private void UseCard(Vector3 spawnPosition)
    {
        if (!IsCardReady()) return;

        GameObject allyObj = Instantiate(cardData.allyPrefab, spawnPosition, Quaternion.identity);
        EntityHealth ally = allyObj.GetComponent<EntityHealth>();

        isAllyAlive = true;
        ally.OnDeath += HandleAllyDeath;

        OnAnyCardUsed?.Invoke();

        visualTransform.DOPunchScale(gameplayScale * 0.2f, 0.3f);
        isHovered = false;
    }

    private void HandleAllyDeath()
    {
        isAllyAlive = false;
        specificCooldownTimer = cardData.specificCooldown;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsCardReady())
        {
            isHovered = true;
            visualTransform.DOKill();
            visualTransform.DOScale(gameplayScale * 1.05f, 0.2f).SetEase(Ease.OutQuad);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        visualTransform.DOKill();
        visualTransform.DOScale(gameplayScale, 0.2f).SetEase(Ease.OutQuad);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDragging && IsCardReady())
        {
            Vector2 randomOffset = Random.insideUnitCircle * 2f;
            Vector3 spawnPos = playerTransform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            UseCard(spawnPos);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsCardReady()) return;

        isDragging = true;
        originalPosition = visualTransform.position;

        visualTransform.DOScale(gameplayScale * 0.5f, 0.2f).SetEase(Ease.OutBack);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        visualTransform.position = Mouse.current.position.ReadValue();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        visualTransform.DOScale(gameplayScale, 0.3f).SetEase(Ease.OutBack);
        visualTransform.DOMove(originalPosition, 0.3f).SetEase(Ease.OutBack);

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldSpawnPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
        worldSpawnPosition.z = 0;

        UseCard(worldSpawnPosition);
    }
}