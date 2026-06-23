using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Sistemata.UI.MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Panels")] 
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject statsPanel;
        [SerializeField] private GameObject aboutPanel;
        [SerializeField] private GameObject loadingPanel;
        
        [Header("Focus Elements")]
        [SerializeField] private GameObject mainMenuDefaultButton;
        [SerializeField] private GameObject statsBackButton; 
        [SerializeField] private GameObject aboutBackButton;

        [Header("Loading UI")] 
        [SerializeField] private Slider loadingSlider;

        private GameObject _currentActivatePanel = null;
        
        public void Play()
        {
            StartCoroutine(LoadSceneAsync("Scenes/ProceduralScene"));
        }

        public void OpenStatsPanel()
        {
            DeactivateMainMenu();

            statsPanel.SetActive(true);
            _currentActivatePanel = statsPanel;
            
            EventSystem.current.firstSelectedGameObject = statsBackButton;
            EventSystem.current.SetSelectedGameObject(statsBackButton);
        }
        
        public void OpenAboutPanel()
        {
            DeactivateMainMenu();

            aboutPanel.SetActive(true);
            _currentActivatePanel = aboutPanel;
            
            EventSystem.current.firstSelectedGameObject = aboutBackButton;
            EventSystem.current.SetSelectedGameObject(aboutBackButton);
        }

        public void ClosePanel()
        {
            _currentActivatePanel.SetActive(false);
            _currentActivatePanel = null;
            ActivateMainMenu();
        }

        private void DeactivateMainMenu()
        {
            if (mainMenuPanel.TryGetComponent<CanvasGroup>(out var canvas))
            {
                canvas.interactable = false;
                canvas.blocksRaycasts = false;
            }

            EventSystem.current.SetSelectedGameObject(null);
        }
        
        private void ActivateMainMenu()
        {
            if (mainMenuPanel.TryGetComponent<CanvasGroup>(out var canvas))
            {
                canvas.interactable = true;
                canvas.blocksRaycasts = true;
            }

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.firstSelectedGameObject = mainMenuDefaultButton;
            EventSystem.current.SetSelectedGameObject(mainMenuDefaultButton);
        }

        public void ExitGame()
        {
            Application.Quit();
        }
        
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            if (mainMenuPanel.TryGetComponent<CanvasGroup>(out var canvas))
            {
                canvas.interactable = false;
            }
            loadingPanel.SetActive(true);

            var operation = SceneManager.LoadSceneAsync(sceneName);

            while (operation is { isDone: false })
            {
                var progress = Mathf.Clamp01(operation.progress / 0.9f);
                
                if (loadingSlider)
                    loadingSlider.value = progress;

                yield return null;
            }
        }
    }
}