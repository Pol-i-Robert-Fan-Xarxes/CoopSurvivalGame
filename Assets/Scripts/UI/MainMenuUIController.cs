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
    [SerializeField] private Button btn_join;
    [SerializeField] private Button btn_hostCancel;
    [SerializeField] private Button btn_joinGame;

    [SerializeField] private Button btn_option0;
    [SerializeField] private Button btn_option1;
    [SerializeField] private Button btn_option2;
    [SerializeField] private Button btn_option3;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI txt_feedback;

    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private GameManager _gameManager;

    bool _gameCreated = false;
    bool _rejoinCDActive = false;

    float _rejoinCDCount = 0;
    float _rejoinCD = 1.1f;

    private void Awake()
    {
        joinGame.SetActive(false);
        createGame.SetActive(false);
        editor.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        _networkManager = GameObject.FindGameObjectWithTag("NetManager").GetComponent<NetworkManager>();
        txt_feedback.text = "";
        btn_option0.interactable = false;
        _gameManager._gameData.skin = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (_networkManager._client != null) txt_feedback.text = _networkManager._client.feedbackText;
        //else OnJoinCancelClick();

        if (_networkManager._server != null) txt_feedback.text = _networkManager._server.feedbackText;

        //Server related
        //Updates the front-end that shows the number of players and the button to start the game (minimum 2 players to play)
        if (_gameCreated && _networkManager._server != null)
        {
            int numConn = 1 + _networkManager._server.GetNumOfClients();

            if (numConn > 1) btn_startGame.interactable = true;
            else btn_startGame.interactable = false;
        }

        //Client related
        // Just a hard coded cooldown for the client Join Game button. 
        if (_rejoinCDActive)
        {
            _rejoinCDCount += Time.deltaTime;
            if (_rejoinCDCount > _rejoinCD)
            {
                btn_join.interactable = true;
                _rejoinCDActive = false;
                _rejoinCDCount = 0;
            }
        }

    }

    

    #region MainMenu
    
    public void OnSinglePlayerClick()
    {
        _gameManager._singlePlayer = true;
        
        if (string.IsNullOrEmpty(inp_playerName.text))
        {
            inp_playerName.text = "Player 1";
        }
        _gameManager._gameData._localPlayerName = inp_playerName.text;

        _gameManager._gameData._scene = 1;
    }

    public void OnJoinGamePointerEnter()
    {
        if (!btn_joinGame.interactable) return;
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

    public void OnSkinClick0()
    {
        btn_option0.interactable = false;
        btn_option1.interactable = true;
        btn_option2.interactable = true;
        btn_option3.interactable = true;
        _gameManager._gameData.skin = 0;
    }
    public void OnSkinClick1()
    {
        btn_option0.interactable = true;
        btn_option1.interactable = false;
        btn_option2.interactable = true;
        btn_option3.interactable = true;
        _gameManager._gameData.skin = 1;
    }
    public void OnSkinClick2()
    {
        btn_option0.interactable = true;
        btn_option1.interactable = true;
        btn_option2.interactable = false;
        btn_option3.interactable = true;
        _gameManager._gameData.skin = 2;
    }
    public void OnSkinClick3()
    {
        btn_option0.interactable = true;
        btn_option1.interactable = true;
        btn_option2.interactable = true;
        btn_option3.interactable = false;
        _gameManager._gameData.skin = 3;
    }

    #endregion

    #region JoinGame
    public void OnJoinClick()
    {
        CheckInputEmptyness();

        _gameManager._gameData._localPlayerName = inp_playerName.text;
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
        btn_join.gameObject.SetActive(false);
        btn_joinGame.interactable = false;
    }

    public void OnJoinCancelClick()
    {
        _networkManager.ForceConnectionClose();
        txt_feedback.text = "";
        btn_joinCancel.gameObject.SetActive(false);
        btn_join.gameObject.SetActive(true);
        btn_join.interactable = false;
        _rejoinCDActive = true;
    }
    #endregion

    #region CreateGame
    public void OnCreateClick()
    {
        _gameCreated = true;
        // btn_startGame.interactable = true;
        btn_joinGame.interactable = false;

        NetworkFeedback netFeed = _networkManager.StartServer();

        if (netFeed == NetworkFeedback.SERVER_SUCCESS) 
        {
            _networkManager._server.feedbackText = "Online - " + 
                (_networkManager._server.GetNumOfClients() + 1) + "/" + 
                (_networkManager._server.GetMaxClients() + 1);
        }

        btn_hostCancel.gameObject.SetActive(true);
    }
    public void OnHostCancelClick()
    {
        _gameCreated = false;
        _networkManager.ForceConnectionClose();
        txt_feedback.text = "";
        btn_hostCancel.gameObject.SetActive(false);
        btn_startGame.interactable = false;
        btn_joinGame.interactable = true;
    }
    public void OnStartGameClick()
    {
        if (string.IsNullOrEmpty(inp_playerName.text))
        {
            inp_playerName.text = "Player 1";
        }
        _gameManager._gameData._localPlayerName = inp_playerName.text;

        _gameManager._gameData._scene = 1;

        Debug.Log("Starting a game as " + inp_playerName.text + ".");
    }
    #endregion

    private void CheckInputEmptyness()
    {
        if (string.IsNullOrEmpty(inp_ip.text))
        {
            inp_ip.text = "127.0.0.1";
        }

        if (string.IsNullOrEmpty(inp_port.text))
        {
            inp_port.text = "9050";
        }

        if (string.IsNullOrEmpty(inp_playerName.text))
        {
            inp_playerName.text = "Player 2";
        }
    }
}
