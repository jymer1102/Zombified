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

        if (GameManager.Instance == null) return;

        // Verify if player already owns this weapon type using REAL ownership tracking
        bool alreadyOwnsWeapon = weaponManager.IsWeaponOwned(weaponToBuy);

        if (!alreadyOwnsWeapon)
        {
            // Check if the player can actually afford this weapon
            if (GameManager.Instance.score < purchaseCost)
            {
                Debug.Log($"Not enough points to buy {weaponToBuy}. Need {purchaseCost}, have {GameManager.Instance.score}.");
                return;
            }

            // Deduct cost and unlock the weapon for real
            GameManager.Instance.AddScore(-purchaseCost);
            weaponManager.UnlockWeapon(weaponToBuy);
            weaponManager.EquipWeapon(weaponToBuy);
            Debug.Log($"Purchased weapon asset: {weaponToBuy} for {purchaseCost} points.");
        }
        else
        {
            // Check if the player can afford an ammo refill
            if (GameManager.Instance.score < ammoRefillCost)
            {
                Debug.Log($"Not enough points to refill {weaponToBuy}. Need {ammoRefillCost}, have {GameManager.Instance.score}.");
                return;
            }

            GameManager.Instance.AddScore(-ammoRefillCost);
            RefillWeaponAmmoPool(weaponManager, weaponToBuy);
            Debug.Log($"Refilled ammunition reserves for {weaponToBuy} for {ammoRefillCost} points.");
        }

        // Trigger interactive response audio cues if available
        if (AudioManager.Instance != null && AudioManager.Instance.purchaseSuccessClip != null)
        {
            AudioManager.Instance.Play2DSFX(AudioManager.Instance.purchaseSuccessClip);
        }
    }

    private void RefillWeaponAmmoPool(WeaponManager wm, WeaponManager.WeaponType type)
    {
        // Safely set clips back up to their design caps using custom global stats
        if (type == WeaponManager.WeaponType.Pistol)
        {
            wm.currentPistolAmmo = wm.maxPistolClip;
            wm.reservePistolAmmo = 60;
        }
        else if (type == WeaponManager.WeaponType.AR)
        {
            wm.currentARAmmo = wm.maxARClip;
            wm.reserveARAmmo = 180;
        }
    }
}
