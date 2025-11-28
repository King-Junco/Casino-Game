using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField]
    private float walkSpeed = 5f;

    [SerializeField]
    private float runSpeed = 8f;

    [SerializeField]
    private float rotationSpeed = 10f;

    [Header("Jump Settings")]
    [SerializeField]
    private float jumpHeight = 2f;

    [SerializeField]
    private float gravity = -9.81f;

    [Header("Animation")]
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector2 moveInput;
    private bool isRunning = false;
    private bool isGrounded;
    private float currentSpeed;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // If animator isn't assigned, try to find it
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Find camera if not assigned
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        CheckGrounded();
        HandleMovement();
        HandleRotation();
        UpdateAnimations();
    }

    void CheckGrounded()
    {
        // Check if character is on the ground
        isGrounded = controller.isGrounded;

        // Reset vertical velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    // Called by Input System for movement
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Called by Input System for running
    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isRunning = true;
        }
        else if (context.canceled)
        {
            isRunning = false;
        }
    }

    // Called by Input System for jumping
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // Trigger jump animation
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }
    }

    void HandleMovement()
    {
        // Determine current speed based on running state
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Get camera direction
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        // Flatten camera direction (ignore Y axis)
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate movement direction relative to camera
        Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

        // Apply horizontal movement
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleRotation()
    {
        // Rotate character to face movement direction
        if (moveInput.magnitude >= 0.1f)
        {
            // Get camera direction for rotation
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 cameraRight = cameraTransform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            // Calculate target direction
            Vector3 targetDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            // Check if character is moving
            bool isMoving = moveInput.magnitude > 0.1f;

            // Set animation parameters
            animator.SetBool("param_idletowalk", isMoving);
            animator.SetBool("isRunning", isRunning && isMoving);
        }
    }
}