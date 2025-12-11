using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject Player;
    [SerializeField] private RangedWeapon weapon;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image reloadProgressBar;
    [SerializeField] private GameObject reloadIndicator;
    [SerializeField] private TextMeshProUGUI crosshairText;

    [Header("Crosshair")]
    [SerializeField] private bool showCrosshair = true;
    [SerializeField] private string crosshairCharacter = "+";
    [SerializeField] private Color crosshairColor = Color.white;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

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

        StartCoroutine(UpdateReloadProgress());
    }

    void OnReloadComplete()
    {
        if (reloadIndicator != null)
            reloadIndicator.SetActive(false);

        if (reloadProgressBar != null)
            reloadProgressBar.fillAmount = 0f;
    }

    System.Collections.IEnumerator UpdateReloadProgress()
    {
        float startTime = Time.time;
        float reloadDuration = weapon.GetReloadTime(); // This should match weapon reload time

        while (weapon != null && weapon.IsReloading())
        {
            float elapsed = Time.time - startTime;
            float progress = elapsed / reloadDuration;

            if (reloadProgressBar != null)
                reloadProgressBar.fillAmount = progress;

            yield return null;
        }
    }
}
