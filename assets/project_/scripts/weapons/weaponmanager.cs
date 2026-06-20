using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public enum WeaponType { Knife, Pistol, AR }

    [Header("Current Active Weapon")]
    public WeaponType currentWeapon = WeaponType.Knife;

    [Header("Pistol Ammunition Pools")]
    public int currentPistolAmmo = 12;
    public int maxPistolClip = 12;
    public int reservePistolAmmo = 60;

    [Header("AR Ammunition Pools")]
    public int currentARAmmo = 30;
    public int maxARClip = 30;
    public int reserveARAmmo = 180;

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
        // Alpha keybinds for selection
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(WeaponType.Knife);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(WeaponType.Pistol);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeapon(WeaponType.AR);

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
    /// Swaps the current weapon state and forces a model hierarchy refresh.
    /// </summary>
    public void EquipWeapon(WeaponType newWeapon)
    {
        if (currentWeapon == newWeapon) return;

        currentWeapon = newWeapon;
        Debug.Log($"Switched weapon array index to: {currentWeapon}");
        UpdateWeaponVisuals();
    }

    /// <summary>
    /// Mathematical loop to scroll cleanly forward or backward through weapon enums.
    /// </summary>
    void CycleWeapon(int direction)
    {
        int totalWeapons = System.Enum.GetValues(typeof(WeaponType)).Length;
        int nextIndex = (int)currentWeapon + direction;

        if (nextIndex >= totalWeapons) nextIndex = 0;
        if (nextIndex < 0) nextIndex = totalWeapons - 1;

        EquipWeapon((WeaponType)nextIndex);
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
    /// Toggles GameObject visibilities based on which weapon state is active.
    /// </summary>
    void UpdateWeaponVisuals()
    {
        if (knifeModel != null) knifeModel.SetActive(currentWeapon == WeaponType.Knife);
        if (pistolModel != null) pistolModel.SetActive(currentWeapon == WeaponType.Pistol);
        if (arModel != null) arModel.SetActive(currentWeapon == WeaponType.AR);
    }
}
