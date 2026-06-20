using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Spawning Coordinates")]
    public Transform[] spawnPoints;
    
    [Header("Timing Configurations")]
    public float spawnInterval = 3.0f;
    private float spawnTimer = 0.0f;

    void Update()
    {
        if (GameManager.Instance == null) return;

        // Stop spawning if the level requirements have already been met
        if (GameManager.Instance.currentKillsInLevel >= GameManager.Instance.killsNeededPerLevel)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            SpawnZombieFromPool();
            spawnTimer = 0.0f;
        }
    }

    void SpawnZombieFromPool()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        // Choose a random spawn point location
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform targetPoint = spawnPoints[randomIndex];

        if (ObjectPooler.Instance != null)
        {
            // Grab a pre-warmed zombie from our performance pool instead of using Instantiate
            ObjectPooler.Instance.GetPooledZombie(targetPoint.position, targetPoint.rotation);
            Debug.Log($"Zombie deployed from pool at location: {targetPoint.name}");
        }
        else
        {
            Debug.LogWarning("ObjectPooler instance is missing from the scene pipeline!");
        }
    }
}
