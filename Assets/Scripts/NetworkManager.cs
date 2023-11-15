using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    
    public static NetworkManager _instance;

    private void Awake()
    {
        if (_instance == null) _instance = this;
    }
}