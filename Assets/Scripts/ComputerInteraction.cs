using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ComputerInteraction : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public PlayerMovement playerMovement;

    [Header("Câmera")]
    public Camera playerCamera;
    public Transform computerViewPoint;

    [Header("Interface")]
    public GameObject interactionText;
    public GameObject minimap;
    public GameObject compass;
    public GameObject Aim;

    [Header("Configurações")]
    public float interactionDistance = 10f;
    public float zoomDuration = 0.5f;

    private bool playerNearby = false;
    private bool usingComputer = false;
    private bool movingCamera = false;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    void Start()
    {
        if (interactionText != null)
            interactionText.SetActive(false);

        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        if (player == null || playerCamera == null)
            return;

        // ESTÁ NO COMPUTADOR
        if (usingComputer)
        {
            if (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame &&
                !movingCamera)
            {
                ExitComputer();
            }

            return;
        }

        // DISTÂNCIA ATÉ O COMPUTADOR
        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        if (distance <= interactionDistance)
        {
            if (!playerNearby)
            {
                playerNearby = true;

                if (interactionText != null)
                    interactionText.SetActive(true);
            }

            // APERTOU E
            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                EnterComputer();
            }
        }
        else
        {
            if (playerNearby)
            {
                playerNearby = false;

                if (interactionText != null)
                    interactionText.SetActive(false);
            }
        }
    }

    void EnterComputer()
    {
        usingComputer = true;
        movingCamera = true;

        // SALVA A CÂMERA
        originalCameraPosition = playerCamera.transform.position;
        originalCameraRotation = playerCamera.transform.rotation;

        // ESCONDE O TEXTO
        if (interactionText != null)
            interactionText.SetActive(false);

        // ESCONDE MINIMAPA
        if (minimap != null)
            minimap.SetActive(false);

        // ESCONDE BÚSSOLA
        if (compass != null)
            compass.SetActive(false);
        // ESCONDE MIRA
        if (Aim != null)
            Aim.SetActive(false);

        // DESATIVA MOVIMENTO DO PLAYER
        if (playerMovement != null)
            playerMovement.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(ZoomToComputer());
    }

    IEnumerator ZoomToComputer()
    {
        Vector3 startPosition = playerCamera.transform.position;
        Quaternion startRotation = playerCamera.transform.rotation;

        float time = 0f;

        while (time < zoomDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / zoomDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            playerCamera.transform.position = Vector3.Lerp(
                startPosition,
                computerViewPoint.position,
                t
            );

            playerCamera.transform.rotation = Quaternion.Slerp(
                startRotation,
                computerViewPoint.rotation,
                t
            );

            yield return null;
        }

        playerCamera.transform.position = computerViewPoint.position;
        playerCamera.transform.rotation = computerViewPoint.rotation;

        movingCamera = false;
    }

    void ExitComputer()
    {
        movingCamera = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(ReturnFromComputer());
    }

    IEnumerator ReturnFromComputer()
    {
        Vector3 startPosition = playerCamera.transform.position;
        Quaternion startRotation = playerCamera.transform.rotation;

        float time = 0f;

        while (time < zoomDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / zoomDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            playerCamera.transform.position = Vector3.Lerp(
                startPosition,
                originalCameraPosition,
                t
            );

            playerCamera.transform.rotation = Quaternion.Slerp(
                startRotation,
                originalCameraRotation,
                t
            );

            yield return null;
        }

        playerCamera.transform.position = originalCameraPosition;
        playerCamera.transform.rotation = originalCameraRotation;

        // REATIVA MOVIMENTO DO PLAYER
        if (playerMovement != null)
            playerMovement.enabled = true;

        // VOLTA MINIMAPA
        if (minimap != null)
            minimap.SetActive(true);

        // VOLTA BÚSSOLA
        if (compass != null)
            compass.SetActive(true);
        // VOLTA MIRA
        if (Aim != null)
            Aim.SetActive(true);

        usingComputer = false;
        movingCamera = false;
    }
}