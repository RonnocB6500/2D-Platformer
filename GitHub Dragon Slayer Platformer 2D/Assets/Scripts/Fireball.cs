using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float bounceForce = 0f;
    public int damage = 1;

    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
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
