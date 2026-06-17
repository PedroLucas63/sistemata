using Sistemata.Stats;
using Sistemata.Audio; // Adicionado o namespace para conversar com o AudioManager
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sistemata.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float gravity = 10f;

        [Header("Audio - Passos")]
        [SerializeField] private AudioClip[] footstepSounds; // Array para colocar os arquivos de áudio
        [SerializeField] private float stepInterval = 0.35f; // Ritmo das pisadas (menor = passos rápidos)
        [Range(0f, 1f)][SerializeField] private float stepVolume = 0.35f; // Volume controlado do passo

        public Vector2 MoveInput { get; private set; }
        public Vector2 LastMoveInput { get; private set; } = Vector2.down;

        private CharacterController _controller;
        private InputSystemActions _inputActions;
        private Vector3 _velocity;
        private EntityStats _stats;
        private Stat _moveSpeedStat;
        private float _stepTimer; // Controla o tempo interno entre cada som de passo

        public float MoveSpeed
        {
            get
            {
                _moveSpeedStat ??= _stats.GetStat(StatType.MoveSpeed);
                return _moveSpeedStat?.Get() ?? 5f;
            }
        }

        public float BaseMoveSpeed
        {
            get
            {
                _moveSpeedStat ??= _stats.GetStat(StatType.MoveSpeed);
                return _moveSpeedStat?.BaseValue ?? 5f;
            }
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _stats = GetComponent<EntityStats>();
            _inputActions = new InputSystemActions();
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
            _inputActions.Player.Move.performed += OnMove;
            _inputActions.Player.Move.canceled += OnMove;
        }

        private void OnDisable()
        {
            _inputActions.Player.Move.performed -= OnMove;
            _inputActions.Player.Move.canceled -= OnMove;
            _inputActions.Player.Disable();
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            MoveInput = ctx.ReadValue<Vector2>();
        }

        private void Update()
        {
            Move();
            ApplyGravity();
            HandleFootstepAudio(); // Gerencia o relógio dos passos a cada frame
        }

        private void Move()
        {
            if (MoveInput != Vector2.zero) LastMoveInput = MoveInput;

            _moveSpeedStat ??= _stats.GetStat(StatType.MoveSpeed);

            var moveDirection = new Vector3(
                 MoveInput.x,
                 0f,
                 MoveInput.y
            );

            moveDirection.Normalize();
            _controller.Move(moveDirection * (MoveSpeed * Time.deltaTime));
        }

        private void ApplyGravity()
        {
            if (_controller.isGrounded) _velocity.y = 0;
            else _velocity.y -= gravity * Time.deltaTime;

            _controller.Move(_velocity * Time.deltaTime);
        }

        /// <summary>
        /// Avalia se o jogador cumpre os pré-requisitos físicos para gerar som de passos.
        /// </summary>
        private void HandleFootstepAudio()
        {
            // Se houver input de movimento E o jogador estiver fisicamente encostando no chão...
            if (MoveInput != Vector2.zero)
            {
                _stepTimer -= Time.deltaTime;

                if (_stepTimer <= 0f)
                {
                    PlayFootstepSound();
                    _stepTimer = stepInterval; // Reinicia o relógio baseado no intervalo definido
                }
            }
            else
            {
                // Se parar de andar ou pular, zeramos o timer. 
                // Assim, no instante exato em que ele der o primeiro passo no chão, o som responde na hora!
                _stepTimer = 0f;
            }
        }

        /// <summary>
        /// Sorteia um áudio da lista e solicita a execução 2D centralizada para o AudioManager.
        /// </summary>
        private void PlayFootstepSound()
        {
            if (footstepSounds == null || footstepSounds.Length == 0 || AudioManager.Instance == null) return;

            // Seleciona um índice aleatório dentro da lista de áudios informada no Inspector
            int randomIndex = Random.Range(0, footstepSounds.Length);
            AudioClip chosenClip = footstepSounds[randomIndex];

            // Executa via SFX 2D (Focado no centro, não fica "para trás" quando o player corre)
            AudioManager.Instance.PlaySFX2D(chosenClip, stepVolume);
        }
    }
}