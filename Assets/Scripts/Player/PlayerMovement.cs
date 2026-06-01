using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
     [RequireComponent(typeof(CharacterController))]
     public class PlayerMovement : MonoBehaviour
     {
          [Header("Movement")] 
          [SerializeField] private float moveSpeed = 5f;
          [SerializeField] private float gravity = 10f;
     
          public Vector2 MoveInput { get; private set; }
          
          private CharacterController _controller;
          private InputSystemActions _inputActions;
          private Vector3 _velocity;
          
          private void Awake()
          {
               _controller = GetComponent<CharacterController>();
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
               var moveDirection = new Vector3(
                    MoveInput.x,
                    0f,
                    MoveInput.y
               );
          
               moveDirection.Normalize();
          
               _controller.Move(moveDirection * (moveSpeed * Time.deltaTime));
          }

          private void ApplyGravity()
          {
               if (_controller.isGrounded) _velocity.y = 0;
               else _velocity.y -= gravity * Time.deltaTime;
               
               _controller.Move(_velocity * Time.deltaTime);
          }
     }
}
