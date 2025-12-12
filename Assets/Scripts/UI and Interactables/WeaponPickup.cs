using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Prefabs")]
    [SerializeField] private List<GameObject> weaponPrefabs = new List<GameObject>();

    [Header("UI References")]
    [SerializeField] private GameObject popupMenuPanel;
    [SerializeField] private GameObject weaponButtonPrefab;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private Transform weaponAttachPoint; // Where weapon attaches to player
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool pauseGameOnMenu = false; // Set to false - UI doesn't work when paused!

    private GameObject player;
    private bool playerInRange = false;
    private bool menuOpen = false;
    private Canvas menuCanvas;

    void Start()
    {
        if (popupMenuPanel != null)
        {
            popupMenuPanel.SetActive(false);
            // Get or add canvas
            menuCanvas = popupMenuPanel.GetComponentInParent<Canvas>();
        }

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

    void OpenWeaponMenu()
    {
        if (popupMenuPanel == null || weaponPrefabs.Count == 0)
        {
            Debug.LogWarning("Popup menu panel or weapon prefabs not assigned!");
            return;
        }

        menuOpen = true;
        popupMenuPanel.SetActive(true);

        if (pauseGameOnMenu)
            Time.timeScale = 0f;

        // Clear existing buttons
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // Create button for each weapon
        for (int i = 0; i < weaponPrefabs.Count; i++)
        {
            int weaponIndex = i; // Capture for lambda
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
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SelectWeapon(weaponIndex));

                // Debug to verify button setup
                Debug.Log($"Created button for weapon {weaponIndex}: {buttonText?.text}");
            }
        }
    }

    void SelectWeapon(int weaponIndex)
    {
        Debug.Log($"SelectWeapon called with index: {weaponIndex}");

        if (weaponIndex < 0 || weaponIndex >= weaponPrefabs.Count)
        {
            Debug.LogWarning($"Invalid weapon index: {weaponIndex}");
            return;
        }

        GameObject weaponPrefab = weaponPrefabs[weaponIndex];
        if (weaponPrefab == null)
        {
            Debug.LogWarning($"Weapon prefab at index {weaponIndex} is null!");
            return;
        }

        // Find or create weapon attach point
        Transform attachPoint = weaponAttachPoint;
        if (attachPoint == null && player != null)
        {
            // Look for existing weapon holder
            Transform holder = player.transform.Find("WeaponHolder");
            if (holder == null)
            {
                GameObject holderObj = new GameObject("WeaponHolder");
                holderObj.transform.SetParent(player.transform);
                holderObj.transform.localPosition = new Vector3(0.78f, 0f, 0f);
                holderObj.transform.localRotation = Quaternion.identity;
                attachPoint = holderObj.transform;
            }
            else
            {
                attachPoint = holder;
            }
        }

        // Destroy existing weapons
        if (attachPoint != null)
        {
            foreach (Transform child in attachPoint)
            {
                Destroy(child.gameObject);
            }
        }

        // Instantiate new weapon
        GameObject newWeapon = Instantiate(weaponPrefab, attachPoint);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        Debug.Log($"Successfully equipped: {newWeapon.name}");

        CloseWeaponMenu();
    }

    void CloseWeaponMenu()
    {
        Debug.Log("Closing weapon menu");
        menuOpen = false;

        if (popupMenuPanel != null)
            popupMenuPanel.SetActive(false);

        if (pauseGameOnMenu)
            Time.timeScale = 1f;
    }

    void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}