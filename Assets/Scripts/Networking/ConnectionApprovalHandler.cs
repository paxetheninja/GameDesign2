using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionApprovalHandler : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnOnClientDisconnectCallback;
    }

    private void OnOnClientDisconnectCallback(ulong obj)
    {
        if (NetworkManager.Singleton.IsServer)
            return;

        Debug.Log($"Disconnected. Reason: {NetworkManager.Singleton.DisconnectReason}");

        var uiStartMenu = FindAnyObjectByType<UIStartMenu>();

        if (uiStartMenu is null || uiStartMenu.IsDestroyed())
        {
            // This case happens if the server disconnects unexpectedly. Handle the scene load.
            SceneManager.LoadScene("Startup");
        }
        else
        {
            uiStartMenu.JoinLobbyFailed(NetworkManager.Singleton.DisconnectReason);
        }

    }

    private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.CreatePlayerObject = true;
        response.PlayerPrefabHash = null;

        if (SceneManager.GetActiveScene().name == "Startup")
        {
            response.Approved = true;
        }
        else
        {
            response.Approved = false;
            response.Reason = "denied";

            Debug.Log($"Client with id {request.ClientNetworkId} tried to connect while ingame. Connection request denied.");
        }
    }
}
