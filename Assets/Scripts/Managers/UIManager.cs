using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace Sistemata.Core
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI do Gameplay (Durante a partida)")]
        public TextMeshProUGUI gameplayTimerText;

        [Header("Painéis UI")]
        public GameObject bossWarningPanel;
        public GameObject gameOverPanel;

        [Header("Textos Game Over")]
        public TextMeshProUGUI killsText;
        public TextMeshProUGUI timeSurvivedText;

        private void Start()
        {
            bossWarningPanel.SetActive(false);
            gameOverPanel.SetActive(false);

            GameManager.Instance.OnBossWarning += ShowBossWarning;
            GameManager.Instance.OnGameOver += ShowGameOverScreen;
        }

        private void Update()
        {
            if (GameManager.Instance.currentState == GameState.Normal && gameplayTimerText != null)
            {
                gameplayTimerText.text = GameManager.Instance.GetTimerText();
            }
        }

        private void ShowBossWarning()
        {
            bossWarningPanel.SetActive(true);

            if (gameplayTimerText != null)
            {
                gameplayTimerText.gameObject.SetActive(false);
            }

            Invoke(nameof(HideBossWarning), 3f);
        }

        private void HideBossWarning()
        {
            bossWarningPanel.SetActive(false);
        }

        private void ShowGameOverScreen(int kills, float time)
        {
            gameOverPanel.SetActive(true);
            killsText.text = "Monstros Abatidos: " + kills;

            int minutes = Mathf.FloorToInt(time / 60F);
            int seconds = Mathf.FloorToInt(time - minutes * 60);
            timeSurvivedText.text = string.Format("Tempo Sobrevivido: {0:00}:{1:00}", minutes, seconds);
        }

        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBossWarning -= ShowBossWarning;
                GameManager.Instance.OnGameOver -= ShowGameOverScreen;
            }
        }
    }
}
