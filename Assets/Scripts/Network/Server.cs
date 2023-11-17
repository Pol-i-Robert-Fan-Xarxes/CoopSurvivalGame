using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine;
using System.Threading;

public class Server
{
    public int PORT = 9050;

    private Socket _socket;
    private EndPoint _endPoint;

    private byte[] _bufferReceive;
    private ArraySegment<byte> _bufferReceiveSegment;

    public bool _connected = true;
    public bool _running = true;

    public void Initialize()
    {
        _bufferReceive = new byte[4096];
        _bufferReceiveSegment = new(_bufferReceive);

        _endPoint = new IPEndPoint(IPAddress.Any, PORT);

        _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        
    }

    public NetworkFeedback StartServer()
    {
        _socket.Bind(_endPoint);

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
    private void Send()
    {

        // SEND GAMEINFO FROM HOST  // OPTIONAL AND GAMEINFO FROM CLIENT IN SERVER

        //Serialize

    }
    #endregion
}
