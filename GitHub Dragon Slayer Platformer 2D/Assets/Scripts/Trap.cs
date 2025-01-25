using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public float bounceForce = 10f;
    public int damage = 1;
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if(this.damage == 0)
            {
                animator.SetTrigger("trigger");
            }
            HandlePlayerBounce(collision.gameObject);
            //animator.enabled = true;
        }
    }


    private void HandlePlayerBounce(GameObject player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if(rb)
        {
            //reset player velocity
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

            //apply bounce force
            rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);    
        }
    }
}
