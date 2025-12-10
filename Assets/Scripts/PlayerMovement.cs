using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // This will hold a reference to our Rigidbody2D component
    private Rigidbody2D rb2d;

    // This will hold a reference to our Animator component
    private Animator animator;

    // A variable to keep track if the character is grounded
    bool isGrounded = true;

    // A variable to keep track if the character is dashing
    bool canDash = true;
    bool isDashing = false;

    // Variable jump variables
    private bool isJumping = false;
    private float jumpTimeCounter;

    // Get the current horizontal input
    float horizontalInput;

    private SpriteRenderer spriteRenderer;



    // Movement Speeds
    [Header("Movement Settings")]
    [SerializeField] float runSpeed = 5.0f;
    [SerializeField] float jumpForce = 8.0f;  // Initial jump force
    [SerializeField] float jumpTime = 0.35f;  // How long you can hold jump for max height

    [Header("Dash Settings")]
    [SerializeField] float dashSpeed = 15.0f;
    [SerializeField] float dashDuration = 0.5f;
    [SerializeField] float dashCooldown = 2.0f;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.55f, 0.12f); // Width and Height
    [SerializeField] private LayerMask groundLayer;

    // Start is called before the first frame update
    void Start()
    {

        spriteRenderer = GetComponent<SpriteRenderer>();
        // Find the Rigidbody2D component on this GameObject
        // and store a reference in rigidbody
        rb2d = GetComponent<Rigidbody2D>();

        // Find the Animator component on this GameObject
        // and store a reference in animator
        animator = GetComponent<Animator>();

    }

    void Update()
    {
        if (isDashing) return; // Don't move normally while dashing

        // Check if grounded using overlap circle
        isGrounded = Physics2D.OverlapCapsule(groundCheck.position, groundCheckSize, CapsuleDirection2D.Horizontal, 0.3f, groundLayer);

        // Get the current horizontal input
        horizontalInput = Input.GetAxis("Horizontal");

        // Does the scale match the direction? If not, flip it on the x axis
        SpriteFlip();

        // Dash ability
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        // VARIABLE JUMP LOGIC
        // Start jump when button pressed and grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb2d.velocity = new Vector2(rb2d.velocity.x, jumpForce);
        }

        // Continue adding upward force while holding jump
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

        // Stop jump early if button released
        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isDashing) return; // Don't move normally while dashing

        // Move the rigidbody by the horizontal input at speed
        rb2d.velocity = new Vector2(horizontalInput * runSpeed, rb2d.velocity.y);

        // Set the Animator parameters
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("xVelocity", Mathf.Abs(rb2d.velocity.x));
        animator.SetFloat("yVelocity", rb2d.velocity.y);
    }

    void SpriteFlip()
    {
        // Only flip if there's actual input
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

    // Visualize the ground check in the editor
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }

}