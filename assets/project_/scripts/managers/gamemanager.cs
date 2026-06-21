using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Stats & State")]
    public int maxHealth = 100;
    public int playerHealth = 100;
    public int score = 0;
    public bool isGameOver = false;

    [Header("Level & Progression Systems")]
    public int currentLevel = 1;
    public int currentKillsInLevel = 0;
    public int killsNeededPerLevel = 10;

    // Matches the README exactly: Level 1=10, 2=15, 3=25, 4=30, 5=50
    private readonly int[] killQuotaByLevel = { 10, 15, 25, 30, 50 };

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

    void Start()
    {
        killsNeededPerLevel = GetQuotaForLevel(currentLevel);

        // Initialize UI display values immediately on match start
        if (HorrorUIManager.Instance != null)
        {
            HorrorUIManager.Instance.UpdateHealthText(playerHealth, maxHealth);
        }
    }

    int GetQuotaForLevel(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, killQuotaByLevel.Length - 1);
        return killQuotaByLevel[index];
    }

    /// <summary>
    /// Processes player damage tracking, handles dramatic screen shake/blood splatters, 
    /// and handles death state triggers.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (isGameOver) return;

        playerHealth -= amount;
        Debug.Log($"Player hit! Remaining Health: {playerHealth}");

        // 1. Violent camera jolt when the player gets bit/scratched
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(0.2f, 0.15f);
        }

        // 2. Update the stylized Crimson UI and flash blood vignettes on the player's screen
        if (HorrorUIManager.Instance != null)
        {
            HorrorUIManager.Instance.UpdateHealthText(playerHealth, maxHealth);
            
            float healthPercent = (float)playerHealth / maxHealth;
            HorrorUIManager.Instance.TriggerDamageFlash(healthPercent);
        }

        // 3. Evaluate death thresholds
        if (playerHealth <= 0)
        {
            TriggerGameOver();
        }
    }

    /// <summary>
    /// Adds (or subtracts, if negative) points to the player's runtime score metric.
    /// </summary>
    public void AddScore(int points)
    {
        score += points;
        score = Mathf.Max(score, 0); // never let score go negative
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
        killsNeededPerLevel = GetQuotaForLevel(currentLevel);

        // Restore the player to full health at the start of every new level
        playerHealth = maxHealth;

        // Top off ammo and grenades for the fresh level too
        WeaponManager playerWeapons = FindAnyObjectByType<WeaponManager>();
        if (playerWeapons != null)
        {
            playerWeapons.ResetForNewLevel();
        }

        if (HorrorUIManager.Instance != null)
        {
            HorrorUIManager.Instance.UpdateHealthText(playerHealth, maxHealth);
        }

        Debug.Log($"Level Complete! Progressing to Level: {currentLevel}. Health restored to full.");

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

    /// <summary>
    /// Resets all run state back to a fresh start. Call this from the main menu's
    /// "New Game" / "Play" button before loading Level 1.
    /// </summary>
    public void ResetForNewGame()
    {
        currentLevel = 1;
        currentKillsInLevel = 0;
        killsNeededPerLevel = GetQuotaForLevel(currentLevel);
        playerHealth = maxHealth;
        score = 0;
        isGameOver = false;
    }
}
