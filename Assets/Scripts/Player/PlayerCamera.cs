using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Vector3 _startOffset;
    [SerializeField] private Quaternion _rotation;

    private Camera mainCamera;

    void Update()
    {
        if (mainCamera is null || mainCamera.IsDestroyed())
            mainCamera = Camera.main;

        // We need to do this in update. We can't reparent the mainCamera to the local player object
        // because this would create a mismatch of all player objects for all clients. Therefore Netcode instantly reverts the reparent.
        mainCamera.transform.rotation = _rotation;
        mainCamera.transform.position = transform.position + _startOffset;
    }
}
