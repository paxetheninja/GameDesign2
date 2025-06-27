using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class RawMaterialSource : BaseWorkstation
{
    [Tooltip("The type of component that can be picked up from here. This component will get instantiated and spawned.")]
    public ComponentType materialSourceType;

    [Tooltip("The type of components that can be trashed here. Should probably include the one that can be picked up here at least.")]
    public List<ComponentType> trashableComponentTypes;

    public override bool PickupAction(out GameObject gameObjectToPickup, Vector3 targetPosition, Quaternion additionalRotation, ulong itemInHand)
    {
        GameObject rawMaterialPrefab = ComponentRecipesManager.Instance.GetPrefabOfComponentType(materialSourceType);

        GameObject newMaterial = Instantiate(rawMaterialPrefab, targetPosition, additionalRotation * rawMaterialPrefab.transform.rotation);
        newMaterial.GetComponent<NetworkObject>().Spawn(true);

        gameObjectToPickup = newMaterial;

        return true;
    }

    public override bool PlaceDownAction(GameObject gameObjectInHand)
    {
        if (gameObjectInHand is null)
            return false;

        if (!trashableComponentTypes.Contains(gameObjectInHand.GetComponent<ComponentDescriptor>().type)) 
            return false;

        gameObjectInHand.GetComponent<NetworkObject>().Despawn();
        return true;

    }
}
