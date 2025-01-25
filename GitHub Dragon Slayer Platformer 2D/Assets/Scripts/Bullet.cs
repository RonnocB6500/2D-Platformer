using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Bullet : MonoBehaviour
{
    public int bulletDamage = 1;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Dragon dragon = collision.GetComponent<Dragon>();
        if(dragon)
        {
            dragon.TakeDamage(bulletDamage);

            Destroy(gameObject);
        }
    }
}
