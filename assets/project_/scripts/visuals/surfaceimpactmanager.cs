using UnityEngine;

public class SurfaceImpactManager : MonoBehaviour
{
    public static SurfaceImpactManager Instance { get; private set; }

    [Header("Gore Particle Prefabs")]
    public GameObject zombieBloodSplatterPrefab;
    public GameObject environmentBloodDecalPrefab;
    
    [Header("Standard Impact Prefabs")]
    public GameObject concreteSparkPrefab;
    public GameObject bulletHoleDecalPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    /// <summary>
    /// Spawns realistic impact visuals based on what surface the bullet hit.
    /// </summary>
    public void SpawnImpactEffect(RaycastHit hit, bool hitZombie)
    {
        if (hitZombie)
        {
            // Spawn a 3D burst of blood particles bursting out backward from the hit point
            if (zombieBloodSplatterPrefab != null)
            {
                GameObject bloodGore = Instantiate(zombieBloodSplatterPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(bloodGore, 2.0f); // Clean up particles from memory
            }

            // Stick a dripping blood splatter texture decal onto the wall behind/under the zombie
            if (environmentBloodDecalPrefab != null)
            {
                // Offset slightly along the surface normal to prevent z-fighting texture glitching
                Vector3 spawnPos = hit.point + (hit.normal * 0.01f); 
                GameObject bloodDecal = Instantiate(environmentBloodDecalPrefab, spawnPos, Quaternion.LookRotation(-hit.normal));
                Destroy(bloodDecal, 15.0f); // Decals persist longer for grit, then clean up
            }
        }
        else
        {
            // Hit scenery: spawn debris sparks and a structural bullet hole
            if (concreteSparkPrefab != null)
            {
                GameObject sparks = Instantiate(concreteSparkPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(sparks, 1.0f);
            }

            if (bulletHoleDecalPrefab != null)
            {
                Vector3 spawnPos = hit.point + (hit.normal * 0.01f);
                GameObject hole = Instantiate(bulletHoleDecalPrefab, spawnPos, Quaternion.LookRotation(-hit.normal));
                // Parent it to the object hit so if the object moves, the bullet hole moves with it
                hole.transform.SetParent(hit.transform);
                Destroy(hole, 20.0f);
            }
        }
    }
}
