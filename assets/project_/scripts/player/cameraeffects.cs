using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [Header("ADS Positions")]
    public Transform cameraTransform;
    public Vector3 hipfirePosition = new Vector3(0f, 0.8f, 0f);
    public Vector3 adsPosition = new Vector3(0f, 0.65f, 0.2f);
    public float adsSpeed = 12f;

    [Header("Camera Juiciness")]
    public float bobFrequency = 5f;
    public float bobGain = 0.05f;

    private PlayerMovement movement;
    private CharacterController controller;
    private float bobTimer = 0f;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleCameraPosition();
    }

    void HandleCameraPosition()
    {
        if (cameraTransform == null) return;

        // 1. Position Interpolation for ADS (Right Click)
        Vector3 targetPosition = Input.GetMouseButton(1) ? adsPosition : hipfirePosition;
        
        // 2. Add Headbob if the player is moving on the ground
        if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            targetPosition.y += Mathf.Sin(bobTimer) * bobGain;
        }
        else
        {
            bobTimer = 0f;
        }

        // Smoothly slide the camera into place
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, targetPosition, adsSpeed * Time.deltaTime);
    }
}
