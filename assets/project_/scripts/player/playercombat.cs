using UnityEngine;
using Project.Interfaces;

public class PlayerCombat : MonoBehaviour
{
    public Camera playerCamera;
    private WeaponManager weaponManager;
    private CharacterController characterController;

    [Header("Weapon Configurations")]
    public float pistolDamage = 25f;
    public float arDamage = 45f;
    public float knifeDamage = 50f;

    public float attackRange = 50f;
    public float knifeRange = 2f;

    [Header("Fire Rate")]
    [Tooltip("Shots per second for the AR while holding the trigger.")]
    public float arFireRate = 8f;
    [Tooltip("Shots per second for the pistol. Semi-auto, so this mostly just prevents spam-clicking exploits.")]
    public float pistolFireRate = 4f;
    private float nextFireTime = 0f;

    [Header("Dynamic Accuracy Systems")]
    public float pistolSpread = 0.01f;
    public float arSpread = 0.04f;
    public float movementSpreadPenalty = 0.05f;

    [Header("Grenades")]
    public GameObject grenadePrefab;
    public Transform throwOrigin;

    void Start()
    {
        weaponManager = GetComponent<WeaponManager>();
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleFiring();
        HandleGrenadeThrow();
    }

    void HandleFiring()
    {
        if (weaponManager == null) return;

        bool wantsToFire;

        // AR is full-auto (holding the button keeps firing), everything else is semi-auto (one click = one shot)
        if (weaponManager.currentWeapon == WeaponManager.WeaponType.AR)
        {
            wantsToFire = Input.GetMouseButton(0);
        }
        else
        {
            wantsToFire = Input.GetMouseButtonDown(0);
        }

        if (!wantsToFire) return;
        if (Time.time < nextFireTime) return;

        ExecuteAttack();
    }

