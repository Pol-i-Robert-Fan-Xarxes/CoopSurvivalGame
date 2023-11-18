using System;
using System.Diagnostics;

[Serializable]
public class GameInfo
{
    public GameInfo() 
    {

    }

    public GameData _gameData;

    public void SetGameData(GameData gameData) { _gameData = gameData; }
    public void UnpackGameData(ref GameData target)
    {
        target._isPaused = _gameData._isPaused;
        target._scene = _gameData._scene;
    }

    public PlayerData _playerData;

    public void SetPlayerData(PlayerData data) { _playerData = data; }
    public void UnpackPlayerData(ref Player target)
    {
        target.SetPosition(_playerData.position); 
    }
}