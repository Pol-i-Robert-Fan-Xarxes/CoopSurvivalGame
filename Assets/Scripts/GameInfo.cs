using System;
using System.Diagnostics;

[Serializable]
public class GameInfo
{
    public GameInfo() 
    {

    }

    //public GameData _gameData;

    //public void SetGameData(GameData gameData) { _gameData = gameData; }
    //public void UnpackGameData(ref GameData target, bool host = false)
    //{
    //    target._remotePlayerName = _gameData._localPlayerName;

    //    if (host) return;

    //    target._isPaused = _gameData._isPaused;
    //    target._scene = _gameData._scene; 
    //}

    public PlayerData _playerData;

    public void SetPlayerData(PlayerData data) { _playerData = data; }
    public void UnpackPlayerData(ref Player target)
    {

        target.SetPosition(_playerData.position);

        target.SetAnimData(_playerData.flip, _playerData.speed);
        
    }
}