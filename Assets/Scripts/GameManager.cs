using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public struct GameData
{
    public string _localPlayerName;
    public string _remotePlayerName;

    //Shared player data
    public int level;
    public int xp;
    public int kills;

    public Timer gameTimer;

    //Flags
    public bool _isPaused;
    public int _scene;
}

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;
    public static GameManager Instance => _instance;

    [HideInInspector] public LocalPlayer _localPlayer;
    [HideInInspector] public Player _remotePlayer;

    private NetworkManager _networkManager;
    private GameUIController _gameUiController;

    private bool _playersLoaded = false;

    public GameData _gameData = new GameData();
    private GameInfo _gameInfo = new GameInfo();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _networkManager = NetworkManager.Instance;
    }

    private void Update()
    {
        LoadScene();
        LoadPlayers();

        GameSceneHandler();

        if (Input.GetKeyDown(KeyCode.Space)) { PauseGame(); }
    }
    private void FixedUpdate()
    {
        if (_gameData._isPaused) return;
        if (_playersLoaded) 
        {
            MovePlayers();
        }
    }

    public void LoadScene()
    {
        if (SceneManager.GetActiveScene().buildIndex != _gameData._scene)
        {
            SceneManager.LoadScene(_gameData._scene);
        }
    }

    private void GameSceneHandler()
    {
        if (_gameData._scene != 1) return;
        if (_gameUiController == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("UI Controller");
            _gameUiController = go.GetComponent<GameUIController>();

            _gameData.gameTimer = new Timer();
            _gameData.gameTimer.Start();
        }

        ExecutePause();
        UpdateTimer();

    }

    private void PauseGame()
    {
        if (_gameUiController == null) return;
        _gameData._isPaused = !_gameData._isPaused;
    }
    private void ExecutePause()
    {
        if (_gameData._isPaused)
            _gameData.gameTimer.Pause();
        else
            _gameData.gameTimer.Resume();

        _gameUiController.Pause(_gameData._isPaused);
    }

    private void UpdateTimer()
    {
        if (_gameData.gameTimer == null) _gameData.gameTimer = new Timer();

        _gameData.gameTimer.Update();
        _gameUiController.SetTxtTime(_gameData.gameTimer.GetTime());
    }

    #region Player Related

    public void LoadPlayers()
    {
        if (_gameData._scene == 1 && !_playersLoaded)
        {
            int i = 0;
            if (_localPlayer == null)
            {
                GameObject lp = GameObject.FindGameObjectWithTag("LocalPlayer");
                _localPlayer = lp.GetComponent<LocalPlayer>();
            }
            else
            {
                _localPlayer.GetComponentInChildren<Text>().text = _gameData._localPlayerName;
                i += 1;
            }

            if (_remotePlayer == null)
            {
                GameObject rp = GameObject.FindGameObjectWithTag("Player");
                _remotePlayer = rp.GetComponent<Player>();
            }
            else
            {
                _remotePlayer.GetComponentInChildren<Text>().text = _gameData._remotePlayerName;
                i+= 1;
            }

            if (i >= 2) _playersLoaded = true;
        }    
    }

    public void MovePlayers()
    {
        if (_localPlayer != null)
        {
            _localPlayer.Movement();
        }

        if (_remotePlayer != null)
        {
            _remotePlayer.Movement();
        }
    }
    #endregion

    #region Package Related

    public void Unpackage(GameInfo info, bool isHost)
    {
        if (info == null) return;

        if (isHost)
        {
            UnpackHost(ref info);
        }
        else
        {
            UnpackClient(ref info);
        }
    }

    private void UnpackHost(ref GameInfo info)
    {
        info.UnpackGameData(ref _gameData, true);
        if (_remotePlayer != null)
            info.UnpackPlayerData(ref _remotePlayer);
    }

    private void UnpackClient(ref GameInfo info)
    {
        info.UnpackGameData(ref _gameData);
        if (_remotePlayer != null)
            info.UnpackPlayerData(ref _remotePlayer);
    }

    public GameInfo Package(bool isHost)
    {
        if (isHost)
        {
            PackHost();
        }
        else
        {
            PackClient();
        }

        return _gameInfo;
    }

    private void PackHost()
    {
        _gameInfo.SetGameData(_gameData);

        if (_localPlayer != null)
            _gameInfo.SetPlayerData(_localPlayer._playerData);
    }

    private void PackClient()
    {
        _gameInfo.SetGameData(_gameData);
        if (_localPlayer != null)
            _gameInfo.SetPlayerData(_localPlayer._playerData);
    }

    #endregion
}