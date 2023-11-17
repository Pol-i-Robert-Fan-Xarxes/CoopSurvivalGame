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
    private SocketReceiveMessageFromResult _res;
    private IPEndPoint _ipep;
    private EndPoint _remote;

    private byte[] _bufferReceive;
    private ArraySegment<byte> _bufferReceiveSegment;

    public bool _connected = true;
    public bool _running = true;

    private NetworkManager _networkManager;
    public bool _changeOrder = false;

    public Server()
    {
        _networkManager = NetworkManager.Instance;
    }

    public void Initialize()
    {
        _bufferReceive = new byte[4096];
        _bufferReceiveSegment = new(_bufferReceive);

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
                //DESERIALIZE CLIENT INFO AND UPDATE IT TO GAMEMANAGER'S CLIENT PLAYER

                int recv = 0;
                recv = _socket.ReceiveFrom(_bufferReceive, ref _remote);

                string msg = Encoding.ASCII.GetString(_bufferReceive, 0, recv);

                Debug.Log("Server Received: "+msg);

                if (msg.Equals("/OkayCorazon48465645189/"))
                {
                    _changeOrder = true;
                }
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
    public void Send(byte[]data)
    {
        // SEND GAMEINFO FROM HOST  // OPTIONAL AND GAMEINFO FROM CLIENT IN SERVER
        //Serialize

        _socket.SendTo(data, data.Length, SocketFlags.None, _remote);
        Debug.Log("ServerData Send");

    }
    #endregion
}
