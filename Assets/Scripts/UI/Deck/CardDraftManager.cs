using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CardDraftManager : MonoBehaviour
{
    [Header("Configurações de Regra")]
    public float draftIntervalSeconds = 60f;
    public int maxCardsInDeck = 3;
    public int optionsPerDraft = 2;
    public bool triggerDraftAtStart = true;

    [Header("Referências")]
    public GameObject cardPrefab;
    public Transform draftOptionsContainer;
    public CanvasGroup draftPanelCanvasGroup;
    public List<CardData> allAvailableCards;

    [Header("Integração com Gameplay")]
    public Transform gameplayDeckContainer;
    public Transform playerTransform;

    private float timer;
    private int currentCardsInDeck = 0;
    private List<GameObject> spawnedOptionCards = new List<GameObject>();

    void Start()
    {
        timer = draftIntervalSeconds;
        draftPanelCanvasGroup.alpha = 0;
        draftPanelCanvasGroup.gameObject.SetActive(false);

        if (triggerDraftAtStart)
        {
            ShowDraftOptions();
        }
    }

    void Update()
    {
        // O cronômetro só roda se o jogador tiver menos de 3 cartas e o painel de draft não estiver aberto
        if (currentCardsInDeck < maxCardsInDeck && !draftPanelCanvasGroup.gameObject.activeSelf)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                ShowDraftOptions();
                timer = draftIntervalSeconds;
            }
        }
    }

    void ShowDraftOptions()
    {
        Time.timeScale = 0f;

        draftPanelCanvasGroup.gameObject.SetActive(true);
        draftPanelCanvasGroup.DOFade(1f, 0.3f).SetUpdate(true);

        for (int i = 0; i < optionsPerDraft; i++)
        {
            CardData randomCard = allAvailableCards[Random.Range(0, allAvailableCards.Count)];

            GameObject cardObj = Instantiate(cardPrefab, draftOptionsContainer);
            CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
            cardDisplay.Setup(randomCard);

            cardDisplay.OnCardChosen += HandleCardChosen;

            cardObj.transform.localScale = Vector3.zero;
            cardObj.transform.DOScale(1f, 0.4f).SetDelay(i * 0.15f).SetEase(Ease.OutBack).SetUpdate(true);

            spawnedOptionCards.Add(cardObj);
        }
    }

    void HandleCardChosen(CardDisplay chosenCardDisplay)
    {
        Time.timeScale = 1f;
        currentCardsInDeck++;

        draftPanelCanvasGroup.DOFade(0f, 0.3f).OnComplete(() => draftPanelCanvasGroup.gameObject.SetActive(false));

        GameObject chosenCardObj = chosenCardDisplay.gameObject;
        CardData chosenData = chosenCardDisplay.cardData;

        foreach (GameObject cardOption in spawnedOptionCards)
        {
            if (cardOption != chosenCardObj)
            {
                Destroy(cardOption);
            }
        }
        spawnedOptionCards.Clear();

        ConvertAndAnimateToDeck(chosenCardObj, chosenData);
    }

    void ConvertAndAnimateToDeck(GameObject cardObj, CardData data)
    {
        Destroy(cardObj.GetComponent<CardDisplay>());

        GameplayCardUI gameplayLogic = cardObj.AddComponent<GameplayCardUI>();
        gameplayLogic.visualTransform = cardObj.transform.GetChild(0);
        gameplayLogic.cardImage = gameplayLogic.visualTransform.GetComponent<Image>();
        gameplayLogic.SetupCard(data, playerTransform);

        cardObj.transform.SetParent(gameplayDeckContainer);

        RectTransform rect = cardObj.GetComponent<RectTransform>();

        rect.DOScale(1.2f, 0.2f).SetEase(Ease.OutQuad);
        rect.DORotate(new Vector3(0, 0, Random.Range(-15f, 15f)), 0.2f);

        rect.DOAnchorPos(Vector2.zero, 0.5f).SetDelay(0.2f).SetEase(Ease.InBack).OnComplete(() => {
            rect.DORotate(Vector3.zero, 0.2f);
            rect.DOScale(1f, 0.2f);

            gameplayLogic.visualTransform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
        });
    }
}