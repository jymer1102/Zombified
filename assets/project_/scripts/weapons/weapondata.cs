using UnityEngine;

/// <summary>
/// Defines the stats for a single weapon. Create one asset per weapon
/// (Assets > Create > Zombified > Weapon Data) and assign it in WeaponManager.
/// Centralizing these numbers here means balance changes don't require touching code.
/// </summary>
[CreateAssetMenu(fileName = "New Weapon", menuName = "Zombified/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;

    [Header("Damage & Range")]
    public float damage;
    public float range = 50f;

    [Header("Fire Behavior")]
    [Tooltip("Shots per second. Used for full-auto weapons like the AR.")]
    public float fireRate = 8f;
    [Tooltip("If true, holding the mouse button keeps firing. If false, one click = one shot.")]
    public bool isFullAuto = false;

    [Header("Accuracy & Spread Bloom")]
    [Tooltip("How accurate the gun is on the first shot. 0 means perfect laser accuracy.")]
    public float baseSpread = 0.02f;

    [Tooltip("How much wider the bullet cone gets when running or jumping.")]
    public float movementPenalty = 0.05f;

    [Header("Ammo")]
    public int maxClip = 12;
    public int maxReserve = 60;
    public float reloadTime = 1.5f;
}
