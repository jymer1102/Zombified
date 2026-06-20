using UnityEngine;

public class WeaponData : MonoBehaviour
{
    [Header("Base Weapon Settings")]
    public string weaponName;
    public float damage;
    public float fireRate;
    public float range;

    [Header("Accuracy & Spread Bloom")]
    [Tooltip("How accurate the gun is on the first shot. 0 means perfect laser accuracy.")]
    public float baseSpread = 0.02f;
    
    [Tooltip("How much wider the bullet cone gets when running or jumping.")]
    public float movementPenalty = 0.05f;

    [Tooltip("The maximum possible random angle the bullet can fly off course.")]
    public float maxSpread = 0.15f;
}
