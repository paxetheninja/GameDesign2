using System;
using Unity.Netcode;
using UnityEngine;

public abstract class BaseWorkstation : NetworkBehaviour
{
    public WorkstationType workstationType = WorkstationType.Unknown;

    [SerializeField] protected GameObject progressBarPrefab;
    public GameObject activeProgressBar;
    [SerializeField] protected NetworkVariable<float> interactionProgress = new NetworkVariable<float>();
    [SerializeField] protected NetworkVariable<int> interactionPlayersCount = new NetworkVariable<int>();

    /// <summary>
    /// Tries to execute the pickup action on the workstation. Will return the picked up game object as out variable if workstation allowed it.
    /// </summary>
    /// <param name="gameObjectToPickup">Out Variable. The spawned GameObject that gets picked up.</param>
    /// <param name="targetPosition">The position where the game object should get placed.</param>
    /// <param name="additionalRotation">The rotation that should get added to the prefab rotation. This will mostly come from the player rotation.</param>
    /// <param name="itemInHand">The items network id that is currently in the players hands. Relevant for stacking components.</param>
    /// <returns>True if the pickup action is successful on this workstation.</returns>
    public virtual bool PickupAction(out GameObject gameObjectToPickup, Vector3 targetPosition, Quaternion additionalRotation, ulong itemInHand)
    {
        gameObjectToPickup = null;
        // Do nothing. Nothing can be picked up from this workstation.
        return false;
    }

    /// <summary>
    /// Tries to execute the place down action on the workstation.
    /// </summary>
    /// <param name="gameObjectInHand">The GameObject that is currently in the hands of the player. This is the object that gets placed on the workstation.</param>
    /// <returns>True if the place down action is successful on this workstation.</returns>
    public virtual bool PlaceDownAction(GameObject gameObjectInHand)
    {
        // Do nothing. Nothing can be placed down on this workstation.
        return false;
    }

    /// <returns>The item type of this workstation that is relevant for the next action. Use Component.Unknown if no item is present.</returns>
    public virtual ComponentType GetCurrentItemType()
    {
        // This workstation does not have anything on it.
        return ComponentType.Unknown;
    }

    public virtual bool InteractionStart()
    {
        // Do nothing. This workstation does not allow interaction.
        return false;
    }

    public virtual bool InteractionEnd()
    {
        // Do nothing. This workstation does not allow interaction.
        return false;
    }

    public virtual bool CanInteract()
    {
        // Do nothing. This workstation does not allow interaction.
        return false;
    }

    /// <summary>
    /// Should reset the workstation to the starting state. This might include deleting internal values such as holding component id for example.
    /// </summary>
    /// <returns>True if the reset worked.</returns>
    public virtual bool ResetWorkstation()
    {
        // Do nothing.
        return true;
    }
}

public enum WorkstationType
{
    Unknown,
    RawMaterialSource,
    Table,
    Trashcan,
    Potterswheel,
    Oven,
    Customer,
    GrindingTable,
    PaintersTable
}
