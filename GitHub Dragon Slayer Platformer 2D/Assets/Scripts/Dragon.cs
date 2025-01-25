using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragon : MonoBehaviour, IDamageable
{
    private Transform player;
    public float chaseSpeed = 4f;
    public float jumpForce = 2f;

    public int levelContainer;


    public Rigidbody2D rb;
    public Transform transform;
    public GameObject gameObject;
    public Vector3 ogPosition;

    private bool shouldJump;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    bool isGrounded;
    bool isOnPlatform;

    [Header("Attack")]
    public int damage = 1;
    public GameObject fireBallPrefab;
    public float fireBallSpeed = 5f;
    private Vector2 playerDirection;
    public float attackWait = 7f;
    private bool isAttacking = false;


    public float maxHealth = 2;
    private float currentHealth;
    public bool HasTakenDamage {get;set;}


    private SpriteRenderer spriteRenderer;
    private Color ogColor;

    Animator animator;

    //Loot Table
    [Header("Loot")]
    public List<LootItem> lootTable = new List<LootItem>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ogPosition = transform.position;
        //rb = GetComponent<Rigidbody2D>();
        //transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        ogColor = spriteRenderer.color;

        MenuButtonController.OnMenu += DragonKill;
        GameController.OnReset += DragonReset;
        GameController.OnStopLevel += DragonKill;
        GameController.OnStartLevel += DragonReset;
    }

    // Update is called once per frame
    void Update()
    {
        //Is grounded?
        //isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);
        
        //Player Direction
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        
        playerDirection = new Vector2(player.position.x - transform.position.x, player.position.y - transform.position.y); 

        //Player above detection
        bool isPlayerAbove = Physics2D.Raycast(transform.position, Vector2.up, 5f, 1 << player.gameObject.layer);

        GroundCheck();

        Vector2 distance = new Vector2(playerDirection.x, playerDirection.y);

        playerDirection = playerDirection.normalized;

        if(distance.sqrMagnitude < 200 && !isAttacking)
        {
            rb.linearVelocity = new Vector2(0, 0);
            StartCoroutine(AttackWait());
        }
        else if(isGrounded)
        {
            // Chase player
            rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

            if(transform.localScale.x != direction)
            {
                Vector3 ls = transform.localScale;
                ls.x *= -1f;
                transform.localScale = ls;
            }

            if(rb.linearVelocity != Vector2.zero)
            {
                animator.SetBool("dragonWalking", true);
            }
            else
            {
                animator.SetBool("dragonWalking", false);
            }
            // else if(rb.linearVelocity.x >= 0f)
            // {
            //     Vector3 ls = transform.localScale;
            //     ls.x *= 1f;
            //     transform.localScale = ls;
            // }

            //Jump if there's gap ahead && no ground in front
            //else if there's player above and platform above

            //If ground
            RaycastHit2D groundInFront = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 2f, groundLayer);
            //If gap
            RaycastHit2D gapAhead = Physics2D.Raycast(transform.position + new Vector3(direction, 0, 0), Vector2.down, 2f, groundLayer);
            //If platform above
            RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2.up, 5f, groundLayer);

            if(!groundInFront.collider && !gapAhead.collider)
            {
                shouldJump = true;
            }
            else if(isPlayerAbove && platformAbove.collider)
            {
                shouldJump = true;
            }
        }
        // else if(isAttacking)
        // {
        //     rb.linearVelocity = new Vector2(0, 0);
        // }
    }

    public void GroundCheck()
    {
        if(Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    void FireAttack()
    {
        animator.SetTrigger("dragonAttack");
        Vector3 adjust = new Vector3(0, -0.5f, 0);
        GameObject fireBall = Instantiate(fireBallPrefab, transform.position + adjust, Quaternion.identity);
        fireBall.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(playerDirection.x, playerDirection.y) * fireBallSpeed;
        Destroy(fireBall, 5f);
    }

    public IEnumerator AttackWait()
    {
        FireAttack();
        isAttacking = true;
        yield return new WaitForSeconds(2f);
        isAttacking = false;
        yield return new WaitForSeconds(attackWait);
        
    }

    private void FixedUpdate()
    {
        if(isGrounded && shouldJump)
        {
            shouldJump = false;
            Vector2 direction = (player.position - transform.position).normalized;

            Vector2 jumpDirection = direction * jumpForce;

            rb.AddForce(new Vector2(jumpDirection.x, jumpForce), ForceMode2D.Impulse);
        }
    }

    public void Damage(float damageAmount)
    {
        currentHealth -= damageAmount;

        HasTakenDamage = true;

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        foreach(LootItem lootItem in lootTable)
        {
            if(Random.Range(0f, 100f) <= lootItem.dropChance)
            {
                InstantiateLoot(lootItem.itemPrefab);
            }
            break;
        }
        transform.position = ogPosition;
        gameObject.SetActive(false);
    }

    public void DragonReset()
    {
        if((LevelSelect.selectedLevel == levelContainer) && (gameObject != null))
        {
            gameObject.SetActive(true);
            rb.bodyType = RigidbodyType2D.Dynamic;
            currentHealth = maxHealth;
            transform.position = ogPosition;
            isAttacking = false;
        }
    }

    public void DragonKill()
    {
        if(gameObject != null)
        {
            //transform.position = ogPosition;
            rb.bodyType = RigidbodyType2D.Static;
            gameObject.SetActive(false);
        }
    }



    private IEnumerator FlashWhite()
    {
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = ogColor;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        StartCoroutine(FlashWhite());
        if(currentHealth <= 0)
        {
            Die();
        }
    }


    void InstantiateLoot(GameObject loot)
    {
        if(loot)
        {
            GameObject droppedLoot = Instantiate(loot, transform.position, Quaternion.identity);

            droppedLoot.GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
    }
}
