using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EnemyData
{
    public string netId;
    public Vector3 position;
    public Vector2 dirVector;

    public int health;
    public bool alive;
    public int enemType;
}

public class Enemy : MonoBehaviour
{
    //ENEMY RELATED
    public EnemyData _enemyData;

    protected int _xp = 2;
    protected int _damage = 5;

    [SerializeField]protected float _speed = 1.0f;
    protected Rigidbody2D _rigidBody;
    protected SpriteRenderer _sprite;
    protected Animator _animator;
    protected Collider2D _collider;
    protected bool _alive = false;
    protected int _enemType = 0;

    public bool _local = false; // Bool that saves if the enemy is a local enemy or a remote enemy

    public void Hurt (int damage)
    {
        _enemyData.health -= damage;
        NetworkManager._instance.SendEnemy(Action.UPDATE, _enemyData);
        Die();
    }

    public int health 
    {
        get { return _enemyData.health; }
        set { _enemyData.health = value; }
    }
    public int xp
    {
        get { return _xp; }
        set { _xp = value; }
    }
    public int damage
    {
        get { return _damage; }
        set { _damage = value; }
    }
    public int enemType
    {
        get { return _enemyData.enemType; }
        set { _enemyData.enemType = value; }
    }
    public bool alive
    {
        get { return _enemyData.alive; }
        set { _enemyData.alive = value; }
    }
    public Animator animator
    {
        get { return _animator; }
        set { _animator = value; }
    }
    public SpriteRenderer sprite
    {
        get { return _sprite; }
    }

    //ENEMY MOVEMENT RELATED

    protected List<Player> _listOfPlayers;
    protected Player _target;
    protected float predictTimer = 5.0f;

    //HIT RELATED
    protected float hitTimer = 0.20f;

    //DEAD RELATED
    float deadTimer = 1.0f;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
        GatherAllPlayers();

        _enemyData.dirVector = Vector2.zero;
        //_enemyData.alive = true;
        //_enemyData.enemType = 1;
        //_enemyData.position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager._instance._gameData._isPaused) return;
        if (!alive) return;
        if (NetworkManager._instance._isClient)
        {
            transform.position = _enemyData.position;
            Die();
        }
        else
        {
                if (Die() == false)
                {
                    DetectClosestPlayer();

                    Movement();
                    Debug.Log("Hey mate");
                }
        }

        AnimationFlip();
    }

    protected void GatherAllPlayers()
    {
        _listOfPlayers = new List<Player>
        {
            GameManager.Instance._localPlayer
        };

        foreach (var player in GameManager.Instance._remotePlayers)
        {
            _listOfPlayers.Add(player);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.CompareTag("AttackArea"))
        {

            StartCoroutine(HitColor());

        }
        
    }

    protected void AnimationFlip()
    {
        if (_enemyData.dirVector.x != 0)
        {
            _sprite.flipX = _enemyData.dirVector.x < 0;
        }
    }

    protected void DetectClosestPlayer()
    {
        float shortestDist = 1000;

        foreach (var player in _listOfPlayers)
        {
            if (player._playerData.health <= 0) continue;
            float distance = Vector3.Distance(this.transform.position, player.transform.position);

            if (distance < shortestDist)
            {
                shortestDist = distance;
                _target = player;

            }
        }
    }

    protected void PredictPlayerPosition()
    {
        if (_target == null) return;

        Vector3 posPredict = _target.transform.position + new Vector3(_target.inputVector.x, _target.inputVector.y) * _target._playerData.movementSpeed * predictTimer;

        _enemyData.dirVector = posPredict - transform.position;

    }

    protected void Movement()
    {
        if(enemType == 1 || enemType == 3 || enemType == 4) 
        {
            _enemyData.dirVector = _target.transform.position - this.transform.position;
        }
        else if(enemType == 2)
        {
            PredictPlayerPosition();
        }

        _rigidBody.MovePosition(_rigidBody.position + _enemyData.dirVector.normalized * _speed * Time.fixedDeltaTime);

        _enemyData.position = transform.position;
        NetworkManager._instance.SendEnemy(Action.UPDATE, _enemyData);
    }

    protected bool Die(bool broadcast = true) 
    {
        if(health <= 0) 
        {
            if (!gameObject.activeSelf) return true;
            StartCoroutine(EnemyDead(broadcast));

            return true;
        }
        return false;
    }

    private IEnumerator HitColor()
    {
        animator.SetTrigger("Hit");

        yield return new WaitForSeconds(hitTimer);

        animator.ResetTrigger("Hit");

    }

    private IEnumerator EnemyDead(bool broadcast)
    {
        // Broadcast means that it will be sent to other clients
        //if (broadcast) NetworkManager._instance.SendEnemy(Action.UPDATE, _enemyData);

        //Desactivar enemic a la pool
        _animator.SetBool("Dead", true);
        _collider.enabled = false;

        yield return new WaitForSeconds(deadTimer);

        GameManager._instance.AddXp(_xp);
        GameManager.Instance.UpdateKillCounter();

        gameObject.SetActive(false);
        alive = false;
        _collider.enabled = true;
    }

    #region Remote data management

    public void UpdateDataFromRemote(EnemyData newData)
    {
        health = newData.health;
        alive = newData.alive;  
        Die(false);

        _enemyData.position = newData.position;
        _enemyData.dirVector = newData.dirVector;

        if (newData.alive)
        {
            transform.position = newData.position;
            gameObject.SetActive(true);
        }
    }
    #endregion
}
