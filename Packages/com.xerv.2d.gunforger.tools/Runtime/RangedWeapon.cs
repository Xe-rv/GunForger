using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// RangedWeapon implements a configurable, reusable ranged weapon component.
/// - Handles aiming (cursor-based), firing modes (automatic, semi, burst),
///   ammunition, reloading, projectile spawning, spread (time/shot/hybrid),
///   recoil, sounds and animator triggers.
/// - Designed to be attached to a weapon GameObject and configured via the Inspector.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class RangedWeapon : MonoBehaviour
{
    // ---------- Inspector-configurable identity ----------
    [Header("Weapon Identity")]
    [SerializeField] private string playerTag = "Player"; // Tag used to find player GameObject
    [SerializeField] private string weaponName = "Gun"; // Display name for the weapon (UI)
    [SerializeField] private Sprite weaponSprite; // Optional sprite to assign at Awake
    [SerializeField] private bool TopDown = false; // If true, adjusts firePoint rotation for top-down style
    [SerializeField] private bool SideScroller = true; // If true, apply side-scroller flipping logic
    [SerializeField] private bool FullRotation = true; // If true, allow full rotation and flipping logic

    // ---------- Ammunition ----------
    [Header("Ammunition")]
    [SerializeField] private int ammoPerMagazine = 30; // Bullets per magazine
    [SerializeField] private int maxTotalAmmo = 300; // Max reserve ammo carried
    [SerializeField] private int currentAmmo; // Current reserve ammo (runtime)
    [SerializeField] private int currentMagazineAmmo; // Bullets in the current magazine (runtime)
    [SerializeField] private bool infiniteAmmo = false; // If true, magazines do not consume reserve ammo
    [SerializeField] private bool infiniteMagazines = false; // If true, reloading refills without consuming reserve

    // ---------- Fire rate and modes ----------
    [Header("Fire Rate")]
    [SerializeField] private float fireRate = 0.1f; // Minimum seconds between shots
    [SerializeField] private bool AutomaticOn = true; // Default automatic mode (hold to fire)
    [SerializeField] private bool BurstFireOn = false; // Enable burst-fire mode
    [SerializeField] private int burstCount = 3; // Number of shots per burst
    [SerializeField] private float burstDelay = 0.1f; // Delay between shots within a burst

    // ---------- Reload ----------
    [Header("Reload")]
    [SerializeField] private float reloadTime = 2f; // Seconds required to reload
    [SerializeField] private bool autoReload = true; // Automatically reload when magazine empty

    // ---------- Projectile / muzzle ----------
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab; // Prefab used when spawning projectiles
    [SerializeField] private Transform firePoint; // Transform representing muzzle origin
    [SerializeField] private int projectilesPerShot = 1; // # of projectiles per shot (shotgun, etc.)
    [SerializeField] private float projectileSpread = 5f; // Spread half-angle in degrees
    [SerializeField] private float projectileSpeed = 20f; // Movement speed for spawned projectiles
    [SerializeField] private float projectileDamage = 10f; // Damage value assigned to projectile
    [SerializeField] private Vector3 projectileScale = Vector3.one; // Scale applied to the instantiated projectile

    // ---------- Spread system ----------
    [Header("Spread System")]
    [SerializeField] private bool gradualSpread = false; // If true, spread scales with firing behaviour
    public enum SpreadMode { None, TimeBased, ShotBased, Hybrid } // Spread calculation modes
    [SerializeField] private SpreadMode spreadMode = SpreadMode.Hybrid;
    [SerializeField] private float perfectAccuracyTime = 0.5f; // Time window for "perfect" accuracy in time-based systems
    [SerializeField] private int perfectAccuracyShots = 3; // Number of initial shots with perfect accuracy in shot-based systems
    [SerializeField] private float maxSpreadRecoveryTime = 2f; // Time to fully recover accuracy after spraying

    // ---------- Effects ----------
    [Header("Effects")]
    [SerializeField] private AudioClip fireSound; // Sound to play on fire
    [SerializeField] private AudioClip reloadSound; // Sound to play on reload start/completion
    [SerializeField] private AudioClip emptySound; // Sound when attempting to fire with empty magazine
    [SerializeField] private Animator animator; // Optional animator to trigger fire/reload animations
    [SerializeField] private string fireAnimationTrigger = "Fire"; // Trigger parameter name for firing

    // ---------- Recoil ----------
    [Header("Recoil")]
    [SerializeField] private bool hasRecoil = true; // If true, apply positional recoil on fire
    [SerializeField] private float recoilAmount = 0.1f; // Magnitude of recoil offset in local space
    [SerializeField] private float recoilRecoverySpeed = 5f; // Speed at which recoil interpolates back to origin

    // ---------- Aiming ----------
    [Header("Aiming")]
    [SerializeField] private bool rotateTowardsCursor = true; // If true, rotate weapon to face cursor

    // ---------- Unity events (hookable in Inspector) ----------
    [Header("Events")]
    public UnityEvent OnFire; // Invoked when the weapon fires
    public UnityEvent OnReloadStart; // Invoked when reload begins
    public UnityEvent OnReloadComplete; // Invoked when reload completes
    public UnityEvent OnAmmoChanged; // Invoked when ammo values change (UI update)

    // ---------- Internal runtime state ----------
    private float nextFireTime; // Next allowed fire Time.time value
    private bool isReloading; // True while reload coroutine is running
    private int burstShotsFired; // Counter used by burst coroutine
    private bool isFiring; // True while currently executing a burst (blocks other input)
    private bool isSoundPlaying; // Flag reserved for potential sound gating (unused currently)
    private bool isAutomatic; // Active automatic mode (mirrors AutomaticOn on Awake)
    private bool isBurstFire; // Active burst mode (mirrors BurstFireOn on Awake)
    private Camera mainCamera; // Cached reference to Camera.main
    private SpriteRenderer spriteRenderer; // Cached SpriteRenderer for flipping
    private GameObject Player; // Cached player GameObject (found by playerTag)
    private AudioSource audioSource; // Cached/created AudioSource used to play sounds
    private Vector3 originalPosition; // Original localPosition used to return after recoil
    private Vector3 recoilOffset; // Current applied recoil offset (interpolated)
    private Vector3 firePointOriginalLocalPosition; // Stored original local position of firePoint for flipping

    // Spread runtime variables
    private float lastShotTime = 0f; // Timestamp of last shot (Time.time)
    private int consecutiveShots = 0; // Count of consecutive shots without recovery
    private float currentSpreadMultiplier = 0f; // Multiplier applied to projectileSpread when gradualSpread enabled

    /// <summary>
    /// Awake caches references, initializes ammo and modes, and ensures required components exist.
    /// </summary>
    void Awake()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // Initialize mode flags based on serialized defaults.
        isAutomatic = AutomaticOn;
        isBurstFire = BurstFireOn;

        // Ensure there is an AudioSource to play clips.
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Apply provided weapon sprite to SpriteRenderer if assigned.
        if (weaponSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = weaponSprite;

        // If no firePoint was assigned in the inspector, create a default child transform.
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            firePoint = fp.transform;
        }

        // Attempt to find player GameObject by tag once at startup (used for flipping logic).
        if (GameObject.FindGameObjectWithTag(playerTag) != null)
        {
            Player = GameObject.FindGameObjectWithTag(playerTag);
        }

        // Adjust firePoint rotation for top-down presets (developer note: can be changed in inspector).
        if (TopDown)
        {
            Debug.LogWarning("TopDown aiming mode selected");
            firePoint.transform.rotation = Quaternion.Euler(0, 0, 90);
        }

        // Initialize ammo counts and positional references.
        currentAmmo = maxTotalAmmo;
        currentMagazineAmmo = ammoPerMagazine;
        originalPosition = transform.localPosition;
        firePointOriginalLocalPosition = firePoint.localPosition;
    }

    /// <summary>
    /// Update handles aiming, recoil recovery, input processing and passive spread recovery.
    /// </summary>
    void Update()
    {
        if (rotateTowardsCursor)
            AimAtCursor();

        HandleRecoilRecovery();
        HandleInput();

        // When not firing, gradually restore the spread multiplier back to zero after the perfect-accuracy window.
        if (!isFiring && Time.time - lastShotTime > perfectAccuracyTime && currentSpreadMultiplier > 0f)
        {
            currentSpreadMultiplier = Mathf.Lerp(currentSpreadMultiplier, 0f, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// HandleInput reads player input for reloading and firing, honoring automatic vs semi-auto modes.
    /// </summary>
    void HandleInput()
    {
        if (isReloading)
            return;

        // Manual reload input (R)
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartReload();
            return;
        }

        // Automatic reload if configured and magazine empty but reserve ammo exists.
        if (autoReload && currentMagazineAmmo <= 0 && currentAmmo > 0)
        {
            StartReload();
            return;
        }

        // Firing input:
        // - Automatic: hold mouse button
        // - Semi-auto: press mouse button
        if (isAutomatic)
        {
            if (Input.GetMouseButton(0))
                TryFire();
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
                TryFire();
        }
    }

    /// <summary>
    /// AimAtCursor rotates the weapon to face the world cursor position and applies flipping logic.
    /// </summary>
    void AimAtCursor()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector2 direction = (mousePos - firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);

        // For side-scrollers adjust sprite/flips; for full rotation manage firepoint orientation.
        if (SideScroller)
            GunFlipper(direction);
        else if (FullRotation)
            FirepointFlipper();
    }

    /// <summary>
    /// FirepointFlipper corrects local rotation of the firePoint when parent scale is flipped.
    /// This ensures the muzzle still points correctly when the parent is mirrored.
    /// </summary>
    void FirepointFlipper()
    {
        if (transform.parent != null && transform.parent.localScale.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + 180);
            firePoint.localRotation = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            firePoint.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    /// <summary>
    /// GunFlipper applies sprite flipping and adjusts the firePoint local position for side-scroller characters.
    /// It uses the Player's localScale to determine facing direction.
    /// </summary>
    /// <param name="dir">Normalized direction from firePoint to cursor.</param>
    void GunFlipper(Vector2 dir)
    {
        if (Player == null || spriteRenderer == null)
            return;

        // If player is flipped (facing left)
        if (Player.transform.localScale.x < 0)
        {
            spriteRenderer.flipX = true;
            if (dir.x < 0)
            {
                // Cursor left: flip vertically and mirror firePoint both axes
                spriteRenderer.flipY = true;
                firePoint.localPosition = new Vector3(-firePointOriginalLocalPosition.x, -firePointOriginalLocalPosition.y, firePointOriginalLocalPosition.z);
            }
            else
            {
                // Cursor right: don't flip Y, mirror X only
                spriteRenderer.flipY = false;
                firePoint.localPosition = new Vector3(-firePointOriginalLocalPosition.x, firePointOriginalLocalPosition.y, firePointOriginalLocalPosition.z);
            }
        }
        else
        {
            // Player facing right: reset X flip and adjust Y based on cursor side
            spriteRenderer.flipX = false;
            if (dir.x < 0)
            {
                spriteRenderer.flipY = true;
                firePoint.localPosition = new Vector3(firePointOriginalLocalPosition.x, -firePointOriginalLocalPosition.y, firePointOriginalLocalPosition.z);
            }
            else
            {
                spriteRenderer.flipY = false;
                firePoint.localPosition = firePointOriginalLocalPosition;
            }
        }
    }

    /// <summary>
    /// TryFire enforces fireRate, checks ammo and decides to either start a burst or perform a single Fire.
    /// </summary>
    void TryFire()
    {
        // Enforce minimum time between shots
        if (Time.time < nextFireTime)
            return;

        // Handle empty magazine -> play empty sound and optionally disable automatic to avoid spam
        if (currentMagazineAmmo <= 0)
        {
            PlaySound(emptySound);

            // Prevents holding down the trigger from repeatedly playing empty sound.
            if (isAutomatic)
            {
                isAutomatic = false;
            }
            return;
        }

        // If burst mode is active, start coroutine to handle timed burst shots
        if (isBurstFire)
        {
            if (!isFiring)
            {
                StartCoroutine(FireBurst());
            }
        }
        else
        {
            Fire();
        }
    }

    /// <summary>
    /// Fire performs the actual shot: reduce ammo (unless infinite), schedule next fire time,
    /// invoke events, spawn projectiles, start animations and apply recoil.
    /// </summary>
    void Fire()
    {
        // Deduct one from magazine unless infinite ammo is enabled
        if (!infiniteAmmo)
        {
            currentMagazineAmmo--;
            OnAmmoChanged?.Invoke();
        }

        nextFireTime = Time.time + fireRate;
        OnFire?.Invoke();

        // Spawn the configured projectiles (handles multi-projectile shots like shotguns)
        for (int i = 0; i < projectilesPerShot; i++)
        {
            SpawnProjectile(i);
        }

        // Trigger animation and sound
        if (animator != null)
            animator.SetTrigger(fireAnimationTrigger);

        PlaySound(fireSound);

        // Apply recoil displacement for visual feedback/feel
        if (hasRecoil)
        {
            ApplyRecoil();
        }
    }

    /// <summary>
    /// SpawnProjectile instantiates the projectile prefab, sets its scale and assigns speed/damage + initial direction.
    /// Supports even distribution for multiple projectiles and randomized spread for single-projectile weapons.
    /// </summary>
    /// <param name="index">Index of the projectile in the current shot (0..projectilesPerShot-1).</param>
    void SpawnProjectile(int index)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("No projectile prefab assigned!");
            return;
        }

        // Determine spread angle for this projectile
        float spreadAngle = 0f;
        if (projectilesPerShot > 1)
        {
            // For multi-projectile shots, distribute evenly across the full spread width.
            float totalSpread = projectileSpread * 2;
            spreadAngle = -projectileSpread + (totalSpread / (projectilesPerShot - 1)) * index;
        }
        else if (projectileSpread > 0)
        {
            // For single projectile setups, choose random angle within spread range
            if (gradualSpread)
            {
                // Update spread multiplier that depends on mode (time/shot/hybrid)
                CalculateSpreadMultiplier();
                spreadAngle = Random.Range(-projectileSpread, projectileSpread) * currentSpreadMultiplier;
            }
            else
            {
                // Immediate full spread (no gradual scaling)
                spreadAngle = Random.Range(-projectileSpread, projectileSpread);
            }
        }

        // Calculate direction vector applying the spread rotation to the firePoint's right direction
        Quaternion spreadRotation = Quaternion.Euler(0, 0, spreadAngle);
        Vector2 direction = spreadRotation * firePoint.right;

        // Instantiate and configure projectile
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        proj.transform.localScale = projectileScale;

        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.speed = projectileSpeed;
            projectile.damage = projectileDamage;
            projectile.Initialize(direction, gameObject.tag);
        }
        else
        {
            Debug.LogWarning("Projectile prefab doesn't have Projectile component!");
        }
    }

    /// <summary>
    /// CalculateSpreadMultiplier updates <see cref="currentSpreadMultiplier"/> based on selected <see cref="SpreadMode"/>.
    /// - TimeBased: spread scales by time since last shot.
    /// - ShotBased: spread scales by number of consecutive shots.
    /// - Hybrid: combination of both heuristics.
    /// - None: no scaling (full spread).
    /// </summary>
    void CalculateSpreadMultiplier()
    {
        float timeSinceLastShot = Time.time - lastShotTime;

        switch (spreadMode)
        {
            case SpreadMode.TimeBased:
                // If sufficient time has passed, reset to perfect accuracy (0)
                if (timeSinceLastShot > perfectAccuracyTime)
                    currentSpreadMultiplier = 0f;
                else
                    currentSpreadMultiplier = Mathf.Clamp01(1f - (timeSinceLastShot / perfectAccuracyTime));
                break;

            case SpreadMode.ShotBased:
                // Reset shot counter if a long time has passed since last shot
                if (timeSinceLastShot > maxSpreadRecoveryTime)
                    consecutiveShots = 0;

                consecutiveShots++;

                // First N shots are perfectly accurate
                if (consecutiveShots <= perfectAccuracyShots)
                    currentSpreadMultiplier = 0f;
                else
                    currentSpreadMultiplier = Mathf.Clamp01((float)(consecutiveShots - perfectAccuracyShots) / perfectAccuracyShots);
                break;

            case SpreadMode.Hybrid:
                // Hybrid uses both time and shot count; reset if enough idle time elapsed
                if (timeSinceLastShot > maxSpreadRecoveryTime)
                {
                    consecutiveShots = 0;
                    currentSpreadMultiplier = 0f;
                }
                else
                {
                    consecutiveShots++;

                    // First few shots remain accurate
                    if (consecutiveShots <= perfectAccuracyShots)
                        currentSpreadMultiplier = 0f;
                    else
                    {
                        float shotFactor = Mathf.Clamp01((float)(consecutiveShots - perfectAccuracyShots) / perfectAccuracyShots);
                        float timeFactor = Mathf.Clamp01(1f - (timeSinceLastShot / perfectAccuracyTime));
                        // Use the more punitive factor (bigger spread)
                        currentSpreadMultiplier = Mathf.Max(shotFactor, timeFactor);
                    }
                }
                break;

            case SpreadMode.None:
            default:
                // No gradual spread - always full spread (multiplier of 1)
                currentSpreadMultiplier = 1f;
                break;
        }

        // Record time of this evaluation as last shot time
        lastShotTime = Time.time;
    }

    /// <summary>
    /// FireBurst coroutine handles timed bursts: fires up to <see cref="burstCount"/> shots with <see cref="burstDelay"/> between them.
    /// </summary>
    System.Collections.IEnumerator FireBurst()
    {
        isFiring = true;
        burstShotsFired = 0;

        // Fire until burstCount reached or out of magazine ammo
        while (burstShotsFired < burstCount && currentMagazineAmmo > 0)
        {
            Fire();
            burstShotsFired++;

            if (burstShotsFired < burstCount)
                yield return new WaitForSeconds(burstDelay);
        }

        isFiring = false;
        // Enforce fireRate after the burst completes
        nextFireTime = Time.time + fireRate;
    }

    /// <summary>
    /// StartReload initiates the reload coroutine if conditions allow.
    /// </summary>
    void StartReload()
    {
        // Guard: prevent reload if already reloading, no reserve ammo, or magazine already full.
        if (isReloading || currentAmmo <= 0 || currentMagazineAmmo >= ammoPerMagazine)
            return;

        StartCoroutine(Reload());
    }

    /// <summary>
    /// Reload coroutine plays reload effects and transfers ammo from reserve to magazine respecting infiniteMagazines setting.
    /// </summary>
    System.Collections.IEnumerator Reload()
    {
        isReloading = true;

        // Reset spread tracking while reloading (player cannot fire)
        consecutiveShots = 0;
        currentSpreadMultiplier = 0f;

        OnReloadStart?.Invoke();
        PlaySound(reloadSound);

        if (animator != null)
            animator.SetBool("IsReloading", isReloading);

        // Wait for reload animation/time
        yield return new WaitForSeconds(reloadTime);

        // Refill magazine: either consume from currentAmmo or fully refill if infinite
        if (!infiniteMagazines)
        {
            int ammoNeeded = ammoPerMagazine - currentMagazineAmmo;
            int ammoToReload = Mathf.Min(ammoNeeded, currentAmmo);

            currentMagazineAmmo += ammoToReload;
            currentAmmo -= ammoToReload;
        }
        else
        {
            // Infinite magazines: simply restore magazine count
            currentMagazineAmmo = ammoPerMagazine;
        }

        isReloading = false;
        if (animator != null)
            animator.SetBool("IsReloading", isReloading);

        OnReloadComplete?.Invoke();
        OnAmmoChanged?.Invoke();
    }

    /// <summary>
    /// ApplyRecoil computes a local-space recoilOffset based on weapon orientation and player facing.
    /// The offset is applied to transform.localPosition by HandleRecoilRecovery over time.
    /// </summary>
    void ApplyRecoil()
    {
        // SideScroller/FullRotation apply recoil along the firePoint.right axis and flip based on player facing.
        if (SideScroller || FullRotation)
        {
            if (Player != null && Player.transform.localScale.x < 0)
                recoilOffset = firePoint.right * recoilAmount;
            else
                recoilOffset = -firePoint.right * recoilAmount;
        }
        else
        {
            // In non-side-scroller modes, compute recoil as a local-space offset opposite to shooting direction.
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 shootDirection = (mouseWorldPos - (Vector2)transform.position).normalized;
            recoilOffset = transform.InverseTransformDirection(-shootDirection) * recoilAmount;
        }
    }

    /// <summary>
    /// HandleRecoilRecovery interpolates the recoilOffset back to zero and updates transform.localPosition.
    /// </summary>
    void HandleRecoilRecovery()
    {
        if (recoilOffset.magnitude > 0.001f)
        {
            // Smoothly interpolate offset back to zero using recovery speed
            recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
            transform.localPosition = originalPosition + recoilOffset;
        }
        else
        {
            transform.localPosition = originalPosition;
        }
    }

    /// <summary>
    /// PlaySound plays a one-shot AudioClip via the cached AudioSource (if available).
    /// </summary>
    /// <param name="clip">AudioClip to play (null-safe).</param>
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // ---------------- Public API ----------------

    /// <summary>
    /// AddAmmo increases the reserve ammo by the specified amount and clamps to maxTotalAmmo.
    /// Also re-enables automatic fire if AutomaticOn is set (useful after empty sound disabled it).
    /// </summary>
    /// <param name="amount">Amount of reserve ammo to add (positive integer).</param>
    public void AddAmmo(int amount)
    {
        currentAmmo += amount;
        currentAmmo = Mathf.Min(currentAmmo, maxTotalAmmo);

        // Re-enable automatic firing if the inspector-default AutomaticOn was true.
        if (AutomaticOn)
            isAutomatic = true;

        OnAmmoChanged?.Invoke();
    }

    /// <summary>Returns the current bullets in the magazine.</summary>
    public int GetCurrentMagazineAmmo() => currentMagazineAmmo;

    /// <summary>Returns the current reserve ammo total.</summary>
    public int GetCurrentTotalAmmo() => currentAmmo;

    /// <summary>Returns the configured magazine capacity.</summary>
    public int GetMaxMagazineAmmo() => ammoPerMagazine;

    /// <summary>Returns the configured maximum reserve ammo.</summary>
    public int GetMaxTotalAmmo() => maxTotalAmmo;

    /// <summary>Returns the reload duration in seconds.</summary>
    public float GetReloadTime() => reloadTime;

    /// <summary>Indicates whether a reload is in progress.</summary>
    public bool IsReloading() => isReloading;

    /// <summary>Returns the weapon display name.</summary>
    public string GetWeaponName() => weaponName;

    /// <summary>
    /// Returns the current calculated spread multiplier (0 = perfectly accurate, 1 = full spread).
    /// Useful for UI/visual feedback.
    /// </summary>
    public float GetCurrentSpreadMultiplier() => currentSpreadMultiplier;

    /// <summary>
    /// OnDrawGizmosSelected renders the firePoint and spread cone in the Editor for debugging and tuning.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (firePoint == null)
            return;

        // Draw the firePoint origin and forward ray
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(firePoint.position, 0.1f);
        Gizmos.DrawRay(firePoint.position, firePoint.right * 2f);

        // Draw the spread cone: either current gradual spread or full spread
        if (projectileSpread > 0 && gradualSpread)
        {
            float visualSpread = projectileSpread * currentSpreadMultiplier;
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);

            Vector3 spreadUp = Quaternion.Euler(0, 0, visualSpread) * firePoint.right * 2f;
            Vector3 spreadDown = Quaternion.Euler(0, 0, -visualSpread) * firePoint.right * 2f;

            Gizmos.DrawRay(firePoint.position, spreadUp);
            Gizmos.DrawRay(firePoint.position, spreadDown);
        }
        else if (projectileSpread > 0)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);

            Vector3 spreadUp = Quaternion.Euler(0, 0, projectileSpread) * firePoint.right * 2f;
            Vector3 spreadDown = Quaternion.Euler(0, 0, -projectileSpread) * firePoint.right * 2f;

            Gizmos.DrawRay(firePoint.position, spreadUp);
            Gizmos.DrawRay(firePoint.position, spreadDown);
        }
    }
}