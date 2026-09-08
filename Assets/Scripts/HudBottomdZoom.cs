using UnityEngine;
using UnityEngine.InputSystem;

public class HudBottomZoom : MonoBehaviour
{
    [Header("Zoom")]
    public float zoomScale = 1.5f;
    public float zoomSpeed = 8f;

    [Header("Posição do Zoom")]
    public Vector3 zoomPosition;

    [Header("Rotação do Zoom")]
    public float zoomRotationX = 9f;
    public float zoomRotationY = 0f;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    void Start()
    {
        originalPosition = transform.localPosition;
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        if (Mouse.current == null)
            return;

        Vector3 targetPosition;
        Vector3 targetScale;
        Quaternion targetRotation;

        // SEGURANDO BOTÃO DIREITO
        if (Mouse.current.rightButton.isPressed)
        {
            targetPosition = zoomPosition;
            targetScale = originalScale * zoomScale;

            targetRotation = Quaternion.Euler(
                zoomRotationX,
                zoomRotationY,
                originalRotation.eulerAngles.z
            );
        }
        else
        {
            targetPosition = originalPosition;
            targetScale = originalScale;
            targetRotation = originalRotation;
        }

        // POSIÇÃO
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * zoomSpeed
        );

        // TAMANHO
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * zoomSpeed
        );

        // ROTAÇÃO
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * zoomSpeed
        );
    }
}