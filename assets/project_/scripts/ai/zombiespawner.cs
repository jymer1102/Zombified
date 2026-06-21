using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Spawning Coordinates")]
    public Transform[] spawnPoints;
    
    [Header("Timing Configurations")]
    public float spawnInterval = 3.0f;
    private float spawnTimer = 0.0f;

    [Header("Tier Spawn Weights (per the README's level design)")]
    [Tooltip("Chance (0-1) that a spawned zombie is the Strong variant once they start appearing.")]
    public float strongSpawnChance = 0.25f;
    [Tooltip("Level at which Strong zombies start appearing alongside Regular ones (README: level 3).")]
    public int strongUnlockLevel = 3;
    [Tooltip("Level at which a single Boss zombie spawns instead of the regular wave (README: level 5).")]
    public int bossLevel = 5;

    private bool bossSpawnedThisLevel = false;

    void OnEnable()
    {
        bossSpawnedThisLevel = false;
        spawnTimer = 0f;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // Stop spawning if the level requirements have already been met
        if (GameManager.Instance.currentKillsInLevel >= GameManager.Instance.killsNeededPerLevel)
        {
            return;
        }

        // Boss level: spawn exactly one boss and nothing else
        if (GameManager.Instance.currentLevel >= bossLevel)
        {
            if (!bossSpawnedThisLevel)
            {
                SpawnZombieFromPool(ZombieHealth.ZombieTier.Boss);
                bossSpawnedThisLevel = true;
            }
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            ZombieHealth.ZombieTier tierToSpawn = ChooseTierForCurrentLevel();
            SpawnZombieFromPool(tierToSpawn);
            spawnTimer = 0.0f;
        }
    }

    ZombieHealth.ZombieTier ChooseTierForCurrentLevel()
    {
        bool strongUnlocked = GameManager.Instance.currentLevel >= strongUnlockLevel;

        if (strongUnlocked && Random.value < strongSpawnChance)
        {
            return ZombieHealth.ZombieTier.Strong;
        }

        return ZombieHealth.ZombieTier.Regular;
    }

    void SpawnZombieFromPool(ZombieHealth.ZombieTier tier)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        // Choose a random spawn point location
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform targetPoint = spawnPoints[randomIndex];

        if (ObjectPooler.Instance != null)
        {
            // Grab a pre-warmed zombie from our performance pool instead of using Instantiate
            GameObject zombie = ObjectPooler.Instance.GetPooledZombie(targetPoint.position, targetPoint.rotation, tier);
            Debug.Log($"Zombie deployed ({tier}) from pool at location: {targetPoint.name}");
        }
        else
        {
            Debug.LogWarning("ObjectPooler instance is missing from the scene pipeline!");
        }
    }
}
