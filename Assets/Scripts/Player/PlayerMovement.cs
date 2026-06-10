using Sistemata.Stats;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sistemata.Player
{
     [RequireComponent(typeof(CharacterController))]
     public class PlayerMovement : MonoBehaviour
     {
          [Header("Movement Settings")] 
          [SerializeField] private float gravity = 10f;
     
          public Vector2 MoveInput { get; private set; }
          public Vector2 LastMoveInput { get; private set; } = Vector2.down;
          
          private CharacterController _controller;
          private InputSystemActions _inputActions;
          private Vector3 _velocity;
          private EntityStats _stats;
          private Stat _moveSpeedStat;

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
     }
}
