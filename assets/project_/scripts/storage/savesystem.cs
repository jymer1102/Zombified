using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    // Keys used to save and look up data in local storage
    private const string HIGH_SCORE_KEY = "HighestLevelUnlocked";
    private const string VOLUME_KEY = "GameVolume";
    private const string SENSITIVITY_KEY = "MouseSensitivity";

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
    /// Saves the player's progression data.
    /// </summary>
    public void SaveGameProgress(int highestLevel)
    {
        // Only overwrite if the new level reached is higher than previous saves
        int currentSavedLevel = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 1);
        if (highestLevel > currentSavedLevel)
        {
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highestLevel);
            PlayerPrefs.Save();
            Debug.Log($"Progress Saved! Highest Level Unlocked: {highestLevel}");
        }
    }

    /// <summary>
    /// Returns the highest level the player has unlocked. Defaults to 1.
    /// </summary>
    public int LoadHighestLevel()
    {
        return PlayerPrefs.GetInt(HIGH_SCORE_KEY, 1);
    }

    /// <summary>
    /// Saves custom settings menu options.
    /// </summary>
    public void SaveSettings(float volume, float sensitivity)
    {
        PlayerPrefs.SetFloat(VOLUME_KEY, volume);
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, sensitivity);
        PlayerPrefs.Save();
        Debug.Log("Game settings successfully updated and saved.");
    }

    /// <summary>
    /// Loads the stored volume level. Defaults to max volume (1.0).
    /// </summary>
    public float LoadVolume()
    {
        return PlayerPrefs.GetFloat(VOLUME_KEY, 1.0f);
    }

    /// <summary>
    /// Loads the stored mouse sensitivity. Defaults to standard look speed (2.0).
    /// </summary>
    public float LoadSensitivity()
    {
        return PlayerPrefs.GetFloat(SENSITIVITY_KEY, 2.0f);
    }

    /// <summary>
    /// Fully clears out all player saves and resets data to default.
    /// </summary>
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("All game save files have been successfully wiped.");
    }
}
