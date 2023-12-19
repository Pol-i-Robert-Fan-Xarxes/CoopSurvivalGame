using UnityEngine;


public class LocalPlayer : Player
{

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _playerAttackHandler = GetComponent<PlayerAttack>();
    }

    void Start()
    {
        _playerData = new PlayerData();
    }

    void Update()
    {
        _playerData.position = transform.position;
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
    public override void Movement()
    {
        inputVector.x = Input.GetAxis("Horizontal");
        inputVector.y = Input.GetAxis("Vertical");
        _playerData.dirVector = inputVector;

        _rigidBody.MovePosition(_rigidBody.position + inputVector.normalized * movementSpeed * Time.fixedDeltaTime);
    }

    private void HandleAnimation()
    {
        _animator.SetFloat("Speed", inputVector.magnitude);
        _playerData.speed = inputVector.magnitude;

        if (inputVector.x != 0) 
        {
            _spriteRenderer.flipX = inputVector.x < 0;
            _playerData.flip = _spriteRenderer.flipX;
        }
    }
    #endregion

    //public PlayerInfo GetPlayerInfo()
    //{
    //    PlayerInfo info = new PlayerInfo(this);

    //    return info;
    //}

}
