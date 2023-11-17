using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Threading;
using System.Text;

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

    public IPAddress _hostIPAddress;

    private NetworkManager _networkManager;

    public Client(string ip, string port)
    {
        _hostIPAddress = IPAddress.Parse(ip);
        PORT = int.Parse(port);

        _networkManager = NetworkManager.Instance;
    }

    public void Initialize()
    {
        _bufferReceive = new byte[4096];
        _bufferReceiveSegment = new(_bufferReceive);

        _ipep = new IPEndPoint(_hostIPAddress, PORT);

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public NetworkFeedback ConnectToHost()
    {
        try
        {
            _socket.Connect(_ipep);
            _remote = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
            _connected = true;

            Thread recibeThread = new Thread(Recieve);
            recibeThread.Start();

            byte[] data = new byte[1024];
            data = Encoding.ASCII.GetBytes("Player 2");
            Send(data);

            return NetworkFeedback.CONNECTION_SUCCESS;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting to the server: {e.Message}");
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
                int recv = 0;
                recv = _socket.ReceiveFrom(_bufferReceive, ref _remote);

                string msg = Encoding.ASCII.GetString(_bufferReceive, 0, recv);

                Debug.Log("Client Received: " + msg);

                if (msg.Equals("/PacoCanviaALaCoolScene48465645189/"))
                {
                    byte[] data = new byte[1024];
                    data = Encoding.ASCII.GetBytes("/OkayCorazon48465645189/");
                    Send(data);

                    _networkManager.LoadScene();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Recibe Thread Error: " + e.Message);
        }
        finally
        {
            _socket.Close();
            _running = false;
            Debug.Log("Client Recibe closed!");
        }
    }
    #endregion

    #region Send
    public void Send(byte[] data)
    {
        _socket.SendTo(data, data.Length, SocketFlags.None, _ipep);
        Debug.Log("ClientData Send");
    }
    #endregion
}
