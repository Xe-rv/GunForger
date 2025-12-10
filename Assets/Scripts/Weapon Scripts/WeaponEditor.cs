#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RangedWeapon))]
public class WeaponEditor : Editor
{
    private SerializedProperty weaponName;
    private SerializedProperty weaponSprite;
    private SerializedProperty ammoPerMagazine;
    private SerializedProperty maxTotalAmmo;
    private SerializedProperty infiniteAmmo;
    private SerializedProperty fireRate;
    private SerializedProperty isAutomatic;
    private SerializedProperty isBurstFire;
    private SerializedProperty burstCount;
    private SerializedProperty reloadTime;
    private SerializedProperty projectilePrefab;
    private SerializedProperty projectilesPerShot;
    private SerializedProperty projectileSpread;
    
    private bool showAmmoSettings = true;
    private bool showFireSettings = true;
    private bool showProjectileSettings = true;
    private bool showEffectSettings = true;
    
    void OnEnable()
    {
        weaponName = serializedObject.FindProperty("weaponName");
        weaponSprite = serializedObject.FindProperty("weaponSprite");
        ammoPerMagazine = serializedObject.FindProperty("ammoPerMagazine");
        maxTotalAmmo = serializedObject.FindProperty("maxTotalAmmo");
        infiniteAmmo = serializedObject.FindProperty("infiniteAmmo");
        fireRate = serializedObject.FindProperty("fireRate");
        isAutomatic = serializedObject.FindProperty("isAutomatic");
        isBurstFire = serializedObject.FindProperty("isBurstFire");
        burstCount = serializedObject.FindProperty("burstCount");
        reloadTime = serializedObject.FindProperty("reloadTime");
        projectilePrefab = serializedObject.FindProperty("projectilePrefab");
        projectilesPerShot = serializedObject.FindProperty("projectilesPerShot");
        projectileSpread = serializedObject.FindProperty("projectileSpread");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Modular Weapon System", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Quick Stats Box
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        RangedWeapon weapon = (RangedWeapon)target;
        EditorGUILayout.LabelField("Quick Stats", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"DPS: {CalculateDPS(weapon):F1}");
        EditorGUILayout.LabelField($"Shots Per Second: {1f / fireRate.floatValue:F2}");
        EditorGUILayout.LabelField($"Magazine Duration: {ammoPerMagazine.intValue * fireRate.floatValue:F1}s");
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        // Default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        
        // Utility Buttons
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Weapon Utilities", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create Fire Point"))
        {
            CreateFirePoint(weapon);
        }
        
        if (GUILayout.Button("Setup Basic Projectile"))
        {
            CreateBasicProjectile();
        }
        
        EditorGUILayout.EndVertical();
        
        serializedObject.ApplyModifiedProperties();
    }
    
    float CalculateDPS(RangedWeapon weapon)
    {
        float shotsPerSecond = 1f / fireRate.floatValue;
        SerializedProperty damage = serializedObject.FindProperty("projectileDamage");
        SerializedProperty projPerShot = serializedObject.FindProperty("projectilesPerShot");
        return shotsPerSecond * damage.floatValue * projPerShot.intValue;
    }
    
    void CreateFirePoint(RangedWeapon weapon)
    {
        Transform existingFirePoint = weapon.transform.Find("FirePoint");
        if (existingFirePoint != null)
        {
            Debug.LogWarning("Fire Point already exists!");
            return;
        }
        
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(weapon.transform);
        firePoint.transform.localPosition = new Vector3(0.5f, 0, 0);
        
        // Visualize in editor
        Gizmos.color = Color.red;
        
        Debug.Log("Fire Point created!");
        EditorUtility.SetDirty(weapon);
    }
    
    void CreateBasicProjectile()
    {
        GameObject projectile = new GameObject("Projectile");
        
        // Add sprite renderer
        SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = Color.yellow;
        
        // Add components
        projectile.AddComponent<Rigidbody2D>();
        CircleCollider2D col = projectile.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        projectile.AddComponent<Projectile>();
        
        projectile.transform.localScale = Vector3.one * 0.2f;
        
        // Save as prefab
        string path = "Assets/Projectile.prefab";
        PrefabUtility.SaveAsPrefabAsset(projectile, path);
        DestroyImmediate(projectile);
        
        Debug.Log($"Basic projectile prefab created at {path}");
    }
}

[CustomEditor(typeof(Projectile))]
public class ProjectileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        Projectile projectile = (Projectile)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Projectile Info", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField($"Current Speed: {projectile.speed}");
            EditorGUILayout.LabelField($"Damage: {projectile.damage}");
        }
        
        EditorGUILayout.EndVertical();
    }
}

// Menu items for quick creation
public class WeaponSystemMenu
{
    [MenuItem("GameObject/2D Weapon System/Create Weapon", false, 10)]
    static void CreateWeapon()
    {
        GameObject weapon = new GameObject("RangedWeapon");
        weapon.AddComponent<SpriteRenderer>();
        weapon.AddComponent<RangedWeapon>();
        
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(weapon.transform);
        firePoint.transform.localPosition = new Vector3(0.5f, 0, 0);
        
        Selection.activeGameObject = weapon;
        Debug.Log("Ranged Weapon created! Don't forget to assign a projectile prefab.");
    }
    
    [MenuItem("GameObject/2D Weapon System/Create Player with Weapon", false, 11)]
    static void CreatePlayerWithWeapon()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Default");
        
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        sr.color = Color.green;
        
        player.AddComponent<Rigidbody2D>().gravityScale = 0;
        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        player.AddComponent<Health>();
        
        // Create weapon as child
        GameObject weapon = new GameObject("Weapon");
        weapon.transform.SetParent(player.transform);
        weapon.transform.localPosition = new Vector3(0.3f, 0, 0);
        
        SpriteRenderer weaponSr = weapon.AddComponent<SpriteRenderer>();
        weaponSr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        weaponSr.sortingOrder = 1;
        
        weapon.AddComponent<RangedWeapon>();
        
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(weapon.transform);
        firePoint.transform.localPosition = new Vector3(0.5f, 0, 0);
        
        Selection.activeGameObject = player;
        Debug.Log("Player with weapon created! Assign a projectile prefab to the weapon.");
    }
    
    [MenuItem("GameObject/2D Weapon System/Create Enemy Target", false, 12)]
    static void CreateEnemy()
    {
        GameObject enemy = new GameObject("Enemy");
        enemy.tag = "Enemy";
        
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        sr.color = Color.red;
        
        enemy.AddComponent<BoxCollider2D>();
        enemy.AddComponent<Health>();
        
        Selection.activeGameObject = enemy;
    }
    
    [MenuItem("Assets/Create/2D Weapon System/Weapon Preset", false, 10)]
    static void CreateWeaponPreset()
    {
        // Create a scriptable object for weapon presets in the future
        Debug.Log("Weapon preset creation - To be implemented with ScriptableObject");
    }
}
#endif
