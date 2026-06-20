using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Camera playerCamera;
    private WeaponManager weaponManager;

    [Header("Weapon Configurations")]
    public float pistolDamage = 25f;
    public float arDamage = 45f;
    public float knifeDamage = 50f;
    
    public float attackRange = 50f;
    public float knifeRange = 2f;

    void Start()
    {
        weaponManager = GetComponent<WeaponManager>();
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

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Try to find the visual recoil script on whatever weapon model is currently active
        WeaponVisuals currentVisuals = GetComponentInChildren<WeaponVisuals>();

        if (weaponManager.currentWeapon == WeaponManager.WeaponType.Knife)
        {
            if (Physics.Raycast(ray, out hit, knifeRange))
            {
                ProcessHit(hit, knifeDamage);
            }
        }
        else if (weaponManager.currentWeapon == WeaponManager.WeaponType.Pistol && weaponManager.currentPistolAmmo > 0)
        {
            // Apply gun kickback/recoil visual
            if (currentVisuals != null) currentVisuals.TriggerRecoil();

            if (Physics.Raycast(ray, out hit, attackRange))
            {
                ProcessHit(hit, pistolDamage);
            }
        }
        else if (weaponManager.currentWeapon == WeaponManager.WeaponType.AR && weaponManager.currentARAmmo > 0)
        {
            // Apply gun kickback/recoil visual
            if (currentVisuals != null) currentVisuals.TriggerRecoil();

            if (Physics.Raycast(ray, out hit, attackRange))
            {
                ProcessHit(hit, arDamage);
            }
        }
    }

    void ProcessHit(RaycastHit hit, float damage)
    {
        ZombieHealth zombie = hit.transform.GetComponent<ZombieHealth>();
        if (zombie != null)
        {
            zombie.TakeDamage(damage);
            Debug.Log($"Hit zombie for {damage} damage!");
        }
    }
}
