using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownPlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool smoothRotation = true;
    [SerializeField] private float rotationSpeed = 10f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 moveVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component missing! Please add one to " + gameObject.name);
        }
    }

    private void Update()
    {
        // Get input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Normalize diagonal movement
        moveInput = moveInput.normalized;

        // Calculate velocity
        moveVelocity = moveInput * moveSpeed;

        // Handle rotation towards movement direction
        if (smoothRotation && moveInput != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else if (!smoothRotation && moveInput != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void FixedUpdate()
    {
        // Move the character
        rb.velocity = moveVelocity;
    }
}