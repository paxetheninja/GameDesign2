using Unity.Netcode;
using UnityEngine;

public class ServerLocalNetworkTransform : NetworkBehaviour
{
    private NetworkVariable<Vector3> position = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> rotation = new NetworkVariable<Quaternion>();


    void Update()
    {
        if (IsServer)
        {
            position.Value = transform.localPosition;
            rotation.Value = transform.localRotation;
        }
        else
        {
            transform.localPosition = position.Value;
            transform.localRotation = rotation.Value;
        }
    }
}
