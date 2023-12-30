using UnityEngine;
using UnityEngine.UI;

public class LocalPlayer : Player
{

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _playerAttackHandler = GetComponent<PlayerAttack>();
        _txt_name = GetComponentInChildren<Text>();
        _sld_health = GetComponentInChildren<Slider>();

        SetHealthUI();
    }

    void Start()
    {
        //_playerData = new PlayerData();
    }

    void Update()
    {
        if (!_spriteRenderer.enabled) return;
        _playerData.position = transform.position;
    }

    private void FixedUpdate()
    {
        if (!_spriteRenderer.enabled) return;
        Movement();
    }

    private void LateUpdate()
    {
        if (!_spriteRenderer.enabled) return;
        HandleAnimation();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            SetDamage(collision.gameObject.GetComponent<Enemy>().damage);

            IsDead();
        }
    }

    //Player Movement
    #region Movement&Animations
    public override void Movement()
    {
        if (GameManager._instance._gameData._isPaused) return;
        inputVector.x = Input.GetAxis("Horizontal");
        inputVector.y = Input.GetAxis("Vertical");
        _playerData.dirVector = inputVector;

        _rigidBody.MovePosition(_rigidBody.position + inputVector.normalized * _playerData.movementSpeed * Time.fixedDeltaTime);
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
}
