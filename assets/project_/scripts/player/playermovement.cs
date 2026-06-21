using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;

    [Header("Movement Constants")]
    public float walkSpeed = 5.0f;
    public float sprintSpeed = 8.5f;
    public float crouchSpeed = 2.5f;
    public float proneSpeed = 1.0f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Crouch / Prone / Slide Settings")]
    public float standingHeight = 2.0f;
    public float crouchingHeight = 1.2f;
    public float proneHeight = 0.5f;
    public float crouchTransitionSpeed = 8f;
    [Tooltip("How long you can hold a slide before it ends and you settle into a crouch.")]
    public float slideDuration = 0.8f;
    [Tooltip("Initial speed boost applied at the start of a slide.")]
    public float slideSpeed = 11f;
    [Tooltip("How fast the slide speed decays back down to crouch speed.")]
    public float slideDeceleration = 8f;

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
    private bool isCrouching;
    private bool isProne;
    private bool isSliding;
    private float slideTimer;
    private float currentSlideSpeed;

    // Public read-only hooks for other systems (e.g. ZombieAI stealth checks, CameraEffects sway)
    public bool IsCrouching => isCrouching || isProne || isSliding;
    public bool IsProne => isProne;
    public bool IsSliding => isSliding;
    public bool IsSprinting => isSprinting;
    public bool IsMoving => controller != null && controller.velocity.sqrMagnitude > 0.1f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentStamina = maxStamina;
        controller.height = standingHeight;

        LoadSavedSensitivity();

        // Locks the mouse cursor to the center of the screen for absolute control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Pulls the saved mouse sensitivity from SaveSystem (if it exists) so it
    /// persists between play sessions instead of resetting every launch.
    /// </summary>
    void LoadSavedSensitivity()
    {
        if (SaveSystem.Instance != null)
        {
            mouseSensitivity = SaveSystem.Instance.LoadSensitivity();
        }
    }

    /// <summary>
    /// Updates sensitivity at runtime and saves it. Call this from a settings
    /// slider's OnValueChanged event.
    /// </summary>
    public void SetMouseSensitivity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;

        if (SaveSystem.Instance != null)
        {
            float currentVolume = AudioManager.Instance != null ? AudioManager.Instance.GetMasterVolume() : 1.0f;
            SaveSystem.Instance.SaveSettings(currentVolume, mouseSensitivity);
        }
    }

    void Update()
    {
        HandleMouseLook();
        HandleStanceInput();
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

    /// <summary>
    /// Handles Ctrl tap (crouch toggle), Ctrl hold while moving (prone), and
    /// Ctrl while sprinting (slide), per the README's movement spec.
    /// </summary>
    void HandleStanceInput()
    {
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl);
        bool ctrlPressedThisFrame = Input.GetKeyDown(KeyCode.LeftControl);
        float z = Input.GetAxis("Vertical");
        bool isMovingForward = z > 0.1f;

        // Start a slide if we're sprinting and just pressed crouch
        if (ctrlPressedThisFrame && isSprinting && !isSliding && isGrounded)
        {
            StartSlide();
            return;
        }

        if (isSliding)
        {
            UpdateSlide();
            return;
        }

        // Holding Ctrl while moving forward (not sprinting) eases into prone
        if (ctrlHeld && isMovingForward && !isProne)
        {
            isProne = true;
            isCrouching = false;
        }
        // Tapping Ctrl while standing still (or already crouched) toggles a regular crouch
        else if (ctrlPressedThisFrame && !isMovingForward)
        {
            isCrouching = !isCrouching;
            isProne = false;
        }
        // Releasing Ctrl stands back up out of prone
        else if (!ctrlHeld && isProne)
        {
            isProne = false;
        }

        UpdateColliderHeight();
    }

    void StartSlide()
    {
        isSliding = true;
        isCrouching = true;
        isProne = false;
        slideTimer = slideDuration;
        currentSlideSpeed = slideSpeed;
    }

    void UpdateSlide()
    {
        slideTimer -= Time.deltaTime;
        currentSlideSpeed = Mathf.Max(crouchSpeed, currentSlideSpeed - slideDeceleration * Time.deltaTime);

        if (slideTimer <= 0f)
        {
            isSliding = false; // settles into a normal crouch once the slide ends
        }

        UpdateColliderHeight();
    }

    void UpdateColliderHeight()
    {
        float targetHeight = standingHeight;
        if (isProne) targetHeight = proneHeight;
        else if (isCrouching || isSliding) targetHeight = crouchingHeight;

        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        // Keep the controller's center aligned so it shrinks/grows from the feet up, not from the middle
        controller.center = new Vector3(0f, controller.height / 2f, 0f);
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
        bool canSprint = !isCrouching && !isProne && !isSliding;
        if (Input.GetKey(KeyCode.LeftShift) && isMovingForward && currentStamina > 5f && canSprint)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

        // Apply active move speed calculation based on current stance
        float currentSpeed;
        if (isSliding) currentSpeed = currentSlideSpeed;
        else if (isProne) currentSpeed = proneSpeed;
        else if (isCrouching) currentSpeed = crouchSpeed;
        else if (isSprinting) currentSpeed = sprintSpeed;
        else currentSpeed = walkSpeed;

        Vector3 moveDirection;
        if (isSliding)
        {
            // Slides keep moving in the direction you were facing when the slide started, not strafe input
            moveDirection = transform.forward;
        }
        else
        {
            moveDirection = transform.right * x + transform.forward * z;
        }

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Jump Execution - disabled while prone or sliding (can't jump from the ground like that)
        if (Input.GetButtonDown("Jump") && isGrounded && !isProne && !isSliding)
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
