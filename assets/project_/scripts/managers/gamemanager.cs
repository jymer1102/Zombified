using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Stats & State")]
    public int playerHealth = 100;
    public int score = 0;
    public bool isGameOver = false;

    [Header("Level & Progression Systems")]
    public int currentLevel = 1;
    public int currentKillsInLevel = 0;
    public int killsNeededPerLevel = 10;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Processes player damage tracking and handles death state triggers.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (isGameOver) return;

        playerHealth -= amount;
        Debug.Log($"Player hit! Remaining Health: {playerHealth}");

        if (playerHealth <= 0)
        {
            TriggerGameOver();
        }
    }

    /// <summary>
    /// Adds points to the player's runtime score metric.
    /// </summary>
    public void AddScore(int points)
    {
        score += points;
        Debug.Log($"Score updated: {score}");
    }

    /// <summary>
    /// Increments the player's kill count and evaluates level completion logic.
    /// </summary>
    public void RegisterKill()
    {
        currentKillsInLevel++;
        AddScore(100); // Reward standard kill points
        Debug.Log($"Zombie eliminated! Progress: {currentKillsInLevel}/{killsNeededPerLevel}");

        if (currentKillsInLevel >= killsNeededPerLevel)
        {
            AdvanceToNextLevel();
        }
    }

    void AdvanceToNextLevel()
    {
        currentLevel++;
        currentKillsInLevel = 0;
        
        // Dynamic scaling: Increase difficulty requirements per map tier
        killsNeededPerLevel += 5;

        Debug.Log($"Level Complete! Progressing to Level: {currentLevel}");

        // Command scene manager to load the corresponding environment
        if (LevelSceneManager.Instance != null)
        {
            LevelSceneManager.Instance.LoadMapForLevel(currentLevel);
        }

        // Save progress to local disk cache
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGameProgress(currentLevel);
        }
    }

    void TriggerGameOver()
    {
        isGameOver = true;
        Debug.Log("GAME OVER: Player health pool depleted.");
        
        // Bring player safely back to the Main Menu layout scene
        if (LevelSceneManager.Instance != null)
        {
            LevelSceneManager.Instance.LoadMainMenu();
        }
    }
}
