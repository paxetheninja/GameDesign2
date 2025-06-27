using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HighScore;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// INSTRUCTIONS:

// 1) Remove legacy GamePlayManager from scene
// 2) Add GameplayManager prefab to scene
// 3) Set customerManager of GameplayManager to the one in the scene
// 4) Set the Animation "Grid Phase Warning Text Animation" field on the GridManager object to the 
//    "Build Phase Warning Text" object (child of GameplayManager)

public class GameplayManager : NetworkBehaviour
{
    private enum PlayPhase
    {
        Init,
        BuildPhase,
        PlayPhase,
        Won,
        Lost
    }

    [Serializable]
    public struct DifficultySettings
    {
        public int builtTime;
        public int customerCount;
        public float firstCustomerTime;
        public float timeBetweenCustomers;
        public int customerPatience;
    }

    // difficuly (1,2), player_count (1,2,3,4), DifficultySettings
    private static List<Tuple<int, int, DifficultySettings>> difficultyMap = new()
    {
        new Tuple<int, int, DifficultySettings>(1, 1, new DifficultySettings()
        {
            builtTime = 45,
            customerCount = 8,
            firstCustomerTime = 1,
            timeBetweenCustomers = 30,
            customerPatience = 100
        }),
        new Tuple<int, int, DifficultySettings>(1, 2, new DifficultySettings()
        {
            builtTime = 45,
            customerCount = 8,
            firstCustomerTime = 1,
            timeBetweenCustomers = 25,
            customerPatience = 90
        }),
        new Tuple<int, int, DifficultySettings>(1, 3, new DifficultySettings()
        {
            builtTime = 30,
            customerCount = 8,
            firstCustomerTime = 1,
            timeBetweenCustomers = 20,
            customerPatience = 80
        }),
        new Tuple<int, int, DifficultySettings>(1, 4, new DifficultySettings()
        {
            builtTime = 30,
            customerCount = 8,
            firstCustomerTime = 1,
            timeBetweenCustomers = 15,
            customerPatience = 70
        }),
        new Tuple<int, int, DifficultySettings>(2, 1, new DifficultySettings()
        {
            builtTime = 45,
            customerCount = 12,
            firstCustomerTime = 1,
            timeBetweenCustomers = 35,
            customerPatience = 140
        }),
        new Tuple<int, int, DifficultySettings>(2, 2, new DifficultySettings()
        {
            builtTime = 45,
            customerCount = 12,
            firstCustomerTime = 1,
            timeBetweenCustomers = 30,
            customerPatience = 130
        }),
        new Tuple<int, int, DifficultySettings>(2, 3, new DifficultySettings()
        {
            builtTime = 30,
            customerCount = 12,
            firstCustomerTime = 1,
            timeBetweenCustomers = 25,
            customerPatience = 120
        }),
        new Tuple<int, int, DifficultySettings>(2, 4, new DifficultySettings()
        {
            builtTime = 30,
            customerCount = 12,
            firstCustomerTime = 1,
            timeBetweenCustomers = 25,
            customerPatience = 110
        })
    };

    [SerializeField] private bool startOnSceneLoaded = true;
    [SerializeField] private bool useDifficultyMap = true;
    [SerializeField] private Text timeInfoText;
    
    public AnimationClip spinHoverAnimation;
    public UIManager uiManager;

    public DifficultySettings difficultySettings;
    public CustomerManager customerManager;

    private PlayPhase _playPhase = PlayPhase.Init;
    private float _playTime = 0.0f;
    
    private readonly NetworkVariable<float> _netPlayTime = new();
    private readonly NetworkVariable<int> _netBuildTimeRemaining = new();
    
