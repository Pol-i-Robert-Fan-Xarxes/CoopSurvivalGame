using System;
using System.Collections.Generic;
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

    //public void SetGameData(GameData gameData)
    //{
    //    this = gameData;
    //}

    public void UnpackGameData(ref GameData newData)
    {
        _isPaused = newData._isPaused;
        _scene = newData._scene;
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;
    public static GameManager Instance => _instance;

    [HideInInspector] public LocalPlayer _localPlayer;
    //[HideInInspector] public Player _remotePlayer;
    [HideInInspector] public List<Player> _remotePlayers;

    private NetworkManager _networkManager;
    private GameUIController _gameUiController;
    
    private int _numberOfDeaths = 0;
    public int numberOfDeaths
    {
        get { return _numberOfDeaths; }
        set { _numberOfDeaths = value; }
    }

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
        _remotePlayers = new List<Player>();
    }

    private void Update()
    {
        LoadScene();
        LoadLocalPlayer();

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

    public void UpdateKillCounter()
    {
        if (_gameUiController == null) return;
        numberOfDeaths++;
        _gameUiController.SetTxtKills(numberOfDeaths.ToString());
    }

    #region Player Related

    public void LoadLocalPlayer()
    {
        //Load local player
        if (_gameData._scene == 1 && !_playersLoaded)
        {
            if (_localPlayer == null)
            {
                GameObject lp = GameObject.FindGameObjectWithTag("LocalPlayer");
                _localPlayer = lp.GetComponent<LocalPlayer>();
            }

            if (_localPlayer != null)
            {
                _localPlayer._playerData.name = _gameData._localPlayerName;
                _localPlayer._playerData.netId = System.Guid.NewGuid().ToString();
                _localPlayer.GetComponentInChildren<Text>().text = _gameData._localPlayerName;

                _networkManager.SendLocalPlayer(Action.CREATE, _localPlayer._playerData);
                _playersLoaded = true;
            }
        }
    }

    public void CreateRemotePlayer(PlayerData data)
    {
        Debug.Log("Creating remote player");
        var reference = Resources.Load<Player>("Prefabs/RemotePlayer");
        Player player = Instantiate(reference, new Vector3(0, 0, 0), Quaternion.identity);
        //player.GetComponentInChildren().text = _gameData._remotePlayerName[id];
        player._playerData = data;
        player.GetComponentInChildren<Text>().text = player._playerData.name;

        _remotePlayers.Add(player);
    }

    public void MovePlayers()
    {
        if (_localPlayer != null)
        {
            _localPlayer.Movement();
            _networkManager.SendLocalPlayer(Action.UPDATE, _localPlayer._playerData);
        }

        foreach (var rp in _remotePlayers)
        {
            if (rp != null)
            {
                rp.Movement();
            }
        }
    }
    #endregion

    #region Package Related

    //public void Unpackage(GameInfo info, bool isHost)
    //{
    //    if (info == null) return;

    //    if (isHost)
    //    {
    //        UnpackHost(ref info);
    //    }
    //    else
    //    {
    //        UnpackClient(ref info);
    //    }
    //}

    //private void UnpackHost(ref GameInfo info)
    //{
    //    //info.UnpackGameData(ref _gameData, true);
    //    if (_remotePlayer != null)
    //        info.UnpackPlayerData(ref _remotePlayer);
    //}

    //private void UnpackClient(ref GameInfo info)
    //{
    //    //info.UnpackGameData(ref _gameData);
    //    if (_remotePlayer != null)
    //        info.UnpackPlayerData(ref _remotePlayer);
    //}

    //public GameInfo Package(bool isHost)
    //{
    //    if (isHost)
    //    {
    //        PackHost();
    //    }
    //    else
    //    {
    //        PackClient();
    //    }

    //    return _gameInfo;
    //}

    //private void PackHost()
    //{
    //    //_gameInfo.SetGameData(_gameData);

    //    //if (_localPlayer != null)
    //    //    _gameInfo.SetPlayerData(_localPlayer._playerData);
    //}

    //private void PackClient()
    //{
    //    //_gameInfo.SetGameData(_gameData);
    //    //if (_localPlayer != null)
    //    //    _gameInfo.SetPlayerData(_localPlayer._playerData);
    //}

    #endregion
}