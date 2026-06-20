using UnityEngine;

public class WeaponVisuals : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swayAmount = 0.02f;
    public float maxSwayAmount = 0.06f;
    public float swaySmoothing = 4f;

    [Header("Recoil Settings")]
    public Vector3 recoilRotation = new Vector3(-5f, 2f, 0f); // Kick up and slightly right
    public float recoilSmoothing = 10f;
    public float returnSmoothing = 5f;

    private Vector3 initialPosition;
    private Quaternion currentRotation;
    private Quaternion targetRotation;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        HandleWeaponSway();
        ApplyRecoilMath();
    }

    void HandleWeaponSway()
    {
        // Capture mouse movement for weapon lag/sway
        float movementX = -Input.GetAxis("Mouse X") * swayAmount;
        float movementY = -Input.GetAxis("Mouse Y") * swayAmount;

        // Clamp it so the gun doesn't fly off the screen
        movementX = Mathf.Clamp(movementX, -maxSwayAmount, maxSwayAmount);
        movementY = Mathf.Clamp(movementY, -maxSwayAmount, maxSwayAmount);

        Vector3 targetPosition = new Vector3(movementX, movementY, 0) + initialPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * swaySmoothing);
    }

    void ApplyRecoilMath()
    {
        // Smoothly snap back to center over time
        targetRotation = Quaternion.Lerp(targetRotation, Quaternion.identity, returnSmoothing * Time.deltaTime);
        currentRotation = Quaternion.Lerp(currentRotation, targetRotation, recoilSmoothing * Time.deltaTime);
        transform.localRotation = currentRotation;
    }

    // Call this public function from PlayerCombat.cs whenever a bullet fires!
    public void TriggerRecoil()
    {
        targetRotation *= Quaternion.Euler(recoilRotation);
    }
}
