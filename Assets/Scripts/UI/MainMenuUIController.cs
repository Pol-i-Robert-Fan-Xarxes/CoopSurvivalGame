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
    [SerializeField] private Button btn_joinCancel;
    [SerializeField] private Button btn_hostCancel;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI txt_feedback;

    public NetworkManager _networkManager;

    private void Awake()
    {
        joinGame.SetActive(false);
        createGame.SetActive(false);
        editor.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        txt_feedback.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    #region MainMenu
    public void OnJoinGamePointerEnter()
    {
        joinGame.SetActive(true);
        createGame.SetActive(false);
        editor.SetActive(false);
    }
    public void OnCreateGamePointerEnter()
    {
        joinGame.SetActive(false);
        createGame.SetActive(true);
        editor.SetActive(false);
    }
    public void OnEditorPointerEnter()
    {
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
        
        NetworkFeedback netFeed = _networkManager.ConnectToServer(inp_ip.text, inp_port.text);
    
        if (netFeed == NetworkFeedback.CONNECTION_SUCCESS)
        {
            txt_feedback.text = "Waiting for the host to start";
        }
        else if (netFeed == NetworkFeedback.SERVER_ERROR)
        {
            txt_feedback.text = "Error connecting to the server!";
        }
        btn_joinCancel.gameObject.SetActive(true);
    }

    public void OnJoinCancelClick()
    {
        _networkManager.ForceConnectionClose();
        txt_feedback.text = "";
        btn_joinCancel.gameObject.SetActive(false);
    }
    #endregion

    #region CreateGame
    public void OnCreateClick()
    {
        btn_startGame.interactable = true;

        NetworkFeedback netFeed = _networkManager.StartServer();

        if (netFeed == NetworkFeedback.SERVER_SUCCESS) 
        {
            txt_feedback.text = "Online";
        }

        btn_hostCancel.gameObject.SetActive(true);
    }
    public void OnHostCancelClick()
    {
        _networkManager.ForceConnectionClose();
        txt_feedback.text = "";
        btn_hostCancel.gameObject.SetActive(false);
        btn_startGame.interactable = false;
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
