using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isInvulnerable = false;

    [Header("Death Settings")]
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private float deathDelay = 0f;
    [SerializeField] private bool destroyOnDeath = true;

    [Header("Damage Effects")]
    [SerializeField] private GameObject damageEffectPrefab;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color damageFlashColor = Color.red;

    [Header("Events")]
    public UnityEvent<float> OnDamageTaken;
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnDeath;

    private Color originalColor;
    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable || isDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth);

        // Visual feedback
        if (damageEffectPrefab != null)
        {
            Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        }

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

    public void Heal(float amount)
    {
        if (isDead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

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

    private System.Collections.IEnumerator DamageFlash()
    {
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsDead() => isDead;
    public void SetInvulnerable(bool invulnerable) => isInvulnerable = invulnerable;
}
