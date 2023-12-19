using System;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Threading;

public class Client
{
    public int PORT = 9050;

    private Socket _socket;
    private IPEndPoint _ipep;
    private EndPoint _remote;

    private byte[] _bufferReceive;
    private ArraySegment<byte> _bufferReceiveSegment;

    public bool _connected = false;
    public bool _running = true;

    public bool _helloPackage = false;
    //public bool _newPackage = false;

    public IPAddress _hostIPAddress;
    //private GameInfo _recvInfo = new GameInfo();

    //public GameInfo GetPackage()
    //{
    //    if (_newPackage)
    //    {
    //        _newPackage = false;
    //        return _recvInfo;
    //    }

    //    return null;
    //}

    public Client(string ip, string port)
    {
        _hostIPAddress = IPAddress.Parse(ip);
        PORT = int.Parse(port);
    }

    public void Initialize()
    {
        _bufferReceive = new byte[4096];
        _bufferReceiveSegment = new(_bufferReceive);

        _ipep = new IPEndPoint(_hostIPAddress, PORT);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _remote = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
    }

    public NetworkFeedback ConnectToHost()
    {
        try
        {
            _socket.Connect(_ipep);
            _connected = true;

            // Start the receive thread
            Thread receiveThread = new Thread(Recieve);
            receiveThread.Start();

            return NetworkFeedback.CONNECTION_SUCCESS;
        }
        catch (SocketException ex)
        {
            if (ex.SocketErrorCode == SocketError.HostNotFound)
            {
                Debug.LogError("Host not found. Check the server address and try again.");
            }
            else if (ex.SocketErrorCode == SocketError.ConnectionRefused)
            {
                Debug.LogError("Connection refused. The server may not be running or is unreachable.");
            }
            else
            {
                Debug.LogError($"Error connecting to the server: {ex.Message}");
            }
            return NetworkFeedback.CONNECTION_ERROR;
        }
    }

    #region Recieve
    private void Recieve()
    {
        try
        {
            while (_connected)
            {
                try
                {
                    int recv = 0;
                    recv = _socket.ReceiveFrom(_bufferReceive, ref _remote);
                    //_recvInfo = NetworkManager.Deserialize(_bufferReceive, recv);
                    NetworkManager.Deserialize(_bufferReceive, recv);
                    //_newPackage = true;
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        Debug.Log("Couldn't connect to a host server.");
                        Thread.Sleep(1000);
                        Initialize();
                        _socket.Connect(_ipep);
                    }
                    else
                    {
                        Debug.Log("Recieve Thread Error: " + ex.Message);
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
            Debug.Log("Client Recieve thread closed!");
        }
    }
    #endregion

    #region Send
    public void Send(byte[] data)
    {
        try
        {
            _socket.SendTo(data, data.Length, SocketFlags.None, _ipep);
        }
        catch (SocketException ex)
        {

        }
    }
    #endregion
}
