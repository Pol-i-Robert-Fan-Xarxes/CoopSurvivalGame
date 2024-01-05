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

    private bool _connected = false;
    
    private bool _running = true;
    public bool IsRunning => _running;

    public bool _helloPackage = false; // Saves if the hello package has been sent
    public bool _helloBackPackage = false; //Saves if the hello back package has been received
    public string feedbackText = "";

    public IPAddress _hostIPAddress;

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

    public void Shutdown()
    {
        _connected = false;
    }

    public NetworkFeedback ConnectToHost()
    {
        try
        {
            _socket.Connect(_ipep);
            _connected = true;

            // Start the receive thread
            Thread receiveThread = new Thread(Receive);
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

    #region Receive
    private void Receive()
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
                    feedbackText = "Waiting host";
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        if (GameManager._instance._gameData._scene == 0)
                        { //If the Main menu scene, try to reconnect
                            feedbackText = "Couldn't connect to a host server.";
                            Debug.LogAssertion(feedbackText);

                            Thread.Sleep(1000);
                            Initialize();
                            _socket.Connect(_ipep);
                        }
                        else
                        { //If in a game, connection lost
                            feedbackText = "Connection lost with the host";
                            _connected = false;
                        }
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
        catch (SocketException)
        {

        }
    }

    #endregion
}
