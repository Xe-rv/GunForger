using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// WeaponPickup provides an in-world weapon selection UI. When player is in range, pressing interactKey opens a menu to choose a weapon prefab.
/// Selected weapon is instantiated and attached to the player's WeaponHolder (or created one).
/// </summary>
public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Prefabs")]
    [SerializeField] private List<GameObject> weaponPrefabs = new List<GameObject>();

    [Header("UI References")]
    [SerializeField] private GameObject popupMenuPanel;      // Panel containing weapon selection buttons
    [SerializeField] private GameObject weaponButtonPrefab;  // Prefab for buttons (must contain Button + TextMeshProUGUI)
    [SerializeField] private Transform buttonContainer;      // Parent transform for generated buttons
    [SerializeField] private TextMeshProUGUI promptText;     // Prompt shown when in range
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Settings")]
    [SerializeField] private float interactionRange = 2f;    // Radius to detect player proximity
    [SerializeField] private Transform weaponAttachPoint;    // Optional explicit attach point
    [SerializeField] private string playerTag = "Player";

    private GameObject player;
    private bool playerInRange = false;
    private bool menuOpen = false;

    void Start()
    {
        if (popupMenuPanel != null)
            popupMenuPanel.SetActive(false);

        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        CheckPlayerProximity();

        if (playerInRange && !menuOpen)
        {
            if (promptText != null)
            {
                promptText.gameObject.SetActive(true);
                promptText.text = $"Press [{interactKey}] to select weapon";
            }

            if (Input.GetKeyDown(interactKey))
            {
                OpenWeaponMenu();
            }
        }
        else if (!playerInRange && promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }

        // Close menu with Escape
        if (menuOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseWeaponMenu();
        }
    }

    /// <summary>
    /// Locate the player GameObject by tag and compute distance to determine proximity.
    /// </summary>
    void CheckPlayerProximity()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag(playerTag);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.transform.position);
        playerInRange = distance <= interactionRange;
    }

    /// <summary>
    /// Opens the popup menu and populates it with buttons for each configured weapon prefab.
    /// Pauses game time while menu is open.
    /// </summary>
    void OpenWeaponMenu()
    {
        if (popupMenuPanel == null || weaponPrefabs.Count == 0)
        {
            Debug.LogWarning("Popup menu panel or weapon prefabs not assigned!");
            return;
        }

        menuOpen = true;
        popupMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Pause game

        // Clear previous buttons
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // Create button for each weapon
        for (int i = 0; i < weaponPrefabs.Count; i++)
        {
            int weaponIndex = i; // capture for closure
            GameObject buttonObj = Instantiate(weaponButtonPrefab, buttonContainer);

            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (weaponPrefabs[weaponIndex] != null)
            {
                RangedWeapon weapon = weaponPrefabs[weaponIndex].GetComponent<RangedWeapon>();
                if (weapon != null && buttonText != null)
                {
                    buttonText.text = weapon.GetWeaponName();
                }
                else if (buttonText != null)
                {
                    buttonText.text = weaponPrefabs[weaponIndex].name;
                }
            }

            if (button != null)
            {
                button.onClick.AddListener(() => SelectWeapon(weaponIndex));
            }
        }
    }

    /// <summary>
    /// Instantiate and attach the chosen weapon to the player's attach point (creates WeaponHolder if needed).
    /// Replaces any existing child weapons.
    /// </summary>
    /// <param name="weaponIndex">Index in weaponPrefabs</param>
    void SelectWeapon(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= weaponPrefabs.Count)
            return;

        GameObject weaponPrefab = weaponPrefabs[weaponIndex];
        if (weaponPrefab == null)
            return;

        // Determine attach point (explicit or created under player)
        Transform attachPoint = weaponAttachPoint;
        if (attachPoint == null && player != null)
        {
            Transform holder = player.transform.Find("WeaponHolder");
            if (holder == null)
            {
                GameObject holderObj = new GameObject("WeaponHolder");
                holderObj.transform.SetParent(player.transform);
                holderObj.transform.localPosition = Vector3.zero;
                holderObj.transform.localRotation = Quaternion.identity;
                attachPoint = holderObj.transform;
            }
            else
            {
                attachPoint = holder;
            }
        }

        // Remove existing weapons
        foreach (Transform child in attachPoint)
        {
            Destroy(child.gameObject);
        }

        // Instantiate new weapon as child
        GameObject newWeapon = Instantiate(weaponPrefab, attachPoint);
        newWeapon.transform.localRotation = Quaternion.identity;

        Debug.Log($"Equipped: {newWeapon.name}");

        CloseWeaponMenu();
    }

    void CloseWeaponMenu()
    {
        menuOpen = false;
        if (popupMenuPanel != null)
            popupMenuPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game
    }

    void OnDrawGizmosSelected()
    {
        // Visualize interaction radius in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}