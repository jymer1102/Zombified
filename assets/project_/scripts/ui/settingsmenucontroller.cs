using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Connects the settings panel sliders (volume, mouse sensitivity) to the actual
/// systems that use them. Drop this on your Settings panel GameObject, drag in
/// the sliders, and it handles loading current values + saving changes.
/// </summary>
public class SettingsMenuController : MonoBehaviour
{
    [Header("UI References")]
    public Slider volumeSlider;
    public Slider sensitivitySlider;
    public Text volumeValueLabel;     // optional - shows "75%" next to the slider
    public Text sensitivityValueLabel; // optional - shows "2.0" next to the slider

    [Header("Sensitivity Range")]
    [Tooltip("Lowest sensitivity the slider can be set to.")]
    public float minSensitivity = 0.5f;
    [Tooltip("Highest sensitivity the slider can be set to.")]
    public float maxSensitivity = 5.0f;

    private PlayerMovement playerMovement;

    void OnEnable()
    {
        // Find the player fresh each time the panel opens, in case it didn't exist yet at scene load
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }

        LoadCurrentValuesIntoUI();
    }

    /// <summary>
    /// Pulls whatever the current saved/active volume and sensitivity are
    /// and reflects them in the slider positions, so the panel doesn't open
    /// looking reset even though nothing actually changed.
    /// </summary>
    void LoadCurrentValuesIntoUI()
    {
        if (volumeSlider != null)
        {
            float currentVolume = AudioManager.Instance != null ? AudioManager.Instance.GetMasterVolume() : 1.0f;
            volumeSlider.SetValueWithoutNotify(currentVolume);
            UpdateVolumeLabel(currentVolume);
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = minSensitivity;
            sensitivitySlider.maxValue = maxSensitivity;

            float currentSensitivity = playerMovement != null ? playerMovement.mouseSensitivity : 2.0f;
            sensitivitySlider.SetValueWithoutNotify(currentSensitivity);
            UpdateSensitivityLabel(currentSensitivity);
        }
    }

    /// <summary>
    /// Wire this to the volume slider's OnValueChanged event in the Inspector.
    /// </summary>
    public void OnVolumeChanged(float newValue)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(newValue);
        }

        UpdateVolumeLabel(newValue);
    }

    /// <summary>
    /// Wire this to the sensitivity slider's OnValueChanged event in the Inspector.
    /// </summary>
    public void OnSensitivityChanged(float newValue)
    {
        if (playerMovement != null)
        {
            playerMovement.SetMouseSensitivity(newValue);
        }

        UpdateSensitivityLabel(newValue);
    }

    void UpdateVolumeLabel(float value)
    {
        if (volumeValueLabel != null)
        {
            volumeValueLabel.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }
    }

    void UpdateSensitivityLabel(float value)
    {
        if (sensitivityValueLabel != null)
        {
            sensitivityValueLabel.text = value.ToString("F1");
        }
    }

    /// <summary>
    /// Call from a "Reset to Defaults" button if you want one.
    /// </summary>
    public void ResetToDefaults()
    {
        OnVolumeChanged(1.0f);
        OnSensitivityChanged(2.0f);
        LoadCurrentValuesIntoUI();
    }
}
