using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Grid : NetworkBehaviour
{
    public Vector3Int cells;
    public Vector3 size;
    
    private readonly NetworkVariable<Vector3Int> _netCells = new();
    private readonly NetworkVariable<Vector3> _netSize = new();

    private Bounds _bounds;

    private ulong _gridId;
        
    private static readonly Vector3Int InvalidGridCoords = new(9999, 9999, 9999);
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            _netCells.Value = cells;
            _netSize.Value = size;
        }
        else
        {
            cells = _netCells.Value;
            size = _netSize.Value;
        }
        
        GridManager.Instance.grids.Add(this);
        _gridId = GetComponent<NetworkObject>().NetworkObjectId;
        InitBounds();
    }

    public void Init(Vector3Int _cells, Vector3 _size)
    {
        cells = _cells;
        size = _size;
        InitBounds();
    }
    
    private void InitBounds()
    {
        _bounds = new Bounds(transform.position + new Vector3(0, size.y/2, 0), 
            new Vector3(size.x * cells.x, size.y * cells.y, size.z * cells.z));
    }

    public Vector3 GetGridPosition(Vector3Int gridCoordinates)
    {
        float posX = _bounds.min.x + gridCoordinates.x * size.x + size.x / 2;
        float posY = _bounds.min.y + gridCoordinates.y * size.y;// + size.y / 2;
        float posZ = _bounds.min.z + gridCoordinates.z * size.z + size.z / 2;

        return new Vector3(posX, posY, posZ);
    }

    private Vector3Int GetClosestGridCoordinates(Vector3 position, int cellY)
    {
        float x = Math.Clamp((float)Math.Floor((position.x - _bounds.min.x) / size.x), 0, cells.x-1);
        // float y = Math.Clamp((float)Math.Floor((position.y - _bounds.min.y) / size.x), 0, cells.y-1);
        float z = Math.Clamp((float)Math.Floor((position.z - _bounds.min.z) / size.x), 0, cells.z-1);

        return new Vector3Int((int)x, cellY, (int)z);
    }

    private GridObject FindObjectAtCoordinates(Vector3Int gridCoords)
    {
        return GridManager.Instance.gridObjects.FirstOrDefault(gridObj => gridObj.IsAtPosition(gridCoords) && !gridObj.IsPickedUp() && gridObj.gridId == _gridId);
    }

    public Vector3Int GetNearestFreeSlot(Vector3 worldPosition, Vector3Int objSize, int cellY)
    {
        Vector3Int nearest = InvalidGridCoords;
        float minDistance = float.PositiveInfinity;
        for (int x = 0; x < cells.x; x++)
        {
            for (int z = 0; z < cells.z; z++)
            {
                Vector3Int gridCoords = new Vector3Int(x, cellY, z);

                float distance = Vector3.Distance(GetGridPosition(gridCoords), worldPosition);
                if (!(distance < minDistance)) continue;
            
                var possible = true;
                for (int xLocal = x; xLocal < x + objSize.x; xLocal++)
                {
                    for (int zLocal = z; zLocal < z + objSize.z; zLocal++)
                    {
                        if (xLocal >= cells.x || zLocal >= cells.z ||
                            FindObjectAtCoordinates(new Vector3Int(xLocal, cellY, zLocal)) != null)
                        {
                            possible = false;
                            goto InnerLoopEnd;
                        }
                    }
                }
            
                InnerLoopEnd:
                if (possible == false) continue;
            
                nearest = gridCoords;
            
                minDistance = distance;
            }
        }

        return nearest;
    }

    public GameObject GetNearestObject(Vector3 worldPosition)
    {
        GameObject nearest = null;
        float minDistance = float.PositiveInfinity;

        foreach (var gridObj in GridManager.Instance.gridObjects.Where(g => g.gridId == _gridId))
        {
            Vector3 objPos = gridObj.transform.position;
            objPos.y = 0;
            worldPosition.y = 0;
            float distance = Vector3.Distance(objPos, worldPosition);
            if (!(distance < minDistance)) continue;
            nearest = gridObj.gameObject;
            minDistance = distance;
        }

        return nearest;
    }

    private void OnDrawGizmos()
    {
        Bounds b = new Bounds(transform.position + new Vector3(0, size.y/2, 0), 
            new Vector3(size.x * cells.x, size.y * cells.y, size.z * cells.z));

        for (int x = 0; x <= cells.x; x++)
        {
            float posX = b.min.x + size.x * x;
            float posY = b.min.y;
            float posZ = b.min.z;
        
            Vector3 pos = new Vector3(posX, posY, posZ);

            Gizmos.DrawLine(pos, pos + Vector3.forward * size.z * cells.z);
        }
    
        for (int z = 0; z <= cells.z; z++)
        {
            float posX = b.min.x;
            float posY = b.min.y;
            float posZ = b.min.z + size.z * z;
        
            Vector3 pos = new Vector3(posX, posY, posZ);

            Gizmos.DrawLine(pos, pos + Vector3.right * size.x * cells.x);
        }
    }
}
