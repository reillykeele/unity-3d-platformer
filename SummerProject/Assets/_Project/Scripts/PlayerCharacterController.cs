using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterController : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 1f;
    [SerializeField] public float sprintSpeed = 1.5f;
    [SerializeField] public float gravity = 1f;
    [SerializeField] public const float groundedGravity = 0.05f;
    [Range(0,1)] 
    [SerializeField] public float playerRotationSpeed = 1f;
    [SerializeField] public float cameraRotationSpeed = 1f;

    private PlayerInput _playerInput;
    private CharacterController _controller;
    private Animator _animator;
    private Camera _camera;
    private GameObject _cameraTarget;

    private Vector2 currMovementInput;
    private Vector3 currMovement;
    private float currVelocity;
    private bool isMovementPressed;

    private Vector2 currCameraInput;

    private bool isSprinting;

    private int velocityAnimHash;

    void OnEnable() { _playerInput.Enable(); }
    void OnDisable() { _playerInput.Disable(); }

    public void Awake()
    {
        // Bind components
        _playerInput = new PlayerInput();
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _camera = FindObjectOfType<Camera>();
        _cameraTarget = GameObject.Find("Camera Follow Target");

        // Setup input system
        _playerInput.PlayerControls.Movement.started += OnMovementInput;
        _playerInput.PlayerControls.Movement.canceled += OnMovementInput;
        _playerInput.PlayerControls.Movement.performed += OnMovementInput;

        _playerInput.PlayerControls.Camera.started += OnCameraInput;
        _playerInput.PlayerControls.Camera.canceled += OnCameraInput;
        _playerInput.PlayerControls.Camera.performed += OnCameraInput;

        _playerInput.PlayerControls.Sprint.started += OnCameraInput;
        _playerInput.PlayerControls.Sprint.canceled += OnCameraInput;
        _playerInput.PlayerControls.Sprint.performed += OnCameraInput;

        // Setup animation hashes
        velocityAnimHash = Animator.StringToHash("Velocity");
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        currMovementInput = context.ReadValue<Vector2>();
        currMovement = new Vector3(currMovementInput.x, 0, currMovementInput.y);
        currMovement = _cameraTarget.transform.forward * currMovement.z +
                       _cameraTarget.transform.right * currMovement.x;
        currMovement.y = 0;
        isMovementPressed = currMovementInput != Vector2.zero;
    }

    private void OnCameraInput(InputAction.CallbackContext context)
    {
        currCameraInput = context.ReadValue<Vector2>();
    }

    private void OnSprintInput(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValue<bool>() || isSprinting;
    }

    void Update()
    {
        ProcessCamera();
        ProcessMovement(Time.deltaTime);
        ProcessDirection();
        ProcessAnimation();
    }
    
    private void ProcessMovement(float deltaTime)
    {
        currVelocity = currMovement.magnitude;

        var move = isSprinting ? currMovement * sprintSpeed : currMovement * moveSpeed;

        if (_controller.isGrounded)
            move.y -= groundedGravity;
        else
            move.y -= gravity;

        _controller.Move(move * deltaTime);
    }

    private void ProcessDirection()
    {
        var posToLookAt = new Vector3(currMovement.x, 0, currMovement.z);
        if (isMovementPressed)
        {
            var cameraTargetRotation = _cameraTarget.transform.rotation;

            var targetRotation = Quaternion.LookRotation(posToLookAt);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, playerRotationSpeed);

            _cameraTarget.transform.rotation = cameraTargetRotation;
        }
    }

    private void ProcessAnimation()
    {
        _animator.SetFloat(velocityAnimHash, currVelocity);
    }

    private void ProcessCamera()
    {
        _cameraTarget.transform.rotation *= Quaternion.AngleAxis(currCameraInput.x * cameraRotationSpeed, Vector3.up);
        _cameraTarget.transform.rotation *= Quaternion.AngleAxis(currCameraInput.y * cameraRotationSpeed, Vector3.right);

        // Clamp the camera vertically 
        var angles = _cameraTarget.transform.localEulerAngles;
        angles.x =  angles.x > 180 && angles.x < 340 ? 340 :
                    angles.x < 180 && angles.x > 40  ? 40 :
                    angles.x;
        angles.z = 0;
        
        _cameraTarget.transform.localEulerAngles = angles;
    }
}
