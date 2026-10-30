using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Preferences affect presentation and ambient population targets, never mission simulation.
public sealed class GridShiftPerformanceSettings : MonoBehaviour
{
    public static float PedestrianDensity => PlayerPrefs.GetFloat("GS.Perf.Pedestrians", 1);
    public static float TrafficDensity => PlayerPrefs.GetFloat("GS.Perf.Traffic", 1);
    public static GridShiftPerformanceSettings Instance { get; private set; }
    readonly Dictionary<Volume, float> volumeWeights = new Dictionary<Volume, float>();
    readonly Dictionary<UniversalRenderPipelineAsset, UniversalRenderPipelineAsset> pipelines = new Dictionary<UniversalRenderPipelineAsset, UniversalRenderPipelineAsset>();
    bool dirty = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install() { if (Instance == null) new GameObject("PerformanceSettings").AddComponent<GridShiftPerformanceSettings>(); }
    void Awake() { Instance = this; DontDestroyOnLoad(gameObject); SceneManager.sceneLoaded += Loaded; }
    void Loaded(Scene scene, LoadSceneMode mode) { dirty = true; }
    void LateUpdate() { if (dirty) { dirty = false; Apply(); } }
    public static void Set(string key, float value) { PlayerPrefs.SetFloat("GS.Perf." + key, value); if (Instance != null) Instance.dirty = true; }
    public static int Preset => Mathf.Clamp(PlayerPrefs.GetInt("GS.Perf.Preset", QualitySettings.GetQualityLevel() == 0 ? 0 : 2), 0, 2);
    public static string PresetName => Preset == 0 ? "Baixa" : Preset == 1 ? "Média" : "Alta";
    public static void SelectPreset(int level)
    {
        level = Mathf.Clamp(level, 0, 2); PlayerPrefs.SetInt("GS.Perf.Preset", level);
        // The authored project has Mobile and PC pipelines. Medium uses PC with shorter shadows.
        int quality = level == 0 ? 0 : QualitySettings.names.Length - 1;
        QualitySettings.SetQualityLevel(quality); PlayerPrefs.SetInt("GS.Quality", quality);
        Set("Shadows", level); Set("Distance", level == 0 ? 300 : level == 1 ? 600 : 1000);
        Set("Effects", level == 0 ? 0 : 1);
    }
    public void Apply()
    {
        var asset = (QualitySettings.renderPipeline != null ? QualitySettings.renderPipeline : GraphicsSettings.defaultRenderPipeline) as UniversalRenderPipelineAsset;
        if (asset != null && (PlayerPrefs.HasKey("GS.Perf.Shadows") || PlayerPrefs.HasKey("GS.Perf.Effects")))
        {
            UniversalRenderPipelineAsset runtime = null;
            foreach (var pair in pipelines) if (pair.Value == asset) { runtime = asset; break; }
            if (runtime == null && !pipelines.TryGetValue(asset, out runtime)) { runtime = Instantiate(asset); runtime.name = asset.name + " (runtime preferences)"; pipelines.Add(asset, runtime); }
            QualitySettings.renderPipeline = runtime;
            if (PlayerPrefs.HasKey("GS.Perf.Shadows"))
            {
                int level = Mathf.Clamp(Mathf.RoundToInt(PlayerPrefs.GetFloat("GS.Perf.Shadows", 2)), 0, 2);
                runtime.shadowDistance = level == 0 ? 25 : level == 1 ? 60 : 100;
                runtime.mainLightShadowmapResolution = level == 0 ? 512 : level == 1 ? 1024 : 2048;
                runtime.shadowCascadeCount = level == 2 ? 4 : 2;
            }
        }
        var deadVolumes = new List<Volume>(); foreach (var pair in volumeWeights) if (pair.Key == null) deadVolumes.Add(pair.Key);
        foreach (var volume in deadVolumes) volumeWeights.Remove(volume);
        if (PlayerPrefs.HasKey("GS.Perf.Effects")) foreach (var volume in FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!volumeWeights.ContainsKey(volume)) volumeWeights.Add(volume, volume.weight);
            volume.weight = volumeWeights[volume] * Mathf.Clamp01(PlayerPrefs.GetFloat("GS.Perf.Effects", 1));
        }
        foreach (var player in FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var camera = player.CameraTransform != null ? player.CameraTransform.GetComponent<Camera>() : null;
            if (camera == null) continue;
            if (PlayerPrefs.HasKey("GS.Perf.Distance")) camera.farClipPlane = Mathf.Clamp(PlayerPrefs.GetFloat("GS.Perf.Distance", 800), 100, 1500);
            if (PlayerPrefs.HasKey("GS.Perf.Effects")) camera.GetUniversalAdditionalCameraData().renderPostProcessing = PlayerPrefs.GetFloat("GS.Perf.Effects", 1) > 0;
        }
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= Loaded;
        foreach (var pair in pipelines) { if (QualitySettings.renderPipeline == pair.Value) QualitySettings.renderPipeline = pair.Key; Destroy(pair.Value); }
        if (Instance == this) Instance = null;
    }
}
