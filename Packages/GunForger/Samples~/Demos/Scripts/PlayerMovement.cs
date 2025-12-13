using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // References to components
    private Rigidbody2D rb2d;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Ground / movement state
    bool isGrounded = true;
    bool canDash = true;
    bool isDashing = false;

    // Variable jump variables
    private bool isJumping = false;
    private float jumpTimeCounter;

    // Input & direction
    float horizontalInput;

    [Header("Movement Settings")]
    [SerializeField] float runSpeed = 5.0f;
    [SerializeField] float jumpForce = 8.0f;
    [SerializeField] float jumpTime = 0.35f;

    [Header("Dash Settings")]
    [SerializeField] float dashSpeed = 15.0f;
    [SerializeField] float dashDuration = 0.5f;
    [SerializeField] float dashCooldown = 2.0f;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.55f, 0.12f); // Size used by OverlapCapsule
    [SerializeField] private LayerMask groundLayer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isDashing) return; // Disable normal input while dashing

        // Ground check using a capsule overlap (editor-visible via OnDrawGizmosSelected)
        isGrounded = Physics2D.OverlapCapsule(groundCheck.position, groundCheckSize, CapsuleDirection2D.Horizontal, 0.3f, groundLayer);

        // Read horizontal input (A/D or Left/Right)
        horizontalInput = Input.GetAxis("Horizontal");

        // Flip sprite to face movement direction when input present
        SpriteFlip();

        // Dash input
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        // VARIABLE JUMP: start jump when pressed and grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb2d.velocity = new Vector2(rb2d.velocity.x, jumpForce);
        }

        // Continue holding jump to extend jump height (variable height)
        if (Input.GetButton("Jump") && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        // Stop jump early when button released
        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return; // Don't apply normal movement while dashing

        // Apply horizontal velocity while preserving vertical velocity
        rb2d.velocity = new Vector2(horizontalInput * runSpeed, rb2d.velocity.y);

        // Update animator parameters for visual feedback
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("xVelocity", Mathf.Abs(rb2d.velocity.x));
        animator.SetFloat("yVelocity", rb2d.velocity.y);
    }

    /// <summary>
    /// Flip the transform's x-scale to mirror the sprite when changing movement direction.
    /// </summary>
    void SpriteFlip()
    {
        if (horizontalInput > 0 && transform.localScale.x < 0 ||
            horizontalInput < 0 && transform.localScale.x > 0)
        {
            Vector3 newScale = new Vector3(
                -1f * transform.localScale.x,
                transform.localScale.y,
                transform.localScale.z);
            transform.localScale = newScale;
        }
    }

    /// <summary>
    /// Dash coroutine temporarily disables gravity and moves the player in facing direction for dashDuration.
    /// </summary>
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        animator.SetBool("isDashing", true);
        float originalGravity = rb2d.gravityScale;
        rb2d.gravityScale = 0f;

        float dashDirection = Mathf.Sign(transform.localScale.x);
        rb2d.velocity = new Vector2(dashDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);
        rb2d.gravityScale = originalGravity;

        isDashing = false;
        animator.SetBool("isDashing", false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // Draw the ground check box in the editor for tuning
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }
}