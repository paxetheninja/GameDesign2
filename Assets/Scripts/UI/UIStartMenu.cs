using System;
using System.Collections;
using System.Collections.Generic;
using HighScore;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class UIStartMenu : NetworkBehaviour
{

    [SerializeField]
    UIDocument _startMenuUI;
    [SerializeField]
    UIDocument _lobbyMenuUI;
    [SerializeField]
    UIDocument _joinGameUI;
    [SerializeField]
    UIDocument _tutorialSelectionMenuUI;
    [SerializeField]
    UIDocument _loadingLobbyUI;
    [SerializeField]
    ConnectingManager _connectingManager;

    [SerializeField]
    string _playerName;
    

    [SerializeField]
    bool _isHost;

    Label _labelPlayerName;
    Label _labelHostID;
    TextField _inputHostIDpassive;
    Label _labelPlayersConnected;
    TextField _inputHostID;

    Button _buttonStartTutorial1;
    Button _buttonStartTutorial2;
    Button _buttonStartTutorial3;
    Button _buttonBackTutorial;

    Button _buttonLevel1;
    Button _buttonLevel2;
    Button _buttonRandom;
    Button _buttonEasy;
    Button _buttonHard;
    TextField _inputSeed;
    VisualElement _sectionRandomSettings;

    ProgressBar _loadingBar;
    [SerializeField]
    float _loadingProgress = 0;

    // TODO: make _hostID, _chosenLevel and _connectedPlayer network variables to update the screen in the lobby

    public string _hostID;
    
    public NetworkVariable<int> _chosenLevel = new();

    bool _difficultyEasy = true;

    Label _labelJoinPlayerName;
    Label _labelJoinHostID;
    Label _labelJoinPlayersConnected;
    Button _buttonJoinLevel1;
    Button _buttonJoinLevel2;
    Button _buttonJoinRandom;

    
    void Awake()
    {
        _connectingManager = FindObjectOfType<ConnectingManager>();

        // ======================== Start UI ==============================

        _chosenLevel.OnValueChanged += (old, old2) => {
            UpdatePassiveToggleButtons();
            };
        var rootStartMenu = _startMenuUI.rootVisualElement;
        TextField playerNameField = rootStartMenu.Q<TextField>("InputPlayerName");
        if (PersistentInfoHolder.Instance != null && PersistentInfoHolder.Instance.LocalPlayerName != "defaultLocalPlayerName")
            playerNameField.value = PersistentInfoHolder.Instance.LocalPlayerName;
        else
            playerNameField.value = RandomName();

        Button buttonStartTutorial = rootStartMenu.Q<Button>("ButtonStartTutorial");
        buttonStartTutorial.RegisterCallback<ClickEvent>((evt) =>
        {
            PersistentInfoHolder.Instance.LocalPlayerName = playerNameField.value;
            _playerName = playerNameField.value;
            StartTutorial();
        });
        Button buttonStartHosting = rootStartMenu.Q<Button>("ButtonStartHosting");
        buttonStartHosting.RegisterCallback<ClickEvent>((evt) =>
        {
            PersistentInfoHolder.Instance.LocalPlayerName = playerNameField.value;
            _playerName = playerNameField.value;
            StartHosting();
        });
        _inputHostID = rootStartMenu.Q<TextField>("InputHostID");

        Button buttonJoinGame = rootStartMenu.Q<Button>("ButtonJoinGame");
        buttonJoinGame.RegisterCallback<ClickEvent>((evt) =>
        {
            _hostID = _inputHostID.value;
            PersistentInfoHolder.Instance.LobbyCode = _hostID;
            PersistentInfoHolder.Instance.LocalPlayerName = playerNameField.value;
            _playerName = playerNameField.value;
            JoinGame();
        });

        // ======================== Lobby UI ==============================
        var rootLobbyMenu = _lobbyMenuUI.rootVisualElement;
        rootLobbyMenu.style.display = DisplayStyle.None;
        _labelPlayerName = rootLobbyMenu.Q<Label>("LabelPlayerName");
        _labelHostID = rootLobbyMenu.Q<Label>("LabelHostID");
        _inputHostIDpassive = rootLobbyMenu.Q<TextField>("InputHostID");
        _labelPlayersConnected = rootLobbyMenu.Q<Label>("LabelPlayersConnected");
        _labelPlayerName.text = "name: " + _playerName;
        _inputHostIDpassive.value = _hostID;
        _labelPlayersConnected.text = "0";
        _sectionRandomSettings = rootLobbyMenu.Q<VisualElement>("SectionRandomSettings");
        _buttonLevel1 = rootLobbyMenu.Q<Button>("ButtonLevel1");
        _buttonLevel1.RegisterCallback<ClickEvent>((evt) =>
        {   
            _chosenLevel.Value = 1;
            UpdateToggleButtons();
        });
        _buttonLevel2 = rootLobbyMenu.Q<Button>("ButtonLevel2");
        _buttonLevel2.RegisterCallback<ClickEvent>((evt) =>
        {
            _chosenLevel.Value = 2;
            UpdateToggleButtons();
        });
        _buttonRandom = rootLobbyMenu.Q<Button>("ButtonRandom");
        _buttonRandom.RegisterCallback<ClickEvent>((evt) =>
        {
            _chosenLevel.Value = 0;
            UpdateToggleButtons();
        });
        _inputSeed = rootLobbyMenu.Q<TextField>("InputSeed");
        _inputSeed.value = Random.Range(0, 999999).ToString();

        _buttonEasy = rootLobbyMenu.Q<Button>("ButtonEasy");
        _buttonEasy.RegisterCallback<ClickEvent>((evt) =>
        {
            _difficultyEasy = true;
            if (_buttonHard.ClassListContains("ButtonLevelSelected"))
            {
                _buttonHard.RemoveFromClassList("ButtonLevelSelected");
            }
            if (!_buttonEasy.ClassListContains("ButtonLevelSelected"))
            {
                _buttonEasy.AddToClassList("ButtonLevelSelected");
            }
        });

        _buttonHard = rootLobbyMenu.Q<Button>("ButtonHard");
        _buttonHard.RegisterCallback<ClickEvent>((evt) =>
        {
            _difficultyEasy = false;
            if (_buttonEasy.ClassListContains("ButtonLevelSelected"))
            {
                _buttonEasy.RemoveFromClassList("ButtonLevelSelected");
            }
            if (!_buttonHard.ClassListContains("ButtonLevelSelected"))
            {
                _buttonHard.AddToClassList("ButtonLevelSelected");
            }
        });

        Button buttonStartGame = rootLobbyMenu.Q<Button>("ButtonStartGame");
        buttonStartGame.RegisterCallback<ClickEvent>((evt) =>
        {
            StartGame();
        });

        Button buttonLeaveLobby = rootLobbyMenu.Q<Button>("ButtonLeaveLobby");
        buttonLeaveLobby.RegisterCallback<ClickEvent>((evt) =>
        {
            NetworkManager.Singleton.Shutdown();
            _startMenuUI.rootVisualElement.style.display = DisplayStyle.Flex;
            _lobbyMenuUI.rootVisualElement.style.display = DisplayStyle.None;
            _isHost = true;
        });

        // ======================== Join Game UI ==============================
        var rootJoinGameMenu = _joinGameUI.rootVisualElement;
        rootJoinGameMenu.style.display = DisplayStyle.None;
        _labelJoinPlayerName = rootJoinGameMenu.Q<Label>("LabelPlayerName");
        _labelJoinHostID = rootJoinGameMenu.Q<Label>("LabelHostID");
        _labelJoinPlayersConnected = rootJoinGameMenu.Q<Label>("LabelPlayersConnected");
        _buttonJoinLevel1 = rootJoinGameMenu.Q<Button>("ButtonLevel1");
        _buttonJoinLevel2 = rootJoinGameMenu.Q<Button>("ButtonLevel2");
        _buttonJoinRandom = rootJoinGameMenu.Q<Button>("ButtonRandom");
        Button buttonLeaveJoinedLobby = rootJoinGameMenu.Q<Button>("ButtonLeaveLobby");
        buttonLeaveJoinedLobby.RegisterCallback<ClickEvent>((evt) =>
        {
            NetworkManager.Singleton.Shutdown();
            _startMenuUI.rootVisualElement.style.display = DisplayStyle.Flex;
            _joinGameUI.rootVisualElement.style.display = DisplayStyle.None;
        });


        // ======================== SelectTutorial UI ==============================
        var rootTutorialSelectionMenuUI = _tutorialSelectionMenuUI.rootVisualElement;
        rootTutorialSelectionMenuUI.style.display = DisplayStyle.None;
        _buttonStartTutorial1 = rootTutorialSelectionMenuUI.Q<Button>("ButtonStartTutorial1");
        _buttonStartTutorial1.RegisterCallback<ClickEvent>((evt) =>
        {
            StartCoroutine(_connectingManager.Example_ConfigureTransportAndStartNgoAsHostAndStartTutorial(1));
            _tutorialSelectionMenuUI.rootVisualElement.style.display = DisplayStyle.None;
            _loadingLobbyUI.rootVisualElement.style.display = DisplayStyle.Flex;
            StartCoroutine(LoadJoinLobby(0.02f));
        });
        _buttonStartTutorial2 = rootTutorialSelectionMenuUI.Q<Button>("ButtonStartTutorial2");
        _buttonStartTutorial2.RegisterCallback<ClickEvent>((evt) =>
        {
            StartCoroutine(_connectingManager.Example_ConfigureTransportAndStartNgoAsHostAndStartTutorial(2));
            _tutorialSelectionMenuUI.rootVisualElement.style.display = DisplayStyle.None;
            _loadingLobbyUI.rootVisualElement.style.display = DisplayStyle.Flex;
            StartCoroutine(LoadJoinLobby(0.02f));
        });
        _buttonStartTutorial3 = rootTutorialSelectionMenuUI.Q<Button>("ButtonStartTutorial3");
        _buttonStartTutorial3.RegisterCallback<ClickEvent>((evt) =>
        {
            StartCoroutine(_connectingManager.Example_ConfigureTransportAndStartNgoAsHostAndStartTutorial(3));
            _tutorialSelectionMenuUI.rootVisualElement.style.display = DisplayStyle.None;
            _loadingLobbyUI.rootVisualElement.style.display = DisplayStyle.Flex;
            StartCoroutine(LoadJoinLobby(0.02f));
        });

        _buttonBackTutorial = rootTutorialSelectionMenuUI.Q<Button>("ButtonBack");
        _buttonBackTutorial.RegisterCallback<ClickEvent>((evt) =>
        {
            _tutorialSelectionMenuUI.rootVisualElement.style.display = DisplayStyle.None;
            _startMenuUI.rootVisualElement.style.display = DisplayStyle.Flex;
    
        });

        // ======================== LoadingLobby UI ==============================
        var rootLoadingLobbyUI = _loadingLobbyUI.rootVisualElement;
        rootLoadingLobbyUI.style.display = DisplayStyle.None;

        _loadingBar = rootLoadingLobbyUI.Q<ProgressBar>("LoadingBar");

    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += SetLoadingCompleted;
        
        // This case happens if we were ingame but returned to menu. Because we stay connected with each other, we want to show the lobby directly.
        if (IsServer || IsClient)
        {
            _startMenuUI.rootVisualElement.style.display = DisplayStyle.None;
            _isHost = IsServer;
            _labelPlayerName.text = PersistentInfoHolder.Instance.LocalPlayerName;

            if (IsServer)
            {
                _connectingManager.UpdateLobbyInformation();
                _lobbyMenuUI.rootVisualElement.style.display = DisplayStyle.Flex;
            }
            else
            {
                UpdatePassiveToggleButtons();
                _joinGameUI.rootVisualElement.style.display = DisplayStyle.Flex;
            }
        }
    }

   
        
    IEnumerator LoadLobby(float time)
    {
        _loadingProgress = 0;
        while (_loadingProgress < 0.9)
        {
            _loadingProgress += 0.01f;
            _loadingBar.value = _loadingProgress;
            yield return new WaitForSeconds(time); // Delay for 1 second
        }
    }

    IEnumerator LoadJoinLobby(float time)
    {
        _loadingProgress = 0;
        while (_loadingProgress < 0.9)
        {
            _loadingProgress += 0.01f;
            _loadingBar.value = _loadingProgress;
            yield return new WaitForSeconds(time); // Delay for 1 second
        }
    }
    void SetLoadingCompleted(ulong clientId)
    {
        Debug.Log("Connection Completed, OnClientConnectedCallback triggerd");
        _loadingProgress = 1;

        if (IsServer)
        {
            _loadingLobbyUI.rootVisualElement.style.display = DisplayStyle.None;
            _lobbyMenuUI.rootVisualElement.style.display = DisplayStyle.Flex;
        }
        else
        {
            _loadingLobbyUI.rootVisualElement.style.display = DisplayStyle.None;
            _joinGameUI.rootVisualElement.style.display = DisplayStyle.Flex;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            _chosenLevel.Value = 1;
    }

    void StartGame()
    {
        DistributePlayerColors();

        switch (_chosenLevel.Value)
        {
            case 0: // random
                Debug.Log("Start random game with seed: " + _inputSeed.value);
                int.TryParse(_inputSeed.value, out var seed);
                PersistentInfoHolder.Instance.Seed = seed == 0 ? -1 : seed;
                PersistentInfoHolder.Instance.Difficulty = _difficultyEasy ? 1 : 2;
                HighScoreManager.Instance.currentLevelName = "" + seed;
                _connectingManager.LoadRandomLevel();
                break;
            case 1: //start level 1
                Debug.Log("Load Level 1");
                HighScoreManager.Instance.currentLevelName = "Level 1";
                _connectingManager.LoadCuratedLevel1();
                PersistentInfoHolder.Instance.Difficulty = 1;
                break;
            case 2: //start level 2
                Debug.Log("Load Level 2");
                HighScoreManager.Instance.currentLevelName = "Level 2";
                _connectingManager.LoadCuratedLevel2();
                PersistentInfoHolder.Instance.Difficulty = 2;
                break;
        }
        // scene change
        // use _playerName ?
        // is random 
    }

    public void DistributePlayerColors()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsList;

        for (int i = 0; i < clients.Count; i++)
        {
            clients[i].PlayerObject.GetComponent<NetworkedMaterialAssigner>().SetColorClientRpc(NetworkedMaterialAssigner.PlayerColors[i]);
        }
    }

    void UpdateToggleButtons()
    {
        switch (_chosenLevel.Value)
        {
            case 0:
                DeactivateToggleButtons(new List<Button> { _buttonLevel1, _buttonLevel2, _buttonRandom });
                _sectionRandomSettings.style.display = DisplayStyle.Flex;
                _buttonRandom.AddToClassList("ButtonLevelSelected");
                break;
            case 1:
                DeactivateToggleButtons(new List<Button> { _buttonLevel1, _buttonLevel2, _buttonRandom });
                _sectionRandomSettings.style.display = DisplayStyle.None;
                _buttonLevel1.AddToClassList("ButtonLevelSelected");
                break;

            case 2:
                DeactivateToggleButtons(new List<Button> { _buttonLevel1, _buttonLevel2, _buttonRandom});
                _sectionRandomSettings.style.display = DisplayStyle.None;
                _buttonLevel2.AddToClassList("ButtonLevelSelected");
                break;
        }
    }


    
    void DeactivateToggleButtons(List<Button> toggleButtons)
    {
        for(int i = 0; i < toggleButtons.Count; i++)
        {
            if (toggleButtons[i].ClassListContains("ButtonLevelSelected"))
            {
                toggleButtons[i].RemoveFromClassList("ButtonLevelSelected");
            }
        }
    }


    // =============== use functions below for integration ===============================
    public void UpdateHostID()
    {
        _inputHostIDpassive.value = PersistentInfoHolder.Instance.LobbyCode;
        _labelHostID.text = PersistentInfoHolder.Instance.LobbyCode;
        _labelJoinHostID.text = PersistentInfoHolder.Instance.LobbyCode;
    }

    public void UpdateLocalPlayerName()
    {
        _labelPlayerName.text = PersistentInfoHolder.Instance.LocalPlayerName;
        _labelJoinPlayerName.text = PersistentInfoHolder.Instance.LocalPlayerName;
    }
    
    public void UpdatePlayersConnected()
    {
        _labelPlayersConnected.text = PersistentInfoHolder.Instance.ConnectedPlayersCount.ToString();
        _labelJoinPlayersConnected.text = PersistentInfoHolder.Instance.ConnectedPlayersCount.ToString();
    }
    
    public void UpdatePassiveToggleButtons()
    {
        switch (_chosenLevel.Value)
        {
            case 0:
                DeactivateToggleButtons(new List<Button> { _buttonJoinLevel1, _buttonJoinLevel2, _buttonJoinRandom });
                _buttonJoinRandom.AddToClassList("ButtonLevelSelected");
                break;
            case 1:
                DeactivateToggleButtons(new List<Button> { _buttonJoinLevel1, _buttonJoinLevel2, _buttonJoinRandom });
                _buttonJoinLevel1.AddToClassList("ButtonLevelSelected");
                break;

            case 2:
                DeactivateToggleButtons(new List<Button> { _buttonJoinLevel1, _buttonJoinLevel2, _buttonJoinRandom });
                _buttonJoinLevel2.AddToClassList("ButtonLevelSelected");
                break;
        }
    }



    void StartTutorial()
    {
        // use _playerName
        // change scene
        //StartCoroutine(_connectingManager.Example_ConfigureTransportAndStartNgoAsHostAndStartTutorial());
        _startMenuUI.rootVisualElement.style.display = DisplayStyle.None;
        _tutorialSelectionMenuUI.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    void StartHosting()
    {
        _loadingProgress = 0;
        StartCoroutine(_connectingManager.Example_ConfigureTransportAndStartNgoAsHost());
        _startMenuUI.rootVisualElement.style.display = DisplayStyle.None;
        _joinGameUI.rootVisualElement.style.display = DisplayStyle.None;
        _lobbyMenuUI.rootVisualElement.style.display = DisplayStyle.None;
        _loadingLobbyUI.rootVisualElement.style.display = DisplayStyle.Flex;
        _isHost = true;
        StartCoroutine(LoadLobby(0.02f)); 

    }
    IEnumerator GetHostID(float time)
    {
        yield return new WaitForSeconds(time);
        _hostID = PersistentInfoHolder.Instance.LobbyCode;
        UpdateHostID();
    }
    void JoinGame()
    {
        UpdatePassiveToggleButtons();
        _isHost = false;
        //_labelPlayerName.text = "name: " + _playerName;
        _labelJoinPlayerName.text =  PersistentInfoHolder.Instance.LocalPlayerName;
        UpdateHostID(); // uses entered hostID 

        StartCoroutine(_connectingManager.Example_ConfigureTransportAndStartNgoAsConnectingPlayer(_hostID));
        _startMenuUI.rootVisualElement.style.display = DisplayStyle.None;
        _loadingLobbyUI.rootVisualElement.style.display = DisplayStyle.Flex;
        StartCoroutine(LoadJoinLobby(0.02f));
    }

    public void JoinLobbyFailed(string reason)
    {
        _loadingProgress = 0;
        _startMenuUI.rootVisualElement.style.display = DisplayStyle.Flex;
        _joinGameUI.rootVisualElement.style.display = DisplayStyle.None;
        _lobbyMenuUI.rootVisualElement.style.display = DisplayStyle.None;
        _loadingLobbyUI.rootVisualElement.style.display = DisplayStyle.None;
        _inputHostID.value = reason;
        NetworkManager.Singleton.Shutdown();
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton is not null)
            NetworkManager.Singleton.OnClientConnectedCallback -= SetLoadingCompleted;

        base.OnDestroy();
    }


    public void UpdateLobbyScreen()
    {
        UpdateLocalPlayerName();
        UpdateHostID();
        UpdatePlayersConnected(); 
        UpdatePassiveToggleButtons();
    }

    string RandomName()
    {
        string[] shortNames = {
        // Female names
        "Emma",
        "Ava",
        "Mia",
        "Lily",
        "Zoe",
        "Nora",
        "Ella",
        "Grace",
        "Maya",
        "Ivy",
    
        // Male names
        "Liam",
        "Noah",
        "Ethan",
        "Logan",
        "Caleb",
        "Lucas",
        "Henry",
        "Leo",
        "Owen",
        "Max" };

        return shortNames[Random.Range(0, shortNames.Length)];
    }


}
