using UnityEngine;

/// <summary>
/// Example weapon configurations demonstrating the system's versatility
/// </summary>
public class WeaponPresetExamples
{
    // PISTOL - Fast, accurate, low damage
    public static void ConfigurePistol(RangedWeapon weapon)
    {
        // Ammo: 12 rounds, 120 total
        // Fire rate: 0.15s (6.67 shots/sec)
        // Damage: 15 per shot
        // Spread: 2 degrees
        // Reload: 1.5s
    }
    
    // ASSAULT RIFLE - Automatic, medium damage, moderate spread
    public static void ConfigureAssaultRifle(RangedWeapon weapon)
    {
        // Ammo: 30 rounds, 300 total
        // Fire rate: 0.08s (12.5 shots/sec)
        // Damage: 12 per shot
        // Spread: 5 degrees
        // Reload: 2.5s
        // Automatic: true
    }
    
    // SHOTGUN - High spread, multiple projectiles, devastating close range
    public static void ConfigureShotgun(RangedWeapon weapon)
    {
        // Ammo: 8 rounds, 64 total
        // Fire rate: 0.8s (1.25 shots/sec)
        // Projectiles per shot: 8
        // Damage: 8 per pellet (64 total if all hit)
        // Spread: 25 degrees
        // Reload: 3s
        // Automatic: false
    }
    
    // SNIPER RIFLE - High damage, slow fire rate, precise
    public static void ConfigureSniper(RangedWeapon weapon)
    {
        // Ammo: 5 rounds, 50 total
        // Fire rate: 1.5s (0.67 shots/sec)
        // Damage: 100 per shot
        // Spread: 0 degrees
        // Reload: 3s
        // Automatic: false
        // Projectile speed: 50
    }
    
    // SUBMACHINE GUN - Very fast fire, low damage, high spread
    public static void ConfigureSMG(RangedWeapon weapon)
    {
        // Ammo: 40 rounds, 400 total
        // Fire rate: 0.05s (20 shots/sec)
        // Damage: 8 per shot
        // Spread: 10 degrees
        // Reload: 2s
        // Automatic: true
    }
    
    // BURST RIFLE - 3-round burst, balanced
    public static void ConfigureBurstRifle(RangedWeapon weapon)
    {
        // Ammo: 30 rounds, 300 total
        // Fire rate: 0.5s between bursts
        // Burst count: 3
        // Burst delay: 0.08s
        // Damage: 15 per shot
        // Spread: 3 degrees
        // Reload: 2.2s
        // Burst fire: true
    }
    
    // ROCKET LAUNCHER - AOE damage, slow reload
    public static void ConfigureRocketLauncher(RangedWeapon weapon)
    {
        // Ammo: 1 round, 10 total
        // Fire rate: 2s
        // Damage: 50 direct
        // AOE: true, 5 radius, 30 damage
        // Spread: 0 degrees
        // Reload: 4s
        // Projectile speed: 15
        // Automatic: false
    }
    
    // MINIGUN - Extremely fast fire, medium damage
    public static void ConfigureMinigun(RangedWeapon weapon)
    {
        // Ammo: 200 rounds, 1000 total
        // Fire rate: 0.03s (33.3 shots/sec)
        // Damage: 10 per shot
        // Spread: 15 degrees (inaccurate)
        // Reload: 5s
        // Automatic: true
        // Has significant recoil
    }
    
    // PLASMA RIFLE - Energy weapon, no spread, penetrating
    public static void ConfigurePlasmaRifle(RangedWeapon weapon)
    {
        // Ammo: infinite (energy based)
        // Fire rate: 0.2s
        // Damage: 20 per shot
        // Spread: 0 degrees
        // Projectile: Can penetrate (2 enemies)
        // Projectile speed: 30
        // No reload needed
    }
    
    // FLAMETHROWER - Continuous short-range stream
    public static void ConfigureFlamethrower(RangedWeapon weapon)
    {
        // Ammo: 100 rounds, 500 total
        // Fire rate: 0.02s (50 shots/sec)
        // Damage: 3 per shot
        // Spread: 20 degrees
        // Projectile speed: 8 (slow)
        // Projectile lifetime: 0.5s
        // Reload: 3s
        // Automatic: true
        // AOE on impact: small radius
    }
}

/// <summary>
/// Static class containing example weapon statistics for reference
/// </summary>
public static class WeaponStats
{
    public struct WeaponData
    {
        public string name;
        public int magazineSize;
        public int totalAmmo;
        public float fireRate;
        public int projectilesPerShot;
        public float damage;
        public float spread;
        public float reloadTime;
        public bool isAutomatic;
        public bool isBurst;
        
