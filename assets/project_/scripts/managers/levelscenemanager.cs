using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSceneManager : MonoBehaviour
{
    public static LevelSceneManager Instance { get; private set; }

    [Header("Scene Configuration Names")]
    public string mainMenuSceneName = "MainMenu";
    public string mapScenePrefix = "Map_Level_";

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
    /// Commands the engine to load a specific map index based on current progression.
    /// </summary>
    public void LoadMapForLevel(int levelIndex)
    {
        // Prevents going beyond your 5 designed game levels
        if (levelIndex > 5)
        {
            Debug.Log("Campaign complete! Returning player back to Main Menu screen.");
            LoadMainMenu();
            return;
        }

        string sceneToLoad = mapScenePrefix + levelIndex;
        Debug.Log($"Initiating loading process for scene pipeline: {sceneToLoad}");
        
        SceneManager.LoadScene(sceneToLoad);
    }

    /// <summary>
    /// Explicitly forces a direct scene jump sequence back to the primary title menu.
    /// </summary>
    public void LoadMainMenu()
    {
        Debug.Log($"Loading menu layout target: {mainMenuSceneName}");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
