using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PlayerActionHandler : NetworkBehaviour
{
    private float maxInteractionRange = 1.0f;

    private Vector3 verticalOffset = new Vector3(0, 1f, 0);

    public KeyCode interactionButton = KeyCode.F;
    public KeyCode pickupButton = KeyCode.E;
    public LayerMask workstationLayers;

    [Tooltip("Hit checks for workstations are done in an arc. 30° for example does one check at -15°, 0° and 15° in front.")]
    public float degreeOfInteractionArc;

    [SerializeField]
    private Button _pickupButtonMobile = null;
    [SerializeField]
    private VisualElement root;

    // ulong.MaxValue is used as "null"
    public NetworkVariable<ulong> currentGameObjectInHand = new NetworkVariable<ulong>();

    public GameObject GoInHand
    {
        get
        {
            NetworkObject no;
            return NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(currentGameObjectInHand.Value, out no)
                ? no.gameObject
                : null;
        }
    }

    private NetworkVariable<bool> isInteracting = new NetworkVariable<bool>();

    public Transform componentHoldingPosition;

    private Animator animator;

    private BaseWorkstation workStationInFront = null;
    private BaseWorkstation workStationInFrontOld = null;
    public GameObject touchButtons;
    private void Start()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        touchButtons = null;
        animator = GetComponent<Animator>();
        InvokeRepeating(nameof(ForceRegisterUI),1.5f,2f );


    }

    private void OnLoadEventCompleted(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
    {
        if (string.Equals(scenename, "startup", StringComparison.OrdinalIgnoreCase))
            return;
        
        touchButtons = null;
        InvokeRepeating(nameof(ForceRegisterUI), 1.5f, 2f);
    }

    public void ForceRegisterUI()
    {
        if (touchButtons != null)
        {
            CancelInvoke(nameof(ForceRegisterUI));
            return;
        }

        StartCoroutine(RegisterTouchButtons(0));
    }

    IEnumerator RegisterTouchButtons(float time)
    {
        
        yield return new WaitForSeconds(time);
        touchButtons = GameObject.Find("TouchButtons");
        if (touchButtons != null)
        {
            root = touchButtons.GetComponent<UIDocument>().rootVisualElement;
            //root = FindObjectOfType<UIDocument>().rootVisualElement;
            Button buttonInteraction = root.Q<Button>("InteractionButton");
            Button pickupButton = root.Q<Button>("PickupButton");
            buttonInteraction.RegisterCallback<PointerDownEvent>((evt) =>
            {
                Debug.Log("interaction started");
                if (IsOwner)
                {

                    DoInteractionStartedServerRpc();
                }
            }, TrickleDown.TrickleDown);

            buttonInteraction.RegisterCallback<PointerUpEvent>((evt) =>
            {
                if (IsOwner)
                {
                    Debug.Log("interaction stopped!");
                    DoInteractionStoppedServerRpc();
                }
            });

            pickupButton.RegisterCallback<ClickEvent>((evt) =>
            {
                if (IsOwner)
                {
                    Debug.Log(currentGameObjectInHand.Value);
                    DoActionServerRpc(NetworkManager.LocalClientId);
                }
            });
        }
    }

    private void Update()
    {
        if (GamePhaseToggle.Instance is null || GamePhaseToggle.Instance.IsBuildingPhase)
            return;

        workStationInFront = GetWorkstation();

        if (Input.GetKeyDown(interactionButton) && IsOwner)
        {
            DoInteractionStartedServerRpc();
        }
        else if (Input.GetKeyUp(interactionButton) && IsOwner)
        {
            DoInteractionStoppedServerRpc();
        }

        if (Input.GetKeyDown(pickupButton) && IsOwner)
        {
            DoActionServerRpc(NetworkManager.LocalClientId);
        }

        if (isInteracting.Value && (workStationInFrontOld != workStationInFront || (workStationInFrontOld is not null && !workStationInFrontOld.CanInteract())))
        {
            DoInteractionStoppedServerRpc();
        }

        workStationInFrontOld = workStationInFront;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentGameObjectInHand.Value = ulong.MaxValue;
        }

        isInteracting.OnValueChanged += OnIsInteractingChanged;
    }

    public override void OnNetworkDespawn()
    {
        isInteracting.OnValueChanged -= OnIsInteractingChanged;
    }

    public void ResetHandsAndInteractingState()
    {
        var g = GoInHand;
        if (g is not null)
            g.GetComponent<NetworkObject>().Despawn();

        currentGameObjectInHand.Value = ulong.MaxValue;

        isInteracting.Value = false;
    }

    private void OnIsInteractingChanged(bool previousvalue, bool newvalue)
    {
        animator.SetBool("IsInteracting", newvalue);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DoActionServerRpc(ulong senderClientId)
    {
        BaseWorkstation workstation = GetWorkstation();
        if (workstation is null)
            return;

        ulong itemInHand = NetworkManager.ConnectedClients[senderClientId].PlayerObject.GetComponent<PlayerActionHandler>().currentGameObjectInHand.Value;

        Tuple<ComponentType, ComponentType> pickupDefinition = ComponentRecipesManager.Instance.GetPickupDefinitionFor(workstation.GetCurrentItemType(),
            NetworkIdToComponentType(itemInHand));

        if (pickupDefinition is not null || itemInHand == ulong.MaxValue)
            DoPickupAction(workstation, itemInHand);
        else
            DoPlaceAction(workstation);
    }

    
    private void DoPickupAction(BaseWorkstation workstation, ulong itemInHand)
    {
        // Do the pickup button action
        GameObject pickedUpGameObject;

        if (workstation.PickupAction(out pickedUpGameObject, componentHoldingPosition.position, transform.rotation, itemInHand))
        {
            pickedUpGameObject.transform.parent = gameObject.transform;
            currentGameObjectInHand.Value = pickedUpGameObject.GetComponent<NetworkObject>().NetworkObjectId;
        }
    }

    private void DoPlaceAction(BaseWorkstation workstation)
    {
        // Do the place down action
        if (workstation.PlaceDownAction(NetworkIdToGameObject(currentGameObjectInHand.Value)))
        {
            currentGameObjectInHand.Value = ulong.MaxValue;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DoInteractionStartedServerRpc()
    {
        if (workStationInFront is null || !workStationInFront.CanInteract())
            return;

        if (workStationInFront.InteractionStart())
        {
            isInteracting.Value = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DoInteractionStoppedServerRpc()
    {
        if (workStationInFrontOld is null || !isInteracting.Value)
            return;

        if (workStationInFrontOld.InteractionEnd())
        {
            isInteracting.Value = false;
        }
    }

    private BaseWorkstation GetWorkstation()
    {
        RaycastHit hit;

        // Note: If this function returns false when it shouldn't, check if the workstation has the correct layer assigned.
        if (RaycastForWorkstations(out hit))
            return hit.transform.GetComponent<BaseWorkstation>();
        else
            return null;
    }

    private bool RaycastForWorkstations(out RaycastHit hit)
    {
        // Note: If this function returns false when it shouldn't, check if the workstation has the correct layer assigned.
        RaycastHit hitLeft, hitMid, hitRight;

        if (Physics.Raycast(transform.position + verticalOffset, transform.rotation * Vector3.forward, out hitMid,
                maxInteractionRange, workstationLayers))
        {
            hit = hitMid;
            return true;
        }
        else if (Physics.Raycast(transform.position + verticalOffset, Quaternion.Euler(0, degreeOfInteractionArc / -2.0f, 0) * transform.rotation * Vector3.forward, out hitLeft,
                     maxInteractionRange, workstationLayers))
        {
            hit = hitLeft;
            return true;
        }
        else if (Physics.Raycast(transform.position + verticalOffset, Quaternion.Euler(0, degreeOfInteractionArc / 2.0f, 0) * transform.rotation * Vector3.forward, out hitRight,
                     maxInteractionRange, workstationLayers))
        {
            hit = hitRight;
            return true;
        }

        hit = new RaycastHit();
        return false;
    }

    private ComponentType NetworkIdToComponentType(ulong id)
    {
        var go = NetworkIdToGameObject(id);

        if (go is not null)
            return go.GetComponent<ComponentDescriptor>().type;
        else
            return ComponentType.Unknown;

    }

    private GameObject NetworkIdToGameObject(ulong id)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var result))
            return result.gameObject;
        else
            return null;
    }

    private void OnDrawGizmos()
    {
        // Middle
        Gizmos.DrawLine(transform.position + verticalOffset, transform.position + verticalOffset + transform.rotation * (maxInteractionRange * Vector3.forward));

        // Left
        Gizmos.DrawLine(transform.position + verticalOffset, transform.position + verticalOffset + Quaternion.Euler(0, degreeOfInteractionArc / -2.0f, 0) * transform.rotation * (maxInteractionRange * Vector3.forward));

        // Right
        Gizmos.DrawLine(transform.position + verticalOffset, transform.position + verticalOffset + Quaternion.Euler(0, degreeOfInteractionArc / 2.0f, 0) * transform.rotation * (maxInteractionRange * Vector3.forward));
    }
}
