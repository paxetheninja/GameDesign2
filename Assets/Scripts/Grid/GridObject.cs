using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Misc;
using Unity.Netcode;
using UnityEngine;

public class GridObject : NetworkBehaviour
{
    public int levelY;
    public Vector3Int size;
    public bool alwaysImmovable; // dont use in script, only in editor
    public bool immovable;

    private Vector3Int _gridCoordinates;
    public ulong gridId = ulong.MaxValue;
    
    private Vector3Int _originalSize;
    private int _direction;
    private IEnumerator _rotationCoroutine;

    private bool _pickedUp = true; // start state is not placed
    private ulong _pickedUpById = ulong.MaxValue;
    private NetworkObject _pickedUpByPlayer;

    public readonly NetworkVariable<bool> _netImmovable = new();
    private readonly NetworkVariable<Vector3> _netPosition = new();
    private readonly NetworkVariable<int> _netDirection = new();
    private readonly NetworkVariable<Vector3Int> _netSize = new();
    private readonly NetworkVariable<Vector3Int> _netGridCoords = new();
    private readonly NetworkVariable<ulong> _netGridId = new();
    public readonly NetworkVariable<bool> _netPickedUp = new();
    private readonly NetworkVariable<ulong> _netPickedUpById = new();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GridManager.Instance.gridObjects.Add(this);
    }

    private void Start()
    {
        if (name.Contains("Black"))
        {
            Debug.Log("Size at start:" + size);
        }
        _originalSize = size;
        _direction = (int) Math.Ceiling( transform.localRotation.eulerAngles.y / 90) % 4;
        AdjustSizeToRotation();
        
        if (!IsServer) return;
        
        PlaceServerRpc();
    }

    private void Update()
    {
        if (IsServer)
        {
            _netDirection.Value = _direction;
            _netSize.Value = size;
            _netPickedUp.Value = _pickedUp;
            _netPickedUpById.Value = _pickedUpById;
            _netGridId.Value = gridId;
            _netImmovable.Value = immovable;

            if (!_pickedUp)
            {
                _netPosition.Value = transform.localPosition;
                _netGridCoords.Value = _gridCoordinates;
            }
        }
        else
        {
            _direction = _netDirection.Value;
            size = _netSize.Value;
            _pickedUp = _netPickedUp.Value;
            _pickedUpById = _netPickedUpById.Value;
            gridId = _netGridId.Value;
            immovable = _netImmovable.Value;

            if (!_pickedUp)
            {
                transform.localPosition = _netPosition.Value;
                _gridCoordinates = _netGridCoords.Value;
            }
        }

        AdjustSizeToRotation();

        _pickedUpByPlayer =
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.GetValueOrDefault(_pickedUpById, null);

        if (_pickedUpByPlayer == null) return;
        Transform playerTransform = _pickedUpByPlayer.transform;
        transform.position = playerTransform.position + playerTransform.forward*2f + new Vector3(0, 1f, 0);
    }

    // adjust size values to current rotation
    private void AdjustSizeToRotation()
    {
        if (_direction % 2 == 0) size = _originalSize;
        else if (_direction % 2 == 1) (size.x, size.z) = (_originalSize.z, _originalSize.x);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RotateServerRpc()
    {
        _direction = (_direction + 1) % 4;
        RotateClientRpc(_direction);
    }

    [ClientRpc]
    private void RotateClientRpc(int targetDirection)
    {
        if (_rotationCoroutine != null) StopCoroutine(_rotationCoroutine);

        _rotationCoroutine = StartRotation(targetDirection * 90, TweeningFunctions.Linear);
        StartCoroutine(_rotationCoroutine);
    }

    IEnumerator StartRotation(float targetRotation, Func<float, float, float, float> tweeningFunction, float time=0.2f)
    {
        float state = 0.0f;
        Quaternion startRotation = transform.localRotation;

        float normalizedTargetRotation = startRotation.eulerAngles.y > targetRotation ? targetRotation + 360 : targetRotation;
        float targetRotationAngle = normalizedTargetRotation - startRotation.eulerAngles.y;
     
        while (state <= 1.0f)
        {
            float delta = Time.deltaTime / time;
            float angle = tweeningFunction(0f, targetRotationAngle, state);
            transform.rotation = startRotation;
            transform.RotateAround(transform.position, Vector3.up, angle);
            if (state < 1.0f && state + delta > 1.0f)
            {
                state = 1.0f;
            }
            else
            {
                state += delta;
            }
            yield return true;
        }

        _rotationCoroutine = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlaceServerRpc()
    {
        var snapRet = GridManager.Instance.Snap(gameObject, size);
        
        Debug.Assert(snapRet.Item2 != null);
        
        _gridCoordinates = snapRet.Item1;
        gridId = snapRet.Item2.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
        _pickedUp = false;
        _pickedUpById = ulong.MaxValue;
        
        GridManager.Instance.CheckSetMovable(gridId);
        GridManager.Instance.CheckSetImmovable(gridId);
        PlaceClientRpc();
    }

    // notify clients that object was placed
    [ClientRpc]
    private void PlaceClientRpc()
    {
        GetComponentInChildren<Collider>().enabled = true;
    }
    
    // notify clients that object was picked up
    public void Pickup()
    {
        GetComponentInChildren<Collider>().enabled = false;
    }

    public bool IsAtPosition(Vector3Int posToCheck)
    {
        return posToCheck.x >= _gridCoordinates.x 
               && posToCheck.x < _gridCoordinates.x + size.x
               && posToCheck.z >= _gridCoordinates.z
               && posToCheck.z < _gridCoordinates.z + size.z;
    }

    public void SetPickedUp(ulong playerObjectId)
    {
        gridId = ulong.MaxValue;
        _pickedUp = true;
        _pickedUpById = playerObjectId;
    }

    public bool IsPickedUp()
    {
        return _pickedUp;
    }

    public void FixGridObject()
    {
        if (_pickedUp)
        {
            PlaceServerRpc();
        }
    }

    public int GetSize()
    {
        return size.x * size.z;
    }
}
