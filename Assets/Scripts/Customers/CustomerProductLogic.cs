using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class CustomerProductLogic : BaseWorkstation
{
    [SerializeField] private Transform componentHoldingPosition;
    public NetworkVariable<ComponentType> Order = new NetworkVariable<ComponentType>();
    public Action<CustomerProductLogic> OrderFulfilledCallback;
    public int difficulty;

    private bool _orderRevealed;
    
    /// <summary>
    /// Generates an Order for this customer based on the given difficulty. (Or should we change this to take out of a set of ComponentTypes?)
    /// </summary>
    /// <param name="difficulty">The difficulty of the Order that should get generated. Might get changed to list of possible ComponentTypes.</param>
    [ServerRpc]
    private void GenerateOrderServerRpc(int difficulty)
    {
        List<ComponentType> possibleTypes;
        Random r;
        switch (difficulty)
        {
            case -6:
                possibleTypes = new List<ComponentType>() { ComponentType.MediumVase1HandleBakedBlack, ComponentType.BigVase2HandleBakedRed, ComponentType.BigVaseBakedBlack, ComponentType.MediumVase2HandleBakedRed };
                r = new Random();
                Order.Value = possibleTypes[r.Next(possibleTypes.Count)];
                break;
            case -5:
                Order.Value = ComponentType.MediumVase2HandleBaked;
                break;
            case 0:
                Order.Value = ComponentType.Clay;
                break;
            case 2:
                possibleTypes = new List<ComponentType>() { ComponentType.PlateBaked };
                r = new Random();
                Order.Value = possibleTypes[r.Next(possibleTypes.Count)];
                break;
            case 5: 
                possibleTypes = new List<ComponentType>() {ComponentType.PlateBaked, ComponentType.Plate1HandleBaked, ComponentType.Plate2HandleBaked, ComponentType.MediumVaseBaked,ComponentType.MediumVase1HandleBaked,ComponentType.MediumVase2HandleBaked, ComponentType.MediumVase3HandleBaked };
                r = new Random();
                Order.Value = possibleTypes[r.Next(possibleTypes.Count)];
                break;
            case 10:
                possibleTypes = new List<ComponentType>()
                {
                    ComponentType.PlateBaked, ComponentType.Plate1HandleBaked, ComponentType.Plate2HandleBaked,
                    ComponentType.PlateBakedRed, ComponentType.Plate1HandleBakedRed, ComponentType.Plate2HandleBakedRed,
                    ComponentType.PlateBakedBlack, ComponentType.Plate1HandleBakedBlack, ComponentType.Plate2HandleBakedBlack,
                    ComponentType.MediumVaseBaked, ComponentType.MediumVase1HandleBaked, ComponentType.MediumVase2HandleBaked, ComponentType.MediumVase3HandleBaked,
                    ComponentType.MediumVaseBakedRed, ComponentType.MediumVase1HandleBakedRed, ComponentType.MediumVase2HandleBakedRed, ComponentType.MediumVase3HandleBakedRed,
                    ComponentType.MediumVaseBakedBlack, ComponentType.MediumVase1HandleBakedBlack, ComponentType.MediumVase2HandleBakedBlack, ComponentType.MediumVase3HandleBakedBlack,
                    ComponentType.BigVaseBaked, ComponentType.BigVase1HandleBaked, ComponentType.BigVase2HandleBaked, ComponentType.BigVase3HandleBaked,
                    ComponentType.BigVaseBakedRed, ComponentType.BigVase1HandleBakedRed, ComponentType.BigVase2HandleBakedRed, ComponentType.BigVase3HandleBakedRed,
                    ComponentType.BigVaseBakedBlack, ComponentType.BigVase1HandleBakedBlack, ComponentType.BigVase2HandleBakedBlack, ComponentType.BigVase3HandleBakedBlack
                };
                r = new Random();
                Order.Value = possibleTypes[r.Next(possibleTypes.Count)];
                break;
            default:
                Order.Value = ComponentType.Clay;
                break;
        }

        _orderRevealed = true;
    }

    public override bool CanInteract()
    {
        return true;
    }

    public override bool InteractionStart()
    {
        if (_orderRevealed) return false;
        
        GenerateOrderServerRpc(difficulty);

        // We can return false here to not start the interaction animation.
        return false;
    }


    public override bool PlaceDownAction(GameObject gameObjectInHand)
    {
        // This should check if a player wants to give the correct item type (the one that this customer ordered) to this customer.

        if (gameObjectInHand.GetComponent<ComponentDescriptor>().type == Order.Value)
        {
            // This means that the Order has been fulfilled. The customer should now leave the workshop.
            gameObjectInHand.transform.parent = transform;
            gameObjectInHand.transform.localPosition = componentHoldingPosition.localPosition;
            gameObjectInHand.transform.localRotation = componentHoldingPosition.localRotation;

            OrderFulfilledCallback(this);

            return true;
        }

        return false;
    }


}
