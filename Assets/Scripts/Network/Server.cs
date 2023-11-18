using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine;
using System.Threading;
using System.Text;

public class Server
{
    public int PORT = 9050;

    private Socket _socket;
    private IPEndPoint _ipep;
    private EndPoint _remote;

    private byte[] _bufferReceive;

    public bool _connected = true;
    public bool _running = true;
    public bool _newPackage = false;


    public GameInfo _recvInfo = new GameInfo();
    public GameInfo GetPackage()
    {
        if (_newPackage)
        {
            _newPackage = false;
            return _recvInfo;
        }

        return null;
    }

    public Server()
    {
    }

    public void Initialize()
    {
        _bufferReceive = new byte[4096];
        _ipep = new IPEndPoint(IPAddress.Any, 9050);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public NetworkFeedback StartServer()
    {
        _socket.Bind(_ipep);
        _remote = (EndPoint) new IPEndPoint(IPAddress.Any, 0);

        Thread recibeThread = new Thread(Recieve);
        recibeThread.Start();

        return NetworkFeedback.SERVER_SUCCESS;
    }

    #region Recieve
    private void Recieve()
    {
        try
        {
            while (_connected)
            {
                int recv = 0;
                recv = _socket.ReceiveFrom(_bufferReceive, ref _remote);
                _recvInfo = NetworkManager.Deserialize(_bufferReceive, recv);
                _newPackage = true;
            }
        }
        catch(System.Exception e)
        {
            Debug.Log("Recibe Thread Error: " + e.Message);
        }
        finally
        {
            _socket.Close();
            _running = false;
            Debug.Log("Server Recibe closed!");
        } 
    }
    #endregion

    #region Send
    public void Send(byte[] data)
    {
        _socket.SendTo(data, data.Length, SocketFlags.None, _remote);
    }
    #endregion
}