using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Properties")]
    public float damage = 10f;
    public float speed = 20f;
    public float lifetime = 5f;
    public LayerMask hitLayers;

    [Header("Area of Effect")]
    public GameObject aoeEffectPrefab;
    public float aoeEffectDuration = 0.5f;
    public bool hasAOE = false;
    public float aoeRadius = 1f;
    public float aoeDamage = 5f;
    public bool aoeDamagesFriendlies = false;

    [Header("Visual Effects")]
    public GameObject impactEffectPrefab;
    public float impactEffectDuration = 0.2f;
    public TrailRenderer trail;

    private Rigidbody2D rb;
    private string ownerTag;
    private Vector2 lastPosition;
    private bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {
        lastPosition = transform.position;
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 direction, string shooterTag)
    {
        ownerTag = shooterTag;
        rb.velocity = direction.normalized * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FixedUpdate()
    {
        if (!hasHit)
        {
            CheckRaycastHit();
            lastPosition = transform.position;
        }
    }

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
                // Don't hit the owner
                if (!string.IsNullOrEmpty(ownerTag) && hit.collider.CompareTag(ownerTag))
                    return;

                if (hit.collider.CompareTag("Player") && !aoeDamagesFriendlies && hasAOE)
                    return;

                Debug.Log($"Raycast Hit {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

                hasHit = true;
                ProcessHit(hit.collider, hit.point);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Skip if we already processed a hit via raycast
        if (hasHit)
            return;

        // Don't hit the owner
        if (!string.IsNullOrEmpty(ownerTag) && other.CompareTag(ownerTag))
            return;

        // Check if we should hit this layer 
        if (((1 << other.gameObject.layer) & hitLayers) == 0)
        {
            Debug.Log($"Ignoring collision with {other.gameObject.name} on layer {LayerMask.LayerToName(other.gameObject.layer)}");
            return;
        }
        else if (other.CompareTag("Player") && !aoeDamagesFriendlies)
        {
            return;
        }

        Debug.Log($"Trigger Hit {other.gameObject.name} on layer {LayerMask.LayerToName(other.gameObject.layer)}");

        hasHit = true;
        Vector2 impactPoint = other.ClosestPoint(transform.position);
        ProcessHit(other, impactPoint);
    }

    void ProcessHit(Collider2D other, Vector2 impactPoint)
    {
        // Apply direct damage
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        if (hasAOE)
        {
            if (aoeEffectPrefab != null)
            {
                GameObject effect = Instantiate(aoeEffectPrefab, impactPoint, Quaternion.identity);
                Destroy(effect, aoeEffectDuration);
            }

            ApplyAOEDamage(impactPoint, other.gameObject);
        }

        if (impactEffectPrefab != null)
        {
            Vector2 direction = rb != null ? rb.velocity.normalized : transform.right;

            // Face opposite to the projectile's direction of travel
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion impactRotation = Quaternion.Euler(0, 0, angle);
            GameObject effect = Instantiate(impactEffectPrefab, impactPoint, impactRotation);
            Destroy(effect, impactEffectDuration);
        }

        Destroy(gameObject);
    }

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

                // Calculate falloff: 100% damage at center, 0% at edge
                float damageMultiplier = Mathf.InverseLerp(aoeRadius, 0, distance);

                Debug.Log($"AOE Damage to {hit.gameObject.name}: {aoeDamage * damageMultiplier} (Radius: {aoeRadius}, Distance: {distance}, Multiplier: {damageMultiplier})");
                health.TakeDamage(aoeDamage * damageMultiplier);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (hasAOE)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
        }
    }
}