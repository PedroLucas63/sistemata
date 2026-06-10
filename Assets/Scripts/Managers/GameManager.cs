using Sistemata.Audio;
using System;
using UnityEngine;

namespace Sistemata.Core
{
    public enum GameState { Normal, BossTransition, Boss, Chaos, GameOver }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Configurações Gerais")]
        public Transform player;
        private CharacterController playerScript;
        public CharacterController PlayerScript { get { return playerScript; } }

        [Header("Áudio")]
        public AudioClip roundMusic;

        public GameState currentState;

        [Header("Configurações de Tempo")]
        public float timeUntilBoss = 300f;
        private float phaseTimer;

        public float totalTimeSurvived { get; private set; }
        public int monstersKilled { get; private set; }

        public event Action OnBossWarning;
        public event Action OnBossSpawn;
        public event Action OnChaosStart;
        public event Action<int, float> OnGameOver;

        [Header("Configurações de projéteis")] public Transform ProjectileParent;
    
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (AudioManager.Instance != null && roundMusic != null)
            {
                AudioManager.Instance.ChangeBGM(roundMusic);
            }
            else
            {
                Debug.LogWarning("AudioManager não encontrado na cena ou roundMusic está vazio!");
            }

            currentState = GameState.Normal;
            phaseTimer = timeUntilBoss;
            Time.timeScale = 1f;

            if (player != null)
                playerScript = player.GetComponent<CharacterController>();

        }

        private void Update()
        {
            if (currentState == GameState.GameOver) return;

            totalTimeSurvived += Time.deltaTime;

            // Fase 1: Timer rolando
            if (currentState == GameState.Normal)
            {
                phaseTimer -= Time.deltaTime;
                if (phaseTimer <= 0)
                {
                    phaseTimer = 0;
                    StartBossPhase();
                }
            }
        }

        public string GetTimerText()
        {
            if (currentState != GameState.Normal)
            {
                return "00:00";
            }

            int minutes = Mathf.FloorToInt(phaseTimer / 60f);
            int seconds = Mathf.FloorToInt(phaseTimer % 60f);

            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        private void StartBossPhase()
        {
            currentState = GameState.BossTransition;
            OnBossWarning?.Invoke();

            Invoke(nameof(SpawnBoss), 3f);
        }

        private void SpawnBoss()
        {
            currentState = GameState.Boss;
            OnBossSpawn?.Invoke();
        }

        public void BossDied()
        {
            if (currentState == GameState.Boss)
            {
                currentState = GameState.Chaos;
                OnChaosStart?.Invoke();
            }
        }

        public void AddKill()
        {
            monstersKilled++;
        }

        public void PlayerDied()
        {
            currentState = GameState.GameOver;
            Time.timeScale = 0f;
            OnGameOver?.Invoke(monstersKilled, totalTimeSurvived);
        }
    }
}

