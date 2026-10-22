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
    private Camera transitionCamera;
    private TMPro.TMP_Text loadingText;
    private bool loading;
    public bool HasTransitionCamera => transitionCamera != null && transitionCamera.isActiveAndEnabled;
    public void BeginLoading(){
        if(transitionCamera==null){var go=new GameObject("TransitionCamera");go.transform.SetParent(transform,false);transitionCamera=go.AddComponent<Camera>();transitionCamera.clearFlags=CameraClearFlags.SolidColor;transitionCamera.backgroundColor=new Color(.025f,.03f,.045f);transitionCamera.cullingMask=0;transitionCamera.depth=100;transitionCamera.targetDisplay=0;}
        transitionCamera.enabled=SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null;loading=true;loadingText.gameObject.SetActive(true);SetLoadingProgress(-1);
    }
    public void SetLoadingProgress(float progress){if(loadingText!=null)loadingText.text="<color=#F0CE38><size=48>GRIDSHIFT</size></color>\n\nPreparando seu destino"+(progress<0?"…":" · "+Mathf.RoundToInt(Mathf.Clamp01(progress)*100)+"%");}
    public void EndLoading(){loading=false;if(transitionCamera!=null)transitionCamera.enabled=false;if(loadingText!=null)loadingText.gameObject.SetActive(false);}
    private void Update(){if(loading&&loadingText!=null)loadingText.alpha=.8f+.2f*Mathf.Sin(Time.unscaledTime*3);}

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
        var label=new GameObject("LoadingLabel",typeof(RectTransform));label.transform.SetParent(transform,false);loadingText=label.AddComponent<TMPro.TextMeshProUGUI>();loadingText.alignment=TMPro.TextAlignmentOptions.Center;loadingText.fontSize=24;loadingText.raycastTarget=false;
        var lr=loadingText.rectTransform;lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=lr.offsetMax=Vector2.zero;label.SetActive(false);

        // Quando uma nova cena terminar de carregar,
        // faz o fade IN automaticamente.
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= AoCarregarCena;
            Instance = null;
        }
    }

    private void CriarFade()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        GameObject imagem = new GameObject("FadeImage");

        imagem.transform.SetParent(transform, false);

        fadeImage = imagem.AddComponent<Image>();
        fadeImage.raycastTarget=false;

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

        if (WorldSession.Instance == null || !WorldSession.Instance.Busy) {EndLoading();StartCoroutine(FadeIn());}
    }

    private void AoCarregarCena(
        Scene cena,
        LoadSceneMode modo)
    {
        // Dá um pequeno tempo para a nova cena terminar
        // de inicializar antes de tirar o preto.
        if (WorldSession.Instance == null || !WorldSession.Instance.Busy) {EndLoading();StartCoroutine(FadeIn());}
    }

    public IEnumerator FadeOut()
    {
        if (fading)
            yield break;

        fading = true;

        yield return Fade(0f, 1f);

        fading = false;
    }

    public IEnumerator FadeIn()
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