    public static GameplayManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
        }
        else
        {
            Instance = this;
        }

        //NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
    }

    private void Start()
    {
        if (useDifficultyMap)
        {
            Debug.Assert(PersistentInfoHolder.Instance.Difficulty is 1 or 2);
            Debug.Assert(PersistentInfoHolder.Instance.ConnectedPlayersCount is 1 or 2 or 3 or 4);
            difficultySettings = difficultyMap.FirstOrDefault(a =>
                a.Item1 == PersistentInfoHolder.Instance.Difficulty &&
                a.Item2 == PersistentInfoHolder.Instance.ConnectedPlayersCount)!.Item3;
        }
        
        _netBuildTimeRemaining.OnValueChanged += OnCountdownChanged;

        if (!IsServer) return;
        
        _netBuildTimeRemaining.Value = difficultySettings.builtTime+1;
    }

    public override void OnDestroy()
    {
        //if (NetworkManager.Singleton == null || NetworkManager.Singleton.SceneManager == null) return;
        //NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
    }

    public void OnLoadEventCompleted() //string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
    {
        if (!IsServer || !startOnSceneLoaded)
            return;

        StartGame();
    }

    [ClientRpc]
    private void UpdatePlayPhaseClientRpc(PlayPhase newValue)
    {
        Debug.Log($"Registered play phase change. New value is {newValue}.");
        _playPhase = newValue;
        
        switch (newValue)
        {
            case PlayPhase.BuildPhase:
                if (IsServer)
                {
                    GamePhaseToggle.Instance.DisableChangePhaseServerRpc();
                    InvokeRepeating(nameof(Countdown), 0, 1);
                }
                break;
            case PlayPhase.PlayPhase:
                if (IsServer)
                {
                    GamePhaseToggle.Instance.EnableChangePhaseServerRpc();
                    GamePhaseToggle.Instance.TogglePhaseServerRpc();
                    GamePhaseToggle.Instance.DisableChangePhaseServerRpc();
                    InvokeRepeating(nameof(SpawnCustomer), difficultySettings.firstCustomerTime, difficultySettings.timeBetweenCustomers);
                }
                break;
            case PlayPhase.Won:
                Won();
                break;
            case PlayPhase.Lost:
                Lost();
                break;
        }
    }
    
    private void OnCountdownChanged(int previousValue, int newValue)
    {
        if (newValue < 0)
        {
            timeInfoText.text = "";
        }
        else
        {
            timeInfoText.text = "Build Time: " + newValue;
        }
        
        if (!IsServer) return;

        if (newValue < 0)
        {
            CancelInvoke(nameof(Countdown));
        }
        else if (newValue == 0)
        {
            UpdatePlayPhaseClientRpc(PlayPhase.PlayPhase);
        }
    }

    public void Countdown()
    {
        _netBuildTimeRemaining.Value -= 1;
    }
    
    public void StartGame()
    {
        Debug.Assert(IsServer);
        UpdatePlayPhaseClientRpc(PlayPhase.BuildPhase);
    }
    
    public void RanOutOfPatience()
    {
        UpdatePlayPhaseClientRpc(PlayPhase.Lost);
    }

    private void Won()
    {
        // TODO: show win ui
        uiManager.ShowWin();

        if (IsServer)
            CancelInvoke(nameof(SpawnCustomer));

        HighScoreManager.SaveHighScoreToFile(_playTime);
    }

    private void Lost()
    {
        // TODO: show lost ui
        uiManager.ShowLose();

        if (IsServer)
            CancelInvoke(nameof(SpawnCustomer));
    }
    
    public void SpawnCustomer()
    {
        if (!IsServer) return;
        
        if (difficultySettings.customerCount > 0)
        {
            customerManager.SpawnNextCustomer(difficultySettings.customerPatience);
            difficultySettings.customerCount--;
        }
        else
        {
            CancelInvoke(nameof(SpawnCustomer));
        }
    }

    void Update()
    {
        if (IsServer)
        {
            _netPlayTime.Value = _playTime;
        }
        else
        {
            _playTime = _netPlayTime.Value;
        }

        if (_playPhase is PlayPhase.Won or PlayPhase.Lost) return;

        switch (_playPhase)
        {
            case PlayPhase.PlayPhase:
                if (IsServer)
                {
                    _playTime += Time.deltaTime;

                    if (difficultySettings.customerCount <= 0 && customerManager.CurrentCustomerCount <= 0)
                        UpdatePlayPhaseClientRpc(PlayPhase.Won);
                }

                timeInfoText.text = "Play Time: " + Math.Round(_playTime, 1).ToString("0.0");
                break;
        }
    }
}
