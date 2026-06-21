using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [Header("ADS Positions")]
    public Transform cameraTransform;
    public Vector3 hipfirePosition = new Vector3(0f, 0.8f, 0f);
    public Vector3 adsPosition = new Vector3(0f, 0.65f, 0.2f);
    public float adsSpeed = 12f;

    [Header("Camera Juiciness - Headbob")]
    public float bobFrequency = 5f;
    public float bobGain = 0.05f;
    [Tooltip("Headbob frequency multiplier while sprinting - faster bob feels faster.")]
    public float sprintBobMultiplier = 1.6f;
    [Tooltip("Headbob frequency multiplier while crouched - slower, heavier feel.")]
    public float crouchBobMultiplier = 0.6f;

    [Header("Idle Breathing Sway")]
    [Tooltip("Subtle constant drift even when standing still - sells the 'someone is holding this' feeling.")]
    public float breathingSwayAmount = 0.008f;
    public float breathingSwaySpeed = 1.2f;

    [Header("Mouse Look Sway")]
    [Tooltip("How much the camera/weapon lags behind sharp mouse movements - adds weight.")]
    public float lookSwayAmount = 0.02f;
    public float lookSwaySmoothing = 6f;

    [Header("FOV Kick")]
    public float baseFOV = 75f;
    public float sprintFOVBoost = 8f;
    public float fovTransitionSpeed = 8f;

    private PlayerMovement movement;
    private CharacterController controller;
    private Camera cam;
    private float bobTimer = 0f;
    private float breathTimer = 0f;
    private Vector3 basePosition; // The "clean" position before shake is added on top
    private Vector2 lookSwayCurrent;
    private Vector2 lastMouseDelta;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();

        if (cam != null) cam.fieldOfView = baseFOV;
    }

    void Update()
    {
        HandleCameraPosition();
        HandleLookSway();
        HandleFOV();
    }

    void HandleCameraPosition()
    {
        if (cameraTransform == null) return;

        // 1. Position Interpolation for ADS (Right Click)
        Vector3 targetPosition = Input.GetMouseButton(1) ? adsPosition : hipfirePosition;

        // 2. Add Headbob if the player is moving on the ground, scaled by current stance
        bool isMoving = controller.isGrounded && controller.velocity.magnitude > 0.1f;
        if (isMoving)
        {
            float frequencyMultiplier = 1f;
            if (movement != null)
            {
                if (movement.IsSprinting) frequencyMultiplier = sprintBobMultiplier;
                else if (movement.IsCrouching) frequencyMultiplier = crouchBobMultiplier;
            }

            bobTimer += Time.deltaTime * bobFrequency * frequencyMultiplier;
            targetPosition.y += Mathf.Sin(bobTimer) * bobGain;
            targetPosition.x += Mathf.Cos(bobTimer * 0.5f) * (bobGain * 0.5f);
        }
        else
        {
            bobTimer = 0f;

            // 3. Idle breathing sway - only applies when NOT moving, so it doesn't fight headbob
            breathTimer += Time.deltaTime * breathingSwaySpeed;
            targetPosition.y += Mathf.Sin(breathTimer) * breathingSwayAmount;
            targetPosition.x += Mathf.Cos(breathTimer * 0.7f) * breathingSwayAmount;
        }

        // Smoothly slide toward the target. This is the "clean" base position,
        // calculated BEFORE any shake offset gets added on top of it.
        basePosition = Vector3.Lerp(basePosition, targetPosition, adsSpeed * Time.deltaTime);

        // Let CameraShake read this base position and add its offset on top,
        // instead of both scripts fighting to directly set localPosition.
        Vector3 shakeOffset = CameraShake.Instance != null ? CameraShake.Instance.GetCurrentOffset() : Vector3.zero;
        cameraTransform.localPosition = basePosition + shakeOffset;
    }

    /// <summary>
    /// Adds a small rotational lag when whipping the mouse around fast - sells weight
    /// without fighting PlayerMovement's actual look rotation.
    /// </summary>
    void HandleLookSway()
    {
        if (cameraTransform == null) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector2 targetSway = new Vector2(-mouseY, mouseX) * lookSwayAmount;
        lookSwayCurrent = Vector2.Lerp(lookSwayCurrent, targetSway, Time.deltaTime * lookSwaySmoothing);

        cameraTransform.localRotation = Quaternion.Euler(lookSwayCurrent.x, lookSwayCurrent.y, 0f) * Quaternion.identity;
    }

    /// <summary>
    /// Widens the FOV slightly while sprinting for a sense of speed, narrows back on stop.
    /// </summary>
    void HandleFOV()
    {
        if (cam == null) return;

        float targetFOV = baseFOV;
        if (movement != null && movement.IsSprinting)
        {
            targetFOV = baseFOV + sprintFOVBoost;
        }

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
    }
}
