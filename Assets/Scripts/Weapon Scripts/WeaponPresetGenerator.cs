using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Editor utility to generate fully configured weapon PREFABS (not just .asset files)
/// Usage: Right-click in Project → Create → 2D Weapon System → Weapon Prefabs → [Weapon Type]
/// </summary>
public static class WeaponPrefabGenerator
{
    private const string MENU_PATH = "Assets/Create/2D Weapon System/Weapon Prefabs/";
    
    // PISTOL - Fast, accurate, low damage
    [MenuItem(MENU_PATH + "Pistol", false, 1)]
    public static void CreatePistol()
    {
        CreateWeaponPrefab("Pistol", weapon =>
        {
            SetField(weapon, "ammoPerMagazine", 12);
            SetField(weapon, "maxTotalAmmo", 120);
            SetField(weapon, "fireRate", 0.15f);
            SetField(weapon, "isAutomatic", false);
            SetField(weapon, "projectileSpread", 2f);
            SetField(weapon, "projectileSpeed", 20f);
            SetField(weapon, "projectileDamage", 15f);
            SetField(weapon, "reloadTime", 1.5f);
            SetField(weapon, "hasRecoil", true);
            SetField(weapon, "recoilAmount", 0.05f);
        }, new Color(0.7f, 0.7f, 0.7f));
    }
    
    // ASSAULT RIFLE - Automatic, medium damage, moderate spread
    [MenuItem(MENU_PATH + "Assault Rifle", false, 2)]
    public static void CreateAssaultRifle()
    {
        CreateWeaponPrefab("AssaultRifle", weapon =>
        {
            SetField(weapon, "ammoPerMagazine", 30);
            SetField(weapon, "maxTotalAmmo", 300);
            SetField(weapon, "fireRate", 0.08f);
            SetField(weapon, "isAutomatic", true);
            SetField(weapon, "projectileSpread", 5f);
            SetField(weapon, "projectileSpeed", 25f);
            SetField(weapon, "projectileDamage", 12f);
            SetField(weapon, "reloadTime", 2.5f);
            SetField(weapon, "hasRecoil", true);
            SetField(weapon, "recoilAmount", 0.08f);
        }, new Color(0.3f, 0.3f, 0.3f));
    }
    
    // SHOTGUN - High spread, multiple projectiles, devastating close range
    [MenuItem(MENU_PATH + "Shotgun", false, 3)]
    public static void CreateShotgun()
    {
        CreateWeaponPrefab("Shotgun", weapon =>
        {
            SetField(weapon, "ammoPerMagazine", 8);
            SetField(weapon, "maxTotalAmmo", 64);
            SetField(weapon, "fireRate", 0.8f);
            SetField(weapon, "isAutomatic", false);
            SetField(weapon, "projectilesPerShot", 8);
            SetField(weapon, "projectileSpread", 25f);
            SetField(weapon, "projectileSpeed", 18f);
            SetField(weapon, "projectileDamage", 8f);
            SetField(weapon, "reloadTime", 3f);
            SetField(weapon, "hasRecoil", true);
            SetField(weapon, "recoilAmount", 0.2f);
        }, new Color(0.6f, 0.4f, 0.2f));
    }
    
    // SNIPER RIFLE - High damage, slow fire rate, precise
    [MenuItem(MENU_PATH + "Sniper Rifle", false, 4)]
    public static void CreateSniper()
    {
        CreateWeaponPrefab("SniperRifle", weapon =>
        {
            SetField(weapon, "ammoPerMagazine", 5);
            SetField(weapon, "maxTotalAmmo", 50);
            SetField(weapon, "fireRate", 1.5f);
            SetField(weapon, "isAutomatic", false);
            SetField(weapon, "projectileSpread", 0f);
            SetField(weapon, "projectileSpeed", 50f);
            SetField(weapon, "projectileDamage", 100f);
            SetField(weapon, "reloadTime", 3f);
            SetField(weapon, "hasRecoil", true);
            SetField(weapon, "recoilAmount", 0.3f);
        }, new Color(0.2f, 0.5f, 0.3f));
    }
    
