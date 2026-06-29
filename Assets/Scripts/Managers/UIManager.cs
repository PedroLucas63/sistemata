using System;
using Sistemata.Player;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Sistemata.Core
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI do Gameplay (Durante a partida)")]
        [SerializeField] private TextMeshProUGUI gameplayTimerText;

        [Header("Painéis UI")]
        [SerializeField] private GameObject bossWarningPanel;
        [SerializeField] private GameObject chaosWarningPanel;
        [SerializeField] private GameObject invasionPanel;
        [SerializeField] private GameObject gameOverPanel;

        [Header("UI de XP e Level")] [SerializeField]
        private GameObject xpPanel;

        [Header("Textos Game Over")]
        [SerializeField] private TextMeshProUGUI killsText;
        [SerializeField] private TextMeshProUGUI timeSurvivedText;

        private Slider _xpSlider;
        private TextMeshProUGUI _levelText;

        private void Start()
        {
            bossWarningPanel.SetActive(false);
            gameOverPanel.SetActive(false);

            if (xpPanel)
            {
                _xpSlider = xpPanel.GetComponentInChildren<Slider>();
                _levelText = xpPanel.GetComponentInChildren<TextMeshProUGUI>();
            }

            GameManager.Instance.OnBossWarning += ShowBossWarning;
            GameManager.Instance.OnChaosWarning += ShowChaosWarning;
            GameManager.Instance.OnInvasionWarning += ShowInvasionWarning;
            GameManager.Instance.OnInvasionStart += ShowInvasionStart;
            GameManager.Instance.OnInvasionEnd += ShowInvasionEnd;
            GameManager.Instance.OnGameOver += ShowGameOverScreen;
            
            PlayerManager.Instance.OnXPChanged +=  UpdateXpPanel;
        }

        private void ShowInvasionWarning()
        {
            invasionPanel.SetActive(true);
            invasionPanel.GetComponentInChildren<TextMeshProUGUI>().text = "Uma invasão vai começar em breve, se prepare!";
            invasionPanel.GetComponent<Image>().color = Color.yellow;
            Invoke(nameof(HideInvasionPanel), 3f);
        }
        
        private void ShowInvasionStart()
        {
            invasionPanel.SetActive(true);
            invasionPanel.GetComponentInChildren<TextMeshProUGUI>().text = "A invasão iniciou!";
            invasionPanel.GetComponent<Image>().color = Color.yellow;
            Invoke(nameof(HideInvasionPanel), 3f);
        }
        
        private void ShowInvasionEnd()
        {
            invasionPanel.GetComponentInChildren<TextMeshProUGUI>().text = "A calmaria foi restabelecida!";
            invasionPanel.GetComponent<Image>().color = Color.deepSkyBlue;
            invasionPanel.SetActive(true);
            Invoke(nameof(HideInvasionPanel), 3f);
        }
        
        private void HideInvasionPanel()
        {
            invasionPanel.SetActive(false);
        }

        private void Update()
        {
            if ((GameManager.Instance.currentState == GameState.Normal ||
                 GameManager.Instance.currentState == GameState.Invasion ||
                 GameManager.Instance.currentState == GameState.InvasionTransition) && gameplayTimerText)
            {
                gameplayTimerText.text = GameManager.Instance.GetTimerText();
            }
        }

        private void UpdateXpPanel(int level, float currentXp, float targetXp)
        {
            if (_xpSlider)
                _xpSlider.value = currentXp / targetXp;
            
            if (_levelText)
                _levelText.text = $"Nível {level}";
        }

        private void UpdateTimerText()
        {
            if (!gameplayTimerText) return;
            if (GameManager.Instance.currentState != GameState.Normal)
                if (GameManager.Instance.currentState != GameState.InvasionTransition)
                    if (GameManager.Instance.currentState != GameState.InvasionTransition)
                        return;
            
            gameplayTimerText.text = GameManager.Instance.GetTimerText();
        }

        private void ShowBossWarning()
        {
            bossWarningPanel.SetActive(true);

            if (gameplayTimerText)
                gameplayTimerText.gameObject.SetActive(false);

            Invoke(nameof(HideBossWarning), 3f);
        }

        private void HideBossWarning()
        {
            bossWarningPanel.SetActive(false);
        }
        private void ShowChaosWarning()
        {
            chaosWarningPanel.SetActive(true);
            Invoke(nameof(HideChaosWarning), 3f);
        }

        private void HideChaosWarning()
        {
            chaosWarningPanel.SetActive(false);
        }


        private void ShowGameOverScreen(int kills, float time)
        {
            gameOverPanel.SetActive(true);
            killsText.text = "Monstros Derrotados: " + kills;

            int minutes = Mathf.FloorToInt(time / 60F);
            int seconds = Mathf.FloorToInt(time - minutes * 60);
            timeSurvivedText.text = string.Format("Tempo Sobrevivido: {0:00}:{1:00}", minutes, seconds);
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenuScene");
        }


        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBossWarning -= ShowBossWarning;
                GameManager.Instance.OnChaosWarning -= ShowChaosWarning;
                GameManager.Instance.OnGameOver -= ShowGameOverScreen;
            }
        }
    }
}
