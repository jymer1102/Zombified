using UnityEngine;
using UnityEngine.UI; // If using standard Unity UI text
// using TMPro; // Uncomment this line if you use TextMeshPro instead

public class UIManager : MonoBehaviour
{
    [Header("UI Text Elements")]
    public Text ammoText;         // Displays clip/reserve ammo
    public Text grenadeText;      // Displays remaining grenades
    public Text levelText;        // Displays current level (1-5)
    public Text progressText;     // Displays kills remaining

    [Header("References")]
    private WeaponManager weaponManager;

    void Start()
    {
        // Find the player in the scene to pull ammo data
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            weaponManager = player.GetComponent<WeaponManager>();
        }
    }

    void Update()
    {
        UpdateWeaponUI();
        UpdateLevelUI();
    }

    void UpdateWeaponUI()
    {
        if (weaponManager == null) return;

        // Update Ammo Text based on what weapon is currently held
        switch (weaponManager.currentWeapon)
        {
            case WeaponManager.WeaponType.Knife:
                if (ammoText != null) ammoText.text = "Weapon: KNIFE";
                break;

            case WeaponManager.WeaponType.Pistol:
                if (ammoText != null) 
                    ammoText.text = $"Weapon: PISTOL\nAmmo: {weaponManager.currentPistolAmmo} / {weaponManager.reservePistolAmmo}";
                break;

            case WeaponManager.WeaponType.AR:
                if (ammoText != null) 
                    ammoText.text = $"Weapon: AR\nAmmo: {weaponManager.currentARAmmo} / {weaponManager.reserveARAmmo}";
                break;
        }

        // Update Grenade Count
        if (grenadeText != null)
        {
            grenadeText.text = $"Grenades: {weaponManager.grenadeCount} / {WeaponManager.MAX_GRENADES}";
        }
    }

    void UpdateLevelUI()
    {
        if (GameManager.Instance == null) return;

        // Display current game level
        if (levelText != null)
        {
            levelText.text = $"LEVEL {GameManager.Instance.currentLevel}";
        }

        // Display how many kills they have towards the target goal, against the real per-level quota
        if (progressText != null)
        {
            progressText.text = $"Kills: {GameManager.Instance.currentKillsInLevel} / {GameManager.Instance.killsNeededPerLevel}";
        }
    }
}
