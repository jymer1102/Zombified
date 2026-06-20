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
    /// Triggers the level loader to launch Level 1.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("Starting ZOMBIFIED... Initializing Level 1 Map.");
        
        if (LevelSceneManager.Instance != null)
        {
            // Start the player on Level 1
            LevelSceneManager.Instance.LoadMapForLevel(1);
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
