using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WeaponPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform weaponHoldPoint; // Optional: specific point to attach weapon
    [SerializeField] private Vector3 localPosition = new Vector3(0.3f, 0, 0);
    [SerializeField] private bool autoPickup = true;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject pickupPrompt;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmount = 0.2f;

    private RangedWeapon weapon;
    private bool playerInRange = false;
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        weapon = GetComponent<RangedWeapon>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;

        // Ensure collider is trigger for pickup
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;

        // Disable weapon functionality until picked up
        if (weapon != null)
            weapon.enabled = false;

        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);
    }

    void Update()
    {
        // Bobbing animation when on ground
        if (!weapon.enabled)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }

        // Manual pickup with key press
        if (!autoPickup && playerInRange && Input.GetKeyDown(pickupKey))
        {
            PickupWeapon();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;

            if (pickupPrompt != null)
                pickupPrompt.SetActive(true);

            if (autoPickup)
            {
                PickupWeapon(other.transform);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;

            if (pickupPrompt != null)
                pickupPrompt.SetActive(false);
        }
    }

    void PickupWeapon(Transform player = null)
    {
        // Find player if not provided
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (player == null)
        {
            Debug.LogWarning("Cannot pickup weapon - player not found!");
            return;
        }

        // Check if player already has a weapon and drop it
        RangedWeapon existingWeapon = player.GetComponentInChildren<RangedWeapon>();
        if (existingWeapon != null && existingWeapon.gameObject != gameObject)
        {
            DropWeapon(existingWeapon);
        }

        // Attach weapon to player
        if (weaponHoldPoint != null)
        {
            transform.SetParent(weaponHoldPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            transform.SetParent(player);
            transform.localPosition = localPosition;
            transform.localRotation = Quaternion.identity;
        }

        // Enable weapon functionality
        if (weapon != null)
            weapon.enabled = true;

        // Disable pickup collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Hide pickup prompt
        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);

        // Disable this script
        this.enabled = false;

        Debug.Log($"Picked up {weapon.GetWeaponName()}");
    }

    void DropWeapon(RangedWeapon weaponToDrop)
    {
        if (weaponToDrop == null) return;

        // Detach from player
        Transform weaponTransform = weaponToDrop.transform;
        weaponTransform.SetParent(null);

        // Disable weapon
        weaponToDrop.enabled = false;

        // Re-enable pickup
        WeaponPickup pickup = weaponToDrop.GetComponent<WeaponPickup>();
        if (pickup != null)
        {
            pickup.enabled = true;
            pickup.startPosition = weaponTransform.position;

            Collider2D col = weaponToDrop.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}

#if UNITY_EDITOR
// Add menu item to create weapon pickup
public static class WeaponPickupMenu
{
    [UnityEditor.MenuItem("GameObject/2D Weapon System/Create Weapon Pickup", false, 13)]
    static void CreateWeaponPickup()
    {
        GameObject weaponPickup = new GameObject("WeaponPickup");
        
        // Add sprite
        SpriteRenderer sr = weaponPickup.AddComponent<SpriteRenderer>();
        sr.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = Color.cyan;
        
        // Add collider for pickup trigger
        CircleCollider2D col = weaponPickup.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1f;
        
        // Add weapon and pickup components
        weaponPickup.AddComponent<RangedWeapon>();
        weaponPickup.AddComponent<WeaponPickup>();
        
        // Create fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(weaponPickup.transform);
        firePoint.transform.localPosition = new Vector3(0.5f, 0, 0);
        
        UnityEditor.Selection.activeGameObject = weaponPickup;
        UnityEngine.Debug.Log("Weapon Pickup created! Configure the weapon and assign a projectile prefab.");
    }
}
#endif
