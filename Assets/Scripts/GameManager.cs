using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;

    public Player _localPlayer;

    private void Awake()
    {
        if (_instance == null) _instance = this;
    }

    void Start()
    {
        
    }
}