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
    public int skin;

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
    [SerializeField] protected SpriteRenderer _deadSprite;
    private Vector2 _nextPos;

    //UI
    public Text _txt_name;
    public Slider _sld_health;

    public PlayerData _playerData;

    protected PlayerAttack _playerAttackHandler;

    private Color hitColor = Color.red;
    private Color originalColor = Color.white;
    private float hitTimer = 0.2f;

    public RuntimeAnimatorController _animationController;

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
        SetSkinAnimator();
    }

    void Update()
    {
        if (!_spriteRenderer.enabled) return;
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
        _playerData.health = health;
        SetHealthUI();
    }

    protected void SetDamage(int damage)
    {
        _playerData.health -= damage;
        SetHealthUI();
        StartCoroutine(HitPlayerColor());
    }

    protected void SetHealthUI()
    {
        _sld_health.value = ((_playerData.health * 100) / _playerData.maxHealth) * 0.01f;
    }

    public virtual void Movement()
    {
        _rigidBody.MovePosition(_rigidBody.position + inputVector.normalized * _playerData.movementSpeed * Time.fixedDeltaTime);

    }

    public void UpdateDataFromRemote(PlayerData newData)
    {
        SetPosition(newData.position);
        SetAnimData(newData.flip, newData.speed);

        SetHealth(newData.health);
        IsDead();
    }

    public void SetPosition(Vector3 position)
    {
        _playerData.position = position;
        _nextPos = _playerData.position;

    }

    public void SetAnimData(bool flip, float speed)
    {
        if (_spriteRenderer == null) return;
        _playerData.flip = flip;
        _playerData.speed = speed;

        _spriteRenderer.flipX = _playerData.flip;
        _animator.SetFloat("Speed", _playerData.speed);
    }

    protected void IsDead()
    {
        if(_playerData.health <= 0)
        {

            Instantiate(Resources.Load("Prefabs/Dead"), transform.position, Quaternion.identity, transform.parent);
            gameObject.SetActive(false);
            //GameManager.Instance._gameData._scene = 2;

            //_spriteRenderer.enabled = false;
            //_deadSprite.gameObject.SetActive(true);
        }
    }

    IEnumerator HitPlayerColor()
    {

        _spriteRenderer.color = hitColor;

        yield return new WaitForSeconds(hitTimer);

        _spriteRenderer.color = originalColor;

    }

    public void SetSkinAnimator()
    {
        RuntimeAnimatorController skin;
        switch (_playerData.skin)
        {
            case 0:
            default:
                {
                    skin = Resources.Load<RuntimeAnimatorController>("Animations/character_0/Character_0");
                }
                break;
            case 1:
                {
                    skin = Resources.Load<RuntimeAnimatorController>("Animations/character_1/Character_1");
                }
                break;
            case 2:
                {
                    skin = Resources.Load<RuntimeAnimatorController>("Animations/character_2/Character_2");
                }
                break;
            case 3:
                {
                    skin = Resources.Load<RuntimeAnimatorController>("Animations/character_3/Character_3");
                }
                break;

        }

        GetComponent<Animator>().runtimeAnimatorController = skin;
    }
}
