using UnityEngine;

public class WeaponVisuals : MonoBehaviour
{
    [Header("Recoil Settings")]
    public float recoilAmount = 0.1f;
    public float recoverySpeed = 5f;

    private Vector3 originalPosition;
    private Vector3 targetPosition;

    void Start()
    {
        // Cache the default local placement of the weapon model on the screen
        originalPosition = transform.localPosition;
        targetPosition = originalPosition;
    }

    void Update()
    {
        // Smoothly snap back to the resting position over time after kicking back
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * recoverySpeed);
        targetPosition = Vector3.Lerp(targetPosition, originalPosition, Time.deltaTime * recoverySpeed);
    }

    /// <summary>
    /// Jolt the weapon model backward along the Z-axis to simulate physical kickback recoil.
    /// </summary>
    public void TriggerRecoil()
    {
        targetPosition -= Vector3.forward * recoilAmount;
    }
}
