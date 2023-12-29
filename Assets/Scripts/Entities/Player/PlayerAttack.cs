using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{

    //PLAYER ATTACK RELATED
    private GameObject attackAreaRight;
    private GameObject attackAreaLeft;
    private GameObject rightHand;
    [SerializeField] private float attackTimer = 500.0f;
    [SerializeField] private float chargeTimer = 500.0f;
    private bool attacking = false;
    private bool charged = false;
    float currentTime = 0.0f;
    float currentChargeTime = 0.0f;

    //PLAYER RELATED
    private SpriteRenderer _playerSprite;

    // Start is called before the first frame update
    void Start()
    {

        attackAreaRight = transform.GetChild(3).gameObject;
        rightHand = transform.GetChild(4).gameObject;
        attackAreaLeft = transform.GetChild(5).gameObject;

        _playerSprite = transform.GetComponent<SpriteRenderer>();

        attackAreaLeft.SetActive(false);
        attackAreaRight.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        rightHand.GetComponent<SpriteRenderer>().flipX = _playerSprite.flipX;

        ChargeLogic();

        AttackLocal();

        AttackCooldown();

    }

    private void AttackCooldown()
    {
        if (attacking == false || charged) return;

        currentTime += Time.deltaTime;

        if (currentTime > attackTimer)
        {
            
            currentTime = 0.0f;
            attacking = false;

            if (_playerSprite.flipX)
            {
                attackAreaLeft.SetActive(false);
                attackAreaRight.SetActive(false);
            }
            else
            {
                attackAreaRight.SetActive(false);
                attackAreaLeft.SetActive(false);
            }

        }
        else
        {

            //float currentAttackNormalized = Mathf.Lerp(90.0f, 0.0f, currentTime); // 0-1
            //rightHand.transform.Rotate(Vector3.forward, currentAttackNormalized);

            if (_playerSprite.flipX)
            {
                //Normalize Values
                float interFactor = currentTime / attackTimer;
                // Use LerpAngle to interpolate between -90 and 0 degrees
                float currentChargeNormalized = Mathf.LerpAngle(-90.0f, 0.0f, interFactor);
                rightHand.transform.rotation = Quaternion.Euler(0.0f, 0.0f, currentChargeNormalized);
            }
            else
            {
                //Normalize Values
                float interFactor = currentTime / attackTimer;
                // Use LerpAngle to interpolate between -90 and 0 degrees
                float currentChargeNormalized = Mathf.LerpAngle(90.0f, 0.0f, interFactor);
                rightHand.transform.rotation = Quaternion.Euler(0.0f, 0.0f, currentChargeNormalized);
            }
        }
    }

    private void ChargeLogic()
    {
        if(charged || attacking)return;

        currentChargeTime += Time.deltaTime;

        if (currentChargeTime > chargeTimer)
        {
            charged = true;
            currentChargeTime = 0.0f;

            // Reset rotation to 0
            rightHand.transform.rotation = Quaternion.identity;
        }
        else
        {

            if(_playerSprite.flipX)
            {
                //Normalize Value
                float interFactor = currentChargeTime / chargeTimer;
                // Use Mathf.LerpAngle to interpolate between 0 and 90 degrees
                float currentChargeNormalized = Mathf.LerpAngle(0.0f, -90.0f, interFactor);
                rightHand.transform.rotation = Quaternion.Euler(0.0f, 0.0f, currentChargeNormalized);
            }
            else 
            {
                //Normalize Value
                float interFactor = currentChargeTime / chargeTimer;
                // Use Mathf.LerpAngle to interpolate between 0 and 90 degrees
                float currentChargeNormalized = Mathf.LerpAngle(0.0f, 90.0f, interFactor);
                rightHand.transform.rotation = Quaternion.Euler(0.0f, 0.0f, currentChargeNormalized);
            }
        }
    }

    public void AttackLocal()
    {
        if(attacking || charged == false)return;

        attacking = true;
        charged = false;

        if (_playerSprite.flipX)
        {
            attackAreaLeft.SetActive(true);
        }
        else
        {
            attackAreaRight.SetActive(true);
        }

    }

}
