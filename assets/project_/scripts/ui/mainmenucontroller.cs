using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Screen Panels")]
    public GameObject mainTitlePanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    void Start()
    {
        // Ensure only the main title screen is active when the game boots up
        ShowMainTitleScreen();
    }

    /// <summary>
    /// Activates the main menu screen and hides sub-menus.
    /// </summary>
    public void ShowMainTitleScreen()
    {
        if (mainTitlePanel != null) mainTitlePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    /// <summary>
    /// Opens the settings menu overlay.
    /// </summary>
    public void OpenSettingsMenu()
    {
        if (mainTitlePanel != null) mainTitlePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    /// <summary>
    /// Opens the game credits screen.
    /// </summary>
    public void OpenCreditsMenu()
    {
        if (mainTitlePanel != null) mainTitlePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    /// <summary>
    /// Starts a brand new run from Level 1, wiping any in-progress stats
    /// (health, score, kill counts) back to defaults first.
    /// </summary>
    public void StartNewGame()
    {
        Debug.Log("Starting ZOMBIFIED... Initializing Level 1 Map.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetForNewGame();
        }

        if (LevelSceneManager.Instance != null)
        {
            LevelSceneManager.Instance.LoadMapForLevel(1);
        }
        else
        {
            Debug.LogError("LevelSceneManager instance missing. Cannot load map.");
        }
    }

    /// <summary>
    /// Continues from the player's highest previously-unlocked level,
    /// using the saved progress from SaveSystem. Falls back to a new game
    /// if no save data exists yet.
    /// </summary>
    public void ContinueGame()
    {
        int highestLevel = 1;

        if (SaveSystem.Instance != null)
        {
            highestLevel = SaveSystem.Instance.LoadHighestLevel();
        }

        Debug.Log($"Continuing ZOMBIFIED... Loading Level {highestLevel}.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetForNewGame();
            GameManager.Instance.currentLevel = highestLevel;
        }

        if (LevelSceneManager.Instance != null)
        {
            LevelSceneManager.Instance.LoadMapForLevel(highestLevel);
        }
        else
        {
            Debug.LogError("LevelSceneManager instance missing. Cannot load map.");
        }
    }

    /// <summary>
    /// Closes the application executable.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Exiting application data pipeline.");
        Application.Quit();
    }
}
