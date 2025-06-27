using System;
using Unity.Netcode;
using UnityEngine;

public class ManufacturingWorkstation : BaseWorkstation
{
    public Transform componentLocation = null;

    [SerializeField] private NetworkVariable<float> currentRecipeDuration = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<ComponentType> currentRecipeResult = new NetworkVariable<ComponentType>();
    [SerializeField] private NetworkVariable<ulong> objectOnTable = new NetworkVariable<ulong>();

    public GameObject GoOnTable
    {
        get
        {
            NetworkObject no;
            return NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectOnTable.Value, out no)
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
        if (componentLocation is null)
            Debug.LogError($"{nameof(componentLocation)} variable is not set. This will crash if the table is used.");
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            objectOnTable.Value = ulong.MaxValue;

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
            GoOnTable.GetComponent<NetworkObject>().Despawn();

            GameObject instantiatedObject = Instantiate(ComponentRecipesManager.Instance.GetPrefabOfComponentType(CurrentRecipe.Item2));
            instantiatedObject.GetComponent<NetworkObject>().Spawn(true);
            PlaceGameObjectOnTable(instantiatedObject);
            objectOnTable.Value = instantiatedObject.GetComponent<NetworkObject>().NetworkObjectId;


            CurrentRecipe = ComponentRecipesManager.Instance.GetInteractionRecipeFor(workstationType,
                objectOnTable.Value == ulong.MaxValue
                    ? ComponentType.Unknown
                    : GoOnTable
                        .GetComponent<ComponentDescriptor>().type);
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
        if (objectOnTable.Value == ulong.MaxValue)
        {
            gameObjectToPickup = null;
            return false;
        }

        Tuple<ComponentType, ComponentType> pickupRecipe = null;

        NetworkObject noOnTable = null;
        NetworkObject noInHand = null;


        noOnTable = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectOnTable.Value];

        if (itemInHand != ulong.MaxValue)
            noInHand = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemInHand];

        pickupRecipe = ComponentRecipesManager.Instance.GetPickupDefinitionFor( noOnTable is null ? ComponentType.Unknown : noOnTable.GetComponent<ComponentDescriptor>().type,
             noInHand is null ? ComponentType.Unknown : noInHand.GetComponent<ComponentDescriptor>().type);
        

        if (pickupRecipe is null)
        {
            gameObjectToPickup = GoOnTable;
            gameObjectToPickup.transform.SetPositionAndRotation(targetPosition, additionalRotation);

            objectOnTable.Value = ulong.MaxValue;
            CurrentRecipe = null;
        }
        else
        {
            // Despawn old components
            noOnTable.Despawn();
            objectOnTable.Value = ulong.MaxValue;

            if (noInHand is not null)
                noInHand.Despawn();

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
            else
            {
                // This means that the table is now empty. We do not call PlaceDownAction(...). Therefore we need something to reset the current recipe.
                CurrentRecipe = null;
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

        if (objectOnTable.Value == ulong.MaxValue)
        {
            // This table is empty. Let the player place the game object onto it.
            PlaceGameObjectOnTable(gameObjectInHand);
            objectOnTable.Value = gameObjectInHand.GetComponent<NetworkObject>().NetworkObjectId;
        }
        else
        {
            // This means that the table is not empty. Check for recipes if the combination of both would create something.
            GameObject gameObjectOnTable = GoOnTable;

            GameObject newCombinedPrefab = ComponentRecipesManager.Instance.GetPrefabOfCombination(
                gameObjectOnTable.GetComponent<ComponentDescriptor>().type,
                gameObjectInHand.GetComponent<ComponentDescriptor>().type);

            if (newCombinedPrefab is null)
                return false;

            GameObject instantiatedObject = Instantiate(newCombinedPrefab);
            instantiatedObject.GetComponent<NetworkObject>().Spawn(true);
            PlaceGameObjectOnTable(instantiatedObject);
            objectOnTable.Value = instantiatedObject.GetComponent<NetworkObject>().NetworkObjectId;

            gameObjectOnTable.GetComponent<NetworkObject>().Despawn();
            gameObjectInHand.GetComponent<NetworkObject>().Despawn();
        }

        CurrentRecipe = ComponentRecipesManager.Instance.GetInteractionRecipeFor(workstationType,
            objectOnTable.Value == ulong.MaxValue
                ? ComponentType.Unknown
                : GoOnTable
                    .GetComponent<ComponentDescriptor>().type);

        return true;
    }

    public override ComponentType GetCurrentItemType()
    {
        if (objectOnTable.Value == ulong.MaxValue)
            return ComponentType.Unknown;
        else
            return NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectOnTable.Value].GetComponent<ComponentDescriptor>().type;
    }

    public override bool ResetWorkstation()
    {
        GameObject onTable = GoOnTable;

        interactionProgress.Value = 0;
        interactionPlayersCount.Value = 0;
        currentRecipeDuration.Value = 0;
        currentRecipeResult.Value = ComponentType.Unknown;
        
        if (IsSpawned)
            objectOnTable.Value = ulong.MaxValue;

        if (onTable is not null)
            onTable.GetComponent<NetworkObject>().Despawn();

        return true;
    }

    /// <summary>
    /// Handles all the transform changes onto the table and the reparenting of the object.
    /// </summary>
    /// <param name="gameObjectToPlace">The game object that gets placed on this table.</param>
    private void PlaceGameObjectOnTable(GameObject gameObjectToPlace)
    {
        objectOnTable.Value = gameObjectToPlace.GetComponent<NetworkObject>().NetworkObjectId;
        gameObjectToPlace.transform.parent = transform;
        gameObjectToPlace.transform.localPosition = componentLocation.localPosition;
        gameObjectToPlace.transform.localRotation = Quaternion.identity;
    }
}
