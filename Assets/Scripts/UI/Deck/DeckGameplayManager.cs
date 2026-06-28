using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;
using Sistemata.Ally;

namespace sistemata.UI.Deck
{
    public class DeckGameplayManager : MonoBehaviour
    {
        public static DeckGameplayManager Instance;

        [Header("Configurações")]
        public float globalCooldownDuration = 3f; // Tempo que todas as cartas bloqueiam ao usar 1

        [Header("Referências")]
        public Transform playerTransform;
        public RectTransform gameplayDeckContainer;
        public CanvasGroup draftPanelCanvasGroup;

        public float GlobalCooldownTimer { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        void OnEnable()
        {
            GameplayCardUI.OnAnyCardUsed += StartGlobalCooldown;
        }

        void OnDisable()
        {
            GameplayCardUI.OnAnyCardUsed -= StartGlobalCooldown;
        }

        void Update()
        {
            if (GlobalCooldownTimer > 0)
            {
                GlobalCooldownTimer -= Time.deltaTime;
            }
        }

        private void StartGlobalCooldown()
        {
            GlobalCooldownTimer = globalCooldownDuration;
        }

        public void TransitionCardsToGameplay(List<GameObject> chosenCardObjects)
        {
            draftPanelCanvasGroup.DOFade(0f, 0.5f);
            draftPanelCanvasGroup.interactable = false;
            draftPanelCanvasGroup.blocksRaycasts = false;

            for (int i = 0; i < chosenCardObjects.Count; i++)
            {
                GameObject cardObj = chosenCardObjects[i];

                AllyBaseData data = cardObj.GetComponent<CardDisplay>().cardData;
                Destroy(cardObj.GetComponent<CardDisplay>());

                GameplayCardUI gameplayLogic = cardObj.AddComponent<GameplayCardUI>();
                gameplayLogic.visualTransform = cardObj.transform.GetChild(0);
                gameplayLogic.cardImage = gameplayLogic.visualTransform.GetComponent<Image>();
                gameplayLogic.SetupCard(data, playerTransform);

                cardObj.transform.SetParent(gameplayDeckContainer);

                float delay = i * 0.15f;

                RectTransform rect = cardObj.GetComponent<RectTransform>();

                rect.DOScale(1.2f, 0.3f).SetDelay(delay).SetEase(Ease.OutQuad);
                rect.DORotate(new Vector3(0, 0, Random.Range(-15f, 15f)), 0.3f).SetDelay(delay);

                rect.DOAnchorPos(Vector2.zero, 0.6f).SetDelay(delay + 0.2f).SetEase(Ease.InBack).OnComplete(() => {
                    rect.DORotate(Vector3.zero, 0.2f);
                    rect.DOScale(1f, 0.2f);
                });
            }
        }
    }
}