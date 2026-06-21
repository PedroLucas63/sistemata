using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sistemata.UI.MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Panels")] 
        [SerializeField] private GameObject statsPanel;
        [SerializeField] private GameObject mainMenu;

        [Header("Stats Texts")] 
        [SerializeField] private TextMeshProUGUI txtMaxDeaths;
        [SerializeField] private TextMeshProUGUI txtMaxTime;

        public void Play()
        {
            SceneManager.LoadScene("ProceduralScene");
        }

        public void OpenStatsPanel()
        {
            txtMaxDeaths.text = "45";
            txtMaxTime.text = "24:12";

            statsPanel.SetActive(true);
            mainMenu.SetActive(false);
        }

        public void CloseStatsPanel()
        {
            statsPanel.SetActive(false);
            mainMenu.SetActive(true);
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}