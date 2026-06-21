using UnityEngine;

/// <summary>
/// Makes a Light flicker like a dying bulb or unstable power source - classic horror
/// atmosphere prop. Drop this on any Light component: hallway lights, emergency lights,
/// the player's headlamp if you want it to occasionally sputter, etc.
/// </summary>
[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
    public enum FlickerStyle { Subtle, Unstable, Dying, Strobe }

    [Header("Flicker Style")]
    public FlickerStyle style = FlickerStyle.Unstable;

    [Header("Intensity Range")]
    public float minIntensity = 0.3f;
    public float maxIntensity = 1.2f;

    [Header("Timing")]
    [Tooltip("Base speed of flicker changes. Higher = faster flickering.")]
    public float flickerSpeed = 8f;
    [Tooltip("Chance per check that the light briefly cuts out completely (Dying/Unstable styles only).")]
    public float blackoutChance = 0.02f;
    public float blackoutDuration = 0.15f;

    [Header("Sound (optional)")]
    public AudioClip flickerBuzzClip;
    [Tooltip("Seconds between possible buzz sound triggers, so it doesn't spam.")]
    public float buzzCooldown = 4f;
    private float buzzTimer = 0f;

    private Light lightSource;
    private float baseIntensity;
    private float noiseOffset;
    private float blackoutTimer = 0f;

    void Start()
    {
        lightSource = GetComponent<Light>();
        baseIntensity = lightSource.intensity;
        noiseOffset = Random.Range(0f, 100f); // so multiple lights don't flicker in perfect sync
    }

    void Update()
    {
        if (buzzTimer > 0f) buzzTimer -= Time.deltaTime;

        if (blackoutTimer > 0f)
        {
            blackoutTimer -= Time.deltaTime;
            lightSource.intensity = 0f;
            return;
        }

        switch (style)
        {
            case FlickerStyle.Subtle:
                HandleSubtleFlicker();
                break;
            case FlickerStyle.Unstable:
                HandleUnstableFlicker();
                break;
            case FlickerStyle.Dying:
                HandleDyingFlicker();
                break;
            case FlickerStyle.Strobe:
                HandleStrobeFlicker();
                break;
        }
    }

    void HandleSubtleFlicker()
    {
        // Gentle, slow wavering - good for ambient room lighting that just feels "off"
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed * 0.3f, noiseOffset);
        lightSource.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise * 0.3f + 0.7f);
    }

    void HandleUnstableFlicker()
    {
        // Erratic flickering with occasional full blackouts - the classic "haunted hallway" light
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseOffset);
        lightSource.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

        if (Random.value < blackoutChance * Time.deltaTime * 60f)
        {
            TriggerBlackout();
        }
    }

    void HandleDyingFlicker()
    {
        // Mostly dim/off, with rare brief flickers up to full brightness - a bulb about to give out
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed * 1.5f, noiseOffset);

        if (noise > 0.85f)
        {
            lightSource.intensity = maxIntensity;
        }
        else
        {
            lightSource.intensity = minIntensity * 0.3f;
        }

        if (Random.value < blackoutChance * 1.5f * Time.deltaTime * 60f)
        {
            TriggerBlackout();
        }
    }

    void HandleStrobeFlicker()
    {
        // Sharp on/off square wave - good for alarm lights, broken fluorescents
        bool on = Mathf.Sin(Time.time * flickerSpeed) > 0f;
        lightSource.intensity = on ? maxIntensity : minIntensity;
    }

    void TriggerBlackout()
    {
        blackoutTimer = blackoutDuration;

        if (buzzTimer <= 0f && AudioManager.Instance != null && flickerBuzzClip != null)
        {
            AudioManager.Instance.Play3DSFX(flickerBuzzClip, transform.position, 0.4f);
            buzzTimer = buzzCooldown;
        }
    }
}
