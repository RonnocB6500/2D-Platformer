using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Transform player;
    public GameObject playerObject;
    bool isFacingRight = true;
    BoxCollider2D playerCollider;

    [Header("Movement")]
    public float moveSpeed = 5f;
    float horizontalMovement;

    [Header("Dashing")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.1f;
    public float dashCooldown = 0.1f;
    bool isDashing;
    bool canDash = true;
    TrailRenderer trailRenderer;

    [Header("Jumping")]
    public float jumpPower = 10f;
    public int maxJumps = 2;
    int jumpsRemaining;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    bool isGrounded;
    bool isOnPlatform;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;

    [Header("WallCheck")]
    public Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask wallLayer;

    [Header("WallMovement")]
    public float wallSlideSpeed = 2f;
    bool isWallSliding;

    //Wall Jumping
    bool isWallJumping;
    float wallJumpDirection;
    float wallJumpTime = 0.2f;
    float wallJumpTimer;
    public Vector2 wallJumpPower = new Vector2(40f, 40f);

    //Attacking
    [Header("Attacking")]
    public Transform attackTransform;
    public float attackRange = 1f;
    public float damageAmount = 1f;
    public LayerMask attackableLayer;
    private RaycastHit2D[] hits;
    public bool shouldBeDamaging {get; private set;} = false;
    private List<IDamageable> iDamageables = new List<IDamageable>();




    Animator animator;

    public static event Action PlayerReset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        playerCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        
        MenuButtonController.OnMenu += FreezePlayer;
        LevelSelect.OnLevel += UnfreezePlayer;
    }

    // Update is called once per frame
    void Update()
    {   
        //For if i want to add animations from tutorial, these lines go at top here:
        //animator.SetFloat("yVelocity", rb.linearVelocity.y);
        //animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
        //animator.SetBool("isWallSliding", isWallSliding);
        //Attack();
        if(isGrounded && Input.GetMouseButtonDown(0)) //Left Click
        {
            animator.SetTrigger("isAttacking");
        }

        if(isDashing)
        {
            return;
        }

        if(isGrounded)
        {
            animator.SetBool("isJumping", false);
        }

        

        GroundCheck();
        ProcessGravity();
        ProcessWallSlide();
        ProcessWallJump();

        if(!isWallJumping)
        {
            rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
            Flip();
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
        if(horizontalMovement != 0f)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    /*public void Attack()
    {
        if(isGrounded && Input.GetMouseButtonDown(0)) //Left Click
        {
            animator.SetTrigger("isAttacking");
            hits = Physics2D.CircleCastAll(attackTransform.position, attackRange, transform.right, 0f, attackableLayer);

            for(int i = 0; i < hits.Length; i++)
            {
                IDamageable iDamageable = hits[i].collider.gameObject.GetComponent<IDamageable>();

                if(iDamageable != null)
                {
                    iDamageable.Damage(damageAmount);
                }
            }
        }
    }*/

    public IEnumerator DamageWhileAttackIsActive()
    {
        shouldBeDamaging = true;

        while(shouldBeDamaging)
        {
            hits = Physics2D.CircleCastAll(attackTransform.position, attackRange, transform.right, 0f, attackableLayer);

            for(int i = 0; i < hits.Length; i++)
            {
                IDamageable iDamageable = hits[i].collider.gameObject.GetComponent<IDamageable>();

                if(iDamageable != null && !iDamageable.HasTakenDamage)
                {
                    iDamageable.Damage(damageAmount);
                    iDamageables.Add(iDamageable);
                }
            }

            yield return null;
        }

        ReturnAttackablesToDamageable();
    }

    private void ReturnAttackablesToDamageable()
    {
        foreach(IDamageable thingThatWasDamaged in iDamageables)
        {
            thingThatWasDamaged.HasTakenDamage = false;
        }

        iDamageables.Clear();
    }

    #region Animation Triggers

    public void shouldBeDamagingToTrue()
    {
        shouldBeDamaging = true;
    }

    public void shouldBeDamagingToFalse()
    {
        shouldBeDamaging = false;
    }

    #endregion


    public void Dash(InputAction.CallbackContext context)
    {
        if(context.performed && canDash)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        Physics2D.IgnoreLayerCollision(7, 8, true);
        canDash = false;
        isDashing = true;
        trailRenderer.emitting = true;

        float dashDirection = isFacingRight ? 1f : -1f;

        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, rb.linearVelocity.y); //Dash movement

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        isDashing = false;
        trailRenderer.emitting = false;
        Physics2D.IgnoreLayerCollision(7, 8, false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void Drop(InputAction.CallbackContext context)
    {  
        if(context.performed && isGrounded && isOnPlatform && playerCollider.enabled)
        {
            StartCoroutine(DisablePlayerCollider(0.25f));
        }
    }

    private IEnumerator DisablePlayerCollider(float disableTime)
    {
        playerCollider.enabled = false;
        yield return new WaitForSeconds(disableTime);
        playerCollider.enabled = true;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = true;
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = false;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {   
        if(jumpsRemaining > 0)
        {
            // Hold down jump button = full height
            if (context.performed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
                jumpsRemaining--;
                animator.SetBool("isJumping", true);
            }
            else if (context.canceled)
            {   
                // Light tap of jump button= half height
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y*0.5f);
                jumpsRemaining--;
            }
        }

        //Wall Jump
        if(context.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y); //Jump away from wall
            wallJumpTimer = 0;

            //Force flip
            if(transform.localScale.x != wallJumpDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 ls = transform.localScale;
                ls.x *= -1f;
                transform.localScale = ls;
            }

            Invoke(nameof(CancelWallJump), wallJumpTime + 0.1f); //Wall jump = 0.5f -- Jump again = 0.6f
        }
    }

    public void GroundCheck()
    {
        if(Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            jumpsRemaining = maxJumps;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer);
    }

    private void ProcessGravity()
    {
        if(!isDashing)
        {
            if(rb.linearVelocity.y < 0)
            {
                rb.gravityScale = baseGravity * fallSpeedMultiplier; //Fall Increasingly faster
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
            }
            else
            {
                rb.gravityScale = baseGravity;
            }
        }
    }

    private void ProcessWallSlide()
    {
        //Not grounded & On a Wall & movement != 0
        if(!isGrounded && WallCheck() & horizontalMovement != 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void ProcessWallJump()
    {
        if(isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = - transform.localScale.x;
            wallJumpTimer = wallJumpTime;

            CancelInvoke(nameof(CancelWallJump));
        }
        else if(wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
    }
    private void Flip()
    {
        if(isFacingRight && horizontalMovement < 0 || !isFacingRight && horizontalMovement > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);

        Gizmos.DrawWireSphere(attackTransform.position, attackRange);

    }

    void FreezePlayer()
    {
        playerObject.SetActive(false);
        rb.bodyType = RigidbodyType2D.Static;
    }

    void UnfreezePlayer()
    {
        playerObject.SetActive(true);
        rb.bodyType = RigidbodyType2D.Dynamic;
        player.position = new Vector3(0, 0, 0);
        PlayerReset.Invoke();
    }
}
