using System.Collections;
using System.Collections.Generic;
using Sistemata.Player;
using UnityEngine;
using UnityEngine.UI;
using Sistemata.Upgrades;

namespace Sistemata.UI.LevelUp
{
    public class LevelUpUIManager : MonoBehaviour
    {
        public static LevelUpUIManager Instance { get; private set; }
        
        [Header("UI Elements")]
        [SerializeField] private GameObject levelUpPanel;
        [SerializeField] private UpgradeCardUI[] cardsUI;
        [SerializeField] private Slider timerBar;

        [Header("Settings")]
        [SerializeField] private float timeLimit = 10f;
        [SerializeField] private float unpauseDelay = 0.2f;

        private float _currentTime;
        private bool _isChoosing;
        private List<UpgradeData> _currentOptions;
        private int _pendingLevelUps = 0;
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        
        public void QueueLevelUp()
        {
            _pendingLevelUps++;
            
            if (!levelUpPanel.activeSelf && !PlayerManager.Instance.IsDead)
            {
                TriggerLevelUp();
            }
        }

        private void Start()
        {
            levelUpPanel.SetActive(false);
        }
        
        private void Update()
        {
            if (!_isChoosing) return;
            _currentTime -= Time.unscaledDeltaTime;

            if (timerBar)
                timerBar.value = _currentTime / timeLimit;

            if (_currentTime <= 0)
                AutoSelectRandom();
        }

        private void TriggerLevelUp()
        {
            _pendingLevelUps--;
            _currentOptions = UpgradePoolManager.Instance.GetRandomUpgrades(3);
            
            if (_currentOptions == null || _currentOptions.Count == 0)
            {
                Debug.LogWarning("Nenhum upgrade disponível na Pool!");
                StartCoroutine(UnpauseRoutine());
                return;
            }

            // Ativa o painel antes de configurar os cards
            levelUpPanel.SetActive(true);
            Time.timeScale = 0f;
            
            _currentTime = timeLimit;
            _isChoosing = true;

            for (var i = 0; i < cardsUI.Length; i++)
            {
                if (i < _currentOptions.Count)
                {
                    cardsUI[i].gameObject.SetActive(true);
                    cardsUI[i].Setup(_currentOptions[i], OnUpgradeSelected);
                }
                else
                {
                    cardsUI[i].gameObject.SetActive(false);
                }
            }
        }

        private void OnUpgradeSelected(UpgradeData selectedData)
        {
            if (!_isChoosing) return;
            _isChoosing = false;
            
            levelUpPanel.SetActive(false);
            UpgradePoolManager.Instance.OnUpgradeChosen(selectedData);
            
            if (_pendingLevelUps > 0 && !PlayerManager.Instance.IsDead)
                TriggerLevelUp();
            else
                StartCoroutine(UnpauseRoutine());
        }

        private void AutoSelectRandom()
        {
            var randomIndex = Random.Range(0, _currentOptions.Count);
            OnUpgradeSelected(_currentOptions[randomIndex]);
        }

        private IEnumerator UnpauseRoutine()
        {
            yield return new WaitForSecondsRealtime(unpauseDelay);
            Time.timeScale = 1f;
        }
    }
}