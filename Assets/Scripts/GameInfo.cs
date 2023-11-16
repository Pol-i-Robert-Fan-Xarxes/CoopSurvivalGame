using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameInfo
{
    
    public GameInfo() 
    {
        _playerInfo = new PlayerInfo();
        _enemyInfo = new List<EnemyInfo>();
    }

    private PlayerInfo _playerInfo;

    private List<EnemyInfo> _enemyInfo;

    public PlayerInfo PlayerInfo => _playerInfo;
    public List<EnemyInfo> EnemyInfoList => _enemyInfo;
    public virtual void SetInfo()
    {
        if (_playerInfo != null)
        {
            _playerInfo.SetInfo();

        }

        if (_enemyInfo != null)
        {
            foreach (var enemy in _enemyInfo)
            {
                enemy.SetInfo();
            }
        }

    }

}

[Serializable]
public class PlayerInfo: GameInfo
{
    //Constructors
    public PlayerInfo() : base() { }

    public PlayerInfo(Player player)
    {
        position = player.transform.position;
        health = player.health;
        movementSpeed = player.movementSpeed;
        this.attackSpeed = player.attackSpeed;
        baseDamage = player.baseDamage;
    }
    public PlayerInfo(Vector2 pos, int hp = 10, float movSpeed = 1.0f, float attackSpeed = 1.0f, int damage = 1) : base()
    {
        position = pos;
        health = hp;
        movementSpeed = movSpeed;
        this.attackSpeed = attackSpeed;
        baseDamage = damage;
    }

    //Functions

    public override void SetInfo() 
    {
        Player _player = GameManager._instance._localPlayer;
        if (_player != null)
        {
            _player.transform.position = position;
            _player.health = health;
            _player.movementSpeed = movementSpeed;
            _player.attackSpeed = attackSpeed;
            _player.baseDamage = baseDamage;
        }
    }

    //Variables
    public Vector3 position = Vector3.zero;
    public int health = 10;
    public float movementSpeed = 1.0f;
    public float attackSpeed = 1.0f;
    public int baseDamage = 1;
}

[Serializable]
public class EnemyInfo: GameInfo
{
    //Constructors
    public EnemyInfo() : base() { }

    public EnemyInfo(Vector2 pos, int hp = 10, float movSpeed = 1.0f, float attackSpeed = 1.0f, int damage = 1) : base()
    {
        position = pos;
        health = hp;
        movementSpeed = movSpeed;
        this.attackSpeed = attackSpeed;
        baseDamage = damage;
    }

    //Functions
    public override void SetInfo()
    {
        
         //_player.transform.position = GetPosition();
         //_player.health = health;
         //_player.movementSpeed = movementSpeed;
         //_player.attackSpeed = attackSpeed;
         //_player.baseDamage = baseDamage;
        
    }

    //Variable Stats
    public Vector3 position = Vector3.zero;
    public int health = 10;
    public float movementSpeed = 1.0f;
    public float attackSpeed = 1.0f;
    public int baseDamage = 1;
}
