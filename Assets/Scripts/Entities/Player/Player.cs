using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PlayerData
{
    public string netId;
    public string name;
    public Vector3 position;
    public Vector2 dirVector;
    public bool flip;
    public float speed;
}

public class Player : MonoBehaviour
{

    //Player control related
    [HideInInspector]public Vector2 inputVector;
    protected Rigidbody2D _rigidBody;
    protected SpriteRenderer _spriteRenderer;
    protected Animator _animator;
    private Vector2 _nextPos;

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
        //_playerData = new PlayerData();
    }

    void Update()
    {
        transform.position = _playerData.position;

        
    }

    private void FixedUpdate()
    {
        //transform.position = Vector2.Lerp(transform.position, _nextPos, 0.1f);
    }

    private void LateUpdate()
    {
        
    }

    public void SetDirVector(Vector2 dirVector) 
    {
        _playerData.dirVector = dirVector;
        inputVector = _playerData.dirVector;
    }

    //public PlayerInfo GetPlayerInfo()
    //{
    //    PlayerInfo info = new PlayerInfo(this);

    //    return info;
    //}

    public virtual void Movement()
    {
        _rigidBody.MovePosition(_rigidBody.position + inputVector.normalized * movementSpeed * Time.fixedDeltaTime);

    }

    public void UpdateDataFromRemote(PlayerData newData)
    {
        SetPosition(newData.position);
        SetAnimData(newData.flip, newData.speed);
    }

    public void SetPosition(Vector3 position)
    {
        _playerData.position = position;
        _nextPos = _playerData.position;

    }

    public void SetAnimData(bool flip, float speed)
    {
        _playerData.flip = flip;
        _playerData.speed = speed;

        _spriteRenderer.flipX = _playerData.flip;
        _animator.SetFloat("Speed", _playerData.speed);
    }
}
