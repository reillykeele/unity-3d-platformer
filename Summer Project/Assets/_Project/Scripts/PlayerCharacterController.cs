using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{

    [Tooltip("Maximum slope the character can jump on")]
    [Range(5f, 60f)]
    public float slopeLimit = 45f;
    [Tooltip("Move speed in meters/second")]
    public float moveSpeed = 2f;
    [Tooltip("Turn speed in degrees/second, left (+) or right (-)")]
    public float turnSpeed = 300;
    [Tooltip("Whether the character can jump")]
    public bool allowJump = false;
    [Tooltip("Upward speed to apply when jumping in meters/second")]
    public float jumpSpeed = 4f;

    public float ForwardInput { get; set; }
    public float HorizontalInput { get; set; }

    private Camera _camera { get; set; }
    private CharacterController _controller { get; set; }

    public void Awake()
    {
        _camera = FindObjectOfType<Camera>();
        _controller = GetComponent<CharacterController>();
    }
    
    void FixedUpdate()
    {
        ProcessActions();
    }

    private void ProcessActions()
    {
        var direction = new Vector3(Mathf.Clamp(HorizontalInput, -1f, 1f), 0, Mathf.Clamp(ForwardInput, -1f, 1f));
        var move = direction * moveSpeed * Time.fixedDeltaTime;
        _controller.Move(move);
        if (move != Vector3.zero)
        {
            transform.forward = direction;
        }
    }
}
