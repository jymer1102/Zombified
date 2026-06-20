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
    /// Fetches an available, inactive zombie from the pool.
    /// </summary>
    public GameObject GetPooledZombie(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        for (int i = 0; i < pooledZombies.Count; i++)
        {
            // Find a zombie that isn't currently active in the game world
            if (!pooledZombies[i].activeInHierarchy)
            {
                GameObject zombie = pooledZombies[i];
                zombie.transform.position = spawnPosition;
                zombie.transform.rotation = spawnRotation;
                zombie.SetActive(true);
                return zombie;
            }
        }

        // Optional expansion fallback: if the pool runs dry, generate an extra one
        if (zombiePrefab != null)
        {
            GameObject obj = Instantiate(zombiePrefab, spawnPosition, spawnRotation);
            obj.transform.SetParent(this.transform);
            pooledZombies.Add(obj);
            return obj;
        }

        return null;
    }
}
