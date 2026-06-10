using Sistemata.Core;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused;

    [Header("UI Elements")]
    [SerializeField] private GameObject pausePanel; // Arraste o painel visual do menu para cá no Inspector

    private InputSystemActions _input;

    void Awake()
    {
        _input = new InputSystemActions();
        _input.Player.Pause.performed += OnPause;
        _input.Player.Enable();
    }

    void Start()
    {
        // Garante que o estado inicial esteja correto
        isPaused = false;
        pausePanel.SetActive(false); // Desativa apenas o visual, mantendo o script rodando
    }

    void OnDestroy()
    {
        if (_input != null)
        {
            _input.Player.Pause.performed -= OnPause;
            _input.Player.Disable();
            _input = null;
        }
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pausePanel.SetActive(false);
    }

    //public void Restart()
    //{
    //    // Correção vital: Descongela o tempo antes de recarregar a cena!
    //    Time.timeScale = 1f;
    //    isPaused = false;
    //    GameManager.Instance.RestartGame();
    //}
}