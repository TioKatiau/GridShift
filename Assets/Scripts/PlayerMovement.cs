using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float gravity = -20f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;

    [Header("Mouse")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.05f;
    [SerializeField] private float maxLookAngle = 80f;

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
        HandleCamera();
    }

    private void HandleMovement()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed)
                input.y += 1;

            if (Keyboard.current.sKey.isPressed)
                input.y -= 1;

            if (Keyboard.current.dKey.isPressed)
                input.x += 1;

            if (Keyboard.current.aKey.isPressed)
                input.x -= 1;
        }

        input = Vector2.ClampMagnitude(input, 1f);

        Vector3 move =
            transform.forward * input.y +
            transform.right * input.x;

        // Define a velocidade
        float currentSpeed = moveSpeed;

        // Corre somente segurando SHIFT + W
        if (Keyboard.current != null &&
            Keyboard.current.leftShiftKey.isPressed &&
            input.y > 0)
        {
            currentSpeed = sprintSpeed;
        }

        // Pulo
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = -2f;

            if (Keyboard.current != null &&
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                verticalVelocity = Mathf.Sqrt(
                    jumpHeight * -2f * gravity
                );
            }
        }
        else
        {
            // Gravidade
            verticalVelocity += gravity * Time.deltaTime;
        }

        move.y = verticalVelocity;

        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private void HandleCamera()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // Rotação horizontal do PLAYER
        float mouseX = mouseDelta.x * mouseSensitivity;

        transform.Rotate(0f, mouseX, 0f);

        // Rotação vertical da CÂMERA
        float mouseY = mouseDelta.y * mouseSensitivity;

        cameraPitch -= mouseY;

        cameraPitch = Mathf.Clamp(
            cameraPitch,
            -maxLookAngle,
            maxLookAngle
        );

        cameraTransform.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }
}