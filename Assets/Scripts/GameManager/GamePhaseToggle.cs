using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.MessageBox;

public class GamePhaseToggle : NetworkBehaviour
{
    private NetworkVariable<bool> isBuildingPhase = new NetworkVariable<bool>();
    [SerializeField]
    private VisualElement root;
    /// <summary>
    /// If false, game phase is in play phase. Use <see cref="TogglePhase"/> to toggle phases.
    /// </summary>
    public bool IsBuildingPhase => isBuildingPhase.Value;
    [SerializeField]
    private NetworkVariable<bool> canChangePhase = new NetworkVariable<bool>(true);

    private static GamePhaseToggle _instance;

    private Button _playmodeButton;
    private Button _rotateButton;
    private Button _interactionButton;
    private Button _liftButton;
    private Button _pickupButton;

    public static GamePhaseToggle Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GamePhaseToggle>();
            }

            return _instance;
        }
    }

    public override void OnNetworkSpawn()
    {
        isBuildingPhase.OnValueChanged += OnPhaseValueChanged;

        if (IsServer)
            isBuildingPhase.Value = true;
    }

    private void OnPhaseValueChanged(bool previousvalue, bool newvalue)
    {
        if (!IsServer)
           return;

        if (isBuildingPhase.Value)
        {
            // Play phase ==> Build phase
            Debug.Log("Changing from Play phase to Build phase.");
            ChangePlayToBuildPhase();
        }
        else
        {
            // Build phase ==> Play phase
            Debug.Log("Changing from Build phase to Play phase.");
            ChangeBuildToPlayPhase();
        }
    }

    void Start()
    {
        InitializePlayerCameraClientRpc();

        GameObject touchButtons = GameObject.Find("TouchButtons");
        if (touchButtons != null) 
        {
            root = touchButtons.GetComponent<UIDocument>().rootVisualElement;
            _playmodeButton = root.Q<Button>("PlaymodeButton");
           _rotateButton = root.Q<Button>("RotateButton");
           _interactionButton = root.Q<Button>("InteractionButton");
           _liftButton = root.Q<Button>("LiftButton");
           _pickupButton = root.Q<Button>("PickupButton");
            _playmodeButton.RegisterCallback<ClickEvent>((evt) =>
            {
                Debug.Log("Switch to play mode");
                TogglePhaseServerRpc();
               
            });
            if (!canChangePhase.Value)
            {
                _playmodeButton.style.display = DisplayStyle.None;
            }
            else
            {
                _playmodeButton.style.display = DisplayStyle.Flex;
            }
        }

        canChangePhase.OnValueChanged += (value, newValue) =>
        {
            if (!canChangePhase.Value)
            {
                _playmodeButton.style.display = DisplayStyle.None;
            }
            else
            {
                _playmodeButton.style.display = DisplayStyle.Flex;
            }
        };

        isBuildingPhase.OnValueChanged += (value, newValue) =>
        {
            if (newValue)
            {
                _interactionButton.style.display = DisplayStyle.None;
                _rotateButton.style.display = DisplayStyle.Flex;
                _pickupButton.style.display = DisplayStyle.None;
                _liftButton.style.display = DisplayStyle.Flex;
                _playmodeButton.text = "produce";
                if (!canChangePhase.Value)
                {
                    _playmodeButton.style.display = DisplayStyle.None;
                }
                else
                {
                    _playmodeButton.style.display = DisplayStyle.Flex;
                }


            }
            else
            {
                _interactionButton.style.display = DisplayStyle.Flex;
                _rotateButton.style.display = DisplayStyle.None;
                _pickupButton.style.display = DisplayStyle.Flex;
                _liftButton.style.display = DisplayStyle.None;
                _playmodeButton.text = "arrange";
                if (!canChangePhase.Value)
                {
                    _playmodeButton.style.display = DisplayStyle.None;
                }
                else
                {
                    _playmodeButton.style.display = DisplayStyle.Flex;
                }
            }
        };
        Application.targetFrameRate = 144;

    }

    [ClientRpc]
    public void InitializePlayerCameraClientRpc()
    {
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerCamera>().enabled = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePhaseServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TogglePhaseServerRpc()
    { 
        if (!canChangePhase.Value)
        {
            Debug.Log("You tried to change the phase, without permission.");
            return;
        }

        Debug.Log("Received phase toggle command.");
        
        isBuildingPhase.Value = !isBuildingPhase.Value;
     
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisableChangePhaseServerRpc()
    {
        Debug.Log("Disabled Phase Change.");
        canChangePhase.Value = false;
    }
    [ServerRpc(RequireOwnership = false)]
    public void EnableChangePhaseServerRpc()
    {
        Debug.Log("Enabled Phase Change.");
        canChangePhase.Value = true;
    }


    private void ChangePlayToBuildPhase()
    {
        // Play phase ==> Build phase
        var workstations = GameObject.FindObjectsOfType<BaseWorkstation>();
        foreach (var w in workstations)
        {
            if (!w.ResetWorkstation())
                Debug.LogError($"{w} workstation failed to reset.");
        }

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            NetworkObject player = client.PlayerObject;
            player.GetComponent<PlayerActionHandler>().ResetHandsAndInteractingState();
            player.GetComponent<PlayerActionHandler>().enabled = false;
        }

        GridManager.Instance.UnFixGridsClientRpc();
    }

    private void ChangeBuildToPlayPhase()
    {
        // Build phase ==> Play phase
        GridManager.Instance.FixGridServerRpc();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            NetworkObject player = client.PlayerObject;
            player.GetComponent<PlayerActionHandler>().enabled = true;
        }
    }
}
