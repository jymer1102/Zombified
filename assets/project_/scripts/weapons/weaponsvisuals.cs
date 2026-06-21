using UnityEngine;

public class WeaponVisuals : MonoBehaviour
{
    [Header("Recoil Physics")]
    [Tooltip("How far back the gun kicks along the Z-axis when fired.")]
    public float recoilKickAmount = 0.15f;
    
    [Tooltip("The speed at which the gun snaps back to its original position.")]
    public float recoilRecoverySpeed = 12f;

    private Vector3 originalLocalPosition;

    void Start()
    {
        // Cache the exact resting position of the gun on the camera layout
        originalLocalPosition = transform.localPosition;
    }

    void Update()
    {
        // Smoothly interpolate the weapon back to its resting point
        transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPosition, Time.deltaTime * recoilRecoverySpeed);
    }

    /// <summary>
    /// Instantly jolts the gun model backward to simulate crisp mechanical kickback.
    /// </summary>
    public void TriggerRecoil()
    {
        // Push the weapon model straight back along the local Z-axis
        transform.localPosition -= Vector3.forward * recoilKickAmount;
    }
}
