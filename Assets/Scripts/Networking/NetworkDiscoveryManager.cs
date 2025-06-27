using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Events;

public class NetworkDiscoveryManager : MonoBehaviour
{
    private SimpleNetworkDiscovery discovery;
    private static object hasConnectedToFirstServerLock = new object();
    private static bool hasConnectedToFirstServer = false;


    public SimpleNetworkDiscovery.ServerFoundEvent OnServerFound => discovery.OnServerFound;

    void Awake()
    {
        discovery = GetComponent<SimpleNetworkDiscovery>();
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClientWithDiscovery()
    {
        discovery.StartClient();
        discovery.ClientBroadcast(new DiscoveryBroadcastData());
        Debug.Log("Finalized broadcasting.");
    }

    public void ConnectToFirstServer(IPEndPoint arg0, DiscoveryResponseData arg1)
    {
        lock (hasConnectedToFirstServerLock)
        {
            if (hasConnectedToFirstServer)
            {
                Debug.Log($"Skipping broadcast to {arg0.Address} because we answered to another ip address already.");
                return;
            }

            hasConnectedToFirstServer = true;

            Debug.Log($"Received broadcast and trying to connect to server {arg1.ServerName} ({arg0.Address}:{arg1.Port}).");
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = arg0.Address.ToString();
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectPort = arg1.Port;
            NetworkManager.Singleton.StartClient();

            NetworkManager.Singleton.OnClientConnectedCallback += obj =>
            {
                Debug.Log("Connected to server.");
            };
        }
    }

    public void ConnectToServer(IPEndPoint arg0, DiscoveryResponseData arg1)
    {
        Debug.Log($"Connecting to {arg1.ServerName} ({arg0.Address}:{arg1.Port}).");
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = arg0.Address.ToString();
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectPort = arg1.Port;
        NetworkManager.Singleton.StartClient();

        NetworkManager.Singleton.OnClientConnectedCallback += obj =>
        {
            Debug.Log("Connected to server.");
        };
    }
}
