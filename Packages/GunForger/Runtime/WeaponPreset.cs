using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WeaponPreset is a ScriptableObject storing reusable weapon configuration data.
/// Use CreateAssetMenu to create presets in the Project window.
/// </summary>
[CreateAssetMenu(fileName = "New Weapon Preset", menuName = "2D Weapon System/Weapon Preset")]
public class WeaponPreset : ScriptableObject
{
    [Header("Weapon Identity")]
    public string weaponName = "Pistol";
    public Sprite weaponSprite;
    public RuntimeAnimatorController weaponAnimator;

    [Header("Ammunition")]
    public int ammoPerMagazine = 30;
    public int maxTotalAmmo = 300;
    public bool infiniteAmmo = false;

    [Header("Fire Rate")]
    public float fireRate = 0.1f;
    public bool isAutomatic = true;
    public bool isBurstFire = false;
    public int burstCount = 3;
    public float burstDelay = 0.1f;

    [Header("Reload")]
    public float reloadTime = 2f;
    public bool autoReload = true;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public int projectilesPerShot = 1;
    public float projectileSpread = 5f;
    public float projectileSpeed = 20f;
    public float projectileDamage = 10f;
    public Vector3 projectileScale = Vector3.one;

    [Header("Effects")]
    public GameObject muzzleFlashPrefab;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;

    [Header("Recoil")]
    public bool hasRecoil = true;
    public float recoilAmount = 0.1f;
    public float recoilRecoverySpeed = 5f;

    [Header("Aiming")]
    public bool rotateTowardsCursor = true;
    public bool flipSpriteWhenAimingLeft = true;

    /// <summary>
    /// ApplyToWeapon should copy preset values to a RangedWeapon instance.
    /// Currently logs readiness: extending RangedWeapon with public setters or making fields accessible will allow full application.
    /// </summary>
    /// <param name="weapon">Target RangedWeapon to apply preset to</param>
    public void ApplyToWeapon(RangedWeapon weapon)
    {
        if (weapon == null)
        {
            Debug.LogError("Cannot apply preset to null weapon!");
            return;
        }

        // Note: prefer creating a public API on RangedWeapon to accept presets instead of reflection.
        Debug.Log($"Weapon preset '{weaponName}' ready to be applied.");
    }
}

#if UNITY_EDITOR
// Custom inspector for WeaponPreset that shows quick stats and helpful documentation in the editor.
[UnityEditor.CustomEditor(typeof(WeaponPreset))]
public class WeaponPresetEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        WeaponPreset preset = (WeaponPreset)target;

        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.LabelField("Weapon Preset", UnityEditor.EditorStyles.boldLabel);
        UnityEditor.EditorGUILayout.HelpBox("Create reusable weapon configurations. Create weapons from presets using the right-click menu in the hierarchy.", UnityEditor.MessageType.Info);
        UnityEditor.EditorGUILayout.Space();

        DrawDefaultInspector();

        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
        UnityEditor.EditorGUILayout.LabelField("Weapon Statistics", UnityEditor.EditorStyles.boldLabel);

        // Compute simple statistics for balancing (shots/sec * damage * projectiles)
        float dps = (preset.fireRate > 0f) ? (1f / preset.fireRate) * preset.projectileDamage * preset.projectilesPerShot : 0f;
        float magazineDuration = preset.ammoPerMagazine * preset.fireRate;

        UnityEditor.EditorGUILayout.LabelField($"DPS: {dps:F1}");
        UnityEditor.EditorGUILayout.LabelField($"Shots/Second: {(preset.fireRate > 0f ? 1f / preset.fireRate : 0f):F2}");
        UnityEditor.EditorGUILayout.LabelField($"Magazine Duration: {magazineDuration:F1}s");
        UnityEditor.EditorGUILayout.LabelField($"Total Shots Available: {preset.maxTotalAmmo + preset.ammoPerMagazine}");

        UnityEditor.EditorGUILayout.EndVertical();
    }
}
#endif
