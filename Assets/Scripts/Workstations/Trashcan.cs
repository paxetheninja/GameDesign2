using Unity.Netcode;
using UnityEngine;

public class Trashcan : BaseWorkstation
{
    public override bool PlaceDownAction(GameObject gameObjectInHand)
    {
        gameObjectInHand.GetComponent<NetworkObject>().Despawn();
        return true;
    }
}
