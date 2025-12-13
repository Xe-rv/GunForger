using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Health component provides damage, healing, death logic and optional visual feedback (flash, effects).
/// Exposes UnityEvents for UI and game logic integration.
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;      // Maximum health value
    [SerializeField] private float currentHealth;         // Current health (runtime)
    [SerializeField] private bool isInvulnerable = false; // Temporarily ignore damage when true

    [Header("Death Settings")]
    [SerializeField] private GameObject deathEffectPrefab; // Optional effect spawned on death
    [SerializeField] private float deathDelay = 0f;        // Delay before destroying the GameObject
    [SerializeField] private bool destroyOnDeath = true;   // Destroy GameObject when health reaches zero

    [Header("Damage Effects")]
    [SerializeField] private GameObject damageEffectPrefab; // Spawned when damage occurs (small hit VFX)
    [SerializeField] private SpriteRenderer spriteRenderer; // Optional sprite renderer used to flash color on damage
    [SerializeField] private float flashDuration = 0.1f;     // How long the flash lasts
    [SerializeField] private Color damageFlashColor = Color.red; // Flash color when taking damage

    [Header("Events")]
    public UnityEvent<float> OnDamageTaken;   // Invoked with damage amount
    public UnityEvent<float> OnHealthChanged; // Invoked with new currentHealth
    public UnityEvent OnDeath;                // Invoked once on death

    private Color originalColor; // Cached original sprite color for flashing
    private bool isDead = false; // Tracks death state to prevent re-entry

    void Awake()
    {
        // Ensure starting health is initialized
        currentHealth = maxHealth;

        // Cache sprite renderer if not assigned and store original color
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    /// <summary>
    /// Apply damage to this object (respects invulnerability and death state).
    /// Invokes events and triggers visual feedback.
    /// </summary>
    /// <param name="damage">Amount of damage to apply (positive value)</param>
    public void TakeDamage(float damage)
    {
        if (isInvulnerable || isDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Notify listeners
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth);

        // Optional damage VFX
        if (damageEffectPrefab != null)
        {
            Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        }

        // Sprite flash visual feedback
        if (spriteRenderer != null)
        {
            StopAllCoroutines();
            StartCoroutine(DamageFlash());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heal the object, clamped to maxHealth and reports via events.
    /// </summary>
    /// <param name="amount">Positive heal amount</param>
    public void Heal(float amount)
    {
        if (isDead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    /// <summary>
    /// Handles death: spawns death effect, invokes OnDeath and destroys object if configured.
    /// </summary>
    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        OnDeath?.Invoke();

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        if (destroyOnDeath)
        {
            Destroy(gameObject, deathDelay);
        }
    }

    /// <summary>
    /// Briefly flashes the sprite to provide damage feedback.
    /// </summary>
    private System.Collections.IEnumerator DamageFlash()
    {
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    // Public accessor helpers
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsDead() => isDead;
    public void SetInvulnerable(bool invulnerable) => isInvulnerable = invulnerable;
}
