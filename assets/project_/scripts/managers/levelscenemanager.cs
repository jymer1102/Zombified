using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSceneManager : MonoBehaviour
{
    public static LevelSceneManager Instance { get; private set; }

    [Header("Scene Naming Conventions")]
    [Tooltip("Ensure your scenes in Build Settings match these names exactly (e.g., Level1, Level2, etc.)")]
    public string sceneNamePrefix = "Level";

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
    /// Loads the corresponding 3D map scene for the current level.
    /// </summary>
    public void LoadMapForLevel(int levelNumber)
    {
        string targetSceneName = sceneNamePrefix + levelNumber;
        
        // Check if scene exists in build settings before loading
        if (Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            Debug.Log($"Loading Map: {targetSceneName}");
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"Scene '{targetSceneName}' cannot be loaded. Ensure it is added to File > Build Settings.");
        }
    }
}
