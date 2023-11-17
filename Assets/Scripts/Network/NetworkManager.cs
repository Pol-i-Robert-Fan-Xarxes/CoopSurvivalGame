using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum NetworkFeedback
{
    CONNECTION_ERROR,
    CONNECTION_SUCCESS,
    SERVER_ERROR,
    SERVER_SUCCESS
}

public enum ServerCommand
{
    EMPTY,
    PAUSED,
    START_GAME
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager _instance;
    public static NetworkManager Instance => _instance;
    //public static NetworkManager GetInstance() 
    //{
    //    if (_instance == null) _instance = new NetworkManager();
    //    return _instance; 
    //}

    private bool _isHost = false;
    public bool _hasSent = false;

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
        
    }

    private void Update()
    {

        if (_isHost && _server != null && !_server._running) _server = null;
        if (!_isHost && _client != null && !_client._running) _client = null;

        //JUST FOR TESTING
        if (_isHost && _server != null)
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                byte[] data = new byte[1024];
                data = Encoding.ASCII.GetBytes("Bon dia senyor Client.");
                _server.Send(data);
            }

            if (Input.GetKeyDown(KeyCode.F1)) 
            {
                byte[] data = new byte[1024];
                data = Encoding.ASCII.GetBytes("/PacoCanviaALaCoolScene48465645189/");
                _server.Send(data);
            }

            if(_server._changeOrder) LoadScene();
        }
        //JUST FOR TESTING
        if (Input.GetKeyDown(KeyCode.F9) && !_isHost && _client != null)
        {
            byte[] data = new byte[1024];
            data = Encoding.ASCII.GetBytes("Bon dia senyor Servidor.");
            _client.Send(data);
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
        }
        else
        { //Client
            _client._connected = false;
        }
    }

    #region Recieve
    public static void Recieve()
    {
       
    }
    #endregion

    #region Send
    private void Send()
    {
        // Serialize
    }
    #endregion

    #region Serialize/Deserialize

    /// <summary>
    /// This function asks for a gameinfo to be serialized and outputs that serialized data converted in the byte array you just passed
    /// </summary>
    /// <param name="infoToSend"></param>
    /// <param name="data"></param>
    public void Serialize(GameInfo infoToSend, ref byte[] data)
    {
        //Convert from our InfoStruct to json
        string json = JsonUtility.ToJson(infoToSend);

        //From Json to byte array
        data = Encoding.ASCII.GetBytes(json);

    }

    /// <summary>
    /// Asks for the data byte array and returns the game info with the data 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public void Deserialize(byte[] data)
    {

        //Convert from our byte array to string json
        string json = Encoding.UTF8.GetString(data);

        GameInfo info = JsonUtility.FromJson<GameInfo>(json);

        Debug.Log("JSON despues de la deserializaci�n: " + json);

        //Update GameManager's Data with game info
        info.SetInfo();

    }

    #endregion Serialize/Deserialize

    //JUST FOR TESTING
    //The following 2 methods will be deleted.
    public void LoadScene()
    {
        SceneManager.LoadScene(1);
    }
}