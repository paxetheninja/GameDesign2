using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PaintersTable : BaseWorkstation
{
    public Transform vaseLocation = null;
    public Transform paintLocation = null;

    [SerializeField] private NetworkVariable<float> currentRecipeDuration = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<ComponentType> currentRecipeResult = new NetworkVariable<ComponentType>();
    [SerializeField] private NetworkVariable<ulong> paintOnTable = new NetworkVariable<ulong>();
    [SerializeField] private NetworkVariable<ulong> vaseOnTable = new NetworkVariable<ulong>();

    public GameObject GoOnTablePaint
    {
        get
        {
            NetworkObject no;
            return NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(paintOnTable.Value, out no)
                ? no.gameObject
                : null;
        }
    }
    public GameObject GoOnTableVase
    {
        get
        {
            NetworkObject no;
            return NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(vaseOnTable.Value, out no)
                ? no.gameObject
                : null;
        }
    }

    private Tuple<float, ComponentType> CurrentRecipe
    {
        get
        {
            if (currentRecipeResult.Value == ComponentType.Unknown)
                return null;
            else
                return new(currentRecipeDuration.Value, currentRecipeResult.Value);
        }
        set
        {
            if (value is null)
            {
                currentRecipeDuration.Value = 0;
                currentRecipeResult.Value = ComponentType.Unknown;
            }
            else
            {
                currentRecipeDuration.Value = value.Item1;
                currentRecipeResult.Value = value.Item2;
            }
        }

    }

    private void Awake()
    {
        if (vaseLocation is null)
            Debug.LogError($"{nameof(vaseLocation)} variable is not set. This will crash if the table is used.");
        if (paintLocation is null)
            Debug.LogError($"{nameof(paintLocation)} variable is not set. This will crash if the table is used.");
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            vaseOnTable.Value = ulong.MaxValue;
            paintOnTable.Value = ulong.MaxValue;
        }
           

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (interactionPlayersCount.Value > 0 && activeProgressBar == null)
        {
            activeProgressBar = ProgressBarScript.SpawnProgressBar(progressBarPrefab, gameObject.transform, interactionProgress, interactionPlayersCount);
        }

        if (!IsServer || CurrentRecipe is null || interactionPlayersCount.Value == 0)
            return;

        interactionProgress.Value += interactionPlayersCount.Value * Time.deltaTime / CurrentRecipe.Item1;

        if (interactionProgress.Value > 1.0f)
        {
            // Interaction is finished
            GoOnTablePaint.GetComponent<NetworkObject>().Despawn();
            GoOnTableVase.GetComponent<NetworkObject>().Despawn();


            GameObject instantiatedObject = Instantiate(ComponentRecipesManager.Instance.GetPrefabOfComponentType(CurrentRecipe.Item2));
            instantiatedObject.GetComponent<NetworkObject>().Spawn(true);
            PlaceVaseGameObjectOnTable(instantiatedObject);
            vaseOnTable.Value = instantiatedObject.GetComponent<NetworkObject>().NetworkObjectId;
            paintOnTable.Value = ulong.MaxValue;

            CurrentRecipe = ComponentRecipesManager.Instance.GetTwoBaseInteractionRecipeFor(workstationType,
                vaseOnTable.Value == ulong.MaxValue
                    ? ComponentType.Unknown
                    : GoOnTableVase
                        .GetComponent<ComponentDescriptor>().type,
                paintOnTable.Value == ulong.MaxValue
                ? ComponentType.Unknown
                : GoOnTablePaint.GetComponent<ComponentDescriptor>().type);
            interactionProgress.Value = 0;
        }
    }

    public override bool InteractionStart()
    {
        if (!CanInteract())
            return false;

        interactionPlayersCount.Value++;

        return true;
    }

    public override bool InteractionEnd()
    {
        // This seems like a very bad idea if we let the user move while holding down the interact button. Using this code, we could walk up to a workstation while the button is pressed -
        // When releasing the button, we could decrease the player count even though we never started to interact with it in the first place. (= negative player count)
        interactionPlayersCount.Value--;

        if (interactionPlayersCount.Value <= 0)
            interactionProgress.Value = 0;

        if (interactionPlayersCount.Value < 0)
            Debug.LogWarning($"Number of interacting players is below zero. ({interactionPlayersCount.Value})");

        return true;
    }

    public override bool CanInteract()
    {
        if (CurrentRecipe is null)
            return false;

        return true;
    }

    public override bool PickupAction(out GameObject gameObjectToPickup, Vector3 targetPosition, Quaternion additionalRotation, ulong itemInHand)
    {
        Tuple<ComponentType, ComponentType> pickupRecipe = null;

        NetworkObject noOnTable = null;
        NetworkObject noInHand = null;


        if (vaseOnTable.Value != ulong.MaxValue)
        {

            noOnTable = NetworkManager.Singleton.SpawnManager.SpawnedObjects[vaseOnTable.Value];

            if (itemInHand != ulong.MaxValue)
                noInHand = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemInHand];

            pickupRecipe = ComponentRecipesManager.Instance.GetPickupDefinitionFor(noOnTable is null ? ComponentType.Unknown : noOnTable.GetComponent<ComponentDescriptor>().type,
                 noInHand is null ? ComponentType.Unknown : noInHand.GetComponent<ComponentDescriptor>().type);


            if (pickupRecipe is null)
            {
                gameObjectToPickup = GoOnTableVase;
                gameObjectToPickup.transform.SetPositionAndRotation(targetPosition, additionalRotation);

                vaseOnTable.Value = ulong.MaxValue;
                CurrentRecipe = null;
              
            }
            else
            {
                // Despawn old components
                noOnTable.Despawn();

                if (noInHand is not null)
                    noInHand.Despawn();

                vaseOnTable.Value = ulong.MaxValue;

                // Spawn new object for hand
                GameObject newGOForHandPrefab = ComponentRecipesManager.Instance.GetPrefabOfComponentType(pickupRecipe.Item2);
                GameObject newGOForHand = Instantiate(newGOForHandPrefab, targetPosition, additionalRotation * newGOForHandPrefab.transform.rotation);
                newGOForHand.GetComponent<NetworkObject>().Spawn(true);

                // Spawn new object for table if there should be any
                GameObject newGOForTablePrefab = ComponentRecipesManager.Instance.GetPrefabOfComponentType(pickupRecipe.Item1);
                if (newGOForTablePrefab is not null)
                {
                    GameObject newGOForTable = Instantiate(newGOForTablePrefab, targetPosition, additionalRotation * newGOForTablePrefab.transform.rotation);
                    newGOForTable.GetComponent<NetworkObject>().Spawn(true);
                    PlaceDownAction(newGOForTable);
                }

                gameObjectToPickup = newGOForHand;
                gameObjectToPickup.transform.SetPositionAndRotation(targetPosition, additionalRotation);
             
            }
            return true;

        }

        if (paintOnTable.Value == ulong.MaxValue)
        {
            gameObjectToPickup = null;
            return false;
        }

        noOnTable = NetworkManager.Singleton.SpawnManager.SpawnedObjects[paintOnTable.Value];

        if (itemInHand != ulong.MaxValue)
            noInHand = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemInHand];

        pickupRecipe = ComponentRecipesManager.Instance.GetPickupDefinitionFor(noOnTable is null ? ComponentType.Unknown : noOnTable.GetComponent<ComponentDescriptor>().type,
             noInHand is null ? ComponentType.Unknown : noInHand.GetComponent<ComponentDescriptor>().type);


        if (pickupRecipe is null)
        {
            gameObjectToPickup = GoOnTablePaint;
            gameObjectToPickup.transform.SetPositionAndRotation(targetPosition, additionalRotation);

            paintOnTable.Value = ulong.MaxValue;
            CurrentRecipe = null;
        }
        else
        {
            // Despawn old components
            noOnTable.Despawn();

            if (noInHand is not null)
                noInHand.Despawn();

            paintOnTable.Value = ulong.MaxValue;

            // Spawn new object for hand
            GameObject newGOForHandPrefab = ComponentRecipesManager.Instance.GetPrefabOfComponentType(pickupRecipe.Item2);
            GameObject newGOForHand = Instantiate(newGOForHandPrefab, targetPosition, additionalRotation * newGOForHandPrefab.transform.rotation);
            newGOForHand.GetComponent<NetworkObject>().Spawn(true);

            // Spawn new object for table if there should be any
            GameObject newGOForTablePrefab = ComponentRecipesManager.Instance.GetPrefabOfComponentType(pickupRecipe.Item1);
            if (newGOForTablePrefab is not null)
            {
                GameObject newGOForTable = Instantiate(newGOForTablePrefab, targetPosition, additionalRotation * newGOForTablePrefab.transform.rotation);
                newGOForTable.GetComponent<NetworkObject>().Spawn(true);
                PlaceDownAction(newGOForTable);
            }

            gameObjectToPickup = newGOForHand;
            gameObjectToPickup.transform.SetPositionAndRotation(targetPosition, additionalRotation);
        }

        return true;

    }
    public override bool PlaceDownAction(GameObject gameObjectInHand)
    {
        if (gameObjectInHand is null)
            return false;

        if (gameObjectInHand.GetComponent<ComponentDescriptor>().type == ComponentType.RedSlip || gameObjectInHand.GetComponent<ComponentDescriptor>().type == ComponentType.BlackSlip)
        {
            //This is paint, put it in the paint slot
            if (paintOnTable.Value == ulong.MaxValue)
            {
                PlacePaintGameObjectOnTable(gameObjectInHand);
                paintOnTable.Value = gameObjectInHand.GetComponent<NetworkObject>().NetworkObjectId;
            }
            else
                return false;


            CurrentRecipe = ComponentRecipesManager.Instance.GetTwoBaseInteractionRecipeFor(workstationType,
            vaseOnTable.Value == ulong.MaxValue
                ? ComponentType.Unknown
                : GoOnTableVase
                    .GetComponent<ComponentDescriptor>().type,
            paintOnTable.Value == ulong.MaxValue
            ? ComponentType.Unknown
            : GoOnTablePaint.GetComponent<ComponentDescriptor>().type);
            return true;
        }
        else
        {
            if (vaseOnTable.Value == ulong.MaxValue)
            {
                // This table is empty. Let the player place the game object onto it.
                PlaceVaseGameObjectOnTable(gameObjectInHand);
                vaseOnTable.Value = gameObjectInHand.GetComponent<NetworkObject>().NetworkObjectId;
            }
            else
            {
                // This means that the table is not empty. Check for recipes if the combination of both would create something.
                GameObject gameObjectOnTable = GoOnTablePaint;

                GameObject newCombinedPrefab = ComponentRecipesManager.Instance.GetPrefabOfCombination(
                    gameObjectOnTable.GetComponent<ComponentDescriptor>().type,
                    gameObjectInHand.GetComponent<ComponentDescriptor>().type);

                if (newCombinedPrefab is null)
                    return false;

                GameObject instantiatedObject = Instantiate(newCombinedPrefab);
                instantiatedObject.GetComponent<NetworkObject>().Spawn(true);
                PlaceVaseGameObjectOnTable(instantiatedObject);
                vaseOnTable.Value = instantiatedObject.GetComponent<NetworkObject>().NetworkObjectId;

                gameObjectOnTable.GetComponent<NetworkObject>().Despawn();
                gameObjectInHand.GetComponent<NetworkObject>().Despawn();
            }

            CurrentRecipe = ComponentRecipesManager.Instance.GetTwoBaseInteractionRecipeFor(workstationType,
                        vaseOnTable.Value == ulong.MaxValue
                            ? ComponentType.Unknown
                            : GoOnTableVase
                                .GetComponent<ComponentDescriptor>().type,
                        paintOnTable.Value == ulong.MaxValue
                        ? ComponentType.Unknown
                        : GoOnTablePaint.GetComponent<ComponentDescriptor>().type);

            return true;
        }
     
    }

    public override ComponentType GetCurrentItemType()
    {
        if (vaseOnTable.Value == ulong.MaxValue)
            return ComponentType.Unknown;
        else
            return NetworkManager.Singleton.SpawnManager.SpawnedObjects[vaseOnTable.Value].GetComponent<ComponentDescriptor>().type;
    }


    public override bool ResetWorkstation()
    {
        GameObject onTableVase = GoOnTableVase;
        GameObject onTablePaint = GoOnTablePaint;
        
        interactionProgress.Value = 0;
        interactionPlayersCount.Value = 0;
        currentRecipeDuration.Value = 0;
        currentRecipeResult.Value = ComponentType.Unknown;
        vaseOnTable.Value = ulong.MaxValue;
        paintOnTable.Value = ulong.MaxValue;

        if (onTableVase is not null)
            onTableVase.GetComponent<NetworkObject>().Despawn();


        if (onTablePaint is not null)
            onTablePaint.GetComponent<NetworkObject>().Despawn();

        return true;
    }

    private void PlaceVaseGameObjectOnTable(GameObject gameObjectToPlace)
    {
        vaseOnTable.Value = gameObjectToPlace.GetComponent<NetworkObject>().NetworkObjectId;
        gameObjectToPlace.transform.parent = transform;
        gameObjectToPlace.transform.localPosition = vaseLocation.localPosition;
        gameObjectToPlace.transform.localRotation = Quaternion.identity;
    }

    private void PlacePaintGameObjectOnTable(GameObject gameObjectToPlace)
    {
        paintOnTable.Value = gameObjectToPlace.GetComponent<NetworkObject>().NetworkObjectId;
        gameObjectToPlace.transform.parent = transform;
        gameObjectToPlace.transform.localPosition = paintLocation.localPosition;
        gameObjectToPlace.transform.localRotation = Quaternion.identity;
    }
}
