using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class Oven : BaseWorkstation
{
    [SerializeField] private Transform componentLocationInside = null;
    [SerializeField] private Transform componentLocationOutside = null;

    [SerializeField] private NetworkVariable<float> currentRecipeDuration = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<ComponentType> currentRecipeResult = new NetworkVariable<ComponentType>();
    [SerializeField] private NetworkVariable<ulong> objectInOven = new NetworkVariable<ulong>();

    [SerializeField] private NetworkVariable<int> pseudoPlayerCount = new NetworkVariable<int>();

    [SerializeField] private Material barOrange;
    [SerializeField] private Material barDefault;

    [SerializeField] private ParticleSystem smokeParticleSystem;

    private MeshRenderer currentBarRenderer;

    private readonly List<ComponentType> inputComponentsWhitelist = new List<ComponentType>();

    private NetworkVariable<bool> isRunning = new NetworkVariable<bool>();

    private bool IsRunning
    {
        get => isRunning.Value;
        set => isRunning.Value = value;
    }

    private bool isSecondHalf = false;

    private void Awake()
    {
        if (componentLocationInside is null)
            Debug.LogError($"{nameof(componentLocationInside)} variable is not set. This will crash if the table is used.");

        if (componentLocationOutside is null)
            Debug.LogError($"{nameof(componentLocationOutside)} variable is not set. This will crash if the table is used.");

        foreach (var inputType in Enum.GetValues(typeof(ComponentType)).Cast<ComponentType>())
        {
            if (ComponentRecipesManager.Instance.GetInteractionRecipeFor(WorkstationType.Oven, inputType) is not null)
            {
                inputComponentsWhitelist.Add(inputType);
            }
        }

        isRunning.OnValueChanged += IsRunningChanged;
    }

    private void IsRunningChanged(bool previousvalue, bool newvalue)
    {
        if (newvalue)
            smokeParticleSystem.Play();
        else
            smokeParticleSystem.Stop();
    }

    private void Update()
    {
        if (pseudoPlayerCount.Value > 0 && activeProgressBar == null)
        {
            activeProgressBar = ProgressBarScript.SpawnProgressBar(progressBarPrefab, gameObject.transform, interactionProgress, pseudoPlayerCount);

            currentBarRenderer = activeProgressBar.GetComponent<ProgressBarScript>().Bar.GetComponentInChildren<MeshRenderer>();
        }

        if (activeProgressBar != null)
        {
            if (interactionProgress.Value == 0.5f)
                currentBarRenderer.material = barOrange;
            else
                currentBarRenderer.material = barDefault;
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer)
            return;

        if (IsRunning)
        {
            float newProgress = interactionProgress.Value;

            newProgress += Time.fixedDeltaTime / (CurrentRecipe.Item1 / 2.0f);

            if (newProgress > 1.0f)
            {
                newProgress = 0.0f;
                IsRunning = false;
                isSecondHalf = false;
                pseudoPlayerCount.Value = 0;

                GoInOven.GetComponent<NetworkObject>().Despawn();

                GameObject instantiatedObject = Instantiate(ComponentRecipesManager.Instance.GetPrefabOfComponentType(CurrentRecipe.Item2));
                instantiatedObject.GetComponent<NetworkObject>().Spawn(true);
                PlaceGameObjectOutsideOven(instantiatedObject);
                objectInOven.Value = instantiatedObject.GetComponent<NetworkObject>().NetworkObjectId;

                CurrentRecipe = null;
            }
            else if (newProgress > 0.5f && !isSecondHalf)
            {
                newProgress = 0.5f;
                IsRunning = false;
                isSecondHalf = true;
            }

            interactionProgress.Value = newProgress;

        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            objectInOven.Value = ulong.MaxValue;

        base.OnNetworkSpawn();
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

    public GameObject GoInOven
    {
        get
        {
            NetworkObject no;
            return NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectInOven.Value, out no)
                ? no.gameObject
                : null;
        }
    }

    public override bool CanInteract()
    {
        return (CurrentRecipe is not null) && (interactionProgress.Value <= 0.0f || interactionProgress.Value == 0.5f || interactionProgress.Value >= 1.0f);
    }

    public override bool InteractionStart()
    {
        if (CanInteract())
        {
            IsRunning = true;
            pseudoPlayerCount.Value = 1;
        }

        // Return false, we don't want to trigger the interaction animation when using an oven.
        return false;
    }


    public override bool PickupAction(out GameObject gameObjectToPickup, Vector3 targetPosition, Quaternion additionalRotation,
        ulong itemInHand)
    {
        if (objectInOven.Value == ulong.MaxValue || interactionProgress.Value is > 0.0f and < 1.0f)
        {
            gameObjectToPickup = null;
            return false;
        }

        gameObjectToPickup = GoInOven;
        gameObjectToPickup.transform.SetPositionAndRotation(targetPosition, additionalRotation);
        objectInOven.Value = ulong.MaxValue;

        return true;
    }

    public override bool PlaceDownAction(GameObject gameObjectInHand)
    {
        if (gameObjectInHand is null)
            return false;

        if (objectInOven.Value != ulong.MaxValue || !inputComponentsWhitelist.Contains(gameObjectInHand.GetComponent<ComponentDescriptor>().type))
            return false;

        objectInOven.Value = gameObjectInHand.GetComponent<NetworkObject>().NetworkObjectId;
        PlaceGameObjectInsideOven(gameObjectInHand);

        CurrentRecipe = ComponentRecipesManager.Instance.GetInteractionRecipeFor(workstationType,
            objectInOven.Value == ulong.MaxValue
                ? ComponentType.Unknown
                : gameObjectInHand
                    .GetComponent<ComponentDescriptor>().type);

        return true;
    }

    private void PlaceGameObjectInsideOven(GameObject gameObjectToPlace)
    {
        objectInOven.Value = gameObjectToPlace.GetComponent<NetworkObject>().NetworkObjectId;
        gameObjectToPlace.transform.parent = transform;
        gameObjectToPlace.transform.localPosition = componentLocationInside.localPosition;
        gameObjectToPlace.transform.localRotation = Quaternion.identity;
    }

    private void PlaceGameObjectOutsideOven(GameObject gameObjectToPlace)
    {
        objectInOven.Value = gameObjectToPlace.GetComponent<NetworkObject>().NetworkObjectId;
        gameObjectToPlace.transform.parent = transform;
        gameObjectToPlace.transform.localPosition = componentLocationOutside.localPosition;
        gameObjectToPlace.transform.localRotation = Quaternion.identity;
    }

    public override bool ResetWorkstation()
    {
        GameObject goInOven = GoInOven;

        interactionProgress.Value = 0;
        interactionPlayersCount.Value = 0;
        currentRecipeDuration.Value = 0;
        currentRecipeResult.Value = ComponentType.Unknown;

        if (IsSpawned)
            objectInOven.Value = ulong.MaxValue;

        IsRunning = false;
        isSecondHalf = false;

        if (goInOven is not null)
            goInOven.GetComponent<NetworkObject>().Despawn();

        return true;
    }
}
