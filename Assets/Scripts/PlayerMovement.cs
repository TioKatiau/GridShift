
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

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch;
    private bool wasGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        wasGrounded = controller.isGrounded;
    }

    private void Update()
    {
        HandleMovement();
        HandleCamera();
    }

    private void HandleMovement()
    {
        Vector2 input = Vector2.zero;

        // =========================
        // INPUT WASD
        // =========================

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

        // =========================
        // VELOCIDADE
        // =========================

        float currentSpeed = moveSpeed;
        bool isRunning = false;

        // SHIFT + W = CORRER
        if (Keyboard.current != null &&
            Keyboard.current.leftShiftKey.isPressed &&
            input.y > 0)
        {
            currentSpeed = sprintSpeed;
            isRunning = true;
        }

        // =========================
        // PULO
        // =========================

        bool jumpPressed = Keyboard.current != null &&
                           Keyboard.current.spaceKey.wasPressedThisFrame;

        // Verifica se está no chão OU se acabou de apertar espaço
        if (controller.isGrounded || jumpPressed)
        {
            if (verticalVelocity < 0)
                verticalVelocity = -2f;

            if (jumpPressed)
            {
                // Física do pulo
                verticalVelocity = Mathf.Sqrt(
                    jumpHeight * -2f * gravity
                );

                // Animação de pulo imediatamente
                if (animator != null)
                {
                    animator.ResetTrigger("Jump");
                    animator.SetTrigger("Jump");
                }
            }
        }
        else
        {
            // Gravidade
            verticalVelocity += gravity * Time.deltaTime;
        }

        // =========================
        // MOVIMENTO HORIZONTAL
        // =========================

        Vector3 moveDirection =
            transform.forward * input.y +
            transform.right * input.x;

        Vector3 horizontalVelocity =
            moveDirection * currentSpeed;

        // =========================
        // MOVIMENTO FINAL
        // =========================

        Vector3 finalVelocity = horizontalVelocity;

        // O pulo fica separado da velocidade
        finalVelocity.y = verticalVelocity;

        controller.Move(finalVelocity * Time.deltaTime);

        // =========================
        // DETECTA ATERRISSAGEM
        // =========================

        bool isGroundedNow = controller.isGrounded;

        if (!wasGrounded && isGroundedNow)
        {
            if (animator != null)
            {
                // Se estiver correndo
                if (isRunning)
                {
                    animator.Play("A_Run A");
                }
                // Se estiver andando
                else if (input.magnitude > 0.1f)
                {
                    animator.Play("A_Walk A");
                }
                // Se estiver parado, deixa o Jump terminar
            }
        }

        wasGrounded = isGroundedNow;

        // =========================
        // ANIMAÇÕES
        // =========================

        if (animator != null)
        {
            // Idle = 0
            // Walk/Run = 1
            animator.SetFloat("Speed", input.magnitude);

            // Shift + W
            animator.SetBool("IsRunning", isRunning);
        }
    }

    private void HandleCamera()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // =========================
        // ROTAÇÃO HORIZONTAL
        // =========================

        float mouseX = mouseDelta.x * mouseSensitivity;

        transform.Rotate(0f, mouseX, 0f);

        // =========================
        // ROTAÇÃO VERTICAL
        // =========================

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
