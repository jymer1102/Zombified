using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject easyZombiePrefab;
    public GameObject knifeZombiePrefab;
    public GameObject strongZombiePrefab;
    public GameObject bossZombiePrefab;

    public Transform[] spawnPoints;
    public float spawnInterval = 3f;

    private float spawnTimer;
    private int currentLevelTracking = 1;

    void Start()
    {
        spawnTimer = spawnInterval;
        if (GameManager.Instance != null)
        {
            currentLevelTracking = GameManager.Instance.currentLevel;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // Keep track of what level the game manager is on
        currentLevelTracking = GameManager.Instance.currentLevel;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnZombieBasedOnLevel();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnZombieBasedOnLevel()
    {
        if (spawnPoints.Length == 0) return;

        // Pick a random spawn point from your array
        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject zombieToSpawn = null;

        // Your progressive level difficulty rules
        switch (currentLevelTracking)
        {
            case 1:
                // Level 1: Easy Zombie
                zombieToSpawn = easyZombiePrefab;
                break;
            case 2:
                // Level 2: Knife Zombie
                zombieToSpawn = knifeZombiePrefab;
                break;
            case 3:
                // Level 3: Knife Zombie and Bigger/Stronger Zombie
                zombieToSpawn = Random.Range(0, 2) == 0 ? knifeZombiePrefab : strongZombiePrefab;
                break;
            case 4:
                // Level 4: Knife Zombie and Bigger/Stronger Zombie
                zombieToSpawn = Random.Range(0, 2) == 0 ? knifeZombiePrefab : strongZombiePrefab;
                break;
            case 5:
                // Level 5: Regular Knife Zombies and Biggest Strongest Boss Zombie
                // Simple logic: 10% chance to spawn the boss, otherwise standard knife zombie
                zombieToSpawn = Random.Range(0, 10) == 0 ? bossZombiePrefab : knifeZombiePrefab;
                break;
        }

        if (zombieToSpawn != null)
        {
            Instantiate(zombieToSpawn, randomPoint.position, randomPoint.rotation);
        }
    }
}
