using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PlayerData
{
    public Vector3 position;
}

public class Player : MonoBehaviour
{

    //Player control related
    [HideInInspector]public Vector2 inputVector;
    protected Rigidbody2D _rigidBody;
    protected SpriteRenderer _spriteRenderer;
    protected Animator _animator;

    //Stats
    public int health = 10;
    public float movementSpeed = 1.0f;
    public float attackSpeed = 1.0f;
    public int baseDamage = 1;

    public PlayerData _playerData;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        
    }

    void Update()
    {
    }

    private void FixedUpdate()
    {

    }

    private void LateUpdate()
    {

    }

    public void SetPosition(Vector3 position)
    {
        _playerData.position = position;
        transform.position = _playerData.position;
    }

    //public PlayerInfo GetPlayerInfo()
    //{
    //    PlayerInfo info = new PlayerInfo(this);

    //    return info;
    //}

}
