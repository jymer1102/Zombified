using UnityEngine;

public class WeaponPurchaseWall : MonoBehaviour
{
    [Header("Purchase Settings")]
    public WeaponManager.WeaponType weaponToBuy;
    public int purchaseCost = 1000;
    public int ammoRefillCost = 250;

    /// <summary>
    /// Executes a weapon acquisition or ammo replenishment processing routine.
    /// This is meant to be called by your PlayerInteractions raycast tracker.
    /// </summary>
    public void AttemptInteraction(GameObject playerObject)
    {
        WeaponManager weaponManager = playerObject.GetComponent<WeaponManager>();
        if (weaponManager == null) return;

        // Verify if player already owns this weapon type
        bool alreadyOwnsWeapon = CheckWeaponOwnership(weaponManager, weaponToBuy);

        if (!alreadyOwnsWeapon)
        {
            // Logic check for purchasing a brand new firearm asset
            Debug.Log($"Purchased weapon asset: {weaponToBuy} for {purchaseCost} points.");
            UnlockWeaponType(weaponManager, weaponToBuy);
        }
        else
        {
            // Logic check for purchasing an ammo crate fill for an existing asset
            Debug.Log($"Refilled ammunition reserves for {weaponToBuy} for {ammoRefillCost} points.");
            RefillWeaponAmmoPool(weaponManager, weaponToBuy);
        }

        // Trigger interactive response audio cues if available
        // if (AudioManager.Instance != null) AudioManager.Instance.Play2DSFX(AudioManager.Instance.purchaseSuccessClip);
    }

    private bool CheckWeaponOwnership(WeaponManager wm, WeaponManager.WeaponType type)
    {
        // Simple placeholder checking logic (expandable based on inventory lists)
        return wm.currentWeapon == type;
    }

    private void UnlockWeaponType(WeaponManager wm, WeaponManager.WeaponType type)
    {
        wm.currentWeapon = type;
        RefillWeaponAmmoPool(wm, type);
    }

    private void RefillWeaponAmmoPool(WeaponManager wm, WeaponManager.WeaponType type)
    {
        // Safely set clips back up to their design caps using custom global stats
        if (type == WeaponManager.WeaponType.Pistol)
        {
            wm.currentPistolAmmo = 12;
            wm.reservePistolAmmo = 60;
        }
        else if (type == WeaponManager.WeaponType.AR)
        {
            wm.currentARAmmo = 30;
            wm.reserveARAmmo = 180;
        }
    }
}
