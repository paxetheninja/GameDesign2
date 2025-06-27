using System;
using UnityEngine;

// This instance will persist between scene loadings and is accessible as singleton.
public class PersistentInfoHolder : MonoBehaviour
{
    public string LocalPlayerName = "defaultLocalPlayerName";

    public string LobbyCode = String.Empty;

    public int ConnectedPlayersCount = 0;
    
    public int Seed = -1;
    
    public int Difficulty = 1;

    public static PersistentInfoHolder Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }
}
