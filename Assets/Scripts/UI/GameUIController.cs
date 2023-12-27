using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private Slider sld_xp;

    [SerializeField] private Text txt_lvl;
    [SerializeField] private Text txt_time;
    [SerializeField] private Text txt_kills;

    [SerializeField] private GameObject obj_pause;
    [SerializeField] private GameObject obj_ConnectionLost;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void Pause(bool value)
    {
        obj_pause.SetActive(value);
    }

    public void ConnectionLost()
    {
        obj_ConnectionLost.SetActive(true);
    }

    public void SetTxtTime(string text)
    {
        txt_time.text = text;
    }

    public void SetTxtKills(string text)
    {
        txt_kills.text = text;
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
}
