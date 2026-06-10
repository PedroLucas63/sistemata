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
        private Vector2 _lastMove = Vector2.down;
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
            if (_playerMovement.MoveInput != Vector2.zero) _lastMove = _playerMovement.MoveInput;
            
            _animator.SetFloat(MoveX, _lastMove.x);
            
            if (_lastMove.x < 0) _spriteRenderer.flipX = true;
            else if (_lastMove.x > 0) _spriteRenderer.flipX = false;
            
            _animator.SetFloat(MoveY, _lastMove.y);
            _animator.SetFloat(Speed, _playerMovement.MoveInput.sqrMagnitude);
            
            _animator.SetFloat(MoveAnimSpeed, SpeedMultiplier);
        }
    }
}