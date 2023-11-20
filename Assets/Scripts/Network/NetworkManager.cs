using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum NetworkFeedback
{
    CONNECTION_ERROR,
    CONNECTION_SUCCESS,
    SERVER_ERROR,
    SERVER_SUCCESS
}

public class NetworkManager : MonoBehaviour
{
    //Instance
    public static NetworkManager _instance;
    public static NetworkManager Instance => _instance;

    private GameManager _gameManager;

    //Flags
    private bool _isHost = false;
    public bool _hasSent = false;

    //Objects
    public Server _server;
    public Client _client;

    //Send Decider
    public float sendBatchTime = 0.001f;
    private float currentTime = 0.0f;

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

    private void Start()
    {
        _gameManager = GameManager.Instance;
    }

    private void Update()
    {
        if (_isHost && _server != null)
            if (!_server._running) _server = null;
        if (!_isHost && _client != null)
            if (!_client._running) _client = null;

        SendDecider();
        ReadPackage();
    }

    private void SendDecider()
    {
        if (_client == null && _server == null) { return; }

        currentTime += Time.deltaTime;
        if( sendBatchTime < currentTime )
        {
            currentTime = 0;

            Send(); 
        }
    }

    private void ReadPackage()
    {
        if (_isHost && _server != null && _server._running) 
        {         
            _gameManager.Unpackage(_server.GetPackage(), true);
        }
        else if (!_isHost && _client != null && _client._running) 
        {
            _gameManager.Unpackage(_client.GetPackage(), false);
        }
    }

    public NetworkFeedback StartServer()
    {
        _isHost = true;
        _server = new Server();
        _server.Initialize();

        return _server.StartServer();
    }

    public NetworkFeedback ConnectToServer(string ip, string port)
    {
        _isHost = false;
        _client = new Client(ip, port);
        _client.Initialize();

        return _client.ConnectToHost();
    }

    public void ForceConnectionClose()
    {
        if (_isHost)
        { //Server
            _server._connected = false;
            _server = null;
        }
        else
        { //Client
            _client._connected = false;
            _client = null;
        }
        currentTime = 0;
    }

    #region Send
    private void Send()
    {
        if (_isHost)
        {
            _server.Send(Serialize(_gameManager.Package(true)));
        }
        else
        {
            //_gameManager.Package(false);
            _client.Send(Serialize(_gameManager.Package(false)));
        }
    }

    #endregion

    #region Serialize/Deserialize

    public byte[] Serialize(GameInfo gameInfo)
    {
        
        string json = JsonUtility.ToJson(gameInfo);
        return Encoding.ASCII.GetBytes(json);
    }

    public static GameInfo Deserialize(byte[] data, int size)
    {
        string json = Encoding.ASCII.GetString(data, 0, size);
        return JsonUtility.FromJson<GameInfo>(json);
    }

    #endregion Serialize/Deserialize
}