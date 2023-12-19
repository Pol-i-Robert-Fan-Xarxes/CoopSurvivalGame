using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

public class Server
{
    public int PORT = 9050;

    private Socket _socket;
    private IPEndPoint _ipep;
    private Dictionary<EndPoint, string> _clients;
    //private EndPoint _remote;

    private byte[] _bufferReceive;

    public bool _connected = true;
    public bool _running = true;

    public int reciveTimeout = 100;
    public bool _isAPlayerConnected = false;

    public Server()
    {
    }

    public void Initialize()
    {
        _bufferReceive = new byte[4096];
        _ipep = new IPEndPoint(IPAddress.Any, 9050);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _clients = new Dictionary<EndPoint, string>();
    }

    public NetworkFeedback StartServer()
    {
        _socket.Bind(_ipep);
        //_remote = (EndPoint) new IPEndPoint(IPAddress.Any, 0);

        Thread recibeThread = new Thread(Recieve);
        recibeThread.Start();

        return NetworkFeedback.SERVER_SUCCESS;
    }

    #region Recieve
    private void Recieve()
    {
        try { 
            while (_connected)
            {
                _socket.ReceiveTimeout = reciveTimeout;
                try { 
                    int recv = 0;
                    EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    recv = _socket.ReceiveFrom(_bufferReceive, ref senderEndPoint);

                    if (!_clients.ContainsKey(senderEndPoint))
                    {
                        string netId = System.Guid.NewGuid().ToString();
                        _clients.Add(senderEndPoint, System.Guid.NewGuid().ToString());
                        Debug.Log(netId);
                    }

                    NetworkManager.Deserialize(_bufferReceive, recv, _clients[senderEndPoint]);
                }
                catch(SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        // No data received within the timeout, continue the loop
                        continue;
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
    #endregion

    #region Send

    //public void Send(byte[] data, string clientNetID)
    //{
    //    try
    //    {
    //        if (_clients.ContainsValue(clientNetID)) 
    //        {
    //            _socket.SendTo(data, data.Length, SocketFlags.None, _clients.First(clientNetID).Key);
    //        }
            
    //        _isAPlayerConnected = true;
    //    }
    //    catch (SocketException ex)
    //    {

    //        _isAPlayerConnected = false;
    //    }
    //}

    public void Broadcast(byte[] data, string exception = "")
    {
        try
        {
            foreach (EndPoint clientEndPoint in _clients.Keys) 
            {
                if (_clients[clientEndPoint] == exception) continue; //Skips exception

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
}