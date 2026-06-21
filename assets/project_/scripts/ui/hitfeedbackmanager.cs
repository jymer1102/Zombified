using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Handles all the "did I hit something" feedback: a flashing hitmarker on crosshair hit,
/// floating damage numbers above the target, and a heartbeat audio/visual cue when low on health.
/// Drop this on the same Canvas as the rest of your HUD.
/// </summary>
public class HitFeedbackManager : MonoBehaviour
{
    public static HitFeedbackManager Instance { get; private set; }

    [Header("Hitmarker")]
    public Image hitmarkerImage;
    public float hitmarkerDuration = 0.15f;
    public Color normalHitColor = Color.white;
    public Color killHitColor = Color.red;
    private float hitmarkerTimer = 0f;

    [Header("Floating Damage Numbers")]
    public GameObject damageNumberPrefab; // A small world-space Text/TextMeshPro prefab
    public Canvas worldSpaceCanvas;        // Optional - if using world-space numbers
    public float damageNumberLifetime = 0.8f;
    public float damageNumberFloatSpeed = 1.2f;

    [Header("Low Health Heartbeat")]
    [Tooltip("Health percentage (0-1) below which the heartbeat effect kicks in.")]
    public float lowHealthThreshold = 0.3f;
    public Image lowHealthPulseOverlay; // A full-screen red-tinted Image, alpha pulses
    private float heartbeatTimer = 0f;
    private bool heartbeatPlaying = false;

    private List<FloatingDamageNumber> activeNumbers = new List<FloatingDamageNumber>();

    private class FloatingDamageNumber
    {
        public GameObject obj;
        public float timer;
        public Vector3 startPos;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (hitmarkerImage != null) SetImageAlpha(hitmarkerImage, 0f);
        if (lowHealthPulseOverlay != null) SetImageAlpha(lowHealthPulseOverlay, 0f);
    }

    void Update()
    {
        UpdateHitmarker();
        UpdateFloatingNumbers();
        UpdateHeartbeat();
    }

    void UpdateHitmarker()
    {
        if (hitmarkerImage == null) return;

        if (hitmarkerTimer > 0f)
        {
            hitmarkerTimer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(hitmarkerTimer / hitmarkerDuration);
            SetImageAlpha(hitmarkerImage, alpha);
        }
    }

    void UpdateFloatingNumbers()
    {
        for (int i = activeNumbers.Count - 1; i >= 0; i--)
        {
            FloatingDamageNumber num = activeNumbers[i];
            if (num.obj == null) { activeNumbers.RemoveAt(i); continue; }

            num.timer -= Time.deltaTime;
            num.obj.transform.position += Vector3.up * damageNumberFloatSpeed * Time.deltaTime;

            Text txt = num.obj.GetComponent<Text>();
            if (txt != null)
            {
                Color c = txt.color;
                c.a = Mathf.Clamp01(num.timer / damageNumberLifetime);
                txt.color = c;
            }

            if (num.timer <= 0f)
            {
                Destroy(num.obj);
                activeNumbers.RemoveAt(i);
            }
        }
    }

    void UpdateHeartbeat()
    {
        if (lowHealthPulseOverlay == null || GameManager.Instance == null) return;

        float healthPercent = (float)GameManager.Instance.playerHealth / GameManager.Instance.maxHealth;
        bool shouldPulse = healthPercent > 0f && healthPercent <= lowHealthThreshold;

        if (shouldPulse)
        {
            // Faster heartbeat the lower your health gets
            float pulseSpeed = Mathf.Lerp(4f, 9f, 1f - (healthPercent / lowHealthThreshold));
            heartbeatTimer += Time.deltaTime * pulseSpeed;

            float pulseAlpha = (Mathf.Sin(heartbeatTimer) * 0.5f + 0.5f) * 0.4f;
            SetImageAlpha(lowHealthPulseOverlay, pulseAlpha);

            if (!heartbeatPlaying)
            {
                heartbeatPlaying = true;
                PlayHeartbeatLoop();
            }
        }
        else
        {
            SetImageAlpha(lowHealthPulseOverlay, 0f);
            heartbeatPlaying = false;
        }
    }

    void PlayHeartbeatLoop()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.lowHealthHeartbeatClip != null)
        {
            AudioManager.Instance.Play2DSFX(AudioManager.Instance.lowHealthHeartbeatClip);
        }
    }

    /// <summary>
    /// Call this from PlayerCombat whenever a shot lands. Flashes the hitmarker,
    /// red if it was a killing blow, white otherwise, and plays the hit sound.
    /// </summary>
    public void ShowHitmarker(bool wasKill)
    {
        hitmarkerTimer = hitmarkerDuration;

        if (hitmarkerImage != null)
        {
            hitmarkerImage.color = wasKill ? killHitColor : normalHitColor;
        }

        if (AudioManager.Instance != null && AudioManager.Instance.hitMarkerClip != null)
        {
            AudioManager.Instance.Play2DSFX(AudioManager.Instance.hitMarkerClip, 0.5f);
        }
    }

    /// <summary>
    /// Spawns a floating damage number at a world position. Call from PlayerCombat
    /// right after a successful hit on a damageable target.
    /// </summary>
    public void SpawnDamageNumber(Vector3 worldPosition, float damageAmount)
    {
        if (damageNumberPrefab == null || worldSpaceCanvas == null) return;

        GameObject numObj = Instantiate(damageNumberPrefab, worldSpaceCanvas.transform);
        numObj.transform.position = worldPosition + Vector3.up * 0.5f;

        Text txt = numObj.GetComponent<Text>();
        if (txt != null)
        {
            txt.text = Mathf.RoundToInt(damageAmount).ToString();
        }

        activeNumbers.Add(new FloatingDamageNumber
        {
            obj = numObj,
            timer = damageNumberLifetime,
            startPos = worldPosition
        });
    }

    void SetImageAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
