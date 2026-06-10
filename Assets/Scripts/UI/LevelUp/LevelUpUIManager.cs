using System.Collections;
using System.Collections.Generic;
using Sistemata.Player;
using UnityEngine;
using UnityEngine.UI;
using Sistemata.Upgrades;

namespace UI.LevelUp
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
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
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

        public void TriggerLevelUp()
        {
            _currentOptions = UpgradePoolManager.Instance.GetRandomUpgrades(3);
            if (_currentOptions.Count == 0) return;

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

            PlayerManager.Instance.ApplyRunUpgrade(selectedData);
            UpgradePoolManager.Instance.OnUpgradeChosen(selectedData);

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