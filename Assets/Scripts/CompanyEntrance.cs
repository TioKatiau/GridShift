using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CompanyEntrance : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Interação")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float interactionDistance = 3f;

    [Header("Cena")]
    [SerializeField] private string cenaDestino = "Office";

    [Header("Flutuação do Texto")]
    [SerializeField] private float alturaFlutuacao = 0.15f;
    [SerializeField] private float velocidadeFlutuacao = 2f;
    [SerializeField] private float alturaDoPrompt = 2f;

    [Header("Câmera")]
    [SerializeField] private Camera playerCamera;

    private bool playerNearby;
    private bool trocandoCena;

    private Vector3 promptPosicaoInicial;
    private void OnEnable() { InteractionFocus.Register(this, interactionDistance); }
    private void OnDisable() { InteractionFocus.Remove(this); }

    private void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);

            promptPosicaoInicial =
                transform.position +
                Vector3.up * alturaDoPrompt;

            interactionPrompt.transform.position =
                promptPosicaoInicial;
        }

        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        if (Time.timeScale<=0 || player == null || trocandoCena || (WorldSession.Instance != null &&
            (WorldSession.Instance.Busy || UnityEngine.SceneManagement.SceneManager.GetActiveScene() != gameObject.scene)))
        {
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
            playerNearby = false;
            return;
        }

        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        if (distance <= interactionDistance && InteractionFocus.Selected(this, player))
        {
            if (!playerNearby)
            {
                playerNearby = true;

                if (interactionPrompt != null)
                    interactionPrompt.SetActive(true);
            }

            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame && InteractionFocus.Consume())
            {
                StartCoroutine(TrocarCena());
            }
        }
        else
        {
            if (playerNearby)
            {
                playerNearby = false;

                if (interactionPrompt != null)
                    interactionPrompt.SetActive(false);
            }
        }

        if (playerNearby && interactionPrompt != null)
        {
            FlutuarPrompt();
            OlharParaCamera();
        }
    }

    private void FlutuarPrompt()
    {
        promptPosicaoInicial = transform.position + Vector3.up * alturaDoPrompt;
        float movimento =
            Mathf.Sin(
                Time.time * velocidadeFlutuacao
            ) * alturaFlutuacao;

        interactionPrompt.transform.position =
            promptPosicaoInicial +
            Vector3.up * movimento;
    }

    private void OlharParaCamera()
    {
        if (playerCamera == null)
            return;

        interactionPrompt.transform.LookAt(
            playerCamera.transform.position
        );

        interactionPrompt.transform.Rotate(
            0f,
            180f,
            0f
        );
    }

    private IEnumerator TrocarCena()
    {
        trocandoCena = true;

        if (WorldSession.Instance != null)
        {
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
            yield return WorldSession.Instance.RequestTransition(cenaDestino, player);
            trocandoCena = false;
            yield break;
        }

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (FadeManager.Instance != null)
        {
            // Fade para PRETO
            yield return FadeManager.Instance.FadeOut();
            FadeManager.Instance.BeginLoading();
        }

        // Troca de cena enquanto está completamente preto
        SceneManager.LoadScene(cenaDestino);
    }
}
