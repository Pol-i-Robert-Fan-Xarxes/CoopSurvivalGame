using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    int damage = 0;
    private void Awake()
    {
        damage = gameObject.GetComponent<LocalPlayer>().baseDamage;
    }
    private void OnCollisionEnter(Collision collision)
    {
        //DO DAMAGE
    }
}
