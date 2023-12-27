using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;

public struct Connection
{
    public EndPoint IP;
    public string NetID;
    public float LastPing;

    public Connection(EndPoint endPoint, string netId, float lastPing = 0)
    {
        IP = endPoint;
        NetID = netId;
        LastPing = lastPing;
    }
}

public class Server
{
    public int PORT = 9050;

    private Socket _socket;
    private IPEndPoint _ipep;
    //private Dictionary<EndPoint, string> _clients; // <IP, NetID>
    private Dictionary<EndPoint, Connection> _clients;
    private Queue<EndPoint> removeClientQueue;

    public int maxClient = 1;

    private byte[] _bufferReceive;

    public bool _connected = true;
    public bool _running = true;

    public int receiveTimeout = 100;
    public bool _isAPlayerConnected = false;

    public string feedbackText = "";

    public Server()
    {
    }

    public void Initialize()
    {
        _bufferReceive = new byte[4096];
        _ipep = new IPEndPoint(IPAddress.Any, 9050);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //_clients = new Dictionary<EndPoint, string>();
        _clients = new Dictionary<EndPoint, Connection>();
        removeClientQueue = new Queue<EndPoint>();
    }

    public NetworkFeedback StartServer()
    {
        _socket.Bind(_ipep);
        //_remote = (EndPoint) new IPEndPoint(IPAddress.Any, 0);

        Thread receiveThread = new Thread(Receive);
        receiveThread.Start();

        return NetworkFeedback.SERVER_SUCCESS;
    }

    #region Receive
    private void Receive()
    {
        try {
            EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (_connected)
            {
                _socket.ReceiveTimeout = receiveTimeout;
                try { 
                    int recv = 0;

                    recv = _socket.ReceiveFrom(_bufferReceive, ref senderEndPoint);

                    //Checks if it's a new client
                    CheckConnection(ref senderEndPoint);

                    // If the received package is from a connected client
                    if (_clients.ContainsKey(senderEndPoint))
                    {
                        //Deserializes and saves the package in a package processing queue.
                        NetworkManager.Deserialize(_bufferReceive, recv, _clients[senderEndPoint].NetID);
                    }  
                }
                catch(SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        // No data received within the timeout, continue the loop
                        continue;
                    }
                    else if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        Debug.Log("Ip '" + senderEndPoint + "' disconnected.");
                        if (_clients.ContainsKey(senderEndPoint))
                        {
                            Debug.Log("'"+senderEndPoint+"' enqueued to the removal list");
                            
                            //HANDLE WHAT HAPPENS WHEN A PLAYER DISCONNECTS 
                            removeClientQueue.Enqueue(senderEndPoint);
                        }
                    }
                    else
                    {
                        // Handle other socket exceptions
                        Debug.LogError("SocketException: " + ex.Message);
                        _connected = false;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Receive Thread Error: " + e.Message);
        }
        finally
        {
            _socket.Close();
            _running = false;
            Debug.Log("Server Recieve thread closed!");
        }
    }

    //Checks if it's a new client, if it is then checks if the server can accept new clients.
    // If the server can't accept new clients, it sents a reject connection package
    private void CheckConnection(ref EndPoint endPoint)
    {
        if (!_clients.ContainsKey(endPoint))
        {
            if (maxClient > _clients.Count )
            {
                string netId = System.Guid.NewGuid().ToString(); //temporal netId, the netid is uodated to the one of the character once received.
                _clients.Add(endPoint, new Connection(endPoint, System.Guid.NewGuid().ToString()));
                Debug.Log(netId);
                feedbackText = "Online - " + (GetNumOfClients()+1) + "/" + (GetMaxClients()+1);
            }
            else
            {
                Debug.Log("Reject package sent");
                //Reject connection package
                byte[] data = NetworkManager.Serialize((string) "Hello", PackDataType.HELLO, Action.DESTROY);
                _socket.SendTo(data, data.Length, SocketFlags.None, endPoint);
            }
        }
    }

    #endregion

    #region Send

    public void Send(byte[] data, string clientNetID)
    {
        CheckRemoval();
        try
        {
            foreach (EndPoint clientEndPoint in _clients.Keys)
            {
                if (_clients[clientEndPoint].NetID == clientNetID)
                {
                    _socket.SendTo(data, data.Length, SocketFlags.None, clientEndPoint);
                    break; //Skips once found
                }
            }

            _isAPlayerConnected = true;
        }
        catch (SocketException ex)
        {

            _isAPlayerConnected = false;
        }
    }

    public void Broadcast(byte[] data, string exception = "")
    {
        CheckRemoval();
        try
        {
            foreach (EndPoint clientEndPoint in _clients.Keys) 
            {
                if (_clients[clientEndPoint].NetID == exception) continue; //Skips exception

                _socket.SendTo(data, data.Length, SocketFlags.None, clientEndPoint);
            }
            _isAPlayerConnected = true;
        }
        catch (SocketException ex)
        {
            
            _isAPlayerConnected = false;
        }
    }
    #endregion

    private void CheckRemoval()
    {
        if (removeClientQueue.Count == 0) return;
        EndPoint clientEndPoint = removeClientQueue.Dequeue();

        _clients.Remove(clientEndPoint);

        if (GameManager.Instance._gameData._scene == 0)
        { //if it is the main menu scene
            feedbackText = "Online - " + (GetNumOfClients() + 1) + "/" + (GetMaxClients() + 1);
        }
        else
        { //If it is the game scene. 

        }

        Debug.Log("Removed from dictionary");
    }

    #region Getters/setters

    public int GetMaxClients()
    {
        return maxClient;
    }

    public int GetNumOfClients()
    {
        return _clients.Count;
    }
 #endregion
}