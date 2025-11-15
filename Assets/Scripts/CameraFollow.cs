using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField]
    private Transform target;

    [Header("Camera Position")]
    [SerializeField]
    private Vector3 offset = new Vector3(0f, 5f, -7f);

    [Header("Follow Settings")]
    [SerializeField]
    private float smoothSpeed = 10f;

    [Header("Mouse Look Settings")]
    [SerializeField]
    private float mouseSensitivity = 100f;

    [SerializeField]
    private float minVerticalAngle = -40f;

    [SerializeField]
    private float maxVerticalAngle = 80f;

    private float rotationX = 0f;
    private float rotationY = 0f;
    private Vector2 lookInput;

    void Start()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize rotation from current camera angle
        Vector3 angles = transform.eulerAngles;
        rotationX = angles.x;
        rotationY = angles.y;
    }

    void Update()
    {
        // Removed old Input System calls - use Input Actions instead if needed
    }

    // Called by Input System
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("Camera has no target assigned!");
            return;
        }

        // Handle mouse rotation
        rotationY += lookInput.x * mouseSensitivity * Time.deltaTime;
        rotationX -= lookInput.y * mouseSensitivity * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        // Calculate rotation and position
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        Vector3 rotatedOffset = rotation * offset;

        // Calculate desired position
        Vector3 desiredPosition = target.position + rotatedOffset;

        // Smoothly move camera
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Make camera look at target
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}