using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{

    [SerializeField] private GameObject attackArea;
    [SerializeField] private float attackTimer = 0.25f;

    private bool attacking = false;
    float currentTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {

        attackArea = transform.GetChild(3).gameObject;

    }

    // Update is called once per frame
    void Update()
    {

        if (currentTime > attackTimer) 
        {
            currentTime = 0.0f;
            Attack();
        }
        else
        {
            //TODO: DO NOT USE DELTA TIME, DIFFERENT CLIENTS HAVE DIFFERENT DT
            currentTime += Time.deltaTime;
        }

    }

    void Attack()
    {

        attacking = true;
        attackArea.SetActive(attacking);

    }

}
