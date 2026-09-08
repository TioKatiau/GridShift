using UnityEngine;
using UnityEngine.InputSystem;

public class HUDParallax : MonoBehaviour
{
    [Header("Movimento do HUD")]
    [SerializeField] private float horizontalAmount = 0.015f;
    [SerializeField] private float verticalAmount = 0.008f;
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Limites")]
    [SerializeField] private float maxHorizontalOffset = 0.03f;
    [SerializeField] private float maxVerticalOffset = 0.02f;

    private Vector3 originalPosition;
    private Vector3 currentOffset;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // Movimento contrário ao mouse
        float targetX = -mouseDelta.x * horizontalAmount;
        float targetY = -mouseDelta.y * verticalAmount;

        // Limita MUITO o movimento
        targetX = Mathf.Clamp(
            targetX,
            -maxHorizontalOffset,
            maxHorizontalOffset
        );

        targetY = Mathf.Clamp(
            targetY,
            -maxVerticalOffset,
            maxVerticalOffset
        );

        Vector3 targetOffset = new Vector3(
            targetX,
            targetY,
            0f
        );

        // Suaviza o movimento
        currentOffset = Vector3.Lerp(
            currentOffset,
            targetOffset,
            smoothSpeed * Time.deltaTime
        );

        transform.localPosition =
            originalPosition + currentOffset;
    }
}