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

    // Juice e Animação
    private Vector3 gameplayScale = new Vector3(0.5f, 0.5f, 0.5f);
    private float originalY;
    private bool isHovered = false;

    private float transitionLockTimer = 0.8f;

    public static event Action OnAnyCardUsed;
    private Transform playerTransform;

    public void SetupCard(AllyBaseData data, Transform player)
    {
        cardData = data;
        playerTransform = player;

        visualTransform.localScale = gameplayScale;
        originalY = 0f;

        transitionLockTimer = 0.8f;
    }

    void Update()
    {
        if (specificCooldownTimer > 0)
        {
            specificCooldownTimer -= Time.deltaTime;
        }

        if (transitionLockTimer > 0)
        {
            transitionLockTimer -= Time.deltaTime;
            return;
        }

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        bool hasGlobalCooldown = DeckGameplayManager.Instance.GlobalCooldownTimer > 0;
        bool hasSpecificCooldown = specificCooldownTimer > 0 || isAllyAlive;

        cardImage.color = (hasSpecificCooldown || hasGlobalCooldown) ? Color.gray : Color.white;

        if (isDragging) return;

        float targetY = originalY;
        if (hasSpecificCooldown)
        {
            targetY = originalY - 30f;
        }
        else if (isHovered && !hasGlobalCooldown)
        {
            targetY = originalY + 25f;
        }

        Vector3 targetPos = new Vector3(0, targetY, 0);
        visualTransform.localPosition = Vector3.Lerp(visualTransform.localPosition, targetPos, Time.deltaTime * 15f);
    }

    private bool IsCardReady()
    {
        return specificCooldownTimer <= 0 && DeckGameplayManager.Instance.GlobalCooldownTimer <= 0 && !isAllyAlive;
    }

    private void UseCard(Vector3 spawnPosition)
    {
        if (!IsCardReady()) return;

        GameObject allyObj = Instantiate(cardData.allyPrefab, spawnPosition, Quaternion.Euler(45f, 0f, 0f));
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
            visualTransform.DOScale(gameplayScale * 1.05f, 0.2f).SetEase(Ease.OutQuad);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        visualTransform.DOScale(gameplayScale, 0.2f).SetEase(Ease.OutQuad);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDragging && IsCardReady())
        {
            Vector2 randomOffset = Random.insideUnitCircle * 2f;
            Vector3 spawnPos = playerTransform.position + new Vector3(randomOffset.x, 0f, randomOffset.y);
            UseCard(spawnPos);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsCardReady()) return;

        isDragging = true;
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

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldSpawnPosition = ray.GetPoint(distance);
            worldSpawnPosition.y = 0f;

            UseCard(worldSpawnPosition);
        }
    }
}