using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialAttack : MonoBehaviour
{

    private GameObject rightHand;

    private Vector3 InitialScale = Vector3.one;
    private float ChargeTime = 1.0f;
    private float currentTime = 0.0f;
    private float AttackTime = 1.0f;
    private float currentAttackTime = 0.0f;
    private bool specialAttackCharged = false;
    [SerializeField] private float rotationSpeed = 5.0f;
    [SerializeField] private float scaleFactor = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        rightHand = transform.GetChild(4).gameObject;
        
    }

    // Update is called once per frame
    void Update()
    {
        ChargeSpecialAttack();

        AttackInCircle();

    }

    private void AttackInCircle()
    {
        if (!specialAttackCharged) return;

        if (currentAttackTime >= AttackTime)
        {

            currentAttackTime = 0.0f;
            rightHand.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            specialAttackCharged = false;

        }
        else
        {

            currentAttackTime += Time.deltaTime;

            float interFactor = currentAttackTime / AttackTime;

            float currentChargeNormalized = Mathf.Lerp(0.0f, 360.0f, interFactor);
            rightHand.transform.rotation = Quaternion.Euler(0.0f, 0.0f, currentChargeNormalized * rotationSpeed);

        }

    }
    private void ChargeSpecialAttack()
    {

        if(!specialAttackCharged)
        {
            ChargingShovel();
        }
        
    }

    private void ChargingShovel()
    {
        if (Input.GetMouseButton(0))
        {

            currentTime += Time.deltaTime;

            if (currentTime >= ChargeTime)
            {
                currentTime = ChargeTime;
            }

            float interFactor = currentTime / ChargeTime;

            float currentChargeNormalizedX = Mathf.Lerp(1.85f, 3.0f, interFactor);
            float currentChargeNormalizedY = Mathf.Lerp(1.0f, 1.5f, interFactor);

            rightHand.transform.localScale = new Vector3(scaleFactor * currentChargeNormalizedX, scaleFactor * currentChargeNormalizedY);

        }
        else if (Input.GetMouseButton(0) == false) 
        {
            if (currentTime > 0.0f && currentTime < ChargeTime)
            {
                currentTime -= Time.deltaTime;

                float interFactor = currentTime / ChargeTime;

                float currentChargeNormalizedX = Mathf.Lerp(3.0f, 1.85f, interFactor);
                float currentChargeNormalizedY = Mathf.Lerp(1.5f, 1.0f, interFactor);

                rightHand.transform.localScale = new Vector3(scaleFactor * currentChargeNormalizedX, scaleFactor * currentChargeNormalizedY);

            }
            else if(currentTime >= ChargeTime)
            {
                specialAttackCharged = true;
                currentTime = 0.0f;
            }
            else
            {
                currentTime = 0.0f;
            }

        }
    }
}
