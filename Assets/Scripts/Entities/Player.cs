using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    //Player control related
    [HideInInspector]public Vector2 inputVector;
    private Rigidbody2D _rigidBody;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;


    //Stats
    public int health = 10;
    public float movementSpeed = 1.0f;
    public float attackSpeed = 1.0f;
    public int baseDamage = 1;

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
        Movement();
    }

    private void LateUpdate()
    {
        HandleAnimation();
    }

    //Player Movement
    #region Movement&Animations
    private void Movement()
    {
        inputVector.x = Input.GetAxis("Horizontal");
        inputVector.y = Input.GetAxis("Vertical");

        _rigidBody.MovePosition(_rigidBody.position + (inputVector.normalized * movementSpeed * Time.fixedDeltaTime));
    }

    private void HandleAnimation()
    {
        _animator.SetFloat("Speed", inputVector.magnitude);

        if (inputVector.x != 0) 
        {
            _spriteRenderer.flipX = inputVector.x < 0;
        }
    }
    #endregion

    public PlayerInfo GetPlayerInfo()
    {
        PlayerInfo info = new PlayerInfo(this);

        return info;
    }

}
