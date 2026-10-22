using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

// A cidade permanece carregada e ativa; somente sua apresentação e seu avatar são suspensos.
public class WorldSession : MonoBehaviour
{
    public static WorldSession Instance { get; private set; }
    [SerializeField] private string cityScene = "SampleScene";
    [SerializeField] private Vector3 officeOffset = new Vector3(10000f, 0f, 0f);
    public bool Busy { get; private set; }
    public bool InOffice { get; private set; }
    private GameObject cityPlayer;
    private bool playerWasActive;
    private Scene office;
    private float clockTime, clockDuration = 1920f;
    private bool clockInitialized;
    public float ClockTime => clockTime;
    public float ClockHour => Mathf.Repeat(clockTime / clockDuration * 24f, 24f);
    public void InitializeClock(float time, float duration)
    {
        if (clockInitialized) return;
        clockDuration = Mathf.Max(1f, duration); clockTime = time; clockInitialized = true;
    }
    private void Update() { if (clockInitialized) clockTime = Mathf.Repeat(clockTime + Time.deltaTime, clockDuration); }
    private readonly Dictionary<Behaviour, bool> presentation = new Dictionary<Behaviour, bool>();
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }
    public Coroutine RequestTransition(string destination, Transform player) => StartCoroutine(Transition(destination, player));
    private void PrepareOfficeInput(Scene scene)
    {
        EventSystem selected = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (EventSystem events in root.GetComponentsInChildren<EventSystem>(true))
        {
            if (selected == null && events.gameObject.activeInHierarchy) { selected = events; events.enabled = true; }
            else events.enabled = false;
        }
        if (selected == null)
        {
            var go = new GameObject("OfficeEventSystem"); SceneManager.MoveGameObjectToScene(go, scene);
            selected = go.AddComponent<EventSystem>();
        }
        var module = selected.GetComponent<InputSystemUIInputModule>();
        if (module == null) { module = selected.gameObject.AddComponent<InputSystemUIInputModule>(); module.AssignDefaultActions(); }
        foreach (BaseInputModule other in selected.GetComponents<BaseInputModule>()) other.enabled = other == module;
    }
    public IEnumerator Transition(string destination, Transform player)
    {
        if (Busy) yield break;
        if (!Application.CanStreamedLevelBeLoaded(destination)) { Debug.LogError("Cena não incluída no Build: " + destination, this); yield break; }
        Busy = true;
        if (FadeManager.Instance != null) yield return FadeManager.Instance.FadeOut();
        if (FadeManager.Instance != null) FadeManager.Instance.BeginLoading();
        float loadingStarted=Time.realtimeSinceStartup;
        yield return null;
        if (destination == cityScene)
        {
            Scene city = SceneManager.GetSceneByName(cityScene);
            if (!city.isLoaded) { Busy = false; Debug.LogError("A cena da cidade não está carregada.", this); if (FadeManager.Instance != null) {FadeManager.Instance.EndLoading();yield return FadeManager.Instance.FadeIn();} yield break; }
            // Remove primeiro o avatar/câmeras do Office, evitando dois controles/event systems ativos.
            if (office.IsValid() && office.isLoaded)
            {
                foreach (GameObject root in office.GetRootGameObjects()) root.SetActive(false);
            }
            SceneManager.SetActiveScene(city);
            if (cityPlayer != null) cityPlayer.SetActive(playerWasActive);
            foreach (var item in presentation) if (item.Key != null) item.Key.enabled = item.Value;
            presentation.Clear();
            if (office.IsValid() && office.isLoaded) yield return SceneManager.UnloadSceneAsync(office);
            InOffice = false;
        }
        else
        {
            if (InOffice) { Busy = false;if(FadeManager.Instance!=null){FadeManager.Instance.EndLoading();yield return FadeManager.Instance.FadeIn();} yield break; }
            cityPlayer = player != null ? player.gameObject : null;
            playerWasActive = cityPlayer != null && cityPlayer.activeSelf;
            Scene city = SceneManager.GetSceneByName(cityScene);
            presentation.Clear();
            foreach (GameObject root in city.GetRootGameObjects())
            foreach (Behaviour b in root.GetComponentsInChildren<Behaviour>(true))
                if (b is Camera || b is AudioListener || b is Canvas || b is EventSystem || b is Light)
                { presentation[b] = b.enabled; b.enabled = false; }
            if (cityPlayer != null) cityPlayer.SetActive(false);
            var operation = SceneManager.LoadSceneAsync(destination, LoadSceneMode.Additive);
            if (operation == null)
            {
                if (cityPlayer != null) cityPlayer.SetActive(playerWasActive);
                foreach (var item in presentation) if (item.Key != null) item.Key.enabled = item.Value;
                presentation.Clear(); Busy = false;
                if (FadeManager.Instance != null) {FadeManager.Instance.EndLoading();yield return FadeManager.Instance.FadeIn();}
                yield break;
            }
            while(!operation.isDone){if(FadeManager.Instance!=null)FadeManager.Instance.SetLoadingProgress(operation.progress/.9f);yield return null;}
            if(FadeManager.Instance!=null)FadeManager.Instance.SetLoadingProgress(1);
            office = SceneManager.GetSceneByName(destination);
            foreach (GameObject root in office.GetRootGameObjects())
            {
                // Não move Canvas sobreposto: suas coordenadas são controladas pela UI.
                if (root.GetComponent<Canvas>() == null) root.transform.position += officeOffset;
            }
            SceneManager.SetActiveScene(office); PrepareOfficeInput(office); InOffice = true;
        }
        Physics.SyncTransforms();
        float remaining=.3f-(Time.realtimeSinceStartup-loadingStarted);if(remaining>0)yield return new WaitForSecondsRealtime(remaining);
        if(FadeManager.Instance!=null)FadeManager.Instance.EndLoading();
        Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        if (FadeManager.Instance != null) yield return FadeManager.Instance.FadeIn();
        Busy = false;
    }
}
