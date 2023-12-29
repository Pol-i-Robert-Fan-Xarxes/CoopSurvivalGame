using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WinMenuUIController : MonoBehaviour
{
    [Header("Buttons (some)")]
    [SerializeField] private Button btn_ExitToMenu;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI txt_KillCount;

    // Start is called before the first frame update
    void Start()
    {
        txt_KillCount.text = GameManager.Instance.numberOfDeaths.ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ReturnToMenu()
    {
        GameManager.Instance._gameData._scene = 0;
        GameManager.Instance._singlePlayer = false;
        GameManager.Instance.SetPlayersLoaded(false);
        GameManager.Instance.numberOfDeaths = 0;
    }
}
