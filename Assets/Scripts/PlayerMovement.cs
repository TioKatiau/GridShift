using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float gravity = -20f;
    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private LayerMask groundMask = ~0;
    [Header("Mouse")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.05f;
    [SerializeField] private float maxLookAngle = 80f;
    [Header("Animation")]
    [SerializeField] private Animator animator;
    private CharacterController controller;
    private float verticalVelocity, cameraPitch;
    private bool wasGrounded, jumpConsumed = true;
    private bool controlsBlocked;
    [Header("Diagnóstico de salto")]
    [SerializeField] private bool groundedDebug;
    [SerializeField] private int jumpCommands, jumpsExecuted;
    [SerializeField] private string jumpStatus;
    public bool IsGrounded => groundedDebug;
    public bool IsRunning { get; private set; }
    public Vector2 MovementInput { get; private set; }
    public int JumpsExecuted => jumpsExecuted;
    private readonly RaycastHit[] groundHits = new RaycastHit[12];
    public float VerticalVelocity => verticalVelocity;
    public bool ControlsBlocked => controlsBlocked;
    public Transform CameraTransform => cameraTransform;
    public void BlockControls(bool blocked) { controlsBlocked = blocked; }
    private void Awake() {
        controller = GetComponent<CharacterController>(); controller.minMoveDistance = 0f;
        if (animator != null) animator.applyRootMotion = false;
    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        wasGrounded = Grounded(); jumpConsumed = !wasGrounded;
    }
    private void Update()
    {
        if (controlsBlocked || (WorldSession.Instance != null && WorldSession.Instance.Busy) || Time.timeScale <= 0f) return;
        var keyboard = Keyboard.current; Vector2 input = Vector2.zero;
        if (keyboard != null)
        {
            input.x = (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0);
            input.y = (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0);
        }
        SimulateMovement(input, keyboard != null && keyboard.leftShiftKey.isPressed,
            keyboard != null && keyboard.spaceKey.wasPressedThisFrame, Time.deltaTime);
        HandleCamera();
    }
    private bool Grounded()
    {
        if (controller == null || !controller.enabled || verticalVelocity > 0.05f) return false;
        Bounds bounds = controller.bounds;
        float radius = controller.radius * Mathf.Min(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z)) * .8f;
        // Começa acima do piso: SphereCast não relata superfícies já sobrepostas
        // na origem (a cápsula pode penetrar o skinWidth no asfalto/calçada).
        Vector3 origin = new Vector3(bounds.center.x, bounds.min.y + radius + .3f, bounds.center.z);
        int count = Physics.SphereCastNonAlloc(origin, radius, Vector3.down, groundHits,
            .3f + controller.skinWidth + .05f, groundMask, QueryTriggerInteraction.Ignore);
        float normalMin = Mathf.Cos(Mathf.Min(60f, controller.slopeLimit) * Mathf.Deg2Rad);
        for (int i = 0; i < count; i++)
            if (!groundHits[i].collider.transform.IsChildOf(transform) && groundHits[i].normal.y >= normalMin) return true;
        return false;
    }
    // Uma única chamada Move por passo: gravidade, teto e aterrissagem compartilham o mesmo estado.
    public void SimulateMovement(Vector2 input, bool runHeld, bool jumpPressed, float dt)
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (!controller.enabled || controlsBlocked || Time.timeScale <= 0f || (WorldSession.Instance != null && WorldSession.Instance.Busy)) return;
        dt = Mathf.Clamp(dt, 0f, .05f); input = Vector2.ClampMagnitude(input, 1f);
        // O contato da última chamada Move autoriza o salto; a sonda próxima ao
        // chão não rearma o pulo antes de uma aterrissagem efetiva.
        bool grounded = verticalVelocity <= 0f && controller.isGrounded;
        groundedDebug = grounded; MovementInput = input;
        if (jumpPressed) { jumpCommands++; jumpStatus = grounded ? "Comando no chão" : "Comando no ar: ignorado"; }
        if (grounded && verticalVelocity <= 0f) { verticalVelocity = -2f; jumpConsumed = false; }
        bool running = runHeld && input.y > 0f;
        IsRunning = running;
        if (jumpPressed && grounded && !jumpConsumed)
        {
            jumpConsumed = true; verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * Mathf.Min(gravity, -.1f));
            jumpsExecuted++; jumpStatus = "Salto executado";
            if (animator != null) { animator.ResetTrigger("Jump"); animator.SetTrigger("Jump"); }
        }
        verticalVelocity += Mathf.Min(gravity, -.1f) * dt;
        Vector3 velocity = (transform.forward * input.y + transform.right * input.x) * (running ? sprintSpeed : moveSpeed);
        velocity.y = verticalVelocity;
        CollisionFlags flags = controller.Move(velocity * dt);
        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) verticalVelocity = 0f;
        bool landed = verticalVelocity <= 0f && (flags & CollisionFlags.Below) != 0;
        groundedDebug = landed;
        if (landed) { verticalVelocity = -2f; jumpConsumed = false; groundedDebug = true; jumpStatus = "Contato com o chão"; }
        if (animator != null)
        {
            if (!wasGrounded && landed)
            {
                string state = running ? "A_Run A" : input.sqrMagnitude > .01f ? "A_Walk A" : null;
                if (state != null && animator.HasState(0, Animator.StringToHash(state))) animator.Play(state);
            }
            animator.SetFloat("Speed", input.magnitude); animator.SetBool("IsRunning", running);
        }
        wasGrounded = landed;
    }
    private void HandleCamera()
    {
        if (Mouse.current == null || cameraTransform == null) return;
        Vector2 delta = Mouse.current.delta.ReadValue() * mouseSensitivity * GamePreferences.MouseSensitivity;
        transform.Rotate(0f, delta.x, 0f);
        cameraPitch = Mathf.Clamp(cameraPitch - delta.y, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }
}
