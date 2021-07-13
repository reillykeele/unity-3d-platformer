using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private PlayerCharacterController _playerController;

    void Start()
    {
        _playerController = GetComponent<PlayerCharacterController>();
    }

    void Update()
    {
        var vertical = Input.GetAxis("Vertical");
        var horizontal = Input.GetAxis("Horizontal");

        _playerController.ForwardInput = vertical;
        _playerController.HorizontalInput = horizontal;
    }
}
