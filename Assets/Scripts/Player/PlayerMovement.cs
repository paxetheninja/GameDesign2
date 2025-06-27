using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : NetworkBehaviour
{
    private int speed = 7;
    private Animator animator;
    private Rigidbody rigidbody;

    [SerializeField] public FixedJoystick _joystick = null;

    private NetworkVariable<bool> isMoving = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Vector3 movement = Vector3.zero;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();

        isMoving.OnValueChanged += UpdateMovingAnimation;

        if (IsOwner)
        {
            _joystick = FindAnyObjectByType<FixedJoystick>();
            SceneManager.sceneLoaded += (scene, mode) => { _joystick = FindAnyObjectByType<FixedJoystick>(); };
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        Vector2 movementJoystick = _joystick is null ? Vector2.zero : _joystick.Direction;

        if (movementJoystick == Vector2.zero)
            movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        else
            movement = Vector3.ClampMagnitude(new Vector3(movementJoystick.x, 0, movementJoystick.y), 1);
        
        rigidbody.velocity = movement * speed;

        if (movement != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(movement);
            rigidbody.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
            isMoving.Value = true;
        }
        else
        {
            isMoving.Value = false;
        }
    }

    private void UpdateMovingAnimation(bool previousvalue, bool newvalue)
    {
        animator.SetBool("IsMoving", newvalue);
    }
}
