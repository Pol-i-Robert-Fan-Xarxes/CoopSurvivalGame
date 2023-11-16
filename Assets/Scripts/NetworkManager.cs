using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    
    public static NetworkManager _instance;

    private bool _isHost = false;
    public bool _hasSent = false;

    private void Awake()
    {
        if (_instance == null) _instance = this;
    }

    private void Update()
    {
        
    }

    private void Setup()
    {

    }
}