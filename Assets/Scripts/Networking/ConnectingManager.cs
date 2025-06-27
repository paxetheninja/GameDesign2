using System;
using System.Collections;
using System.Threading.Tasks;
using HighScore;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Provides public methods to use for the UI buttons during the startup scene, e.g. startHost, findServers ...
/// </summary>
[DefaultExecutionOrder(100)]
public class ConnectingManager : NetworkBehaviour
{
    private int m_MaxConnections = 4;
    [SerializeField]
    UIStartMenu _UIStartMenu;

    private void Awake()
    {
        Example_AuthenticatingAPlayer();

        NetworkManager.Singleton.OnClientConnectedCallback += UpdateConnectedClientsConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += UpdateConnectedClientsDisconnected;

        _UIStartMenu = FindAnyObjectByType<UIStartMenu>();
    }

    public void LoadCuratedLevel1()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("CuratedLevel1", LoadSceneMode.Single);
        //InitializePlayerCameraClientRpc();
        GetComponent<NetworkObject>().Despawn();
    }

    public void LoadCuratedLevel2()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("CuratedLevel2", LoadSceneMode.Single);
        //InitializePlayerCameraClientRpc();
        GetComponent<NetworkObject>().Despawn();
    }

    public void LoadRandomLevel()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("GeneratedLevel", LoadSceneMode.Single);
        //InitializePlayerCameraClientRpc();
        GetComponent<NetworkObject>().Despawn();
    }
    
    public void UpdateLobbyInformation() => UpdateLobbyInformation("", "");

    public void UpdateLobbyInformation(NetworkString oldValue, NetworkString newValue)
    {
        string text = "";
        text += PersistentInfoHolder.Instance.LobbyCode + Environment.NewLine;

        foreach (var networkClient in NetworkManager.Singleton.ConnectedClientsList)
        {
            text += networkClient.PlayerObject.GetComponent<NetworkedPlayerName>().Name.Value + Environment.NewLine;
        }
        UpdateLobbyInformationVisualsClientRpc(text);
        UpdateUILobbyInfoClientRpc(NetworkManager.Singleton.ConnectedClientsList.Count);
    }

    private void UpdateConnectedClientsConnected(ulong newId) => UpdateConnectedClients(newId, true);
    private void UpdateConnectedClientsDisconnected(ulong newId) => UpdateConnectedClients(newId, false);

    private void UpdateConnectedClients(ulong newId, bool isConnected)
    {
        if (!IsServer)
            return;

        if (isConnected)
            NetworkManager.Singleton.ConnectedClients[newId].PlayerObject.GetComponent<NetworkedPlayerName>().Name.OnValueChanged += UpdateLobbyInformation;
        else
        {
            // This instance might be disposed already. Check for this.
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(newId, out var disconnectedClient))
                disconnectedClient.PlayerObject.GetComponent<NetworkedPlayerName>().Name.OnValueChanged -= UpdateLobbyInformation;
        }

        UpdateLobbyInformation();
    }

    [ClientRpc]
    private void UpdateLobbyInformationVisualsClientRpc(string newText)
    {
        _UIStartMenu.UpdateLobbyScreen();
    }

    [ClientRpc]
    private void UpdateUILobbyInfoClientRpc(int playersConnected)
    {
        PersistentInfoHolder.Instance.ConnectedPlayersCount = playersConnected;
    }

    [ClientRpc]
    public void InitializePlayerCameraClientRpc()
    {
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerCamera>().enabled = true;
    }

    public async Task<RelayServerData> AllocateRelayServerAndGetJoinCode(int maxConnections, string region = null)
    {
        Allocation allocation;

        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay create allocation request failed {e.Message}");
            throw;
        }

        Debug.Log($"server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"server: {allocation.AllocationId}");

        try
        {
            PersistentInfoHolder.Instance.LobbyCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }

        return new RelayServerData(allocation, "dtls");
    }

    public IEnumerator Example_ConfigureTransportAndStartNgoAsHostAndStartTutorial(int tutorial)
    {
        var serverRelayUtilityTask = AllocateRelayServerAndGetJoinCode(m_MaxConnections);
        while (!serverRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }
        if (serverRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to start Relay Server. Server not started. Exception: " + serverRelayUtilityTask.Exception.Message);
            yield break;
        }

        var relayServerData = serverRelayUtilityTask.Result;

        // Display the joinCode to the user.

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();

        FindAnyObjectByType<UIStartMenu>().DistributePlayerColors();

        // load tutorial within the coroutine
        switch (tutorial)
        {
            case 1:
                NetworkManager.Singleton.SceneManager.LoadScene("Tutorial1", LoadSceneMode.Single);
                Debug.Log("New Scene Loaded");
                break;
            case 2:
                NetworkManager.Singleton.SceneManager.LoadScene("Tutorial2", LoadSceneMode.Single);
                break;
            case 3:
                NetworkManager.Singleton.SceneManager.LoadScene("Tutorial3", LoadSceneMode.Single);
                break;
            default:
                NetworkManager.Singleton.SceneManager.LoadScene("Tutorial1", LoadSceneMode.Single);
                break;
        }

        //InitializePlayerCameraClientRpc();
        GetComponent<NetworkObject>().Despawn();
        yield return null;
    }

    public IEnumerator Example_ConfigureTransportAndStartNgoAsHost()
    {
        var serverRelayUtilityTask = AllocateRelayServerAndGetJoinCode(m_MaxConnections);
        while (!serverRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }
        if (serverRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to start Relay Server. Server not started. Exception: " + serverRelayUtilityTask.Exception.Message);
            yield break;
        }

        var relayServerData = serverRelayUtilityTask.Result;

        // Display the joinCode to the user.
        Debug.Log("Before SetRelayServerData");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();
        UpdateLobbyInformation();
        yield return null;
    }

    public async Task<RelayServerData> JoinRelayServerFromJoinCode(string joinCode)
    {
        JoinAllocation allocation;
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch
        {
            Debug.Log("Error: Relay create join code request failed");
            _UIStartMenu.JoinLobbyFailed("wrong ID");
            throw;
        }

        Debug.Log($"client: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"host: {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
        Debug.Log($"client: {allocation.AllocationId}");

        return new RelayServerData(allocation, "dtls");
    }

    public IEnumerator Example_ConfigureTransportAndStartNgoAsConnectingPlayer(string joinCode)
    {
        // Populate RelayJoinCode beforehand through the UI
        var clientRelayUtilityTask = JoinRelayServerFromJoinCode(joinCode);

        while (!clientRelayUtilityTask.IsCompleted)
        {
            yield return null;
        }

        if (clientRelayUtilityTask.IsFaulted)
        {
            Debug.Log("Exception thrown when attempting to connect to Relay Server. Exception: " + clientRelayUtilityTask.Exception.Message);
            _UIStartMenu.JoinLobbyFailed("unknown");
            yield break;
        }

        var relayServerData = clientRelayUtilityTask.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();

        PersistentInfoHolder.Instance.LobbyCode = joinCode;
        yield return null;
    }

    async void Example_AuthenticatingAPlayer()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            var playerID = AuthenticationService.Instance.PlayerId;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton is not null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= UpdateConnectedClientsConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= UpdateConnectedClientsDisconnected;
        }
    }
}
