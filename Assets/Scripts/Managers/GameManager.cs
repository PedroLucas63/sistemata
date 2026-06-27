using Sistemata.Audio;
using System;
using Sistemata.Common;
using Sistemata.Save;
using UnityEngine;

namespace Sistemata.Core
{
    public enum GameState
    {
        Normal, 
        InvasionTransition,
        Invasion,
        BossTransition, 
        Boss, 
        ChaosTransition, 
        Chaos, 
        GameOver
    }

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
        public float timeUntilInvasion = 150f;
        public float invasionTime = 15f;
        private float _phaseTimer;
        private bool _invasionStarted = false;

        public float TotalTimeSurvived { get; private set; }
        public int MonstersKilled { get; private set; }
        
        // Eventos
        public event Action OnInvasionWarning;
        public event Action OnInvasionStart;
        public event Action OnInvasionEnd;
        public event Action OnBossWarning;
        public event Action OnBossSpawn;
        public event Action OnChaosWarning;
        public event Action OnChaosStart;
        public event Action<int, float> OnGameOver;

        [Header("Configurações de projéteis")]
        public Transform ProjectileParent;
        
        private RoundData _roundData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
    
            Instance = this;
            _roundData = new RoundData();
            currentState = GameState.Normal;
            _phaseTimer = timeUntilBoss;
            Time.timeScale = 1f;
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

            if (player != null)
                playerScript = player.GetComponent<CharacterController>();

        }

        private void Update()
        {
            if (currentState == GameState.GameOver) return;

            TotalTimeSurvived += Time.deltaTime;
            _roundData.TimeSurvived += Time.deltaTime;;

            if (currentState != GameState.Normal && currentState != GameState.Invasion && currentState != GameState.InvasionTransition) return;
            
            _phaseTimer -= Time.deltaTime;

            if (timeUntilBoss - _phaseTimer >= timeUntilInvasion && !_invasionStarted)
            {
                StartInvasionPhase();
                return;
            }
            
            if (!(_phaseTimer <= 0)) return;
            
            _phaseTimer = 0;
            StartBossPhase();
        }

        public string GetTimerText()
        {
            if (currentState != GameState.Normal && 
                currentState != GameState.Invasion && 
                currentState != GameState.InvasionTransition)
            {
                return "00:00";
            }

            int minutes = Mathf.FloorToInt(_phaseTimer / 60f);
            int seconds = Mathf.FloorToInt(_phaseTimer % 60f);

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
                _roundData.Completed = true;
                currentState = GameState.ChaosTransition;
                OnChaosWarning?.Invoke();

                Invoke(nameof(StartChaosPhase), 3f);
            }
        }

        private void StartInvasionPhase()
        {
            _invasionStarted = true;
            currentState = GameState.InvasionTransition;
            OnInvasionWarning?.Invoke();
            Invoke(nameof(InvasionPhase), 5f);
        }
        
        private void InvasionPhase()
        {
            currentState = GameState.Invasion;
            OnInvasionStart?.Invoke();
            Invoke(nameof(EndInvasionPhase), invasionTime);
        }
        
        private void EndInvasionPhase()
        {
            currentState = GameState.Normal;
            OnInvasionEnd?.Invoke();
        }

        private void StartChaosPhase()
        {
            currentState = GameState.Chaos;
            OnChaosStart?.Invoke();
        }

        public void AddKill()
        {
            MonstersKilled++;
            _roundData.MonstersKilled++;
        }
        
        public void AddCollectible(CollectibleType type, float amount)
        {
            switch (type)
            {
                case CollectibleType.Xp:
                    _roundData.XpCollected += amount;
                    break;
                case CollectibleType.Coin:
                    _roundData.GoldCollected += amount;
                    break;
                case CollectibleType.Magnet:
                case CollectibleType.Bomb:
                case CollectibleType.Life:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        public void AddLevel()
        {
            _roundData.Level++;
        }

        public void PlayerDied()
        {
            currentState = GameState.GameOver;
            Time.timeScale = 0f;
            OnGameOver?.Invoke(MonstersKilled, TotalTimeSurvived);
            SaveManager.Instance.UpdateSave(_roundData);
        }
    }
}