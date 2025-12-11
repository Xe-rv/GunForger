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

    [Header("Penetration")]
    public bool canPenetrate = false;
    public int maxPenetrations = 1;

    private Rigidbody2D rb;
    private int penetrationCount = 0;
    private string ownerTag;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 direction, string shooterTag)
    {
        ownerTag = shooterTag;
        rb.velocity = direction.normalized * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit the gun
        if (!string.IsNullOrEmpty(ownerTag) && other.CompareTag(ownerTag))
            return;

        // Check if we should hit this layer 
        if (((1 << other.gameObject.layer) & hitLayers) == 0 && !hasAOE)
        {
            Destroy(gameObject);
            Debug.Log($"Ignoring collision with {other.gameObject.name} on layer {LayerMask.LayerToName(other.gameObject.layer)}");
            return;
        } else if (other.CompareTag("Player") && !aoeDamagesFriendlies)
        {
            return;
        }

        Debug.Log($"Hit {other.gameObject.name} on layer {LayerMask.LayerToName(other.gameObject.layer)}");

        // Apply direct damage
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        Vector2 impactPoint = other.ClosestPoint(transform.position);

        if (hasAOE)
        {
            ApplyAOEDamage(impactPoint, other.gameObject); // Use collision point, not transform.position

            if (aoeEffectPrefab != null)
            {
                GameObject effect = Instantiate(aoeEffectPrefab, impactPoint, Quaternion.identity, this.transform.parent);
                Destroy(effect, aoeEffectDuration);
            }
        }

        if (impactEffectPrefab != null)
        {
            // Calculate the normal direction from the projectile to the impact point
            Vector2 hitNormal = (impactPoint - (Vector2)transform.position).normalized;

            // Calculate angle from the normal vector
            float angle = Mathf.Atan2(hitNormal.y, hitNormal.x) * Mathf.Rad2Deg;
            Quaternion impactRotation = Quaternion.Euler(0, 0, angle);

            GameObject effect = Instantiate(impactEffectPrefab, impactPoint, impactRotation);
            Destroy(effect, impactEffectDuration);
        }

        // Handle penetration
        if (canPenetrate && penetrationCount < maxPenetrations)
        {
            penetrationCount++;
            return;
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
                float distance = Vector3.Distance(center, transform.position);
                
                // Calculate falloff: 100% damage at center, 0% at edge
                float damageMultiplier = Mathf.InverseLerp(aoeRadius, 0, distance);

                Debug.Log($"AOE Damage to {hit.gameObject.name}: {aoeDamage * damageMultiplier} (Radius: {aoeRadius}, Distance: {distance}, Multiplier: {damageMultiplier})");
                health.TakeDamage(aoeDamage * damageMultiplier);
            }
        }
    }


    void OnDrawGizmosSelected()
    {
        if (hasAOE)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
        }
    }
}