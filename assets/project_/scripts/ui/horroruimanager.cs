using UnityEngine;
using UnityEngine.UI;

public class HorrorUIManager : MonoBehaviour
{
    public static HorrorUIManager Instance { get; private set; }

    [Header("Screen Splatter Overlays")]
    public Image bloodVignetteOverlay; // UI Image covering the screen with an alpha edge-blood texture
    public float flashDuration = 0.5f;
    private float vignetteTargetAlpha = 0f;
    private float damageFlashAlpha = 0f;

    [Header("Darkness Vignette")]
    [Tooltip("A separate dark/black vignette overlay that thickens when the area around the player is poorly lit.")]
    public Image darknessVignetteOverlay;
    [Tooltip("Max alpha of the darkness vignette when the player is in total darkness (light level = 0).")]
    public float maxDarknessAlpha = 0.55f;
    [Tooltip("How quickly the darkness vignette responds to light level changes.")]
    public float darknessTransitionSpeed = 2f;

    [Header("Stylized Horror Text Elements")]
    public Text healthDisplayLabel;
    public Text waveStatusLabel;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        UpdateDamageVignette();
        UpdateDarknessVignette();
    }

    void UpdateDamageVignette()
    {
        if (bloodVignetteOverlay == null) return;

        // Smoothly fade health vignette down over time so the screen doesn't stay permanently blocked
        damageFlashAlpha = Mathf.MoveTowards(damageFlashAlpha, vignetteTargetAlpha, Time.deltaTime / flashDuration);
        SetImageAlpha(bloodVignetteOverlay, damageFlashAlpha);
    }

    void UpdateDarknessVignette()
    {
        if (darknessVignetteOverlay == null) return;

        // Default to "fully lit" (no extra vignette) if the detector doesn't exist in this scene
        float lightLevel = AmbientLightDetector.Instance != null ? AmbientLightDetector.Instance.GetLightLevel() : 1f;

        float targetAlpha = Mathf.Lerp(maxDarknessAlpha, 0f, lightLevel);
        Color current = darknessVignetteOverlay.color;
        current.a = Mathf.Lerp(current.a, targetAlpha, Time.deltaTime * darknessTransitionSpeed);
        darknessVignetteOverlay.color = current;
    }

    /// <summary>
    /// Flashes full screen-space stylized blood graphics onto the camera lens when damaged.
    /// </summary>
    public void TriggerDamageFlash(float playerHealthPercent)
    {
        if (bloodVignetteOverlay == null) return;

        // Instantly jump opacity based on severity of health drop
        damageFlashAlpha = Mathf.Clamp(1f - playerHealthPercent, 0.3f, 0.9f);
        SetImageAlpha(bloodVignetteOverlay, damageFlashAlpha);

        // Let it slowly fade down toward a permanent warning glow if health is critically low
        vignetteTargetAlpha = playerHealthPercent < 0.3f ? 0.25f : 0f;
    }

    /// <summary>
    /// Renders UI layout string data using custom typographic structural styling rules.
    /// Takes both current and max health so it always shows an accurate percentage,
    /// even if maxHealth ever changes from the default 100.
    /// </summary>
    public void UpdateHealthText(int currentHealth, int maxHealth)
    {
        if (healthDisplayLabel == null) return;

        int healthPercent = maxHealth > 0 ? Mathf.RoundToInt(((float)currentHealth / maxHealth) * 100f) : 0;
        healthPercent = Mathf.Clamp(healthPercent, 0, 100);

        // Formats string with rich-text tag formatting directly supported by Unity text engines
        // Uses hex coloring for dark crimson text styling
        healthDisplayLabel.text = $"<color=#8B0000><size=34>VITALS:</size></color> <color=#FF0000><b>{healthPercent}%</b></color>";
    }

    void SetImageAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
