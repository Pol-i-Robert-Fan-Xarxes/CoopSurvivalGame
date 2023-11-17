using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;


public class NetworkManager : MonoBehaviour
{
    public static NetworkManager _instance;
    public static NetworkManager GetInstance() { return _instance; }

    private bool _isHost = false;
    public bool _hasSent = false;

    public Server _server;
    public Client _client;

    private void Awake()
    {
        if (_instance == null) _instance = this;
    }

    private void Start()
    {
        
    }

    private void Update()
    {

    }

    public void StartServer()
    {
        _isHost = true;
        _server = new Server();
    }

    public void ConnectToServer()
    {

    }

    #region Recibe
    private void Recibe()
    {

    }
    #endregion

    #region Send
    private void Send()
    {

    }
    #endregion
}