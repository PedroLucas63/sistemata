using UnityEngine;

namespace Sistemata.Enemy
{
    public class EnemyAnimatorController :  MonoBehaviour
    {
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int MoveAnimSpeed = Animator.StringToHash("MoveAnimSpeed");
        private static readonly int Attack = Animator.StringToHash("Attack");
        
        private Animator _animator;
        private EnemyController _enemyController;

        public float SpeedMultiplier =>
            _enemyController.MoveSpeed / _enemyController.BaseMoveSpeed;

        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            _enemyController = GetComponent<EnemyController>();
        }

        private void Update()
        {
            UpdateAnimation();
        }
        
        private void UpdateAnimation()  
        {
            _animator.SetFloat(MoveX, _enemyController.LastMove.x);
            _animator.SetFloat(MoveY, _enemyController.LastMove.y);
            _animator.SetFloat(Speed, _enemyController.MoveSpeed);
            _animator.SetFloat(MoveAnimSpeed, SpeedMultiplier);
            if (_enemyController.IsAttacking)
                _animator.SetTrigger(Attack);
        }
    }
}