using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public enum PickupType { PistolAmmo, ARAmmo, Grenade }
    
    [Header("Pickup Settings")]
    public PickupType type = PickupType.PistolAmmo;
    public int refillAmount = 15; 

    private bool playerIsClose = false;
    private WeaponManager playerInventory;

    private void Update()
    {
        // If the player is near the ammo and presses the 'E' key
        if (playerIsClose && Input.GetKeyDown(KeyCode.E))
        {
            InteractAndPickup();
        }
    }

    private void InteractAndPickup()
    {
        if (playerInventory != null)
        {
            switch (type)
            {
                case PickupType.PistolAmmo:
                    playerInventory.PickupAmmo(WeaponManager.WeaponType.Pistol, refillAmount);
                    break;

                case PickupType.ARAmmo:
                    playerInventory.PickupAmmo(WeaponManager.WeaponType.AR, refillAmount);
                    break;

                case PickupType.Grenade:
                    playerInventory.PickupGrenades(refillAmount);
                    break;
            }

            // Trigger a small 2D pickup sound cue if available
            if (AudioManager.Instance != null && AudioManager.Instance.pickupClip != null)
            {
                AudioManager.Instance.Play2DSFX(AudioManager.Instance.pickupClip);
            }

            Debug.Log($"Successfully picked up {type}!");
            // Destroy the pickup item from the map after it's collected
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = true;
            playerInventory = other.GetComponent<WeaponManager>();
            Debug.Log("Press 'E' to pick up ammo.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = false;
            playerInventory = null;
        }
    }
}
