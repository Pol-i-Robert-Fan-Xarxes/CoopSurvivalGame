using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    int damage = 0;
    private void Awake()
    {
        damage = gameObject.GetComponentInParent<LocalPlayer>()._stats.baseDamage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //DO DAMAGE
        if (collision.gameObject.CompareTag("Enemy"))
        {

            collision.gameObject.GetComponent<Enemy>().health -= damage;

        }
    }
}
