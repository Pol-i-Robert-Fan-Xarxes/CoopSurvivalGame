using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoss : Enemy
{
    

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

        health = 100;
        xp = 100;
        damage = 100;
        _speed = 0.5f;
        enemType = 3;

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager._instance._gameData._isPaused) return;
        if (!alive) return;
        if (!_local)
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
            }
        }

        AnimationFlip();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("AttackArea"))
        {
            StartCoroutine(HitColor());
        }
    }

    private IEnumerator HitColor()
    {
        animator.SetTrigger("Hit");

        yield return new WaitForSeconds(hitTimer);

        animator.ResetTrigger("Hit");
    }

}
