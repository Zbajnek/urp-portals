using UnityEngine;

namespace Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerLook : MonoBehaviour
    {
        private PlayerControls _controls;
        
        [Header("References")]
        [SerializeField] private Transform head;
        
        [Header("Settings")]
        [SerializeField] private Vector2 mouseSensitivity;
        private Vector2 _mouseDelta;
        private float _yaw, _pitch;

        public static PlayerLook Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
            
            _controls = new PlayerControls();
            
            InitializeControls();
        }

        private void LateUpdate()
        {
            RotateCamera();
        }

        private void RotateCamera()
        {
            _yaw = _mouseDelta.x * (mouseSensitivity.x / 10f);
            _pitch -= _mouseDelta.y * (mouseSensitivity.y / 10f);
            
            _pitch = Mathf.Clamp(_pitch, -89f, 89f);
            
            head.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            transform.Rotate(Vector3.up * _yaw);
        }

        private void OnEnable() => _controls.Enable();
        private void OnDisable() => _controls.Dispose();

        private void InitializeControls()
        {
            _controls.Player.Look.performed += ctx => _mouseDelta = ctx.ReadValue<Vector2>();
            _controls.Player.Look.canceled += _ => _mouseDelta = Vector2.zero;
        }
    }
}