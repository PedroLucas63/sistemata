using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace Sistemata.Core
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI do Gameplay (Durante a partida)")]
        public TextMeshProUGUI gameplayTimerText; // <-- NOVO: Arraste o texto do seu timer aqui no Inspector

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
            // NOVO: Agora também checa se o estado é GameState.Normal antes de tentar atualizar
            if (GameManager.Instance.currentState == GameState.Normal && gameplayTimerText != null)
            {
                gameplayTimerText.text = GameManager.Instance.GetTimerText();
            }
        }

        private void ShowBossWarning()
        {
            bossWarningPanel.SetActive(true);

            // NOVO: Esconde o timer da UI assim que a fase normal acabar
            if (gameplayTimerText != null)
            {
                gameplayTimerText.gameObject.SetActive(false);
            }

            // Desativa o aviso após 3 segundos
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

            // Formata o tempo para Minutos:Segundos
            int minutes = Mathf.FloorToInt(time / 60F);
            int seconds = Mathf.FloorToInt(time - minutes * 60);
            timeSurvivedText.text = string.Format("Tempo Sobrevivido: {0:00}:{1:00}", minutes, seconds);
        }

        // --- FUNÇÕES DOS BOTÕES ---

        public void RestartLevel()
        {
            // Recarrega a cena atual
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f; // Importante resetar o timescale antes de ir pro menu!
            SceneManager.LoadScene("MainMenu"); // Coloque o nome da sua cena de Menu aqui
        }

        private void OnDestroy()
        {
            // Boa prática desinscrever para evitar memory leak
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBossWarning -= ShowBossWarning;
                GameManager.Instance.OnGameOver -= ShowGameOverScreen;
            }
        }
    }
}
