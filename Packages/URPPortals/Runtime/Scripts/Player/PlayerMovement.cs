using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        private PlayerControls _controls;
        private CharacterController _characterController;

        [Header("Movement Settings")] 
        [SerializeField] private float walkSpeed;
        [SerializeField] private float runSpeed;
        private float _moveSpeed;
        private Vector2 _moveDelta;
        private Vector3 _move;
        private float _verticalVelocity;
        private bool _isGrounded;

        private const float Gravity = 9.81f;
        
        public bool IsSprinting { get; private set; }
        public static PlayerMovement Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
            
           _characterController = GetComponent<CharacterController>();
           _controls = new PlayerControls();

           InitializeControls();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            GroundCheck();
            DetermineMoveSpeed();
            MovePlayer();
        }

        private void GroundCheck()
        {
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, _characterController.height / 2f + 0.2f);
        }

        private void MovePlayer()
        {
            var lateralMove = (transform.forward * _moveDelta.y + transform.right * _moveDelta.x) * _moveSpeed;   

            _move.x = lateralMove.x;
            _move.z = lateralMove.z;
            
            ApplyGravity();
            _characterController.Move(_move * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            _move.y -= Gravity * Time.deltaTime;
        }

        private void DetermineMoveSpeed()
        {
            _moveSpeed = IsSprinting ? runSpeed : walkSpeed;
        }

        private void OnMoveStart(InputAction.CallbackContext ctx)
        {
            _moveDelta = ctx.ReadValue<Vector2>();
        }
        
        private void OnMoveFinish(InputAction.CallbackContext ctx)
        {
            _moveDelta = Vector2.zero;
        }

        private void OnSprintStart(InputAction.CallbackContext ctx)
        {
            IsSprinting = true;
        }

        private void OnSprintFinish(InputAction.CallbackContext ctx)
        {
            IsSprinting = false;
        }
        
        private void OnEnable() => _controls.Enable();
        private void OnDisable() => _controls.Dispose();

        private void InitializeControls()
        {
            _controls.Player.Move.performed += OnMoveStart;
            _controls.Player.Move.canceled += OnMoveFinish;
            
            _controls.Player.Sprint.performed += OnSprintStart;
            _controls.Player.Sprint.canceled += OnSprintFinish;
        }
    }
}