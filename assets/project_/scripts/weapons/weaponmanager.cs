using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // Weapon Types
    public enum WeaponType { Knife, Pistol, AR }
    public WeaponType currentWeapon = WeaponType.Knife;

    [Header("Ammo Settings")]
    // Standard magazine/clip capacities
    public const int PISTOL_CLIP_SIZE = 15;
    public const int AR_CLIP_SIZE = 30;
    public const int MAX_GRENADES = 5;

    // Current ammo in the clip
    public int currentPistolAmmo = PISTOL_CLIP_SIZE;
    public int currentARAmmo = AR_CLIP_SIZE;
    public int grenadeCount = 0;

    [Header("Reserve Ammo (Picked up from map)")]
    public int reservePistolAmmo = 45;
    public int reserveARAmmo = 90;

    void Update()
    {
        HandleWeaponSwitching();
        HandleShooting();
        HandleGrenade();
    }

    void HandleWeaponSwitching()
    {
        // Toggle 'Z' to switch between Pistol and AR
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentWeapon == WeaponType.Pistol)
            {
                currentWeapon = WeaponType.AR;
                Debug.Log("Switched to Assault Rifle");
            }
            else
            {
                currentWeapon = WeaponType.Pistol;
                Debug.Log("Switched to Pistol");
            }
        }

        // Toggle 'R' to pull out the Knife
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentWeapon = WeaponType.Knife;
            Debug.Log("Switched to Knife");
        }
    }

    void HandleShooting()
    {
        // Left Click to attack/shoot
        if (Input.GetMouseButtonDown(0))
        {
            switch (currentWeapon)
            {
                case WeaponType.Knife:
                    Debug.Log("Slashed with Knife! (Infinite ammo)");
                    break;

                case WeaponType.Pistol:
                    if (currentPistolAmmo > 0)
                    {
                        currentPistolAmmo--;
                        Debug.Log($"Fired Pistol! Ammo left: {currentPistolAmmo}/{PISTOL_CLIP_SIZE}");
                    }
                    else
                    {
                        Debug.Log("Pistol Out of Ammo!");
                    }
                    break;

                case WeaponType.AR:
                    if (currentARAmmo > 0)
                    {
                        currentARAmmo--;
                        Debug.Log($"Fired AR! Ammo left: {currentARAmmo}/{AR_CLIP_SIZE}");
                    }
                    else
                    {
                        Debug.Log("AR Out of Ammo!");
                    }
                    break;
            }
        }
    }

    void HandleGrenade()
    {
        // Toggle 'Q' to throw grenade
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (grenadeCount > 0)
            {
                grenadeCount--;
                Debug.Log($"Grenade Thrown! Grenades left: {grenadeCount}/{MAX_GRENADES}");
            }
            else
            {
                Debug.Log("No grenades left!");
            }
        }
    }

    public void PickupAmmo(WeaponType type, int amount)
    {
        if (type == WeaponType.Pistol) reservePistolAmmo += amount;
        if (type == WeaponType.AR) reserveARAmmo += amount;
        Debug.Log($"Picked up ammo. Reserve Pistol: {reservePistolAmmo}, Reserve AR: {reserveARAmmo}");
    }

    public void PickupGrenades(int amount)
    {
        grenadeCount = Mathf.Clamp(grenadeCount + amount, 0, MAX_GRENADES);
        Debug.Log($"Picked up grenades. Total: {grenadeCount}");
    }
}
