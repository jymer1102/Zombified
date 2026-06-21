using UnityEngine;

/// <summary>
/// Handles pausing/unpausing the game via Escape, freezing time and unlocking the cursor
/// so the player can interact with a pause panel. Per the README: "hold or press twice to exit fullscreen"
/// is a Unity Editor/build quirk, not something we control in code - this just handles the actual pause logic.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    [Header("Pause UI")]
    public GameObject pausePanel;
    public GameObject settingsPanel; // optional sub-panel reached from the pause menu

    public bool IsPaused { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        IsPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void Update()
    {
        // Don't allow pausing once the game is already over (avoids a confusing double-state)
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (IsPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null) pausePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Game paused.");
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Game resumed.");
    }

    /// <summary>
    /// Call from a "Quit to Menu" button on the pause panel.
    /// </summary>
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f; // always reset timescale before changing scenes, or the menu will be frozen too
        IsPaused = false;

        if (LevelSceneManager.Instance != null)
        {
            LevelSceneManager.Instance.LoadMainMenu();
        }
    }
}
