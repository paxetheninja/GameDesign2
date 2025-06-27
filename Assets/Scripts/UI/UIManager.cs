using HighScore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.Netcode;
using Unity.VectorGraphics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    UIDocument helpMenuUI;
    [SerializeField]
    UIDocument playerControl;
    [SerializeField]
    Canvas _joystickCanvas;
    [SerializeField]
    UIDocument _helpControlUI;
    [SerializeField]
    UIDocument winScreen;
    [SerializeField]
    UIDocument loseScreen;
    [SerializeField]
    UIDocument loadingSceneUI;
    [SerializeField]
    ConnectingManager _connectingManager;

    [SerializeField]
    PlayerMovement _testCharacterMovement;

    public bool enableAndroidTouch;

    Button _tabButtonTable1;
    Button _tabButtonTable2;
    Button _tabButtonPottersWheel;
    Button _tabButtonGrindingTable;
    Button _tabButtonPaintersTable;
    Button _tabButtonOven;

    Button _buttonControlHelp;

    VisualElement _sectionTable1;
    VisualElement _sectionTable2;
    VisualElement _sectionPottersWheel;
    VisualElement _sectionGrindingTable;
    VisualElement _sectionPaintersTable;
    VisualElement _sectionOven;

    private VisualElement _rootLose;
    private VisualElement _rootWin;

    VisualElement _rootHelpMenu;

    VisualElement _rootButtonsControl;

    VisualElement _rootLoadingScene;
    ProgressBar _loadingBar;
    [SerializeField] float _loadingProgress;

    Label _labelHighscoreNames;
    Label _labelHighscorePoints;

    // for testing: 
    [SerializeField]
    string _scoreNames = "Mike:\nIna:\nPeter:";
    [SerializeField]
    string _scorePoints = "42344\n23434\n12345";

    public bool IsHelpEnabled => _rootHelpMenu.visible;


    private void Awake()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SetLoadingCompleted;

        // ======================= Loading Scene =============================
        _rootLoadingScene = loadingSceneUI.rootVisualElement;
        _loadingBar = _rootLoadingScene.Q<ProgressBar>("LoadingBar");

        StartCoroutine(LoadScene(0.02f));

    }

    private void SetLoadingCompleted(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
    {
        Debug.Log("Loading already completed");
        _loadingProgress = 0.7f;
        //loadingSceneUI.rootVisualElement.style.display = DisplayStyle.None;
        //throw new NotImplementedException();
    }
    public void OnDestroy()
    {
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.SceneManager == null) return;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SetLoadingCompleted;
    }
    void Start()
    {
        _testCharacterMovement = FindObjectOfType<PlayerMovement>();

        _scoreNames = "Mike:\nIna:\nPeter:";
        _scorePoints = "42344\n23434\n12345";
        _rootButtonsControl = _helpControlUI.rootVisualElement;
        // ======================= Help Menu UI =============================
        _rootHelpMenu = helpMenuUI.rootVisualElement;
        _rootHelpMenu.visible = false;
        Button buttonCloseHelp = _rootHelpMenu.Q<Button>("ButtonCloseHelp");
        Button buttonCredits = _rootHelpMenu.Q<Button>("ButtonCredits");
        Button buttonRecipies = _rootHelpMenu.Q<Button>("ButtonRecipies");
        Label labelTitleRecipies = _rootHelpMenu.Q<Label>("LabelTitleRecipies");
        Label labelTitleScore = _rootHelpMenu.Q<Label>("LabelTitleScore");
        _labelHighscoreNames = _rootHelpMenu.Q<Label>("LabelHighscoreNames");
        _labelHighscorePoints = _rootHelpMenu.Q<Label>("LabelHighscorePoints");
        VisualElement areaBody = _rootHelpMenu.Q<VisualElement>("AreaBody");
        VisualElement areaScore = _rootHelpMenu.Q<VisualElement>("AreaScore");

        Button buttonExit = _rootHelpMenu.Q<Button>("ButtonExit");
        _tabButtonTable1 = _rootHelpMenu.Q<Button>("TabButtonTable1");
        _tabButtonTable2 = _rootHelpMenu.Q<Button>("TabButtonTable2");
        _tabButtonPottersWheel = _rootHelpMenu.Q<Button>("TabButtonPottersWheel");
        _tabButtonGrindingTable = _rootHelpMenu.Q<Button>("TabButtonGrindingTable");
        _tabButtonPaintersTable = _rootHelpMenu.Q<Button>("TabButtonPaintersTable");
        _tabButtonOven = _rootHelpMenu.Q<Button>("TabButtonOven");

        _buttonControlHelp = _rootHelpMenu.Q<Button>("ButtonControls");

        _sectionTable1 = _rootHelpMenu.Q<VisualElement>("SectionTable1");
        _sectionTable2 = _rootHelpMenu.Q<VisualElement>("SectionTable2");
        _sectionPottersWheel = _rootHelpMenu.Q<VisualElement>("SectionPottersWheel");
        _sectionGrindingTable = _rootHelpMenu.Q<VisualElement>("SectionGrindingTable");
        _sectionPaintersTable = _rootHelpMenu.Q<VisualElement>("SectionPaintersTable");
        _sectionOven= _rootHelpMenu.Q<VisualElement>("SectionOven");

        _tabButtonTable1.RegisterCallback<ClickEvent>((evt) =>
        {
            HideAllSections();
            _sectionTable1.style.display = DisplayStyle.Flex;
            _tabButtonTable1.AddToClassList("ToggleButtonSelected");
        });

        _tabButtonTable2.RegisterCallback<ClickEvent>((evt) =>
        {
            HideAllSections();
            _sectionTable2.style.display = DisplayStyle.Flex;
            _tabButtonTable2.AddToClassList("ToggleButtonSelected");
        });

        _tabButtonPottersWheel.RegisterCallback<ClickEvent>((evt) =>
        {
            HideAllSections();
            _sectionPottersWheel.style.display = DisplayStyle.Flex;
            _tabButtonPottersWheel.AddToClassList("ToggleButtonSelected");
        });

        _tabButtonGrindingTable.RegisterCallback<ClickEvent>((evt) =>
        {
            HideAllSections();
            _sectionGrindingTable.style.display = DisplayStyle.Flex;
            _tabButtonGrindingTable.AddToClassList("ToggleButtonSelected");
        });

        _tabButtonPaintersTable.RegisterCallback<ClickEvent>((evt) =>
        {
            HideAllSections();
            _sectionPaintersTable.style.display = DisplayStyle.Flex;
            _tabButtonPaintersTable.AddToClassList("ToggleButtonSelected");
        });

        _tabButtonOven.RegisterCallback<ClickEvent>((evt) =>
        {
            HideAllSections();
            _sectionOven.style.display = DisplayStyle.Flex;
            _tabButtonOven.AddToClassList("ToggleButtonSelected");
        });
        buttonExit.RegisterCallback<ClickEvent>((evt) =>
        {
            ExitLobby();
        });

        buttonCloseHelp.RegisterCallback<ClickEvent>((evt) =>
        {
            // Reset to initial setup before leaving
            HideAllSections();
            _sectionTable1.style.display = DisplayStyle.Flex;
            _tabButtonTable1.AddToClassList("ToggleButtonSelected");
            _rootHelpMenu.visible = false;
            if (enableAndroidTouch)
                _joystickCanvas.enabled = true;
        });

        buttonCredits.RegisterCallback<ClickEvent>((evt) =>
        {
            labelTitleRecipies.style.display = DisplayStyle.None;
            labelTitleScore.style.display = DisplayStyle.Flex;
            buttonCredits.style.display = DisplayStyle.None;
            buttonRecipies.style.display = DisplayStyle.Flex;
            areaBody.style.display = DisplayStyle.None;
            areaScore.style.display = DisplayStyle.Flex;
            
            HighScoreMap.HighScores highScores = HighScoreManager.GetHighScoresFromFile();
            highScores.highScoreList = highScores.highScoreList.OrderBy(x => x.levelName).ToList();

            string _scoreNames = "";
            string _scorePoints = "";

            for (int i = 0; i < highScores.highScoreList.Count; i++)
            {
                Debug.Log("LevelName: " + highScores.highScoreList[i].levelName + ", points: " + highScores.highScoreList[i].scoreSeconds);
                _scoreNames += highScores.highScoreList[i].levelName + ":\n";
                _scorePoints += highScores.highScoreList[i].scoreSeconds + "\n";
            }
            UpdateScore(_scoreNames, _scorePoints);

        });

        buttonRecipies.RegisterCallback<ClickEvent>((evt) =>
        {
            labelTitleRecipies.style.display = DisplayStyle.Flex;
            labelTitleScore.style.display = DisplayStyle.None;
            buttonCredits.style.display = DisplayStyle.Flex;
            buttonRecipies.style.display = DisplayStyle.None;
            areaBody.style.display = DisplayStyle.Flex;
            areaScore.style.display = DisplayStyle.None;
        });
        _buttonControlHelp.RegisterCallback<ClickEvent>(evt =>
        {
            _rootHelpMenu.style.display = DisplayStyle.None;
            _rootButtonsControl.style.display = DisplayStyle.Flex;  
        });

        // register Buttons
        // ======================= Touch Buttons UI =============================
        var rootPlayerControl = playerControl.rootVisualElement;
        if (rootPlayerControl != null)
        {
            Button helpButton = rootPlayerControl.Q<Button>("HelpButton");
            helpButton.RegisterCallback<ClickEvent>((evt) =>
            {
                if (true)//gameObject.GetComponent<NetworkObject>().IsOwner)
                {
                    _rootHelpMenu.visible = true;
                    _joystickCanvas.enabled = false;
                }
            });
        }

        // ======================= Help Key Buttons UI =============================
        _rootButtonsControl.style.display = DisplayStyle.None;
        Button buttonCloseButtonHelp = _rootButtonsControl.Q<Button>("ButtonCloseHelp");
        buttonCloseButtonHelp.RegisterCallback<ClickEvent>((evt) =>
        {
            _rootHelpMenu.style.display = DisplayStyle.Flex;
            _rootButtonsControl.style.display = DisplayStyle.None;

        });


        Button buttonEnableTouch = _rootButtonsControl.Q<Button>("ButtonEnableTouch");
        Button buttonDisableTouch = _rootButtonsControl.Q<Button>("ButtonDisableTouch");
        buttonEnableTouch.RegisterCallback<ClickEvent>((evt) =>
        {

            EnableAndroidTouch();
            _joystickCanvas.enabled = false; // since we are still in the help menu
            buttonDisableTouch.style.display = DisplayStyle.Flex;
            buttonEnableTouch.style.display = DisplayStyle.None;

        });
        buttonDisableTouch.RegisterCallback<ClickEvent>((evt) =>
        {
            DisableAndroidTouch();
            buttonDisableTouch.style.display = DisplayStyle.None;
            buttonEnableTouch.style.display = DisplayStyle.Flex;

        });

        // ======================= Win Screen =============================
        _rootWin = winScreen.rootVisualElement;
        _rootWin.style.display = DisplayStyle.None;
        Button returnToLobbyWin = _rootWin.Q<Button>("ReturnToLobby");
        returnToLobbyWin.RegisterCallback<ClickEvent>((evt) =>
        {
            ExitLobby();
        }); 
        _rootLose = loseScreen.rootVisualElement;
        _rootLose.style.display = DisplayStyle.None;
        Button returnToLobbyLose = _rootLose.Q<Button>("ReturnToLobby");
        returnToLobbyLose.RegisterCallback<ClickEvent>((evt) =>
        {
            ExitLobby();

        });

        

        if (Application.platform is RuntimePlatform.Android or RuntimePlatform.IPhonePlayer)
        {
            EnableAndroidTouch();

        }
        else
        {
            DisableAndroidTouch();
        }

        ////enableAndroidTouch = true;
        //if (enableAndroidTouch)
        //{
        //    EnableAndroidTouch();
        //}
        //else
        //{
        //    DisableAndroidTouch();

        //}


    }
    public void SetLoadingProgress(float progress)
    {
        _loadingProgress = progress;
    }
    IEnumerator LoadScene(float time)
    {
        _loadingProgress = 0;
        while (_loadingProgress < 0.9)
        {
            if(SceneManager.GetActiveScene().name != "GeneratedLevel")
                _loadingProgress += 0.01f;
            _loadingBar.value = _loadingProgress;
            yield return new WaitForSeconds(time); // Delay for 1 second
        }
        if (enableAndroidTouch) EnableAndroidTouch();
        loadingSceneUI.rootVisualElement.style.display = DisplayStyle.None;
        if (GameplayManager.Instance != null)
            GameplayManager.Instance.OnLoadEventCompleted(); //"", LoadSceneMode.Single, new List<ulong> { }, new List<ulong> { });
    }

    public void DisableAndroidTouch()
    {
        enableAndroidTouch = false;
        _joystickCanvas.enabled = false;
        playerControl.rootVisualElement.style.display = DisplayStyle.None;
        _buttonControlHelp.style.display = DisplayStyle.Flex;
    }
    public void EnableAndroidTouch()
    {
        _testCharacterMovement._joystick = _joystickCanvas.GetComponentInChildren<FixedJoystick>();
        enableAndroidTouch = true;
        _joystickCanvas.enabled = true;
        playerControl.rootVisualElement.style.display = DisplayStyle.Flex;
        _buttonControlHelp.style.display = DisplayStyle.None;
    }

    public void DisableHelp()
    {
        HideAllSections();
        _sectionTable1.style.display = DisplayStyle.Flex;
        _tabButtonTable1.AddToClassList("ToggleButtonSelected");
        _rootHelpMenu.visible = false;
        if (enableAndroidTouch)
            _joystickCanvas.enabled = true;
    }

    public void EnableHelp()
    {
        _joystickCanvas.enabled = false;
        _rootHelpMenu.visible = true;
    }

    public void SetScore(string names, string points)
    {
        _scoreNames = names;
        _scorePoints = points;
    }

    public void ShowWin()
    {
        DisableAndroidTouch();
        _rootWin.style.display = DisplayStyle.Flex;
    }

    public void ShowLose()
    {
        DisableAndroidTouch();
        _rootLose.style.display = DisplayStyle.Flex;
    }

    void UpdateScore(string names, string points)
    {
        _labelHighscoreNames.text = names;
        _labelHighscorePoints.text = points;
    }

    void HideAllSections()
    {
        _sectionTable1.style.display = DisplayStyle.None;
        _sectionTable2.style.display = DisplayStyle.None;
        _sectionPottersWheel.style.display = DisplayStyle.None;
        _sectionGrindingTable.style.display = DisplayStyle.None;
        _sectionPaintersTable.style.display = DisplayStyle.None;
        _sectionOven.style.display = DisplayStyle.None;
        if (_tabButtonTable1.ClassListContains("ToggleButtonSelected"))
        {
            _tabButtonTable1.RemoveFromClassList("ToggleButtonSelected");
        }
        if (_tabButtonTable2.ClassListContains("ToggleButtonSelected"))
        {
            _tabButtonTable2.RemoveFromClassList("ToggleButtonSelected");
        }
        if (_tabButtonPottersWheel.ClassListContains("ToggleButtonSelected"))
        {
            _tabButtonPottersWheel.RemoveFromClassList("ToggleButtonSelected");
        }
        if (_tabButtonGrindingTable.ClassListContains("ToggleButtonSelected"))
        {
            _tabButtonGrindingTable.RemoveFromClassList("ToggleButtonSelected");
        }
        if (_tabButtonPaintersTable.ClassListContains("ToggleButtonSelected"))
        {
            _tabButtonPaintersTable.RemoveFromClassList("ToggleButtonSelected");
        }
        if (_tabButtonOven.ClassListContains("ToggleButtonSelected"))
        {
            _tabButtonOven.RemoveFromClassList("ToggleButtonSelected");
        }
    }

    public void ExitLobby()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClientsList.Count == 1)
            {
                // This means that we are alone in this lobby. Close this lobby and return to start menu, we don't need this lobby anymore.
                NetworkManager.Singleton.Shutdown();
                SceneManager.LoadScene("Startup", LoadSceneMode.Single);
            }
            else
            {
                // Keep this lobby and pull all clients out of the game. Startup scene itself should handle that the lobby should be shown instead of startup ui.
                NetworkManager.Singleton.SceneManager.LoadScene("Startup", LoadSceneMode.Single);
            }
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            // Just disconnect from the server. This client does not want to play anymore :(
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("Startup", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning($"Called {nameof(ExitLobby)} without having any host/client connection established.");
        }
    }
}
