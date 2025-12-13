#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for RangedWeapon. Provides quick stats, game-style presets and in-inspector projectile editing.
/// Also includes utility buttons to create FirePoint and a basic projectile prefab.
/// </summary>
[CustomEditor(typeof(RangedWeapon))]
public class WeaponEditor : Editor
{
    // Serialized properties mirrored from RangedWeapon for custom inspector UI
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
    private SerializedProperty sideScroller;
    private SerializedProperty topDown;
    private SerializedProperty fullRotation;

    private string[] gameStyleOptions = new string[] { "Side Scroller", "Top Down", "Full Rotation" };

    private bool showProjectileSettings = true;
    private Editor projectileEditor;

    void OnEnable()
    {
        // Find serialized properties by name - names must match RangedWeapon private field names
        weaponName = serializedObject.FindProperty("weaponName");
        weaponSprite = serializedObject.FindProperty("weaponSprite");
        ammoPerMagazine = serializedObject.FindProperty("ammoPerMagazine");
        maxTotalAmmo = serializedObject.FindProperty("maxTotalAmmo");
        infiniteAmmo = serializedObject.FindProperty("infiniteAmmo");
        fireRate = serializedObject.FindProperty("fireRate");
        isAutomatic = serializedObject.FindProperty("AutomaticOn");
        isBurstFire = serializedObject.FindProperty("BurstFireOn");
        burstCount = serializedObject.FindProperty("burstCount");
        reloadTime = serializedObject.FindProperty("reloadTime");
        projectilePrefab = serializedObject.FindProperty("projectilePrefab");
        projectilesPerShot = serializedObject.FindProperty("projectilesPerShot");
        projectileSpread = serializedObject.FindProperty("projectileSpread");
        sideScroller = serializedObject.FindProperty("SideScroller");
        topDown = serializedObject.FindProperty("TopDown");
        fullRotation = serializedObject.FindProperty("FullRotation");
    }

