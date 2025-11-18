using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

/// <summary>
/// Simple 2D Character Controller with 8-directional movement,
/// input handling, and a vertical bobbing effect for simple animation.
/// </summary>
public class CharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("The speed at which the character moves.")]
    public float moveSpeed = 5.0f;
    [Tooltip("The speed at which the character rotates to face the movement direction.")]
    public float rotationSpeed = 720.0f;

    [Header("Bobbing Animation Settings (Simulated Run/Walk)")]
    [Tooltip("Maximum vertical distance (in Unity units) the character bobs up and down.")]
    public float bobHeight = 0.1f;
    [Tooltip("The speed of the bobbing motion. Higher value means faster bob.")]
    public float bobSpeed = 10.0f;

    [Header("Input Handling")]
    [Tooltip("A flag to check if the interaction button was pressed this frame.")]
    [SerializeField]
    private bool isInteracting = false;

    // Internal component references and state
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector3 originalLocalPosition;

    void Awake()
    {
        // Get the Rigidbody2D component attached to this GameObject
        rb = GetComponent<Rigidbody2D>();

        // Store the starting local position for the bobbing calculation
        originalLocalPosition = transform.localPosition;
    }

    void Update()
    {
        // 1. Handle Visual Rotation
        HandleRotation();

        // 2. Handle Bobbing Animation
        HandleBobbingAnimation();
    }

    void FixedUpdate()
    {
        // 3. Handle Physics Movement (using Rigidbody2D)
        HandleMovement();
    }

    // --- Input System Methods (must be linked in the Input Action Asset) ---

    // This method is called automatically when the Movement action is performed
    public void OnMove(InputAction.CallbackContext context)
    {
        // Read the 2D vector input (WASD/Arrow Keys/Joystick)
        movementInput = context.ReadValue<Vector2>();
    }

    // This method is called automatically when the Interaction action is performed
    public void OnInteract(InputAction.CallbackContext context)
    {
        // When the button is pressed down (started)
        if (context.started)
        {
            isInteracting = true;
            Debug.Log("Interaction button pressed!");
            // You can call your interaction logic here (e.g., Door.Interact(), Npc.Talk())
        }
        // When the button is released (canceled)
        if (context.canceled)
        {
            isInteracting = false;
        }
    }

    // --- Core Movement Logic ---

    private void HandleMovement()
    {
        // Apply force to the Rigidbody for smooth, physics-based movement.
        // Normalize the vector to prevent faster diagonal movement (8 directions).
        Vector2 velocity = movementInput.normalized * moveSpeed;
        rb.velocity = velocity;
    }

    private void HandleRotation()
    {
        // Only rotate if the character is actually moving
        if (movementInput.magnitude > 0.1f)
        {
            // Calculate the target rotation angle in degrees
            float targetAngle = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg;

            // Smoothly rotate towards the target angle
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle - 90f); // -90 because the sprite might face up by default

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // --- Simulated Animation Logic ---

    private void HandleBobbingAnimation()
    {
        // Check if the character is currently moving
        if (movementInput.magnitude > 0.01f)
        {
            // Calculate the vertical offset using a sine wave based on time and bobSpeed
            float offsetY = Mathf.Sin(Time.time * bobSpeed) * bobHeight;

            // Apply the offset to the current local position (relative to parent)
            Vector3 newLocalPosition = originalLocalPosition;
            newLocalPosition.y += offsetY;

            // Use LocalPosition for bobbing so it doesn't interfere with the Rigidbody's main position/velocity
            transform.localPosition = newLocalPosition;
        }
        else
        {
            // If not moving, smoothly return the character to its original local Y position
            Vector3 currentLocalPosition = transform.localPosition;
            currentLocalPosition.y = Mathf.Lerp(
                currentLocalPosition.y,
                originalLocalPosition.y,
                Time.deltaTime * bobSpeed // Use bobSpeed to control return smoothness
            );
            transform.localPosition = currentLocalPosition;
        }
    }
}