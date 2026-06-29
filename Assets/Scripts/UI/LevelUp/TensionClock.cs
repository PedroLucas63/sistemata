using UnityEngine;

namespace Sistemata.UI.LevelUp
{
    public class TensionClock : MonoBehaviour
    {
        [Header("Componentes")]
        public AudioSource audioSource;
        public AudioClip tickSound;
        public AudioClip tackSound;
        public AudioClip finishSound;

        [Header("Controle de Tensão")]
        [Tooltip("Eixo X: Progresso do tempo (0 a 1). Eixo Y: Tensão aplicada (0 a 1).")]
        public AnimationCurve tensionCurve; 
        
        [Header("Controle de Velocidade")]
        [Tooltip("Tempo em segundos entre cada batida no início.")]
        public float initialInterval = 1.0f; 
        [Tooltip("Tempo em segundos entre cada batida no final (desespero).")]
        public float endInterval = 0.15f;
        
        private float _timeLeft;
        private float _timeToDecide;
        private int _currentSound = 0; // 0 = Tick, 1 = Tack
        private float _soundTimer;
        private bool _isFinished;

        private void OnEnable()
        {
            _timeLeft = _timeToDecide;
            _soundTimer = initialInterval;
            _isFinished = false;
            _currentSound = 0;
            
            audioSource.loop = false;
            audioSource.pitch = 1f;
        }

        private void OnDisable()
        {
            audioSource.Stop();
        }

        private void Update()
        {
            if (_timeLeft > 0)
            {
                _timeLeft -= Time.unscaledDeltaTime;
                
                var timePercent = 1f - (_timeLeft / _timeToDecide);
                var tension = tensionCurve.Evaluate(timePercent);
                
                _soundTimer -= Time.unscaledDeltaTime;

                if (_soundTimer > 0f) return;
                
                var clipToPlay = (_currentSound == 0) ? tickSound : tackSound;
                audioSource.PlayOneShot(clipToPlay);
                    
                _currentSound = 1 - _currentSound; 
                    
                _soundTimer = Mathf.Lerp(initialInterval, endInterval, tension);
            }
            else if (!_isFinished)
            {
                TimeUp();
            }
        }

        private void TimeUp()
        {
            _isFinished = true;
            audioSource.Stop(); 
            
            audioSource.pitch = 1f; 
            audioSource.PlayOneShot(finishSound);
        }

        public void SetTimeToDecide(float timeToDecide)
        {
            _timeToDecide = timeToDecide;

            if (!gameObject.activeInHierarchy) return;
            
            _timeLeft = _timeToDecide;
            _isFinished = false;
        }
    }
}