    void HandleGrenadeThrow()
    {
        if (weaponManager == null) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ThrowGrenade();
        }
    }

    void ThrowGrenade()
    {
        if (grenadePrefab == null || playerCamera == null) return;

        if (!weaponManager.TryUseGrenade())
        {
            Debug.Log("No grenades left!");
            return;
        }

        Vector3 spawnPos = throwOrigin != null ? throwOrigin.position : playerCamera.transform.position + playerCamera.transform.forward;
        GameObject grenadeObj = Instantiate(grenadePrefab, spawnPos, Quaternion.identity);

        GrenadeProjectile grenade = grenadeObj.GetComponent<GrenadeProjectile>();
        if (grenade != null)
        {
            grenade.Launch(playerCamera.transform.forward);
        }

        if (AudioManager.Instance != null && AudioManager.Instance.grenadeThrowClip != null)
        {
            AudioManager.Instance.Play2DSFX(AudioManager.Instance.grenadeThrowClip);
        }
    }

    void ExecuteAttack()
    {
        if (weaponManager == null || playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // Calculate dynamic spread modifications if moving
        bool isMoving = characterController != null && characterController.velocity.sqrMagnitude > 0.1f;
        float activePenalty = isMoving ? movementSpreadPenalty : 0f;

        WeaponVisuals currentVisuals = GetComponentInChildren<WeaponVisuals>();
        RaycastHit hit;

        if (weaponManager.currentWeapon == WeaponManager.WeaponType.Knife)
        {
            nextFireTime = Time.time + (1f / pistolFireRate); // reuse pistol-rate cooldown for knife swings

            if (AudioManager.Instance != null && AudioManager.Instance.knifeSlashClip != null)
            {
                AudioManager.Instance.Play2DSFX(AudioManager.Instance.knifeSlashClip);
            }

            if (Physics.Raycast(ray, out hit, knifeRange))
            {
                ProcessHit(hit, knifeDamage);
            }
        }
        else if (weaponManager.currentWeapon == WeaponManager.WeaponType.Pistol)
        {
            if (!weaponManager.TryConsumeAmmo(WeaponManager.WeaponType.Pistol))
            {
                Debug.Log("Pistol is empty! Press R to reload.");
                return;
            }

            nextFireTime = Time.time + (1f / pistolFireRate);

            FireWeaponEffects(currentVisuals, 0.08f, 0.03f, AudioManager.Instance?.pistolShotClip);

            ray.direction = CalculateSpreadDirection(ray.direction, pistolSpread + activePenalty);

            if (Physics.Raycast(ray, out hit, attackRange))
            {
                ProcessHit(hit, pistolDamage);
            }
        }
        else if (weaponManager.currentWeapon == WeaponManager.WeaponType.AR)
        {
            if (!weaponManager.TryConsumeAmmo(WeaponManager.WeaponType.AR))
            {
                Debug.Log("AR is empty! Press R to reload.");
                return;
            }

            nextFireTime = Time.time + (1f / arFireRate);

            FireWeaponEffects(currentVisuals, 0.12f, 0.07f, AudioManager.Instance?.arShotClip);

            ray.direction = CalculateSpreadDirection(ray.direction, arSpread + activePenalty);

            if (Physics.Raycast(ray, out hit, attackRange))
            {
                ProcessHit(hit, arDamage);
            }
        }
    }

    /// <summary>
    /// Bundles up the shared juice for any gunshot: recoil kick, camera shake, and sound.
    /// </summary>
    void FireWeaponEffects(WeaponVisuals visuals, float shakeDuration, float shakeAmount, AudioClip shotClip)
    {
        if (visuals != null) visuals.TriggerRecoil();
        if (CameraShake.Instance != null) CameraShake.Instance.TriggerShake(shakeDuration, shakeAmount);
        if (AudioManager.Instance != null && shotClip != null) AudioManager.Instance.Play2DSFX(shotClip);
    }

    /// <summary>
    /// Deviates a clean vector trajectory by adding a random offset inside a 2D circle plane.
    /// </summary>
    Vector3 CalculateSpreadDirection(Vector3 forwardVector, float spreadIntensity)
    {
        if (spreadIntensity <= 0) return forwardVector;

        // Generate coordinates inside a unit circle ring
        Vector2 deviationOffset = Random.insideUnitCircle * spreadIntensity;

        // Transform the 2D offset vector back into 3D world relative rotation axes
        Vector3 spreadVector = forwardVector + (playerCamera.transform.right * deviationOffset.x) + (playerCamera.transform.up * deviationOffset.y);
        return spreadVector.normalized;
    }

    /// <summary>
    /// Processes structural combat impacts and routes visual impact particle cues,
    /// plus hitmarker feedback and floating damage numbers.
    /// </summary>
    void ProcessHit(RaycastHit hit, float damage)
    {
        IDamageable damageableTarget = hit.transform.GetComponent<IDamageable>();

        if (damageableTarget != null)
        {
            // Check BEFORE applying damage so we know if this hit is the killing blow
            ZombieHealth zombieHealth = hit.transform.GetComponent<ZombieHealth>();
            bool willKill = zombieHealth != null && zombieHealth.WouldDie(damage);

            damageableTarget.TakeDamage(damage);

            // Hitmarker + floating number feedback
            if (HitFeedbackManager.Instance != null)
            {
                HitFeedbackManager.Instance.ShowHitmarker(willKill);
                HitFeedbackManager.Instance.SpawnDamageNumber(hit.point, damage);
            }

            // Call the surface impact manager to drop high-end blood spray effects at the exact point of impact
            if (SurfaceImpactManager.Instance != null)
            {
                SurfaceImpactManager.Instance.SpawnImpactEffect(hit, true);
            }
        }
        else
        {
            // Hit a wall, floor, or static mesh scenery asset instead of an entity
            if (SurfaceImpactManager.Instance != null)
            {
                SurfaceImpactManager.Instance.SpawnImpactEffect(hit, false);
            }
        }
    }
}
