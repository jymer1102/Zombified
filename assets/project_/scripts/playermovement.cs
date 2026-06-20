using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float proneSpeed = 1.5f;
    public float slideSpeed = 12f;

    [Header("Look Settings")]
    public float mouseSensitivity = 2f;
    public float fieldOfView = 120f; 

    [Header("Height Settings")]
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    public float proneHeight = 0.5f;

    [Header("Slide Settings")]
    public float slideDuration = 0.5f;
    private float slideTimer;
    private Vector3 slideDirection;

    private enum MovementState { Standing, Sprinting, Crouching, Prone, Sliding }
    private MovementState currentState = MovementState.Standing;

    private CharacterController controller;
    public Camera playerCamera;

    private float rotationX = 0f;
    private Vector3 velocity;
    private float gravity = -9.81f;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = fieldOfView;
        }
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        HandleLook();
        HandleStateInputs();
        HandleMovement();
        ApplyGravity();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -85f, 85f); 

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleStateInputs()
    {
        if (currentState == MovementState.Sliding) return;

        bool isMoving = Input.GetAxis("Vertical") > 0; 

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (currentState == MovementState.Sprinting && isMoving)
            {
                currentState = MovementState.Sliding;
                slideTimer = slideDuration;
                slideDirection = transform.forward * slideSpeed;
                AdjustHeight(crouchHeight);
            }
            else if (currentState == MovementState.Standing || currentState == MovementState.Sprinting)
            {
                currentState = MovementState.Crouching;
                AdjustHeight(crouchHeight);
            }
        }
        else if (Input.GetKey(KeyCode.LeftControl) && currentState == MovementState.Crouching)
        {
            currentState = MovementState.Prone;
            AdjustHeight(proneHeight);
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            currentState = MovementState.Standing;
            AdjustHeight(standHeight);
        }

        if (Input.GetKey(KeyCode.LeftShift) && currentState == MovementState.Standing && isMoving)
        {
            currentState = MovementState.Sprinting;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) && currentState == MovementState.Sprinting)
        {
            currentState = MovementState.Standing;
        }
    }

    void HandleMovement()
    {
        if (currentState == MovementState.Sliding)
        {
            controller.Move(slideDirection * Time.deltaTime);
            slideTimer -= Time.deltaTime;
            
            if (slideTimer <= 0)
            {
                currentState = Input.GetKey(KeyCode.LeftControl) ? MovementState.Crouching : MovementState.Standing;
                if (currentState == MovementState.Standing) AdjustHeight(standHeight);
            }
            return;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        float targetSpeed = walkSpeed;
        switch (currentState)
        {
            case MovementState.Sprinting: targetSpeed = sprintSpeed; break;
            case MovementState.Crouching: targetSpeed = crouchSpeed; break;
            case MovementState.Prone:     targetSpeed = proneSpeed;  break;
        }

        controller.Move(move * targetSpeed * Time.deltaTime);
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void AdjustHeight(float targetHeight)
    {
        controller.height = targetHeight;
    }

    public bool IsStealthing()
    {
        return (currentState == MovementState.Crouching || currentState == MovementState.Prone);
    }
}
