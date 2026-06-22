using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Sistemata.UI.MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Panels")] 
        [SerializeField] private GameObject statsPanel;
        [SerializeField] private GameObject mainMenu;
        
        [Header("Focus Elements")]
        [SerializeField] private GameObject statsBackButton; 
        [SerializeField] private GameObject mainMenuDefaultButton;

        public void Play()
        {
            SceneManager.LoadScene("Scenes/ProceduralScene");
        }

        public void OpenStatsPanel()
        {
            if (mainMenu.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            statsPanel.SetActive(true);
            
            EventSystem.current.firstSelectedGameObject = statsBackButton;
            EventSystem.current.SetSelectedGameObject(statsBackButton);
        }

        public void CloseStatsPanel()
        {
            statsPanel.SetActive(false);

            if (mainMenu.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            EventSystem.current.firstSelectedGameObject = mainMenuDefaultButton;
            EventSystem.current.SetSelectedGameObject(mainMenuDefaultButton);
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}