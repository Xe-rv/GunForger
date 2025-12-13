using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject Player;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject reloadIndicator;
    [SerializeField] private TextMeshProUGUI crosshairText;

    [Header("Crosshair")]
    [SerializeField] private bool showCrosshair = true;
    [SerializeField] private string crosshairCharacter = "+";
    [SerializeField] private Color crosshairColor = Color.white;

    private Camera mainCamera;
    private RangedWeapon weapon;

    void Start()
    {
        mainCamera = Camera.main;

        if (reloadIndicator != null)
        reloadIndicator.SetActive(false);

        if (crosshairText != null)
        {
            crosshairText.text = crosshairCharacter;
            crosshairText.color = crosshairColor;
            crosshairText.gameObject.SetActive(showCrosshair);
        }

        Cursor.visible = !showCrosshair;
    }

    void Update()
    {
        if (crosshairText != null && showCrosshair)
        {
            crosshairText.transform.position = Input.mousePosition;
        }

        if (Player != null)
        {
            weapon = Player.GetComponentInChildren<RangedWeapon>();
        }

        if (weapon != null)
        {
            weapon.OnAmmoChanged.AddListener(UpdateAmmoDisplay);
            weapon.OnReloadStart.AddListener(OnReloadStart);
            weapon.OnReloadComplete.AddListener(OnReloadComplete);
            UpdateAmmoDisplay();
            UpdateWeaponName();
        }
        else
        {
            weaponNameText.text = string.Empty;
            ammoText.text = string.Empty;
        }
    }

    void UpdateAmmoDisplay()
    {
        if (ammoText != null && weapon != null)
        {
            ammoText.text = $"{weapon.GetCurrentMagazineAmmo()} / {weapon.GetCurrentTotalAmmo()}";
        }
    }

    void UpdateWeaponName()
    {
        if (weaponNameText != null && weapon != null)
        {
            weaponNameText.text = weapon.GetWeaponName();
        }
    }

    void OnReloadStart()
    {
        if (reloadIndicator != null)
            reloadIndicator.SetActive(true);
    }

    void OnReloadComplete()
    {
        if (reloadIndicator != null)
            reloadIndicator.SetActive(false);
    }

}
