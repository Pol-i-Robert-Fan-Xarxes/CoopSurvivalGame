using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

[Serializable]
public struct PlayerStats
{
    public int maxHealth;
    public int health;
    public float movementSpeed;
    public float attackSpeed;
    public int baseDamage;
}

public class Player : MonoBehaviour
{
    //Player control related
    [HideInInspector]public Vector2 inputVector;
    protected Rigidbody2D _rigidBody;
    protected SpriteRenderer _spriteRenderer;
    protected Animator _animator;
    private Vector2 _nextPos;

    //UI
    public Text _txt_name;
    public Slider _sld_health;

    public PlayerData _playerData;
    public PlayerStats _stats;
    protected PlayerAttack _playerAttackHandler;

    private Color hitColor = Color.red;
    private Color originalColor = Color.white;
    private float hitTimer = 0.2f;
    
    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _playerAttackHandler = GetComponent<PlayerAttack>();
        _txt_name = GetComponentInChildren<Text>();
        _sld_health = GetComponentInChildren<Slider>();

        InitStats();
    }

    void Start()
    {
        
    }

    protected void InitStats()
    {
        _stats = new PlayerStats();

        _stats.maxHealth = 10;
        _stats.health = _stats.maxHealth;

        _stats.movementSpeed = 2.5f;
        _stats.attackSpeed = 1.0f;
        _stats.baseDamage = 12;

        SetHealthUI();
    }

    void Update()
    {
        transform.position = _playerData.position;

        
    }

    private void FixedUpdate()
    {
    }

    private void LateUpdate()
    {
        
    }

    public void SetDirVector(Vector2 dirVector) 
    {
        _playerData.dirVector = dirVector;
        inputVector = _playerData.dirVector;
    }

    public void SetAttackArea(bool attacks)
    {

    }

    public void SetHealth(int health)
    {
        _stats.health = health;
        SetHealthUI();
    }

    private void SetDamage(int damage)
    {
        _stats.health -= damage;
        SetHealthUI();
        StartCoroutine(HitPlayerColor());
    }

    protected void SetHealthUI()
    {
        _sld_health.value = ((_stats.health * 100) / _stats.maxHealth) * 0.01f;
    }

    public virtual void Movement()
    {
        _rigidBody.MovePosition(_rigidBody.position + inputVector.normalized * _stats.movementSpeed * Time.fixedDeltaTime);

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

    private void IsDead()
    {
        if(_stats.health <= 0)
        {
            GameManager.Instance._gameData._scene = 2;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            SetDamage(collision.gameObject.GetComponent<Enemy>().damage);

            IsDead();
        }
    }

    IEnumerator HitPlayerColor()
    {

        _spriteRenderer.color = hitColor;

        yield return new WaitForSeconds(hitTimer);

        _spriteRenderer.color = originalColor;

    }
}
