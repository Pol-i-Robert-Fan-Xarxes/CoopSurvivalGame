using UnityEngine;

public class Timer
{
    public float totalTime = 1800f; 
    private float currentTime;
    
    private bool _isRunning = false;
    public bool IsRunning => _isRunning;
    public bool _runout = false;

    public void Update()
    {
        if (_isRunning)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                _isRunning = false;
                _runout = true;
            }
        }
    }

    public string GetTime()
    {
        string minutes = Mathf.Floor(currentTime / 60).ToString("00");
        string seconds = Mathf.Floor(currentTime % 60).ToString("00");

        return minutes + ":" + seconds;
    }

    public void Start()
    {
        if (_isRunning) return;
        currentTime = totalTime;
        _isRunning = true;
    }

    public void Pause() { _isRunning = false; }
    public void Resume() { _isRunning = true;}
}