        public float DPS => (1f / fireRate) * damage * projectilesPerShot;
        public float MagazineDuration => magazineSize * fireRate;
    }
    
    public static readonly WeaponData Pistol = new WeaponData
    {
        name = "Pistol",
        magazineSize = 12,
        totalAmmo = 120,
        fireRate = 0.15f,
        projectilesPerShot = 1,
        damage = 15f,
        spread = 2f,
        reloadTime = 1.5f,
        isAutomatic = false,
        isBurst = false
    };
    
    public static readonly WeaponData AssaultRifle = new WeaponData
    {
        name = "Assault Rifle",
        magazineSize = 30,
        totalAmmo = 300,
        fireRate = 0.08f,
        projectilesPerShot = 1,
        damage = 12f,
        spread = 5f,
        reloadTime = 2.5f,
        isAutomatic = true,
        isBurst = false
    };
    
    public static readonly WeaponData Shotgun = new WeaponData
    {
        name = "Shotgun",
        magazineSize = 8,
        totalAmmo = 64,
        fireRate = 0.8f,
        projectilesPerShot = 8,
        damage = 8f,
        spread = 25f,
        reloadTime = 3f,
        isAutomatic = false,
        isBurst = false
    };
}

/*
SETUP INSTRUCTIONS:
===================

1. BASIC SETUP:
   - Right-click in Hierarchy → "2D Weapon System" → "Create Player with Weapon"
   - This creates a player GameObject with weapon attached
   - Select the Weapon child object

2. ASSIGN PROJECTILE:
   - Right-click in Hierarchy → "2D Weapon System" → Create a basic projectile prefab first
   - Or click "Setup Basic Projectile" button in the weapon inspector
   - Drag the projectile prefab into the "Projectile Prefab" field

3. CONFIGURE WEAPON:
   - Adjust all parameters in the inspector
   - Use the Quick Stats box to see DPS and other calculations
   - Use Gizmos in Scene view to visualize fire point and spread

4. SETUP TARGETS:
   - Right-click in Hierarchy → "2D Weapon System" → "Create Enemy Target"
   - This creates enemies with Health component
   - Make sure layers are set up correctly in Project Settings

5. CREATE CUSTOM WEAPONS:
   - Right-click in Project → Create → "2D Weapon System" → "Weapon Preset"
   - Configure the preset with desired stats
   - Reference the preset when creating new weapons

6. LAYER SETUP:
   - Ensure "Player" and "Enemy" layers exist
   - Set projectile hit layers to hit enemies but not player
   - Configure collision matrix in Edit → Project Settings → Physics 2D

7. UI SETUP (Optional):
   - Add WeaponUI component to a Canvas
   - Reference the weapon and UI elements
   - The UI will automatically update ammo and reload status

EXAMPLE CONFIGURATIONS:
=======================

PISTOL (Balanced):
- Magazine: 12 | Total: 120
- Fire Rate: 0.15s | Damage: 15
- Spread: 2° | Reload: 1.5s
- DPS: 100

ASSAULT RIFLE (Automatic):
- Magazine: 30 | Total: 300
- Fire Rate: 0.08s | Damage: 12
- Spread: 5° | Reload: 2.5s
- Automatic: Yes
- DPS: 150

SHOTGUN (Close Range):
- Magazine: 8 | Total: 64
- Fire Rate: 0.8s | Damage: 8 per pellet
- Projectiles: 8 | Spread: 25°
- Reload: 3s
- Total Damage: 64 per shot
- DPS: 80

SNIPER (High Damage):
- Magazine: 5 | Total: 50
- Fire Rate: 1.5s | Damage: 100
- Spread: 0° | Reload: 3s
- Projectile Speed: 50
- DPS: 66.7

BURST RIFLE:
- Magazine: 30 | Total: 300
- Burst: 3 shots | Burst Delay: 0.08s
- Fire Rate: 0.5s | Damage: 15
- Spread: 3° | Reload: 2.2s
- Burst Fire: Yes

TIPS:
=====
- Use Gizmos to visualize weapon range and spread
- Test different projectile speeds for different weapon feels
- Combine AOE with slow projectiles for grenade launchers
- Use burst fire with low spread for precision weapons
- High spread + multiple projectiles = shotgun effect
- Penetration works great for energy weapons
- Adjust recoil for weapon feedback
- Use animations to enhance weapon feel
*/