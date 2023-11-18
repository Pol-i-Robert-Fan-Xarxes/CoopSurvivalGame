using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public struct GameData
{
    public GameData(bool pause, int scene)
    {
        _isPaused = pause;
        _scene = scene;
    }

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
    }

    public void LoadScene()
    {
        if (SceneManager.GetActiveScene().buildIndex != _gameData._scene)
        {
            SceneManager.LoadScene(_gameData._scene);
        }
    }

    public void LoadPlayers()
    {
        if (_gameData._scene == 1 &&  !_playersLoaded)
        {
            if (_localPlayer == null)
            {
                GameObject lp = GameObject.Find("LocalPlayer");
                _localPlayer = lp.GetComponent<LocalPlayer>();
                Debug.Log("Missing local");
            }

            if (_remotePlayer == null)
            {
                GameObject rp = GameObject.Find("RemotePlayer");
                _remotePlayer = rp.GetComponent<Player>();
                Debug.Log("Missing remote");
            }
        }    
    }

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
        if (_localPlayer != null)
            _gameInfo.SetPlayerData(_localPlayer._playerData);
    }
}