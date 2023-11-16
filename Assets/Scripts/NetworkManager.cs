using System.Collections;
using System.Collections.Generic;
using System.Text;
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
    
    #region Serialize/Deserialize

    /// <summary>
    /// This function asks for a gameinfo to be serialized and outputs that serialized data converted in the byte array you just passed
    /// </summary>
    /// <param name="infoToSend"></param>
    /// <param name="data"></param>
    public void Serialize(GameInfo infoToSend,ref byte[] data)
    {
        //Convert from our InfoStruct to json
        string json = JsonUtility.ToJson(infoToSend);

        data = new byte[json.Length];
        //From Json to byte array
        data = Encoding.ASCII.GetBytes(json);

    }

    /// <summary>
    /// Asks for the data byte array and returns the game info with the data 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public void Deserialize(byte[] data)
    {

        //Convert from our byte array to string json
        string json = Encoding.UTF8.GetString(data);

        GameInfo info = JsonUtility.FromJson<GameInfo>(json);

        Debug.Log("JSON despues de la deserializaci�n: " + json);

        //Update GameManager's Data with game info
        info.SetInfo();
        
    }

    #endregion Serialize/Deserialize

}