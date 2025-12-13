using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Projectile handles movement, hit detection (raycast + trigger), direct damage and optional AOE/impact effects.
/// Requires Rigidbody2D and Collider2D on the prefab (enforced via RequireComponent in original authoring).
/// Designed for 2D physics with continuous collision detection and optional trigger-based collisions.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Properties")]
    public float damage = 10f;                 // Direct hit damage applied to Health components
    public float speed = 20f;                  // Initial velocity magnitude
    public float lifetime = 5f;                // Self-destruct time (seconds)
    public LayerMask hitLayers;                // Layers this projectile can hit

    [Header("Area of Effect")]
    public GameObject aoeEffectPrefab;         // Optional visual effect prefab for AOE
    public float aoeEffectDuration = 0.5f;     // Lifetime of spawned AOE effect
    public bool hasAOE = false;                // Enable area damage on impact
    public float aoeRadius = 1f;               // Radius for AOE damage
    public float aoeDamage = 5f;               // AOE damage magnitude (falloff applied)
    public bool aoeDamagesFriendlies = false;  // Whether AOE affects objects tagged "Player"

    [Header("Visual Effects")]
    public GameObject impactEffectPrefab;      // Visual effect spawned at impact point
    public float impactEffectDuration = 0.2f;  // Impact effect lifetime
    public TrailRenderer trail;                // Optional trail renderer (tied to prefab)

    // Cached runtime state
    private Rigidbody2D rb;
    private string ownerTag;                   // Tag of the shooter - used to ignore owner collisions
    private Vector2 lastPosition;              // Used for short-range raycast hit detection (fast moving objects)
    private bool hasHit = false;               // Prevent double-processing of impacts

    void Awake()
    {
        // Cache and configure rigidbody for projectile behaviour
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Projectiles do not fall by gravity by default
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {
        // Initialize last position for raycast-based collision checks and schedule destruction
        lastPosition = transform.position;
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Initialize sets the projectile's initial velocity and records the shooter's tag so the projectile can ignore its owner.
    /// Call immediately after Instantiating a projectile.
    /// </summary>
    /// <param name="direction">World-space direction vector the projectile should travel (normalized recommended)</param>
    /// <param name="shooterTag">Tag of shooter GameObject (projectile will ignore collisions with this tag)</param>
    public void Initialize(Vector2 direction, string shooterTag)
    {
        ownerTag = shooterTag;
        rb.velocity = direction.normalized * speed;

        // Rotate the projectile to face travel direction for visuals/effects
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FixedUpdate()
    {
        // If we haven't hit anything yet, check for collisions using a raycast between last and current position.
        if (!hasHit)
        {
            CheckRaycastHit();
            lastPosition = transform.position;
        }
    }

    /// <summary>
    /// Uses a short raycast from the previous FixedUpdate position to the current position to detect fast-moving collisions.
    /// This complements trigger collision handling to avoid tunnelling.
    /// </summary>
    void CheckRaycastHit()
    {
        Vector2 currentPosition = transform.position;
        Vector2 direction = currentPosition - lastPosition;
        float distance = direction.magnitude;

        if (distance > 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(lastPosition, direction.normalized, distance, hitLayers);

            if (hit.collider != null)
            {
                // Ignore the owner (shooter) to prevent self-hits
                if (!string.IsNullOrEmpty(ownerTag) && hit.collider.CompareTag(ownerTag))
                    return;

                // Optionally ignore players for AOE projectiles
                if (hit.collider.CompareTag("Player") && !aoeDamagesFriendlies && hasAOE)
                    return;

                Debug.Log($"Raycast Hit {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

                hasHit = true;
                ProcessHit(hit.collider, hit.point);
            }
        }
    }

    /// <summary>
    /// Trigger-based collision fallback. Projectile prefab's Collider2D should be set as a trigger for this callback.
    /// </summary>
    /// <param name="other">Collider of object we entered</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        // If already handled by raycast, skip
        if (hasHit)
            return;

        // Ignore owner collisions
        if (!string.IsNullOrEmpty(ownerTag) && other.CompareTag(ownerTag))
            return;

        // Check if the other object's layer is among hitLayers
        if (((1 << other.gameObject.layer) & hitLayers) == 0)
        {
            Debug.Log($"Ignoring collision with {other.gameObject.name} on layer {LayerMask.LayerToName(other.gameObject.layer)}");
            return;
        }
        else if (other.CompareTag("Player") && !aoeDamagesFriendlies)
        {
            // Respect friendly-fire toggle for players
            return;
        }

        Debug.Log($"Trigger Hit {other.gameObject.name} on layer {LayerMask.LayerToName(other.gameObject.layer)}");

        hasHit = true;
        Vector2 impactPoint = other.ClosestPoint(transform.position);
        ProcessHit(other, impactPoint);
    }

    /// <summary>
    /// ProcessHit applies damage, spawns impact/aoe effects and destroys the projectile.
    /// </summary>
    /// <param name="other">Collider of the directly hit object</param>
    /// <param name="impactPoint">World-space impact location</param>
    void ProcessHit(Collider2D other, Vector2 impactPoint)
    {
        // Apply direct hit damage if target has a Health component
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // Spawn and apply AOE if configured
        if (hasAOE)
        {
            if (aoeEffectPrefab != null)
            {
                GameObject effect = Instantiate(aoeEffectPrefab, impactPoint, Quaternion.identity);
                Destroy(effect, aoeEffectDuration);
            }

            ApplyAOEDamage(impactPoint, other.gameObject);
        }

        // Spawn impact visual effect if provided; orient it in the projectile's travel direction
        if (impactEffectPrefab != null)
        {
            Vector2 direction = rb != null ? rb.velocity.normalized : transform.right;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion impactRotation = Quaternion.Euler(0, 0, angle);
            GameObject effect = Instantiate(impactEffectPrefab, impactPoint, impactRotation);
            Destroy(effect, impactEffectDuration);
        }

        // Destroy the projectile after processing the hit
        Destroy(gameObject);
    }

    /// <summary>
    /// ApplyAOEDamage finds colliders within aoeRadius on hitLayers and applies damage with linear falloff.
    /// Skips the directly hit gameObject to avoid double-damage.
    /// </summary>
    /// <param name="center">AOE center position</param>
    /// <param name="directHitTarget">Directly hit GameObject to exclude</param>
    void ApplyAOEDamage(Vector2 center, GameObject directHitTarget)
    {
        Debug.Log($"AOE Center: {center}, AOE Radius: {aoeRadius}");
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, aoeRadius, hitLayers);
        Debug.Log($"Found {hits.Length} objects in AOE radius");

        foreach (Collider2D hit in hits)
        {
            // Skip the directly hit target to avoid double damage
            if (hit.gameObject == directHitTarget)
                continue;

            if (!aoeDamagesFriendlies && hit.CompareTag("Player"))
                continue;

            Health health = hit.GetComponent<Health>();
            if (health != null)
            {
                float distance = Vector3.Distance(center, hit.transform.position);

                // Damage falloff: 100% at center, 0% at radius edge
                float damageMultiplier = Mathf.InverseLerp(aoeRadius, 0, distance);
                Debug.Log($"AOE Damage to {hit.gameObject.name}: {aoeDamage * damageMultiplier} (Radius: {aoeRadius}, Distance: {distance}, Multiplier: {damageMultiplier})");
                health.TakeDamage(aoeDamage * damageMultiplier);
            }
        }
    }

    void OnDrawGizmos()
    {
        // Visualize AOE radius in editor when enabled
        if (hasAOE)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
        }
    }
}