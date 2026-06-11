using UnityEngine;

namespace Sistemata.Ally
{
    public class AllyAnimatorController : MonoBehaviour
    {
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int MoveAnimSpeed = Animator.StringToHash("MoveAnimSpeed");
        private static readonly int Attack = Animator.StringToHash("Attack");
        
        private Animator _animator;
        private Ally _ally;
        
        private bool _wasAttackingLastFrame;

        public float SpeedMultiplier => _ally.BaseMoveSpeed > 0 ? (_ally.MoveSpeed / _ally.BaseMoveSpeed) : 1f;

        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            _ally = GetComponent<Ally>();
        }

        private void Update()
        {
            if (!_ally || !_animator) return;
            UpdateAnimation();
        }
        
        private void UpdateAnimation()  
        {
            _animator.SetFloat(MoveX, _ally.LastMove.x);
            _animator.SetFloat(MoveY, _ally.LastMove.y);
            
            var currentMagnitude = _ally.LastMove.magnitude;
            _animator.SetFloat(Speed, currentMagnitude > 0.01f ? _ally.MoveSpeed : 0f);
            _animator.SetFloat(MoveAnimSpeed, SpeedMultiplier);

            if (_ally.IsAttacking)
            {
                if (_wasAttackingLastFrame) return;
                _animator.SetTrigger(Attack);
                _wasAttackingLastFrame = true;
            }
            else
            {
                _wasAttackingLastFrame = false;
            }
        }
    }
}