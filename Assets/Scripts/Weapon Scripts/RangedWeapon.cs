using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RangedWeapon : MonoBehaviour
{
    [Header("Weapon Identity")]
    [SerializeField] private string weaponName = "Gun";
    [SerializeField] private Sprite weaponSprite;
    [SerializeField] private bool SideScroller = true;

    [Header("Ammunition")]
    [SerializeField] private int ammoPerMagazine = 30;
    [SerializeField] private int maxTotalAmmo = 300;
    [SerializeField] private int currentAmmo;
    [SerializeField] private int currentMagazineAmmo;
    [SerializeField] private bool infiniteAmmo = false;

    [Header("Fire Rate")]
    [SerializeField] private float fireRate = 0.1f; // Time between shots
    [SerializeField] private bool AutomaticOn = true;
    [SerializeField] private bool BurstFireOn= false;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstDelay = 0.1f;

    [Header("Reload")]
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private bool autoReload = true;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int projectilesPerShot = 1;
    [SerializeField] private float projectileSpread = 5f; // Degrees
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private Vector3 projectileScale = Vector3.one;

    [Header("Effects")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptySound;
    [SerializeField] private Animator animator;
    [SerializeField] private string fireAnimationTrigger = "Fire";

    [Header("Recoil")]
    [SerializeField] private bool hasRecoil = true;
    [SerializeField] private float recoilAmount = 0.1f;
    [SerializeField] private float recoilRecoverySpeed = 5f;

    [Header("Aiming")]
    [SerializeField] private bool rotateTowardsCursor = true;

    [Header("Events")]
    public UnityEvent OnFire;
    public UnityEvent OnReloadStart;
    public UnityEvent OnReloadComplete;
    public UnityEvent OnAmmoChanged;

    private float nextFireTime;
    private bool isReloading;
    private int burstShotsFired;
    private bool isFiring;
    private bool isSoundPlaying;
    private bool isAutomatic;
    private bool isBurstFire;
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Vector3 originalPosition;
    private Vector3 recoilOffset;
    private Vector3 firePointOriginalLocalPosition; 

    void Awake()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if(AutomaticOn)
            isAutomatic = true;
        else
            isAutomatic = false;

        if(BurstFireOn)
            isBurstFire = true;
        else
            isBurstFire = false;

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (weaponSprite != null)
            spriteRenderer.sprite = weaponSprite;

        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = Vector3.right * 0.5f;
            firePoint = fp.transform;
        }

        currentAmmo = maxTotalAmmo;
        currentMagazineAmmo = ammoPerMagazine;
        originalPosition = transform.localPosition;
        firePointOriginalLocalPosition = firePoint.localPosition;
    }

    void Update()
    {
        if (rotateTowardsCursor)
            AimAtCursor();

        HandleRecoilRecovery();
        HandleInput();
    }

    void HandleInput()
    {
        if (isReloading)
            return;

        // Manual reload
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartReload();
            return;
        }

        // Auto reload when empty
        if (autoReload && currentMagazineAmmo <= 0 && currentAmmo > 0)
        {
            StartReload();
            return;
        }

        // Firing
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

    void AimAtCursor()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector2 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Flip sprite if needed (ie, for side-scrolling shooters)
        if (SideScroller)
            GunFlipper(direction);
    }

    void GunFlipper(Vector2 dir)
    {
        Vector2 direction = dir;
        if (transform.parent.localScale.x < 0)
        {
            spriteRenderer.flipX = true;
            if (direction.x < 0)
            {
                spriteRenderer.flipY = true;
                firePoint.localPosition = new Vector3(-firePointOriginalLocalPosition.x, -firePointOriginalLocalPosition.y, firePointOriginalLocalPosition.z);
            }
            else
            {
                spriteRenderer.flipY = false;
                firePoint.localPosition = new Vector3(-firePointOriginalLocalPosition.x, firePointOriginalLocalPosition.y, firePointOriginalLocalPosition.z);
            }
        }
        else
        {
            spriteRenderer.flipX = false;
            if (direction.x < 0)
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

    void TryFire()
    {
        if (Time.time < nextFireTime)
            return;

        if (currentMagazineAmmo <= 0)
        {
            PlaySound(emptySound);

            // Prevents holding down the trigger sound spam
            if (isAutomatic)
            {
                isAutomatic = false;
            }
            return;
        }

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

    void Fire()
    {
        currentMagazineAmmo--;
        nextFireTime = Time.time + fireRate;

        OnFire?.Invoke();
        OnAmmoChanged?.Invoke();

        // Spawn projectiles
        for (int i = 0; i < projectilesPerShot; i++)
        {
            SpawnProjectile(i);
        }

        // Effects
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            flash.transform.SetParent(firePoint);
        }

        animator.SetTrigger(fireAnimationTrigger);

        PlaySound(fireSound);

        if (hasRecoil)
        {
            ApplyRecoil();
        }
    }

    void SpawnProjectile(int index)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("No projectile prefab assigned!");
            return;
        }

        // Calculate spread
        float spreadAngle = 0f;
        if (projectilesPerShot > 1)
        {
            float totalSpread = projectileSpread * 2;
            spreadAngle = -projectileSpread + (totalSpread / (projectilesPerShot - 1)) * index;
        }
        else if (projectileSpread > 0)
        {
            spreadAngle = Random.Range(-projectileSpread, projectileSpread);
        }

        Quaternion spreadRotation = Quaternion.Euler(0, 0, spreadAngle);
        Vector2 direction = spreadRotation * firePoint.right;

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

    System.Collections.IEnumerator FireBurst()
    {
        isFiring = true;
        burstShotsFired = 0;

        while (burstShotsFired < burstCount && currentMagazineAmmo > 0)
        {
            Fire();
            burstShotsFired++;

            if (burstShotsFired < burstCount)
                yield return new WaitForSeconds(burstDelay);
        }

        isFiring = false;
        nextFireTime = Time.time + fireRate;
    }

    void StartReload()
    {
        if (isReloading || currentAmmo <= 0 || currentMagazineAmmo >= ammoPerMagazine)
            return;

        StartCoroutine(Reload());
    }

    System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        OnReloadStart?.Invoke();

        PlaySound(reloadSound);

        animator.SetBool("IsReloading", isReloading);

        yield return new WaitForSeconds(reloadTime);

        if (!infiniteAmmo)
        {
            int ammoNeeded = ammoPerMagazine - currentMagazineAmmo;
            int ammoToReload = Mathf.Min(ammoNeeded, currentAmmo);

            currentMagazineAmmo += ammoToReload;
            currentAmmo -= ammoToReload;
        }
        else
        {
            currentMagazineAmmo = ammoPerMagazine;
        }

        isReloading = false;
        animator.SetBool("IsReloading", isReloading);
        OnReloadComplete?.Invoke();
        OnAmmoChanged?.Invoke();
    }

    void ApplyRecoil()
    {
        recoilOffset = -firePoint.right * recoilAmount;
    }

    void HandleRecoilRecovery()
    {
        if (recoilOffset.magnitude > 0.001f)
        {
            recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
            transform.localPosition = originalPosition + recoilOffset;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Public API
    public void AddAmmo(int amount)
    {
        currentAmmo += amount;
        currentAmmo = Mathf.Min(currentAmmo, maxTotalAmmo);
        if(AutomaticOn)
            isAutomatic = true; // Re-enable automatic fire after empty sound
        OnAmmoChanged?.Invoke();
    }

    public int GetCurrentMagazineAmmo() => currentMagazineAmmo;
    public int GetCurrentTotalAmmo() => currentAmmo;
    public int GetMaxMagazineAmmo() => ammoPerMagazine;
    public int GetMaxTotalAmmo() => maxTotalAmmo;
    public float GetReloadTime() => reloadTime;
    public bool IsReloading() => isReloading;
    public string GetWeaponName() => weaponName;

    void OnDrawGizmosSelected()
    {
        if (firePoint == null)
            return;

        // Draw fire point
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(firePoint.position, 0.1f);
        Gizmos.DrawRay(firePoint.position, firePoint.right * 2f);

        // Draw spread cone
        if (projectileSpread > 0)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);

            Vector3 spreadUp = Quaternion.Euler(0, 0, projectileSpread) * firePoint.right * 2f;
            Vector3 spreadDown = Quaternion.Euler(0, 0, -projectileSpread) * firePoint.right * 2f;

            Gizmos.DrawRay(firePoint.position, spreadUp);
            Gizmos.DrawRay(firePoint.position, spreadDown);
        }
    }
}