    // SUBMACHINE GUN - Very fast fire, low damage, high spread
    [MenuItem(MENU_PATH + "SMG", false, 5)]
    public static void CreateSMG()
    {
        CreateWeaponPrefab("SMG", weapon =>
        {
            SetField(weapon, "ammoPerMagazine", 40);
            SetField(weapon, "maxTotalAmmo", 400);
            SetField(weapon, "fireRate", 0.05f);
            SetField(weapon, "isAutomatic", true);
            SetField(weapon, "projectileSpread", 10f);
            SetField(weapon, "projectileSpeed", 22f);
            SetField(weapon, "projectileDamage", 8f);
            SetField(weapon, "reloadTime", 2f);
            SetField(weapon, "hasRecoil", true);
            SetField(weapon, "recoilAmount", 0.06f);
        }, new Color(0.9f, 0.9f, 0.3f));
    }
    
    // BURST RIFLE - 3-round burst, balanced
    [MenuItem(MENU_PATH + "Burst Rifle", false, 6)]
    public static void CreateBurstRifle()
    {
        CreateWeaponPrefab("BurstRifle", weapon =>
        {
            SetField(weapon, "ammoPerMagazine", 30);
            SetField(weapon, "maxTotalAmmo", 300);
            SetField(weapon, "fireRate", 0.5f);
            SetField(weapon, "isAutomatic", false);
            SetField(weapon, "isBurstFire", true);
            SetField(weapon, "burstCount", 3);
            SetField(weapon, "burstDelay", 0.08f);
            SetField(weapon, "projectileSpread", 3f);
            SetField(weapon, "projectileSpeed", 24f);
            SetField(weapon, "projectileDamage", 15f);
            SetField(weapon, "reloadTime", 2.2f);
            SetField(weapon, "hasRecoil", true);
            SetField(weapon, "recoilAmount", 0.1f);
        }, new Color(0.4f, 0.4f, 0.6f));
    }
    
    // ROCKET LAUNCHER - AOE damage, slow reload
    [MenuItem(MENU_PATH + "Rocket Launcher", false, 7)]
    public static void CreateRocketLauncher()
    {
        CreateWeaponPrefab("RocketLauncher", weapon =>
        {
            SetField(weapon, "ammoPerMagazine", 1);
            SetField(weapon, "maxTotalAmmo", 10);
            SetField(weapon, "fireRate", 2f);
            SetField(weapon, "isAutomatic", false);
            SetField(weapon, "projectileSpread", 0f);
            SetField(weapon, "projectileSpeed", 15f);
            SetField(weapon, "projectileDamage", 50f);
            SetField(weapon, "projectileScale", Vector3.one * 1.5f);
            SetField(weapon, "reloadTime", 3f);
            SetField(weapon, "hasRecoil", true);
            SetField(weapon, "recoilAmount", 0.4f);
            SetField(weapon, "AutomaticOn", false);
        }, new Color(0.8f, 0.2f, 0.2f));
    }
    
    // MINIGUN - Extremely fast fire, medium damage
    [MenuItem(MENU_PATH + "Minigun", false, 8)]
    public static void CreateMinigun()
    {
        CreateWeaponPrefab("Minigun", weapon =>
        {
            SetField(weapon, "ammoPerMagazine", 200);
            SetField(weapon, "maxTotalAmmo", 1000);
            SetField(weapon, "fireRate", 0.03f);
            SetField(weapon, "isAutomatic", true);
            SetField(weapon, "projectileSpread", 15f);
            SetField(weapon, "projectileSpeed", 28f);
            SetField(weapon, "projectileDamage", 10f);
            SetField(weapon, "reloadTime", 5f);
            SetField(weapon, "hasRecoil", true);
            SetField(weapon, "recoilAmount", 0.15f);
        }, new Color(0.5f, 0.5f, 0.5f));
    }
    
