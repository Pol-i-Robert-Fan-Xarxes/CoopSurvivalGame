using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIController : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject joinGame;
    [SerializeField] private GameObject createGame, editor;

    [Header("Inputs")]
    [SerializeField] private TMP_InputField inp_ip;
    [SerializeField] private TMP_InputField inp_port, inp_playerName;

    [Header("Buttons (some)")]
    [SerializeField] private Button btn_startGame;

    private void Awake()
    {
        joinGame.SetActive(false);
        createGame.SetActive(false);
        editor.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region MainMenu
    public void OnJoinGamePointerEnter()
    {
        Debug.Log("JoinGame");
        joinGame.SetActive(true);
        createGame.SetActive(false);
        editor.SetActive(false);
    }
    public void OnCreateGamePointerEnter()
    {
        Debug.Log("CreateGame");
        joinGame.SetActive(false);
        createGame.SetActive(true);
        editor.SetActive(false);
    }
    public void OnEditorPointerEnter()
    {
        Debug.Log("PlayerEditor");
        joinGame.SetActive(false);
        createGame.SetActive(false);
        editor.SetActive(true);
    }
    public void OnExitClick()
    {
#if UNITY_EDITOR
        // This will stop play mode in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion

    #region Player Editor

    #endregion

    #region JoinGame
    public void OnJoinClick()
    {
        CheckInputEmptyness();
        Debug.Log("Joining to "+inp_ip.text+":"+inp_port.text+" as "+inp_playerName.text+".");
    }
    #endregion

    #region CreateGame
    public void OnCreateClick()
    {
        Debug.Log("Create");
        btn_startGame.interactable = true;
    }
    public void OnStartGameClick()
    {
        if (string.IsNullOrEmpty(inp_playerName.text))
        {
            inp_playerName.text = "Player Host";
        }
        Debug.Log("Starting a game as " + inp_playerName.text + ".");
    }
    #endregion

    private void CheckInputEmptyness()
    {
        if(string.IsNullOrEmpty(inp_ip.text))
        {
            inp_ip.text = "127.0.0.1";
        }

        if (string.IsNullOrEmpty(inp_port.text))
        {
            inp_port.text = "9050";
        }

        if (string.IsNullOrEmpty(inp_playerName.text))
        {
            inp_playerName.text = "Player Client";
        }
    }
}
