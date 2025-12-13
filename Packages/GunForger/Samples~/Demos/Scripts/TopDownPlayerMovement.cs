using UnityEngine;

/// <summary>
/// TopDownPlayerMovement handles 2D top-down input movement and rotation toward the cursor.
/// Requires a Rigidbody2D reference and Camera reference to compute mouse world position.
/// </summary>
public class TopDownPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    public Rigidbody2D rb;
    public Camera cam;

    Vector2 movement;
    Vector2 mousePos;

    void Update()
    {
        // Read raw axis for crisp digital movement
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Cache mouse world position for aiming/rotation
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
    }

    void FixedUpdate()
    {
        // Move rigidbody using normalized movement vector to prevent faster diagonal speed
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);

        // Rotate rigidbody to face mouse cursor (subtract 90deg if sprite's forward is up)
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
    }
}