    // PLASMA RIFLE - Energy weapon, no spread, infinite ammo
    [MenuItem(MENU_PATH + "Plasma Rifle", false, 9)]
    public static void CreatePlasmaRifle()
    {
        CreateWeaponPrefab("PlasmaRifle", weapon =>
        {
            SetField(weapon, "ammoPerMagazine", 999);
            SetField(weapon, "maxTotalAmmo", 0);
            SetField(weapon, "infiniteAmmo", true);
            SetField(weapon, "fireRate", 0.2f);
            SetField(weapon, "isAutomatic", true);
            SetField(weapon, "projectileSpread", 0f);
            SetField(weapon, "projectileSpeed", 30f);
            SetField(weapon, "projectileDamage", 20f);
            SetField(weapon, "projectileScale", Vector3.one * 1.2f);
            SetField(weapon, "reloadTime", 0f);
            SetField(weapon, "hasRecoil", false);
        }, new Color(0.2f, 0.8f, 1f));
    }
    
    // FLAMETHROWER - Continuous short-range stream
    [MenuItem(MENU_PATH + "Flamethrower", false, 10)]
    public static void CreateFlamethrower()
    {
        CreateWeaponPrefab("Flamethrower", weapon =>
        {
            SetField(weapon, "ammoPerMagazine", 100);
            SetField(weapon, "maxTotalAmmo", 500);
            SetField(weapon, "fireRate", 0.02f);
            SetField(weapon, "isAutomatic", true);
            SetField(weapon, "projectileSpread", 20f);
            SetField(weapon, "projectileSpeed", 8f);
            SetField(weapon, "projectileDamage", 3f);
            SetField(weapon, "projectileScale", Vector3.one * 0.5f);
            SetField(weapon, "reloadTime", 3f);
            SetField(weapon, "hasRecoil", false);
        }, new Color(1f, 0.5f, 0f));
    }
    
    // CREATE ALL WEAPONS AT ONCE
    [MenuItem(MENU_PATH + "Create All Weapons", false, 100)]
    public static void CreateAllWeapons()
    {
        CreatePistol();
        CreateAssaultRifle();
        CreateShotgun();
        CreateSniper();
        CreateSMG();
        CreateBurstRifle();
        CreateRocketLauncher();
        CreateMinigun();
        CreatePlasmaRifle();
        CreateFlamethrower();
        
        Debug.Log("✓ Created all 10 weapon prefabs!");
    }
    
    // CORE WEAPON CREATION METHOD
    private static void CreateWeaponPrefab(string weaponName, System.Action<RangedWeapon> configure, Color weaponColor)
    {
        // Create weapon GameObject
        GameObject weapon = new GameObject(weaponName);
        
        // Add sprite renderer
        SpriteRenderer sr = weapon.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = weaponColor;
        
        // Add weapon component
        RangedWeapon rangedWeapon = weapon.AddComponent<RangedWeapon>();
        
        // Create fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(weapon.transform);
        firePoint.transform.localPosition = new Vector3(0.5f, 0, 0);
        
        // Configure weapon stats
        SetField(rangedWeapon, "weaponName", weaponName);
        configure(rangedWeapon);
        
        // Determine save path
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
        {
            path = "Assets";
        }
        else if (System.IO.Path.GetExtension(path) != "")
        {
            path = path.Replace(System.IO.Path.GetFileName(path), "");
        }
        
        // Save as prefab
        string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{weaponName}.prefab");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(weapon, prefabPath);
        
        // Cleanup scene object
        Object.DestroyImmediate(weapon);
        
        // Select and highlight the created prefab
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        
        Debug.Log($"✓ Created weapon prefab: {weaponName} at {prefabPath}");
    }
    
    // HELPER METHOD - Set private fields using reflection
    private static void SetField(RangedWeapon weapon, string fieldName, object value)
    {
        var field = typeof(RangedWeapon).GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(weapon, value);
        }
        else
        {
            Debug.LogWarning($"Field '{fieldName}' not found on RangedWeapon");
        }
    }
}
#endif