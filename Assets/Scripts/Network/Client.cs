using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Threading;

public class Client
{
    public int PORT = 9050;

    private Socket _socket;
    private EndPoint _endPoint;

    private byte[] _bufferRecive;
    private ArraySegment<byte> _bufferReciveSegment;

    public bool _connected = false;
    public bool _running = true;

    public IPAddress _hostIPAddress;

    public Client(string ip, string port)
    {
        _hostIPAddress = IPAddress.Parse(ip);
        PORT = int.Parse(port);
    }

    public void Initialize()
    {
        _bufferRecive = new byte[4096];
        _bufferReciveSegment = new(_bufferRecive);

        _endPoint = new IPEndPoint(_hostIPAddress, PORT);

        _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Bind(_endPoint);

    }

    public NetworkFeedback ConnectToHost()
    {
        try
        {
            _socket.Connect(_endPoint);
            _connected = true;

            Thread recibeThread = new Thread(Recibe);
            recibeThread.Start();

            return NetworkFeedback.CONNECTION_SUCCESS;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting to the server: {e.Message}");
            return NetworkFeedback.CONNECTION_ERROR;
        }
    }

    #region Recibe
    private void Recibe()
    {
        try
        {
            while (_connected)
            {

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
    private void Send()
    {

    }
    #endregion
}
