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
    public int nextLevelXp;
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
    public EnemyManager _enemyManager;
    
    private int _numberOfDeaths = 0;
    public int numberOfDeaths
    {
        get { return _numberOfDeaths; }
        set { _numberOfDeaths = value; }
    }

    public bool _singlePlayer = false;
    private bool _playersLoaded = false;

    public GameData _gameData = new GameData();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);

            AwakeSceneHandler();
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
        if (_gameUiController == null || _enemyManager == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("UI Controller");
            _gameUiController = go.GetComponent<GameUIController>();

            _enemyManager = GameObject.FindGameObjectWithTag("Enemy Manager").GetComponent<EnemyManager>();

            _gameData.gameTimer = new Timer();
            _gameData.gameTimer.Start();

            _gameData.xp = 0;
            _gameData.nextLevelXp = 10;
            _gameData.level = 1;
            _gameUiController.UpdateLevelXpUI(_gameData.xp, _gameData.nextLevelXp, _gameData.level);
        }

        ExecutePause();
        UpdateTimer();

        if (_networkManager._isHost || _singlePlayer) CheckLoseCondition();
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

    public void ExecuteConnectionLost()
    {
        _gameUiController.ConnectionLost();
        _gameData._isPaused = true;
        _gameData.gameTimer.Pause();
    }

    private void UpdateTimer()
    {
        if (_gameData.gameTimer == null) _gameData.gameTimer = new Timer();

        _gameData.gameTimer.Update();
        _gameUiController.SetTxtTime(_gameData.gameTimer.GetTime());

        CheckIfTimerFinished();
    }

    // Move to lose scene if all players are dead
    private void CheckLoseCondition()
    {
        int i = 0;
        if (!_localPlayer.gameObject.activeSelf) i++;
            

        foreach (var rp in _remotePlayers)
        {
            if (!rp.gameObject.activeSelf) i++;
        }

        if (i >= (1 + _remotePlayers.Count)) _gameData._scene = 2;
    }

    private void CheckIfTimerFinished()
    {
        if(_gameData.gameTimer.GetTime() == "00:00" )
        {
            _gameData._scene = 3;
        }
    }

    public void UpdateKillCounter()
    {
        if (_gameUiController == null) return;
        numberOfDeaths++;
        _gameUiController.SetTxtKills(numberOfDeaths.ToString());
    }

    public void SetPlayersLoaded(bool setTo)
    {

        _playersLoaded = setTo;

    }

    #region Player Related

    public void LoadLocalPlayer()
    {
        //Load local player
        if (_gameData._scene == 1 && !_playersLoaded)
        {
            if (_localPlayer == null)
            {
                _localPlayer = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<LocalPlayer>();
            }

            if (_localPlayer != null)
            {
                _localPlayer._playerData.name = _gameData._localPlayerName;
                _localPlayer._playerData.netId = System.Guid.NewGuid().ToString();
                _localPlayer._txt_name.text = _gameData._localPlayerName;

                if (!_singlePlayer)
                {
                    _networkManager.SendLocalPlayer(Action.CREATE, _localPlayer._playerData);
                }
                _playersLoaded = true;
            }
        }
    }

    public void CreateRemotePlayer(PlayerData data)
    {
        Debug.Log("Creating remote player");
        Player player = Instantiate(Resources.Load<Player>("Prefabs/RemotePlayer"), new Vector3(0, 0, 0), Quaternion.identity);
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

    #region Gameplay related
    public void AddXp(int xp)
    {
        _gameData.xp += xp; 

        if (_gameData.xp >= _gameData.nextLevelXp)
        {
            LevelUp();
        }

        _gameUiController.UpdateLevelXpUI(_gameData.xp, _gameData.nextLevelXp, _gameData.level);
    }

    private void LevelUp()
    {
        _gameData.level++;

        _gameData.xp -= _gameData.nextLevelXp;
        _gameData.nextLevelXp = (int) (_gameData.nextLevelXp * 1.25);
    }
    #endregion

    #region Others

    //Aquesta funcio no fa res relacionat amb xarxes.
    // Solves a problem we have during development, allowing testing the game scene without having to start the game from the main menu.
    // The game manager is created first when the game starts. If the game starts by the Game scene this method will make the game manager assume that is single player.
    private void AwakeSceneHandler()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            _singlePlayer = true;
            _gameData._scene = 1;
        }
    }
    #endregion
}