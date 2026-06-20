using UnityEngine;
using UnityEngine.UI;

public class HorrorUIManager : MonoBehaviour
{
    public static HorrorUIManager Instance { get; private set; }

    [Header("Screen Splatter Overlays")]
    public Image bloodVignetteOverlay; // UI Image covering the screen with an alpha edge-blood texture
    public float flashDuration = 0.5f;
    private float vignetteTargetAlpha = 0f;

    [Header("Stylized Horror Text Elements")]
    public Text healthDisplayLabel;
    public Text waveStatusLabel;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        // Smoothly fade health vignette down over time so the screen doesn't stay permanently blocked
        if (bloodVignetteOverlay != null)
        {
            Color curColor = bloodVignetteOverlay.color;
            curColor.a = Mathf.MoveTowards(curColor.a, vignetteTargetAlpha, Time.deltaTime / flashDuration);
            bloodVignetteOverlay.color = curColor;
        }
    }

    /// <summary>
    /// Flashes full screen-space stylized blood graphics onto the camera lens when damaged.
    /// </summary>
    public void TriggerDamageFlash(float playerHealthPercent)
    {
        if (bloodVignetteOverlay == null) return;

        // Instantly jump opacity based on severity of health drop
        Color curColor = bloodVignetteOverlay.color;
        curColor.a = Mathf.Clamp(1f - playerHealthPercent, 0.3f, 0.9f); 
        bloodVignetteOverlay.color = curColor;

        // Let it slowly fade down toward a permanent warning glow if health is critically low
        vignetteTargetAlpha = playerHealthPercent < 0.3f ? 0.25f : 0f;
    }

    /// <summary>
    /// Renders UI layout string data using custom typographic structural styling rules.
    /// </summary>
    public void UpdateHealthText(int currentHealth)
    {
        if (healthDisplayLabel == null) return;
        
        // Formats string with rich-text tag formatting directly supported by Unity text engines
        // Uses hex coloring for dark crimson text styling
        healthDisplayLabel.text = $"<color=#8B0000><size=34>VITALS:</size></color> <color=#FF0000><b>{currentHealth}%</b></color>";
    }
}