    void OnDisable()
    {
        if (projectileEditor != null)
        {
            DestroyImmediate(projectileEditor);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Modular Weapon System", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        // Quick Stats Box for balancing feedback
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        RangedWeapon weapon = (RangedWeapon)target;
        EditorGUILayout.LabelField("Quick Stats", EditorStyles.boldLabel);

        if (fireRate != null && fireRate.floatValue > 0)
        {
            float shotsPerSecond = 1f / fireRate.floatValue;
            EditorGUILayout.LabelField($"Shots Per Second: {shotsPerSecond:F2}");
    
            if (ammoPerMagazine != null && ammoPerMagazine.intValue > 0)
            {
                float magDuration = ammoPerMagazine.intValue * fireRate.floatValue;
                EditorGUILayout.LabelField($"Magazine Duration: {magDuration:F1}s");
        
                // Get damage from projectile if available
                if (projectilePrefab != null && projectilePrefab.objectReferenceValue != null)
                {
                    GameObject prefab = projectilePrefab.objectReferenceValue as GameObject;
                    Projectile proj = prefab?.GetComponent<Projectile>();
            
                    if (proj != null)
                    {
                        float damage = proj.damage;
                        float dps = damage * shotsPerSecond;
                
                        EditorGUILayout.LabelField($"Damage Per Shot: {damage:F1}");
                        EditorGUILayout.LabelField($"DPS: {dps:F1}");
                        EditorGUILayout.LabelField($"Magazine Total Damage: {damage * ammoPerMagazine.intValue:F0}");
                
                        // Account for multiple projectiles per shot
                        if (projectilesPerShot != null && projectilesPerShot.intValue > 1)
                        {
                            EditorGUILayout.LabelField($"Projectiles Per Shot: {projectilesPerShot.intValue}");
                            EditorGUILayout.LabelField($"Total DPS (all projectiles): {dps * projectilesPerShot.intValue:F1}");
                        }
                    }
                }
            }
    
            // Reload efficiency
            if (reloadTime != null && reloadTime.floatValue > 0 && ammoPerMagazine != null)
            {
                float totalCycleTime = (ammoPerMagazine.intValue * fireRate.floatValue) + reloadTime.floatValue;
                float uptime = (ammoPerMagazine.intValue * fireRate.floatValue) / totalCycleTime * 100f;
                EditorGUILayout.LabelField($"Reload Time: {reloadTime.floatValue:F1}s");
                EditorGUILayout.LabelField($"Uptime %: {uptime:F1}%");
            }
    
            // Burst analysis
            if (isBurstFire != null && isBurstFire.boolValue && burstCount != null && burstCount.intValue > 1)
            {
                EditorGUILayout.LabelField($"Burst Count: {burstCount.intValue}");
                float burstDuration = (burstCount.intValue - 1) * fireRate.floatValue;
                EditorGUILayout.LabelField($"Burst Duration: {burstDuration:F2}s");
            }
    
            // Ammo info
            if (infiniteAmmo != null)
            {
                if (infiniteAmmo.boolValue)
                {
                    EditorGUILayout.LabelField("Ammo: Infinite");
                }
                else if (maxTotalAmmo != null)
                {
                    int magazines = maxTotalAmmo.intValue / ammoPerMagazine.intValue;
                    EditorGUILayout.LabelField($"Total Ammo: {maxTotalAmmo.intValue} ({magazines} magazines)");
                }
            }
    
            // Spread info for shotgun-type weapons
            if (projectilesPerShot != null && projectilesPerShot.intValue > 1 && projectileSpread != null)
            {
                EditorGUILayout.LabelField($"Spread Angle: {projectileSpread.floatValue:F1}°");
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Game Style (Side Scroller / Top Down / Full Rotation) selector with helpful tooltip
        if (sideScroller != null && topDown != null && fullRotation != null)
        {
            EditorGUILayout.LabelField("Game Style", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            int currentSelection = 0;
            if (topDown.boolValue && !sideScroller.boolValue && !fullRotation.boolValue)
                currentSelection = 1;
            else if (fullRotation.boolValue && !sideScroller.boolValue && !topDown.boolValue)
                currentSelection = 2;
            else
                currentSelection = 0;

            int newSelection = EditorGUILayout.Popup("Game Type", currentSelection, gameStyleOptions);

            if (newSelection != currentSelection)
            {
                if (newSelection == 0) // Side Scroller
                {
                    sideScroller.boolValue = true;
                    topDown.boolValue = false;
                    fullRotation.boolValue = false;
                }
                else if (newSelection == 1) // Top Down
                {
                    sideScroller.boolValue = false;
                    topDown.boolValue = true;
                    fullRotation.boolValue = false;
                }
                else // Full Rotation
                {
                    sideScroller.boolValue = false;
                    topDown.boolValue = false;
                    fullRotation.boolValue = true;
                }
            }

            string helpText = "";
            switch (newSelection)
            {
                case 0:
                    helpText = "Side Scroller: Weapon sprite flips based on aim direction";
                    break;
                case 1:
                    helpText = "Top Down: Weapon rotates with player on a top-down view";
                    break;
                case 2:
                    helpText = "Side Scroller (With Full Rotation): Weapon rotates 360° freely instead of flipping based on Direction";
                    break;
            }

            EditorGUILayout.HelpBox(helpText, MessageType.Info);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // Draw all serialized properties while skipping the internal style toggles handled above
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (prop.name == "m_Script" || prop.name == "TopDown" || prop.name == "SideScroller" || prop.name == "FullRotation")
                continue;

            EditorGUILayout.PropertyField(prop, true);
        }

        EditorGUILayout.Space();

        // Show or edit projectile prefab properties inline
        DrawProjectileSettings();

        EditorGUILayout.Space();

        // Utility buttons for common authoring tasks
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

    /// <summary>
    /// DrawProjectileSettings shows editable fields from the assigned projectile prefab so designers can tweak projectile values without opening the prefab.
    /// </summary>
    void DrawProjectileSettings()
    {
        if (projectilePrefab == null)
            return;

        GameObject prefab = projectilePrefab.objectReferenceValue as GameObject;

        if (prefab == null)
        {
            EditorGUILayout.HelpBox("No projectile prefab assigned. Create one or assign an existing prefab.", MessageType.Warning);
            return;
        }

        Projectile projectile = prefab.GetComponent<Projectile>();
        if (projectile == null)
        {
            EditorGUILayout.HelpBox("Assigned prefab doesn't have a Projectile component!", MessageType.Error);
            return;
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // Foldout for projectile inspector
        showProjectileSettings = EditorGUILayout.Foldout(showProjectileSettings, "Projectile Settings", true, EditorStyles.foldoutHeader);

        if (showProjectileSettings)
        {
            EditorGUI.indentLevel++;

            // Create a SerializedObject for the prefab instance to edit its fields
            SerializedObject projectileSO = new SerializedObject(projectile);

            // Projectile Properties
            EditorGUILayout.LabelField("Projectile Properties", EditorStyles.boldLabel);
            SerializedProperty damageProp = projectileSO.FindProperty("damage");
            SerializedProperty speedProp = projectileSO.FindProperty("speed");
            SerializedProperty lifetimeProp = projectileSO.FindProperty("lifetime");
            SerializedProperty hitLayersProp = projectileSO.FindProperty("hitLayers");

            if (damageProp != null) EditorGUILayout.PropertyField(damageProp);
            if (speedProp != null) EditorGUILayout.PropertyField(speedProp);
            if (lifetimeProp != null) EditorGUILayout.PropertyField(lifetimeProp);
            if (hitLayersProp != null) EditorGUILayout.PropertyField(hitLayersProp);

            EditorGUILayout.Space();

            // Area of Effect
            EditorGUILayout.LabelField("Area of Effect", EditorStyles.boldLabel);
            SerializedProperty hasAOEProp = projectileSO.FindProperty("hasAOE");

            if (hasAOEProp != null)
            {
                EditorGUILayout.PropertyField(hasAOEProp);

                if (hasAOEProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    SerializedProperty aoeRadiusProp = projectileSO.FindProperty("aoeRadius");
                    SerializedProperty aoeDamageProp = projectileSO.FindProperty("aoeDamage");
                    SerializedProperty aoeDamagesFriendliesProp = projectileSO.FindProperty("aoeDamagesFriendlies");
                    SerializedProperty aoeEffectPrefabProp = projectileSO.FindProperty("aoeEffectPrefab");
                    SerializedProperty aoeEffectDurationProp = projectileSO.FindProperty("aoeEffectDuration");

                    if (aoeRadiusProp != null) EditorGUILayout.PropertyField(aoeRadiusProp);
                    if (aoeDamageProp != null) EditorGUILayout.PropertyField(aoeDamageProp);
                    if (aoeDamagesFriendliesProp != null) EditorGUILayout.PropertyField(aoeDamagesFriendliesProp);
                    if (aoeEffectPrefabProp != null) EditorGUILayout.PropertyField(aoeEffectPrefabProp);
                    if (aoeEffectDurationProp != null) EditorGUILayout.PropertyField(aoeEffectDurationProp);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Space();

            // Visual Effects
            EditorGUILayout.LabelField("Visual Effects", EditorStyles.boldLabel);
            SerializedProperty impactEffectProp = projectileSO.FindProperty("impactEffectPrefab");
            SerializedProperty impactDurationProp = projectileSO.FindProperty("impactEffectDuration");
            SerializedProperty trailProp = projectileSO.FindProperty("trail");

            if (impactEffectProp != null) EditorGUILayout.PropertyField(impactEffectProp);
            if (impactDurationProp != null) EditorGUILayout.PropertyField(impactDurationProp);
            if (trailProp != null) EditorGUILayout.PropertyField(trailProp);

            EditorGUILayout.Space();

            EditorGUI.indentLevel--;

            // Apply any changes to the projectile prefab (marks dirty so they persist)
            if (projectileSO.hasModifiedProperties)
            {
                projectileSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(projectile);
                PrefabUtility.RecordPrefabInstancePropertyModifications(projectile);
            }

            EditorGUILayout.Space();

            // Quick actions: select or open prefab asset
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select Projectile Prefab"))
            {
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
            if (GUILayout.Button("Open Projectile Prefab"))
            {
                AssetDatabase.OpenAsset(prefab);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    // Utility - create a default FirePoint child on the selected RangedWeapon
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

        Debug.Log("Fire Point created!");
        EditorUtility.SetDirty(weapon);
    }

    // Utility - creates a basic projectile prefab with recommended components and saves it to Assets/Projectile.prefab
    void CreateBasicProjectile()
    {
        GameObject projectile = new GameObject("Projectile");

        // Visual placeholder
        SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = Color.yellow;

        // Physics components
        Rigidbody2D rb = projectile.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D col = projectile.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        projectile.AddComponent<Projectile>();

        projectile.transform.localScale = Vector3.one * 0.2f;

        // Save as prefab at root Assets for quick use
        string path = "Assets/Projectile.prefab";
        PrefabUtility.SaveAsPrefabAsset(projectile, path);
        DestroyImmediate(projectile);

        Debug.Log($"Basic projectile prefab created at {path}");
    }
}

// Lightweight editor for Projectile to show runtime info (speed/damage) in inspector
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

// Menu items for quick creation of common objects (weapon, player, enemy) used during prototyping
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
}
#endif