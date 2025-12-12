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
        Time.timeScale = 0f; // Pause game
        
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
                button.onClick.AddListener(() => SelectWeapon(weaponIndex));
            }
        }
    }

    void SelectWeapon(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= weaponPrefabs.Count)
            return;

        GameObject weaponPrefab = weaponPrefabs[weaponIndex];
        if (weaponPrefab == null)
            return;

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
                holderObj.transform.localPosition = Vector3.zero;
                holderObj.transform.localRotation = Quaternion.identity;
                attachPoint = holderObj.transform;
            }
            else
            {
                attachPoint = holder;
            }
        }

        // Destroy existing weapons
        foreach (Transform child in attachPoint)
        {
            Destroy(child.gameObject);
        }

        // Instantiate new weapon
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
        // Draw interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}