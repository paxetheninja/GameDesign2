using Unity.Netcode;
using UnityEngine;

public class NetworkedPlayerName : NetworkBehaviour
{
    public NetworkVariable<NetworkString> Name = new NetworkVariable<NetworkString>("Initial name value.", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        Name.OnValueChanged += OnValueChanged;

        if (IsOwner)
            Name.Value = PersistentInfoHolder.Instance.LocalPlayerName;

        OnValueChanged("oldValue", Name.Value);

        base.OnNetworkSpawn();
    }

    private void OnValueChanged(NetworkString previousvalue, NetworkString newvalue)
    {
        //if (!IsServer)
        //    return;

        gameObject.GetComponent<NetworkObject>().
        gameObject.name = $"Player{OwnerClientId} {newvalue}";
    }
}
