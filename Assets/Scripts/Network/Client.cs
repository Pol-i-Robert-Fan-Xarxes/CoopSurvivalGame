using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;

public class Client
{
    public int PORT = 9050;

    private Socket _socket;
    private EndPoint _endPoint;

    private byte[] _bufferRecive;
    private ArraySegment<byte> _bufferReciveSegment;

    private bool _connected = true;

    public IPAddress _hostIPAddress;

    public void Initialize()
    {
        _bufferRecive = new byte[4096];
        _bufferReciveSegment = new(_bufferRecive);

        _endPoint = new IPEndPoint(_hostIPAddress, PORT);

        _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Bind(_endPoint);
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
        }
    }
    #endregion

    #region Send
    private void Send()
    {

    }
    #endregion
}
