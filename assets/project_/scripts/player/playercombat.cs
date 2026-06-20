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

    [Header("Dynamic Accuracy Systems")]
    public float pistolSpread = 0.01f;
    public float arSpread = 0.04f;
    public float movementSpreadPenalty = 0.05f;

    void Start()
    {
        weaponManager = GetComponent<WeaponManager>();
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Left click to attack
        if (Input.GetMouseButtonDown(0))
        {
            ExecuteAttack();
        }
    }

    void ExecuteAttack()
    {
        if (weaponManager == null || playerCamera == null) return;

        // Base forward direction from the center of the camera viewport
        Vector3 targetDirection = playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        // Calculate dynamic spread modifications if moving
        bool isMoving = characterController != null && characterController.velocity.sqrMagnitude > 0.1f;
        float activePenalty = isMoving ? movementSpreadPenalty : 0f;

        WeaponVisuals currentVisuals = GetComponentInChildren<WeaponVisuals>();
        RaycastHit hit;

        if (weaponManager.currentWeapon == WeaponManager.WeaponType.Knife)
        {
            if (Physics.Raycast(ray, out hit, knifeRange))
            {
                ProcessHit(hit, knifeDamage);
            }
        }
        else if (weaponManager.currentWeapon == WeaponManager.WeaponType.Pistol && weaponManager.currentPistolAmmo > 0)
        {
            if (currentVisuals != null) currentVisuals.TriggerRecoil();
            if (CameraShake.Instance != null) CameraShake.Instance.TriggerShake(0.08f, 0.03f);

            // Apply calculated accuracy deviation
            ray.direction = CalculateSpreadDirection(ray.direction, pistolSpread + activePenalty);

            if (Physics.Raycast(ray, out hit, attackRange))
            {
                ProcessHit(hit, pistolDamage);
            }
        }
        else if (weaponManager.currentWeapon == WeaponManager.WeaponType.AR && weaponManager.currentARAmmo > 0)
        {
            if (currentVisuals != null) currentVisuals.TriggerRecoil();
            if (CameraShake.Instance != null) CameraShake.Instance.TriggerShake(0.12f, 0.07f);

            // Apply a larger calculated deviation for rapid automatic fire
            ray.direction = CalculateSpreadDirection(ray.direction, arSpread + activePenalty);

            if (Physics.Raycast(ray, out hit, attackRange))
            {
                ProcessHit(hit, arDamage);
            }
        }
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

    void ProcessHit(RaycastHit hit, float damage)
    {
        IDamageable damageableTarget = hit.transform.GetComponent<IDamageable>();
        if (damageableTarget != null)
        {
            damageableTarget.TakeDamage(damage);
        }
    }
}
