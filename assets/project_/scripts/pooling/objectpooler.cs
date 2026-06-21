using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    [Header("Pool Settings")]
    public GameObject zombiePrefab;
    public int poolSize = 20;

    private List<GameObject> pooledZombies;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        InitializePool();
    }

    /// <summary>
    /// Pre-spawns zombies at the start of the game and deactivates them.
    /// </summary>
    void InitializePool()
    {
        pooledZombies = new List<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            if (zombiePrefab != null)
            {
                GameObject obj = Instantiate(zombiePrefab);
                obj.SetActive(false);
                // Keep the hierarchy clean by parent-nesting them under this object
                obj.transform.SetParent(this.transform);
                pooledZombies.Add(obj);
            }
        }
    }

    /// <summary>
    /// Fetches an available, inactive zombie from the pool, defaulting to the Regular tier.
    /// Kept for backward compatibility with any code that doesn't care about tier.
    /// </summary>
    public GameObject GetPooledZombie(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        return GetPooledZombie(spawnPosition, spawnRotation, ZombieHealth.ZombieTier.Regular);
    }

    /// <summary>
    /// Fetches an available, inactive zombie from the pool and sets its tier
    /// (Regular / Strong / Boss) before activating it.
    /// </summary>
    public GameObject GetPooledZombie(Vector3 spawnPosition, Quaternion spawnRotation, ZombieHealth.ZombieTier tier)
    {
        GameObject zombie = FindInactiveZombie();

        if (zombie == null && zombiePrefab != null)
        {
            // Optional expansion fallback: if the pool runs dry, generate an extra one
            zombie = Instantiate(zombiePrefab);
            zombie.transform.SetParent(this.transform);
            pooledZombies.Add(zombie);
        }

        if (zombie == null) return null;

        // Set tier BEFORE activating, so OnEnable() in ZombieHealth picks up the right health pool
        ZombieHealth health = zombie.GetComponent<ZombieHealth>();
        if (health != null)
        {
            health.tier = tier;
        }

        zombie.transform.position = spawnPosition;
        zombie.transform.rotation = spawnRotation;
        zombie.SetActive(true);

        return zombie;
    }

    GameObject FindInactiveZombie()
    {
        for (int i = 0; i < pooledZombies.Count; i++)
        {
            if (!pooledZombies[i].activeInHierarchy)
            {
                return pooledZombies[i];
            }
        }
        return null;
    }
}
