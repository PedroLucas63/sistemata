using UnityEngine;

namespace Sistemata.Player 
{
    public class PlayerAnimatorController : MonoBehaviour
    {
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int MoveAnimSpeed = Animator.StringToHash("MoveAnimSpeed");
        
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private PlayerMovement _playerMovement;

        public float SpeedMultiplier =>
            _playerMovement.MoveSpeed / _playerMovement.BaseMoveSpeed;

        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _playerMovement = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            UpdateAnimation();
        }
        
        private void UpdateAnimation()  
        {
            _animator.SetFloat(MoveX, _playerMovement.LastMoveInput.x);

            _spriteRenderer.flipX = _playerMovement.LastMoveInput.x switch
            {
                < 0 => true,
                > 0 => false,
                _ => _spriteRenderer.flipX
            };

            _animator.SetFloat(MoveY, _playerMovement.LastMoveInput.y);
            _animator.SetFloat(Speed, _playerMovement.MoveInput.sqrMagnitude);
            
            _animator.SetFloat(MoveAnimSpeed, SpeedMultiplier);
        }
    }
}