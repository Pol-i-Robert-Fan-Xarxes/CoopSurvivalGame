using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum NetworkFeedback
{
    CONNECTION_ERROR,
    CONNECTION_SUCCESS,
    SERVER_ERROR,
    SERVER_SUCCESS
}

public enum PackDataType
{
    GAME_DATA = 0, //General GameData, the one in GameManager.cs
    PING, //Ping to check if still connected
    HELLO, //First message to be sent
    LOCAL_PLAYER, //To send Data from the local Player
    OTHER_PLAYERS //to send Data from other clients local Player 
}

public enum Action
{
    CREATE = 0,
    UPDATE,
    DESTROY,
    EVENTS
}

[Serializable]
public struct Package
{
    public int NetID;
    public int Action;
    public int DataType;
    public string JsonData;
}

public struct QueuePair
{
    public string originSessionId;
    public Package package;

    public QueuePair(string originSessionId, Package package)
    {
        this.originSessionId = originSessionId;
        this.package = package;
    }
}

public class NetworkManager : MonoBehaviour
{
    //Instance
    public static NetworkManager _instance;
    public static NetworkManager Instance => _instance;

    public static Queue<QueuePair> _recievePackageQueue;

    private GameManager _gameManager;
    //private 

    //Flags
    private bool _isHost = false;
    private bool _isClient = false;
    public bool _hasSent = false;

    //Objects
    public Server _server;
    public Client _client;

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

        _recievePackageQueue =  new Queue<QueuePair>();
    }

    private void Update()
    {
        if (_isHost && _server != null)
            if (!_server._running) _server = null;
        if (!_isHost && _client != null)
            if (!_client._running) _client = null;

        //TODO Use send decider
        UpdateReplication();
        //Send();
        //ReadPackage();
    }

    public NetworkFeedback StartServer()
    {
        _isHost = true;
        _isClient = false;
        _server = new Server();
        _server.Initialize();

        return _server.StartServer();
    }

    public NetworkFeedback ConnectToServer(string ip, string port)
    {
        _isHost = false;
        _isClient = true;
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
            _isHost = false;
        }
        else if (_isClient)
        { //Client
            _client._connected = false;
            _client = null;
            _isClient = false;
        }
    }

    #region Serialize/Deserialize

    public byte[] Serialize<T>(T data, PackDataType dataType, Action action)
    {
        string json = JsonUtility.ToJson(data);

        Package package = new Package()
        {
            DataType = (int) dataType,
            Action = (int) action,
            JsonData = json
        };
        
        return Encoding.ASCII.GetBytes(JsonUtility.ToJson(package));
    }

    public static void Deserialize(byte[] data, int size, string originSessionId = "")
    {
        string json = Encoding.ASCII.GetString(data, 0, size);

        //Deserialize the package
        Package package = JsonUtility.FromJson<Package>(json);

        _recievePackageQueue.Enqueue(new QueuePair(originSessionId, package));

        //return JsonUtility.FromJson<GameInfo>(json);
    }

    #endregion Serialize/Deserialize

#region ReplicationManager

    //Unwrapps replication packages
    private void PackageUnwrapper()
    {
        while (_recievePackageQueue.Count > 0)
        {
            QueuePair qPair = _recievePackageQueue.Dequeue();
            switch ((PackDataType)qPair.package.DataType)
            {
                case PackDataType.GAME_DATA:
                    {
                        UnpackGameData(JsonUtility.FromJson<GameData>(qPair.package.JsonData));
                        break;
                    }
                case PackDataType.PING:
                    {

                        break;
                    }
                case PackDataType.HELLO:
                    {
                        UnpackHello(JsonUtility.FromJson<string>(qPair.package.JsonData), qPair.originSessionId);
                        break;
                    }
                case PackDataType.LOCAL_PLAYER:
                    {
                        UnpackLocalPlayer(JsonUtility.FromJson<PlayerData>(qPair.package.JsonData), qPair.originSessionId, (Action) qPair.package.Action);
                        break;
                    }
                case PackDataType.OTHER_PLAYERS:
                    {

                        break;
                    }
            }
        }
    }

    void UpdateReplication()
    {
        if (!_isClient && !_isHost) return;

        //Only Client
        SendHello();

        //Only Host
        BroadcastGameData();

        PackageUnwrapper();
    }

    #region Game Data
    //Periodical Update (ONLY HOST)
    //It is the only data in the Game that needs to be updated per frame
    //(do to how the game was thought/designed since Practica1)
    private void BroadcastGameData()
    {
        if (!_isHost) return;
        _server.Broadcast(Serialize(_gameManager._gameData, PackDataType.GAME_DATA, Action.EVENTS));
    }

    private void UnpackGameData(GameData newData)
    {
        if (!_isClient) return;

        _gameManager._gameData.UnpackGameData(ref newData);
    }
    #endregion

    #region Hello
    private void SendHello()
    {
        if (!_isClient) return;
        if (_client._helloPackage) return;

        Debug.Log("Hello sent");

        _client.Send(Serialize("Hello", PackDataType.HELLO, Action.EVENTS));
        _client._helloPackage = true;
    }

    //Return it's session Id
    private void UnpackHello(string hello, string originSessionId)
    {
        if (_isHost) return;
        Debug.Log("Hello -> " + hello);
        if (hello == "Hello")
        {
            //_server.Send(Serialize(originSessionId, PackDataType.HELLO, originSessionId));
        }
    }
    #endregion

    #region Local Player
    public void SendLocalPlayer(Action action, PlayerData data)
    {
        if (_isHost)
        {
            _server.Broadcast(Serialize(data, PackDataType.LOCAL_PLAYER, action));
        }

        if(_isClient)
        {
            _client.Send(Serialize(data, PackDataType.LOCAL_PLAYER, action));
        }
    }

    private void UnpackLocalPlayer(PlayerData data, string originSessionId, Action action)
    {
        switch (action)
        {
            case Action.CREATE:
                {
                    bool exists = false;
                    foreach (var rp in _gameManager._remotePlayers)
                    {
                        if (rp._playerData.netId == data.netId) //Check if player already exists locally
                        {
                            exists = true;
                            break;
                        }     
                    }
                    if (!exists) _gameManager.CreateRemotePlayer(data);

                    break;
                }
            case Action.UPDATE:
                {
                    foreach (var rp in _gameManager._remotePlayers)
                    {
                        Debug.Log(rp._playerData.netId+"|"+data.netId + " -> "+_gameManager._remotePlayers.Count);
                        if (rp._playerData.netId == data.netId)
                        {
                            Debug.Log("Player update!");
                            rp.UpdateDataFromRemote(data);
                            break;
                        }
                    }
                    break;
                }
        }

        //If i'm the host I need to tell all the other clients to do the same(except the one i've recieved the package from)
        if (_isHost)
        {
            _server.Broadcast(Serialize(data, PackDataType.LOCAL_PLAYER, action), originSessionId);
        }
    }
    #endregion
#endregion
}