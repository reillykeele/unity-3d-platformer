using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterController : MonoBehaviour
{
    [Header("Movement")] [SerializeField] public float moveSpeed = 1f;
    [SerializeField] public float sprintSpeed = 1.5f;
    [SerializeField] public float maxGravity = -9.81f;
    [SerializeField] public float gravity = 1f;
    [SerializeField] public float groundedGravity = 0.01f;
    [SerializeField] public float jumpVelocity = 5f;
    [Range(0, 1)] [SerializeField] public float playerRotationSpeed = 1f;

    [Header("Camera")] [SerializeField] public float cameraRotationSpeed = 1f;
    [Range(0, 90)] [SerializeField] public float cameraTopAngle = 45f;
    [Range(0, 90)] [SerializeField] public float cameraBottomAngle = 45;

    private float CameraTopAngle
    {
        get => 90 - cameraTopAngle;
        set => cameraTopAngle = value + 90;
    }

    private float CameraBottomAngle
    {
        get => 270 + cameraBottomAngle;
        set => cameraBottomAngle = value - 270;
    }

    private PlayerInput _playerInput;
    private CharacterController _controller;
    private Animator _animator;
    private Camera _camera;
    private GameObject _cameraTarget;

    private Vector2 currMovementInput;
    private Vector3 currMovement;
    private float currMovementY;
    private float currHorizontalVelocity;
    private bool isMovementPressed;

    private Vector2 currCameraInput;

    private bool isSprinting;
    private bool isJumpPressed;

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
        InitInputAction(_playerInput.PlayerControls.Movement, OnMovementInput);
        InitInputAction(_playerInput.PlayerControls.Camera, OnCameraInput);
        InitInputAction(_playerInput.PlayerControls.Sprint, OnSprintInput);
        InitInputAction(_playerInput.PlayerControls.Jump, OnJumpInput);

        // Setup animation hashes
        velocityAnimHash = Animator.StringToHash("Velocity");
    }

    private void InitInputAction(InputAction action, Action<InputAction.CallbackContext> callback)
    {
        action.started += callback;
        action.canceled += callback;
        action.performed += callback;
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
        isSprinting = context.ReadValue<float>() > 0.5 || isSprinting;
    }

    private void OnJumpInput(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValue<float>() > 0.5;
    }

    void Update()
    {
        ProcessCamera();
        ProcessDirection();
        ProcessMovement(Time.deltaTime);
        ProcessAnimation();
    }

    private void ProcessCamera()
    {
        _cameraTarget.transform.rotation *= Quaternion.AngleAxis(currCameraInput.x * cameraRotationSpeed, Vector3.up);
        _cameraTarget.transform.rotation *= Quaternion.AngleAxis(currCameraInput.y * cameraRotationSpeed, Vector3.right);

        // Clamp the camera vertically 
        var angles = _cameraTarget.transform.localEulerAngles;
        angles.x =  angles.x > 180 && angles.x < CameraBottomAngle ? CameraBottomAngle :
            angles.x < 180 && angles.x > CameraTopAngle  ? CameraTopAngle :
            angles.x;
        angles.z = 0;

        _cameraTarget.transform.localEulerAngles = angles;
    }

    private void ProcessMovement(float deltaTime)
    {
        currHorizontalVelocity = currMovement.magnitude;
        if (currHorizontalVelocity <= 0)
            isSprinting = false;

        var move = isSprinting ? currMovement * sprintSpeed : currMovement * moveSpeed;
        move.y = currMovementY;

        if (_controller.isGrounded)
        {
            if (isJumpPressed && move.y <= 0)
            {
                move.y += jumpVelocity;
            }
            else
                move.y = -groundedGravity;
        }
        else
            move.y -= move.y < maxGravity ? 0 : gravity;

        _controller.Move(move * deltaTime);
        currMovementY = move.y;
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
        _animator.SetFloat(velocityAnimHash, currHorizontalVelocity);
    }

}
