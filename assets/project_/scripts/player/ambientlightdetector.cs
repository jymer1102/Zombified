using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Estimates how "lit up" the player currently is by sampling nearby Light components
/// and weighting them by distance, intensity, and range. This drives darkness-reactive
/// UI (heavier vignette in the dark) without needing real light probes baked into the scene.
///
/// SETUP: every Light you want this system to be aware of needs the "DynamicLight" tag
/// added to its GameObject (or its parent). Static baked lighting won't be picked up -
/// this only tracks actual Light components, which is exactly what you want for flashlights,
/// flickering bulbs, explosions, etc.
/// </summary>
public class AmbientLightDetector : MonoBehaviour
{
    public static AmbientLightDetector Instance { get; private set; }

    [Header("Detection Settings")]
    [Tooltip("How far around the player to look for light sources.")]
    public float detectionRadius = 20f;
    [Tooltip("How often (seconds) to rescan for nearby lights. Doesn't need to be every frame.")]
    public float rescanInterval = 0.5f;

    [Header("Result Smoothing")]
    [Tooltip("How quickly the light level value catches up to the real value - prevents flicker-driven jitter.")]
    public float smoothingSpeed = 3f;

    private List<Light> nearbyLights = new List<Light>();
    private float rescanTimer = 0f;
    private float currentLightLevel = 0f; // 0 = pitch black, 1 = fully lit
    private float smoothedLightLevel = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        rescanTimer -= Time.deltaTime;
        if (rescanTimer <= 0f)
        {
            RescanNearbyLights();
            rescanTimer = rescanInterval;
        }

        currentLightLevel = CalculateCurrentLightLevel();
        smoothedLightLevel = Mathf.Lerp(smoothedLightLevel, currentLightLevel, Time.deltaTime * smoothingSpeed);
    }

    /// <summary>
    /// Finds all active lights within range and caches them. Run periodically rather
    /// than every frame since lights don't usually teleport around the map.
    /// </summary>
    void RescanNearbyLights()
    {
        nearbyLights.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("DynamicLight")) continue;

            Light light = hit.GetComponent<Light>();
            if (light == null) light = hit.GetComponentInParent<Light>();

            if (light != null && light.enabled && !nearbyLights.Contains(light))
            {
                nearbyLights.Add(light);
            }
        }

        // Also always check the player's own headlamp, even if it's not tagged/collider-based
        HeadlampController headlamp = GetComponentInChildren<HeadlampController>();
        if (headlamp != null && headlamp.IsHeadlampActive())
        {
            Light headlampLight = headlamp.GetComponent<Light>();
            if (headlampLight == null) headlampLight = headlamp.GetComponentInChildren<Light>();
            if (headlampLight != null && !nearbyLights.Contains(headlampLight))
            {
                nearbyLights.Add(headlampLight);
            }
        }
    }

    /// <summary>
    /// Sums up weighted contribution from every nearby light: closer + brighter + larger range = more light.
    /// Returns a 0-1 value representing how lit the player currently is.
    /// </summary>
    float CalculateCurrentLightLevel()
    {
        float totalContribution = 0f;

        foreach (Light light in nearbyLights)
        {
            if (light == null || !light.enabled) continue;

            float distance = Vector3.Distance(transform.position, light.transform.position);
            float effectiveRange = light.range > 0f ? light.range : detectionRadius;

            if (distance > effectiveRange) continue;

            // Falloff: 1.0 right next to the light, fading to 0 at its range limit
            float distanceFactor = 1f - Mathf.Clamp01(distance / effectiveRange);

            // Weight by the light's actual intensity so a dim bulb contributes less than a bright one
            float weightedContribution = distanceFactor * Mathf.Clamp01(light.intensity / 2f);

            totalContribution += weightedContribution;
        }

        return Mathf.Clamp01(totalContribution);
    }

    /// <summary>
    /// Public read for other systems (like HorrorUIManager) - smoothed so flickering
    /// lights don't make the UI jitter frame to frame.
    /// </summary>
    public float GetLightLevel()
    {
        return smoothedLightLevel;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
