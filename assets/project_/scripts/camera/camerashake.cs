using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 originalPos;
    private float shakeTimer = 0f;
    private float shakeAmount = 0f;
    private float decreaseFactor = 1.0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void OnEnable()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            // Generate a random position offset inside a sphere to simulate the shaking effect
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
            
            // Gradually reduce the shake time remaining
            shakeTimer -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            shakeTimer = 0f;
            transform.localPosition = originalPos;
        }
    }

    /// <summary>
    /// Triggers a screen shake effect.
    /// </summary>
    /// <param name="duration">How long the shake lasts (seconds).</param>
    /// <param name="amount">How violent the shake is.</param>
    public void TriggerShake(float duration, float amount)
    {
        shakeTimer = duration;
        shakeAmount = amount;
    }
}
