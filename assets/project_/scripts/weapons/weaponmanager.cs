using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public enum WeaponType { Knife, Pistol, AR }

    [Header("Current Active Weapon")]
    public WeaponType currentWeapon = WeaponType.Knife;

    [Header("Weapon Ownership")]
    // Tracks which weapons the player has actually bought/unlocked.
    // Index matches the WeaponType enum order: Knife, Pistol, AR.
    public bool[] ownedWeapons = new bool[] { true, false, false }; // Knife always owned

    [Header("Pistol Ammunition Pools")]
    public int currentPistolAmmo = 12;
    public int maxPistolClip = 12;
    public int reservePistolAmmo = 60;

    [Header("AR Ammunition Pools")]
    public int currentARAmmo = 30;
    public int maxARClip = 30;
    public int reserveARAmmo = 180;

    [Header("Grenades")]
    public const int MAX_GRENADES = 5;
    public int grenadeCount = 2;

    [Header("Reload Timing Settings")]
    public float pistolReloadTime = 1.5f;
    public float arReloadTime = 2.5f;
    private bool isReloading = false;

    [Header("Weapon Models (Visual GOBs)")]
    public GameObject knifeModel;
    public GameObject pistolModel;
    public GameObject arModel;

    void Start()
    {
        // Initialize the game with the starting weapon visual set up correctly
        UpdateWeaponVisuals();
    }

    void Update()
    {
        if (isReloading) return;

        HandleWeaponInput();
    }

    /// <summary>
    /// Processes keypress registers for swapping and reloading weapons.
    /// </summary>
    void HandleWeaponInput()
    {
        // Alpha keybinds for selection - only allow switching to owned weapons
        if (Input.GetKeyDown(KeyCode.Alpha1)) TryEquipWeapon(WeaponType.Knife);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryEquipWeapon(WeaponType.Pistol);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryEquipWeapon(WeaponType.AR);

        // Scroll wheel selection matrix
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            CycleWeapon(1);
        }
        else if (scroll < 0f)
        {
            CycleWeapon(-1);
        }

        // Reload registration
        if (Input.GetKeyDown(KeyCode.R))
        {
            AttemptReload();
        }
    }

    /// <summary>
    /// Checks ownership before allowing a weapon swap. Silently does nothing if not owned.
    /// </summary>
    void TryEquipWeapon(WeaponType newWeapon)
    {
        if (!IsWeaponOwned(newWeapon))
        {
            Debug.Log($"You don't own the {newWeapon} yet. Find a purchase wall to unlock it.");
            return;
        }

        EquipWeapon(newWeapon);
    }

    /// <summary>
    /// Swaps the current weapon state and forces a model hierarchy refresh.
    /// </summary>
    public void EquipWeapon(WeaponType newWeapon)
    {
        if (currentWeapon == newWeapon) return;
        if (!IsWeaponOwned(newWeapon)) return;

        currentWeapon = newWeapon;
        Debug.Log($"Switched weapon array index to: {currentWeapon}");
        UpdateWeaponVisuals();
    }

    /// <summary>
    /// Mathematical loop to scroll cleanly forward or backward through OWNED weapon enums only.
    /// </summary>
    void CycleWeapon(int direction)
    {
        int totalWeapons = System.Enum.GetValues(typeof(WeaponType)).Length;
        int nextIndex = (int)currentWeapon;

        // Keep stepping until we land on a weapon we actually own, max one full loop
        for (int i = 0; i < totalWeapons; i++)
        {
            nextIndex += direction;
            if (nextIndex >= totalWeapons) nextIndex = 0;
            if (nextIndex < 0) nextIndex = totalWeapons - 1;

            if (IsWeaponOwned((WeaponType)nextIndex))
            {
                EquipWeapon((WeaponType)nextIndex);
                return;
            }
        }
    }

    public bool IsWeaponOwned(WeaponType type)
    {
        int index = (int)type;
        if (index < 0 || index >= ownedWeapons.Length) return false;
        return ownedWeapons[index];
    }

    /// <summary>
    /// Called by WeaponPurchaseWall when the player buys a new weapon.
    /// </summary>
    public void UnlockWeapon(WeaponType type)
    {
        int index = (int)type;
        if (index < 0 || index >= ownedWeapons.Length) return;
        ownedWeapons[index] = true;
    }

    /// <summary>
    /// Evaluates ammo reserves and triggers a background reload calculation sequence.
    /// </summary>
    void AttemptReload()
    {
        if (currentWeapon == WeaponType.Knife) return;

        if (currentWeapon == WeaponType.Pistol && currentPistolAmmo < maxPistolClip && reservePistolAmmo > 0)
        {
            StartCoroutine(ReloadRoutine(pistolReloadTime, () => {
                int ammoNeeded = maxPistolClip - currentPistolAmmo;
                int ammoToFill = Mathf.Min(ammoNeeded, reservePistolAmmo);
                currentPistolAmmo += ammoToFill;
                reservePistolAmmo -= ammoToFill;
            }));
        }
        else if (currentWeapon == WeaponType.AR && currentARAmmo < maxARClip && reserveARAmmo > 0)
        {
            StartCoroutine(ReloadRoutine(arReloadTime, () => {
                int ammoNeeded = maxARClip - currentARAmmo;
                int ammoToFill = Mathf.Min(ammoNeeded, reserveARAmmo);
                currentARAmmo += ammoToFill;
                reserveARAmmo -= ammoToFill;
            }));
        }
    }

    /// <summary>
    /// Thread simulation to block firing operations while a reload animation timeline passes.
    /// </summary>
    System.Collections.IEnumerator ReloadRoutine(float delay, System.Action reloadAction)
    {
        isReloading = true;
        Debug.Log($"Reloading {currentWeapon}... Standby.");

        // Trigger global audio reload cue if available
        if (AudioManager.Instance != null && AudioManager.Instance.reloadClip != null)
        {
            AudioManager.Instance.Play2DSFX(AudioManager.Instance.reloadClip);
        }

        yield return new WaitForSeconds(delay);

        reloadAction.Invoke();
        isReloading = false;
        Debug.Log($"{currentWeapon} reload sequence complete.");
    }

    /// <summary>
    /// Called by PlayerCombat every time a shot is actually fired. Returns true if ammo was available
    /// and consumed, false if the weapon is empty (so PlayerCombat knows not to fire).
    /// </summary>
    public bool TryConsumeAmmo(WeaponType type)
    {
        if (type == WeaponType.Pistol)
        {
            if (currentPistolAmmo <= 0) return false;
            currentPistolAmmo--;
            return true;
        }
        else if (type == WeaponType.AR)
        {
            if (currentARAmmo <= 0) return false;
            currentARAmmo--;
            return true;
        }

        // Knife has no ammo cost
        return true;
    }

    /// <summary>
    /// Called by AmmoPickup when the player grabs an ammo crate off the ground.
    /// </summary>
    public void PickupAmmo(WeaponType type, int amount)
    {
        if (type == WeaponType.Pistol)
        {
            reservePistolAmmo = Mathf.Min(reservePistolAmmo + amount, 999);
        }
        else if (type == WeaponType.AR)
        {
            reserveARAmmo = Mathf.Min(reserveARAmmo + amount, 999);
        }
    }

    /// <summary>
    /// Called by AmmoPickup when the player grabs a grenade pickup off the ground.
    /// </summary>
    public void PickupGrenades(int amount)
    {
        grenadeCount = Mathf.Min(grenadeCount + amount, MAX_GRENADES);
    }

    /// <summary>
    /// Called by PlayerCombat when a grenade is thrown. Returns true if a grenade was available.
    /// </summary>
    public bool TryUseGrenade()
    {
        if (grenadeCount <= 0) return false;
        grenadeCount--;
        return true;
    }

    /// <summary>
    /// Resets ammo and grenades to full. Called at the start of each level.
    /// </summary>
    public void ResetForNewLevel()
    {
        currentPistolAmmo = maxPistolClip;
        reservePistolAmmo = 60;
        currentARAmmo = maxARClip;
        reserveARAmmo = 180;
        grenadeCount = Mathf.Min(grenadeCount, MAX_GRENADES);
    }

    /// <summary>
    /// Toggles GameObject visibilities based on which weapon state is active.
    /// </summary>
    void UpdateWeaponVisuals()
    {
        if (knifeModel != null) knifeModel.SetActive(currentWeapon == WeaponType.Knife);
        if (pistolModel != null) pistolModel.SetActive(currentWeapon == WeaponType.Pistol);
        if (arModel != null) arModel.SetActive(currentWeapon == WeaponType.AR);
    }
}
