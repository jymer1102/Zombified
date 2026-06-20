using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Progress Settings")]
    public int currentLevel = 1;
    public int currentKillsInLevel = 0;

    // Progression rules: Kill 10, 15, 25, 30, 50 zombies
    private readonly int[] killsRequiredPerLevel = { 10, 15, 25, 30, 50 };

    [Header("Game State")]
    private bool isPaused = false;
    private float escPressTimer = 0f;
    private int escPressCount = 0;

    void Awake()
    {
        // Simple Singleton pattern to access the manager easily from other scripts
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

    void Update()
    {
        HandlePauseAndExit();
    }

    // Call this function from your zombie health scripts whenever a zombie dies
    public void RegisterZombieKill()
    {
        currentKillsInLevel++;
        Debug.Log($"Zombie killed! Progress: {currentKillsInLevel}/{GetRequiredKillsForCurrentLevel()}");

        if (currentKillsInLevel >= GetRequiredKillsForCurrentLevel())
        {
            AdvanceLevel();
        }
    }

    int GetRequiredKillsForCurrentLevel()
    {
        // Safe check to avoid index out of bounds
        if (currentLevel >= 1 && currentLevel <= 5)
        {
            return killsRequiredPerLevel[currentLevel - 1];
        }
        return 50; // Default fallback
    }

    void AdvanceLevel()
    {
        if (currentLevel < 5)
        {
            currentLevel++;
            currentKillsInLevel = 0;
            Debug.Log($"LEVEL UP! Welcome to Level {currentLevel}. Prepare yourself.");
            
            // Here you can trigger scene loading or update difficulty modifiers dynamically
            // SceneManager.LoadScene("Level" + currentLevel);
        }
        else
        {
            Debug.Log("CONGRATULATIONS! You beat all 5 levels of ZOMBIFIED!");
        }
    }

    void HandlePauseAndExit()
    {
        // Toggle Escape to Pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            escPressCount++;
            escPressTimer = 0.4f; // Time window to register a double press

            if (escPressCount >= 2)
            {
                ExitFullscreenOrGame();
            }
            else
            {
                TogglePause();
            }
        }

        // Reset the escape press counter if time runs out
        if (escPressTimer > 0)
        {
            escPressTimer -= Time.deltaTime;
            if (escPressTimer <= 0)
            {
                escPressCount = 0;
            }
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // Freezes the game simulation
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Game Paused");
        }
        else
        {
            Time.timeScale = 1f; // Unfreezes the game
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("Game Resumed");
        }
    }

    void ExitFullscreenOrGame()
    {
        Debug.Log("Escape pressed twice. Exiting fullscreen/game.");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // If running as a standalone build, revert fullscreen mode or quit application
            if (Screen.fullScreen)
            {
                Screen.fullScreen = false;
            }
            else
            {
                Application.Quit();
            }
        #endif
    }
}
