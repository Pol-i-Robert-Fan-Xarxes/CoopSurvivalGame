using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    //ENEMY RELATED
    private int _health = 20;
    private int _xp = 2;
    private int _damage = 5;
    [SerializeField] float _speed = 1.0f;
    private Rigidbody2D _rigidBody;
    private SpriteRenderer _sprite;
    private Animator _animator;
    private bool _alive = false;
    private int _enemType = 0;

    public int health 
    {
        get { return _health; }
        set { _health = value; }
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
        get { return _enemType; }
        set { _enemType = value; }
    }
    public bool alive
    {
        get { return _alive; }
        set { _alive = value; }
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

    List<Player> _listOfPlayers;
    Player _target;
    Vector2 _finalDir = Vector2.zero;
    float predictTimer = 5.0f;

    //HIT RELATED
    Color hitColor = Color.red;
    Color originalColor = Color.white;
    float hitTimer = 0.20f;

    //DEAD RELATED
    float deadTimer = 1.0f;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
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

    // Update is called once per frame
    void Update()
    {
        if(!alive) return;

        if (Die() == false)
        {
            DetectClosestPlayer();

            Movement();
        }

        AnimationFlip();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.CompareTag("AttackArea"))
        {

            _sprite.color = hitColor;

            StartCoroutine(HitColor());

        }
        
    }

    private void AnimationFlip()
    {
        if (_finalDir.x != 0)
        {
            _sprite.flipX = _finalDir.x < 0;
        }
    }

    private void DetectClosestPlayer()
    {
        float shortestDist = 1000;

        foreach (var player in _listOfPlayers)
        {
            float distance = Vector3.Distance(this.transform.position, player.transform.position);

            if (distance < shortestDist)
            {
                shortestDist = distance;
                _target = player;

            }

        }

    }

    private void PredictPlayerPosition()
    {
        if (_target == null) return;

        Vector3 posPredict = _target.transform.position + new Vector3(_target.inputVector.x, _target.inputVector.y) * _target._stats.movementSpeed * predictTimer;

        _finalDir = posPredict - transform.position;

    }

    private void Movement()
    {
        int value = Random.Range(1,2);

        if(enemType == 1) 
        {
            _finalDir = _target.transform.position - this.transform.position;
        }
        else if(enemType == 2)
        {
            PredictPlayerPosition();
        }

        _rigidBody.MovePosition(_rigidBody.position + _finalDir.normalized * _speed * Time.fixedDeltaTime);
    }

    private bool Die() 
    {
        if(health <= 0) 
        {
            StartCoroutine(EnemyDead());

            GameManager._instance.AddXp(_xp);

            return true;
        }
        return false;
    }

    private IEnumerator HitColor()
    {
        yield return new WaitForSeconds(hitTimer);

        _sprite.color = originalColor;

    }

    private IEnumerator EnemyDead()
    {

        //Desactivar enemic a la pool
        alive = false;
        _animator.SetBool("Dead", true);
        GameManager.Instance.UpdateKillCounter();

        yield return new WaitForSeconds(deadTimer);

        gameObject.SetActive(false);
    }

}
