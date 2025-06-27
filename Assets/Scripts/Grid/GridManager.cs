using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class GridManager : NetworkBehaviour
{
    [SerializeField] private Animation gridPhaseWarningTextAnimation;
    
    public List<Grid> grids;
    public List<GridObject> gridObjects;
    
    public GameObject gridPickupFramePrefab;
    public GameObject gridPickupFrameImmovablePrefab;
    public GameObject gridPlaceFramePrefab;

    private ulong _currentSelectedId = ulong.MaxValue;
    private bool _moving;
    
    private GameObject _currentFrame;
    private ulong _currentFramedId = ulong.MaxValue;
    private ulong _currentFramedGridId = ulong.MaxValue;
    
    private bool _gridsFixed;
    private readonly NetworkVariable<bool> _netGridFixed = new();

    public static readonly Vector3Int InvalidGridCoords = new(9999, 9999, 9999);

    public static GridManager Instance;

    Button _liftButton;

    // min workstations in room
    public int AmountOfWorkstationsInARoom = 3;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
        } 
        else 
        { 
            Instance = this;
        }
        StartCoroutine(RegisterTouchButtons(1.5f));
    }
    
    IEnumerator RegisterTouchButtons(float time)
    {

        yield return new WaitForSeconds(time);
        GameObject touchButtons = GameObject.Find("TouchButtons");
        if (touchButtons != null)
        {
            VisualElement root = touchButtons.GetComponent<UIDocument>().rootVisualElement;
            //root = FindObjectOfType<UIDocument>().rootVisualElement;
            _liftButton = root.Q<Button>("LiftButton");
            Button rotateButton = root.Q<Button>("RotateButton");

            _liftButton.RegisterCallback<ClickEvent>((evt) =>
            {
                if (!_gridsFixed)
                {
                    PickupPlace();
                }
            });

            rotateButton.RegisterCallback<ClickEvent>((evt) =>
            {
                if (!_gridsFixed)
                {
                    Rotate();
                }
            });
        }
    }
    
    public void Update()
    {
        if (_gridsFixed) return;

        var playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;

        if (playerObject == null) return;

        FrameObjectInRange(playerObject.transform.position);
        FrameClosestFreeGridPosition();
        
        if (Input.GetKeyDown("e"))
        {
            PickupPlace();
        }
        else if (Input.GetKeyDown("q"))
        {
            Rotate();
        }
    }

    private void PickupPlace()
    {
        if (!_moving)
        {
            if (_currentFramedId != ulong.MaxValue)
            {
                PickupServerRpc(NetworkManager.Singleton.LocalClient.ClientId, _currentFramedId, _currentFramedGridId);
                _liftButton.text = "Place";
            }
        }
        else
        {
            Place();
           
        }
    }

    // for pickup the server function is called first to make sure 2 clients cant pickup the same objects
    [ServerRpc(RequireOwnership = false)]
    private void PickupServerRpc(ulong clientId, ulong objectToPickupId, ulong objectToPickupGridId)
    {        
        GridObject gridObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectToPickupId]
            .GetComponent<GridObject>();

        if (gridObj.immovable)
        {
            PlayBuildPhaseWarningAnimationClientRpc(clientId);
            return;
        }

        if (gridObj.IsPickedUp() || gridObj.alwaysImmovable) return;

        SoundsScript.Instance.SoundGridPickup();
        
        gridObj.SetPickedUp(NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.NetworkObjectId);

        CheckSetImmovable(objectToPickupGridId);
        
        PickupClientRpc(clientId, objectToPickupId);
    }

    [ClientRpc]
    private void PlayBuildPhaseWarningAnimationClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClient.ClientId != clientId) return;
        
        gridPhaseWarningTextAnimation.Play();
    }

    [ClientRpc]
    private void PickupClientRpc(ulong clientId, ulong pickedUpObjectId)
    {
        GridObject gridObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[pickedUpObjectId]
            .GetComponent<GridObject>();
    
        // all clients disable collider locally
        gridObj.Pickup();

        // code below is only for client who picked up the workstation   
        if (NetworkManager.Singleton.LocalClient.ClientId != clientId) return;
        

        _moving = true;
        Destroy(_currentFrame);
        _currentFrame = null;
    
        _currentSelectedId = pickedUpObjectId;
        _currentFramedId = ulong.MaxValue;
        _currentFramedGridId = ulong.MaxValue;
    }

    // For place the local function is called first and then the server place is called to make sure sync is correct
    private void Place()
    {
        if (_currentSelectedId == ulong.MaxValue || _currentFrame == null) return;
        _liftButton.text = "Lift";
        SoundsScript.Instance.SoundGridPlace();

        _moving = false;
        ulong objId = _currentSelectedId;
        _currentSelectedId = ulong.MaxValue;
    
        NetworkManager.Singleton.SpawnManager.SpawnedObjects[objId].GetComponent<GridObject>().PlaceServerRpc();
    }

    public void CheckSetImmovable(ulong gridId)
    {
        var objLeftOnGrid = gridObjects.Where(o => o.gridId == gridId).ToList();
        if (objLeftOnGrid.Count <= AmountOfWorkstationsInARoom)
        {
            foreach (var obj in objLeftOnGrid)
            {
                obj.immovable = true;
            }
        }
    }

    public void CheckSetMovable(ulong gridId)
    {
        var objLeftOnGrid = gridObjects.Where(o => o.gridId == gridId).ToList();
        if (objLeftOnGrid.Count > AmountOfWorkstationsInARoom)
        {
            foreach (var obj in objLeftOnGrid)
            {
                obj.immovable = false;
            }
        }
    }

    private void Rotate()
    {
        if (_currentSelectedId == ulong.MaxValue) return;

        SoundsScript.Instance.SoundGridRotate();
        NetworkManager.Singleton.SpawnManager.SpawnedObjects[_currentSelectedId].GetComponent<GridObject>().RotateServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void FixGridServerRpc()
    {
        FixGridsClientRpc();
    }
    
    // TODO THIS CAN ONLY BE DONE BY SERVER LATER
    [ClientRpc]
    public void FixGridsClientRpc()
    {
        _gridsFixed = true;
        foreach (var gridObj in gridObjects)
        {
            gridObj.FixGridObject();
        }
        
        Destroy(_currentFrame);
        _currentFrame = null;
        _currentFramedId = ulong.MaxValue;
        _currentFramedGridId = ulong.MaxValue;
        _currentSelectedId = ulong.MaxValue;
        _moving = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UnFixGridServerRpc()
    {
        UnFixGridsClientRpc();
    }
    
    [ClientRpc]
    public void UnFixGridsClientRpc()
    {
        _gridsFixed = false;
    }
    
    public Tuple<Vector3Int, Grid> Snap(GameObject obj, Vector3Int objSize)
    {
        var closest = GetNearestFreeSlot(obj.transform.position, obj.GetComponent<GridObject>().size);
        var grid = closest.Item1;
        var gridCoords = closest.Item2;

        if (gridCoords.Equals(InvalidGridCoords)) return new Tuple<Vector3Int, Grid>(Vector3Int.zero, null);

        obj.transform.position = grid.GetGridPosition(gridCoords) + new Vector3((objSize.x-1)*0.5f*grid.size.x, 0, (objSize.z-1)*0.5f*grid.size.z);

        return new Tuple<Vector3Int, Grid>(gridCoords, grid);
    }

    private void FrameObjectInRange(Vector3 worldPosition)
    {
        if (_moving) return;
        
        float currentSmallestDistance = Single.PositiveInfinity;
        GameObject currentNearestObj = null;
        Grid currentNearestObjGrid = null;
        
        foreach (var grid in grids)
        {
            GameObject nearestInGrid = grid.GetNearestObject(worldPosition);
            if (nearestInGrid == null) continue;
            
            float distance = Vector3.Distance(nearestInGrid.transform.position, worldPosition);

            if (distance < currentSmallestDistance)
            {
                currentSmallestDistance = distance;
                currentNearestObj = nearestInGrid;
                currentNearestObjGrid = grid;
            }
        }

        if (currentNearestObj == null) return; // no object found

        if (currentSmallestDistance > 2) // is too far away
        {
            Destroy(_currentFrame);
            _currentFrame = null;
            _currentFramedId = ulong.MaxValue;
            _currentFramedGridId = ulong.MaxValue;
            return;
        }

        ulong currentNearestObjId = currentNearestObj.GetComponent<NetworkObject>().NetworkObjectId;
        if (currentNearestObjId == _currentFramedId) return; // is already framed
        
        
        GridObject gridObj = currentNearestObj.GetComponent<GridObject>();
        
        Destroy(_currentFrame);

        _currentFramedId = currentNearestObjId;
        _currentFramedGridId = currentNearestObjGrid.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
        _currentFrame = Instantiate(!(gridObj.immovable || gridObj.alwaysImmovable) ? gridPickupFramePrefab : gridPickupFrameImmovablePrefab, currentNearestObjGrid.transform);

        _currentFrame.transform.localScale = new Vector3(gridObj.size.x * 0.95f, gridObj.size.y * 0.95f, gridObj.size.z * 0.95f);
        _currentFrame.transform.position = new Vector3(currentNearestObj.transform.position.x, _currentFrame.transform.position.y, currentNearestObj.transform.position.z);
    }

    private void FrameClosestFreeGridPosition()
    {
        if (!_moving) return;
        
        GridObject gridObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_currentSelectedId]
            .GetComponent<GridObject>();

        Vector3 selectedPos = gridObj.gameObject.transform.position;
        var closest = GetNearestFreeSlot(selectedPos, gridObj.size);
        var grid = closest.Item1;
        var gridCoords = closest.Item2;
        Vector3 framePos = grid.GetGridPosition(gridCoords);

        float distanceToNearest = Vector3.Distance(framePos, selectedPos);

        // invalid or too far away
        if (distanceToNearest > 3)
        {
            Destroy(_currentFrame);
            _currentFrame = null;
            return;
        }

        // already frame at same pos
        if (_currentFramedId != ulong.MaxValue && _currentFrame.transform.position == framePos) return;
        
        Destroy(_currentFrame);

        _currentFrame = Instantiate(gridPlaceFramePrefab, grid.gameObject.transform);
        _currentFrame.transform.localScale = new Vector3(gridObj.size.x * 0.95f, gridObj.size.y * 0.95f, gridObj.size.z * 0.95f);
        _currentFrame.transform.position = new Vector3(framePos.x+(gridObj.size.x-1)*0.5f*grid.size.x, _currentFrame.transform.position.y, framePos.z+(gridObj.size.z-1)*0.5f*grid.size.z);
    }

    private Tuple<Grid, Vector3Int> GetNearestFreeSlot(Vector3 worldPosition, Vector3Int objSize)
    {
        Vector3Int currentNearest = InvalidGridCoords;
        Grid currentGrid = null;
        float currentMinDistance = float.PositiveInfinity;
        
        
        foreach (var grid in grids)
        {
            Vector3Int gridCoords = grid.GetNearestFreeSlot(worldPosition, objSize, 0);

            float distance = Vector3.Distance(grid.GetGridPosition(gridCoords), worldPosition);

            if (distance < currentMinDistance)
            {
                currentGrid = grid;
                currentMinDistance = distance;
                currentNearest = gridCoords;
            }
        }

        Debug.Assert(currentGrid != null);
        return new Tuple<Grid, Vector3Int>(currentGrid, currentNearest);
    }
}
