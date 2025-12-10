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
    public float aoeRadius = 2f;
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
        // Don't hit the shooter
        if (!string.IsNullOrEmpty(ownerTag) && other.CompareTag(ownerTag))
            return;

        // Check if we should hit this layer 
        if (((1 << other.gameObject.layer) & hitLayers) == 0)
        {
            Destroy(gameObject);
            Debug.Log($"Ignoring collision with {other.gameObject.name} on layer {LayerMask.LayerToName(other.gameObject.layer)}");
            return; 
        }

        Debug.Log($"Hit {other.gameObject.name} on layer {LayerMask.LayerToName(other.gameObject.layer)}");

        // Apply direct damage
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // Handle AOE damage
        if (hasAOE)
        {
            ApplyAOEDamage(transform.position);

            if (aoeEffectPrefab != null)
            {
                GameObject effect = Instantiate(aoeEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, aoeEffectDuration);
            }
        }

        // Spawn impact effect
        if (impactEffectPrefab != null)
        {
            GameObject effect = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
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

    void ApplyAOEDamage(Vector2 center)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, aoeRadius, hitLayers);

        foreach (Collider2D hit in hits)
        {
            if (!aoeDamagesFriendlies && hit.CompareTag(ownerTag))
                continue;

            Health health = hit.GetComponent<Health>();
            if (health != null)
            {
                float distance = Vector2.Distance(center, hit.transform.position);
                float damageMultiplier = 1f - (distance / aoeRadius);
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
