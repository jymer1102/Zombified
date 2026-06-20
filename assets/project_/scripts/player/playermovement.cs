using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;

    [Header("Movement Constants")]
    public float walkSpeed = 5.0f;
    public float sprintSpeed = 8.5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 25f;  // Depletion per second while sprinting
    public float staminaRegenRate = 15f;  // Recovery per second while resting
    private float currentStamina;

    [Header("State Tracking")]
    public float mouseSensitivity = 2.0f;
    private float xRotation = 0f;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isSprinting;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentStamina = maxStamina;

        // Locks the mouse cursor to the center of the screen for absolute control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleStamina();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the camera up and down (clamped so you can't flip upside down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        Camera mainCam = GetComponentInChildren<Camera>();
        if (mainCam != null)
        {
            mainCam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // Rotate the whole player body left and right
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        // Check if the character controller is touching the floor
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Slight reset clamp to stick cleanly to slopes
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Determine if player wants to sprint and has stamina to do so
        bool isMovingForward = z > 0.1f; 
        if (Input.GetKey(KeyCode.LeftShift) && isMovingForward && currentStamina > 5f)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

        // Apply active move speed calculation
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 moveDirection = transform.right * x + transform.forward * z;
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Jump Execution
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Continuous application of environmental gravity force over time
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleStamina()
    {
        if (isSprinting)
        {
            // Drain stamina continuously while sprinting
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina < 0f) currentStamina = 0f;
        }
        else
        {
            // Recover stamina continuously when walking or standing still
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }
    }

    /// <summary>
    /// Public helper property so other scripts (like UI or Combat) can read current stamina values.
    /// </summary>
    public float GetStaminaPercentage()
    {
        return currentStamina / maxStamina;
    }
}
