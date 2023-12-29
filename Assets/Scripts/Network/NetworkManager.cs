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
    ENEMY //to send Data from enemies 
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
    public bool _isHost = false;
    public bool _isClient = false;
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
        //Skip if it's a single player game
        if (_gameManager._singlePlayer) return;

        if (_isHost && _server != null)
            if (!_server._running) _server = null;
        if (!_isHost && _client != null)
            if (!_client.IsRunning)
            {
                _client = null;
                _isClient = false;
                if (_gameManager._gameData._scene == 1) _gameManager.ExecuteConnectionLost();
            }
       

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
            _client.Shutdown();
            _client = null;
            _isClient = false;
        }
    }

    #region Serialize/Deserialize

    public static byte[] Serialize<T>(T data, PackDataType dataType, Action action)
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

    //Deserializes and saves the package in a queue
    public static void Deserialize(byte[] data, int size, string originSessionId = "")
    {
        string json = Encoding.ASCII.GetString(data, 0, size);

        //Deserialize the package
        Package package = JsonUtility.FromJson<Package>(json);

        _recievePackageQueue.Enqueue(new QueuePair(originSessionId, package));

        //return JsonUtility.FromJson<GameInfo>(json);
    }

    #endregion Serialize/Deserialize

    // True == Connection Accepted || False == Connection Denied or Server not found
    public void ClientAwaitConfirmation(float seconds)
    {
        Timer timer = new Timer();
        timer.totalTime = seconds;

        timer.Start();
        while (timer.IsRunning)
        {
            timer.Update();

            if (_client._helloBackPackage)
            {
                _client.feedbackText = "Connected to the server";
                break;
            }
        }

        _client.feedbackText = "Server connection rejected or server not found.";
    }

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
                        UnpackHello(JsonUtility.FromJson<string>(qPair.package.JsonData), (Action) qPair.package.Action, qPair.originSessionId);
                        break;
                    }
                case PackDataType.LOCAL_PLAYER:
                    {
                        UnpackLocalPlayer(JsonUtility.FromJson<PlayerData>(qPair.package.JsonData), qPair.originSessionId, (Action) qPair.package.Action);
                        break;
                    }
                case PackDataType.ENEMY:
                    {
                        UnpackEnemy(JsonUtility.FromJson<EnemyData>(qPair.package.JsonData), qPair.originSessionId, (Action)qPair.package.Action);
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
        Ping();

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
    
    // Package only sent by clients when they try to connect to a server
    private void SendHello()
    {
        if (_isHost) return;
        if (_client._helloPackage) return;

        Debug.Log("Hello sent");

        _client.Send(Serialize("Hello", PackDataType.HELLO, Action.EVENTS));
        _client._helloPackage = true;
    }

    //Only for the server, handles what to do when a hello package is received
    private void UnpackHello(string hello, Action action, string originSessionId)
    {
        if (_isHost) return;

        if (_isClient)
        {
            Debug.Log("Hello -> " + action.ToString());
            if (action == Action.DESTROY)
            { //Rejected connection
                _client.feedbackText = "Connection rejected by the host";
                
            }
            else if (action == Action.EVENTS)
            { //Accepted connection
                _client._helloBackPackage = true;
                _client.feedbackText = "Waiting host to start";
            }
            
        }
        else if (_isHost)
        { //Return hello
            if (action == Action.EVENTS) // Confirmation sent to client
            {
                _server.Send(Serialize("Hello", PackDataType.HELLO, Action.EVENTS), originSessionId);
            }
        }
    }
    #endregion

    #region Ping
    private void Ping()
    {

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
                        if (rp._playerData.netId == data.netId)
                        {
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

    #region Enemy
    
    public void SendEnemy(Action action, EnemyData data)
    {
        if (_isHost)
        {
            _server.Broadcast(Serialize(data, PackDataType.ENEMY, action));
        }

        if (_isClient)
        {
            _client.Send(Serialize(data, PackDataType.ENEMY, action));
        }
    }
    
    private void UnpackEnemy(EnemyData data, string originNetId, Action action)
    {
        switch(action)
        {
            case Action.CREATE:
                {
                    if (_isClient)
                    {
                        //Tell clients to create enemies inside their enemypool and give them the NetId
                        //_gameManager._enemyManager.InstantiateEnemyClient(data);
                        GameObject.FindGameObjectWithTag("Enemy Manager").GetComponent<EnemyManager>().InstantiateEnemyClient(data);
                    }
                    break;
                }
            case Action.UPDATE:
                {
                    // Host and Client
                    // Update all enemy data searching by netid
                    _gameManager._enemyManager.UpdateRemote(data);

                    if (_isHost)
                    {
                        //Broadcast to all other clients
                        _server.Broadcast(Serialize(data, PackDataType.ENEMY, Action.UPDATE), originNetId);
                    }
                    break;
                }
        }
    }
    #endregion

    #endregion
}