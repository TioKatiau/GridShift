using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [SerializeField] private float duracaoFade = 1f;

    private Image fadeImage;
    private bool fading;

    private void Awake()
    {
        // Impede criar outro FadeManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CriarFade();

        // Quando uma nova cena terminar de carregar,
        // faz o fade IN automaticamente.
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= AoCarregarCena;
        }
    }

    private void CriarFade()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        gameObject.AddComponent<GraphicRaycaster>();

        GameObject imagem = new GameObject("FadeImage");

        imagem.transform.SetParent(transform, false);

        fadeImage = imagem.AddComponent<Image>();

        fadeImage.color = Color.black;

        RectTransform rect = fadeImage.rectTransform;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;

        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void Start()
    {
        // Primeira cena também começa com fade IN
        fadeImage.color = Color.black;

        StartCoroutine(FadeIn());
    }

    private void AoCarregarCena(
        Scene cena,
        LoadSceneMode modo)
    {
        // Dá um pequeno tempo para a nova cena terminar
        // de inicializar antes de tirar o preto.
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeOut()
    {
        if (fading)
            yield break;

        fading = true;

        yield return Fade(0f, 1f);

        fading = false;
    }

    private IEnumerator FadeIn()
    {
        if (fadeImage == null)
            yield break;

        fading = true;

        // Garante que começa totalmente preto
        Color cor = fadeImage.color;
        fadeImage.color = new Color(
            cor.r,
            cor.g,
            cor.b,
            1f
        );

        yield return new WaitForSecondsRealtime(0.1f);

        yield return Fade(1f, 0f);

        fading = false;
    }

    private IEnumerator Fade(float inicio, float fim)
    {
        float tempo = 0f;

        Color cor = Color.black;

        while (tempo < duracaoFade)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracaoFade
            );

            // Suaviza o movimento
            progresso = Mathf.SmoothStep(
                0f,
                1f,
                progresso
            );

            float alpha = Mathf.Lerp(
                inicio,
                fim,
                progresso
            );

            fadeImage.color = new Color(
                cor.r,
                cor.g,
                cor.b,
                alpha
            );

            yield return null;
        }

        fadeImage.color = new Color(
            cor.r,
            cor.g,
            cor.b,
            fim
        );
    }
}