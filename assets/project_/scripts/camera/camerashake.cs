using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private float shakeTimer = 0f;
    private float shakeAmount = 0f;
    private Vector3 currentOffset = Vector3.zero;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            // Generate a random offset inside a sphere to simulate the shaking effect.
            // This is now just a value other scripts (CameraEffects) read and add on top
            // of their own position - it no longer touches transform.localPosition directly.
            currentOffset = Random.insideUnitSphere * shakeAmount;
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            shakeTimer = 0f;
            currentOffset = Vector3.zero;
        }
    }

    /// <summary>
    /// Triggers a screen shake effect.
    /// </summary>
    /// <param name="duration">How long the shake lasts (seconds).</param>
    /// <param name="amount">How violent the shake is.</param>
    public void TriggerShake(float duration, float amount)
    {
        // If a new shake is stronger or longer than what's currently running, use the new one.
        // Otherwise keep the existing shake going instead of cutting it short.
        if (duration > shakeTimer) shakeTimer = duration;
        if (amount > shakeAmount) shakeAmount = amount;
    }

    /// <summary>
    /// Returns the current shake offset so other position-driving scripts (like CameraEffects)
    /// can add it on top of their own calculated position, instead of both scripts
    /// fighting over transform.localPosition directly.
    /// </summary>
    public Vector3 GetCurrentOffset()
    {
        return currentOffset;
    }
}
