using UnityEngine;
using System;

namespace Sistemata.Core
{
    public enum GameState { Normal, BossTransition, Boss, Chaos, GameOver }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState currentState;

        [Header("Configurações de Tempo")]
        public float timeUntilBoss = 300f; // Ex: 300 segundos = 5 minutos
        private float phaseTimer;

        // Status do jogador
        public float totalTimeSurvived { get; private set; }
        public int monstersKilled { get; private set; }

        // Eventos para outros scripts escutarem
        public event Action OnBossWarning;
        public event Action OnBossSpawn;
        public event Action OnChaosStart;
        public event Action<int, float> OnGameOver;

        private void Awake()
        {
            // Padrão Singleton simples
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            currentState = GameState.Normal;
            phaseTimer = timeUntilBoss;
            Time.timeScale = 1f; // Garante que o jogo não está pausado
        }

        private void Update()
        {
            if (currentState == GameState.GameOver) return;

            // Tempo total que o jogador está vivo (usado no Game Over)
            totalTimeSurvived += Time.deltaTime;

            // Fase 1: Timer rolando
            if (currentState == GameState.Normal)
            {
                phaseTimer -= Time.deltaTime;
                if (phaseTimer <= 0)
                {
                    phaseTimer = 0; // Garante que não fique negativo
                    StartBossPhase();
                }
            }
        }

        // NOVA FUNÇÃO: Retorna o tempo restante formatado para a UI
        public string GetTimerText()
        {
            // Se já passou da fase normal, não precisa mais mostrar o countdown do boss
            if (currentState != GameState.Normal)
            {
                return "00:00"; // Ou "BOSS!" se preferir
            }

            // Calcula minutos e segundos baseados no phaseTimer
            int minutes = Mathf.FloorToInt(phaseTimer / 60f);
            int seconds = Mathf.FloorToInt(phaseTimer % 60f);

            // Retorna no formato MM:SS (o :00 garante que sempre terá 2 dígitos, ex: 05:09 em vez de 5:9)
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        private void StartBossPhase()
        {
            currentState = GameState.BossTransition;
            OnBossWarning?.Invoke(); // Dispara o aviso na UI (Pausa o spawn normal)

            // Espera 3 segundos e spawna o Boss
            Invoke(nameof(SpawnBoss), 3f);
        }

        private void SpawnBoss()
        {
            currentState = GameState.Boss;
            OnBossSpawn?.Invoke(); // Manda o spawner instanciar o Boss
        }

        // O script do Boss deve chamar essa função quando a vida dele chegar a zero
        public void BossDied()
        {
            if (currentState == GameState.Boss)
            {
                currentState = GameState.Chaos;
                OnChaosStart?.Invoke(); // Spawner começa a jogar tudo na tela
            }
        }

        // Chame isso nos inimigos sempre que morrerem
        public void AddKill()
        {
            monstersKilled++;
        }

        // O script do Player deve chamar isso quando a vida chegar a zero
        public void PlayerDied()
        {
            currentState = GameState.GameOver;
            Time.timeScale = 0f; // Pausa o jogo
            OnGameOver?.Invoke(monstersKilled, totalTimeSurvived); // Avisa a UI
        }
    }
}

