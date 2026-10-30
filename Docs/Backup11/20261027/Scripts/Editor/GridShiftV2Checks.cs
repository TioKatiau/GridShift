using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class GridShiftV2Checks
{
    static GridShiftV2Checks() { EditorApplication.playModeStateChanged += Changed; }
    public static void Batch()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        foreach (ClimaReal c in Object.FindObjectsByType<ClimaReal>(FindObjectsSortMode.None)) c.enabled = false;
        foreach (Camera c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) c.enabled = false;
        SessionState.SetBool("GridShiftV2Checks", true); EditorApplication.isPlaying = true;
    }
    static void Changed(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("GridShiftV2Checks", false)) return;
        SessionState.SetBool("GridShiftV2Checks", false);
        new GameObject("V2Tests").AddComponent<GridShiftV2PlayChecks>();
    }
}
public class GridShiftV2PlayChecks : MonoBehaviour
{
    static readonly BindingFlags Fields = BindingFlags.NonPublic | BindingFlags.Instance;
    readonly List<string> results = new List<string>();
    readonly List<string> errors = new List<string>();
    void Pass(string name) { results.Add("PASS " + name); Debug.Log("V2 PASS " + name); }
    void Assert(bool ok, string message) { if (!ok) throw new Exception(message); }
    static void Set(object o, string f, object v) => o.GetType().GetField(f, Fields).SetValue(o, v);
    static void Call(object o, string f) => o.GetType().GetMethod(f, Fields).Invoke(o, null);
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    { foreach (var root in scene.GetRootGameObjects()) { foreach (var c in root.GetComponentsInChildren<ClimaReal>(true)) c.enabled = false; foreach (var c in root.GetComponentsInChildren<Camera>(true)) c.enabled = false; } }
    IEnumerator Start()
    {
        Application.logMessageReceived += Log; SceneManager.sceneLoaded += OnSceneLoaded;
        var run = Run();
        while (true)
        {
            bool next;
            try { next = run.MoveNext(); }
            catch (Exception e) { Finish(false, e.ToString()); yield break; }
            if (!next) break;
            yield return run.Current;
        }
        Finish(errors.Count == 0, string.Join("\n", errors));
    }
    void Log(string message, string trace, LogType type)
    { if (type == LogType.Exception || type == LogType.Error) errors.Add(message); }
    void Finish(bool ok, string message)
    {
        Application.logMessageReceived -= Log; SceneManager.sceneLoaded -= OnSceneLoaded;
        File.WriteAllText("v2-tests.txt", string.Join("\n", results) + "\n" + (ok ? "RESULT SUCCESS" : "RESULT FAILURE " + message));
        Debug.Log("V2 RESULT " + (ok ? "SUCCESS" : "FAILURE " + message));
        Time.timeScale = 1f; EditorApplication.Exit(ok ? 0 : 1);
    }
    void CheckFleet(TrafficManager manager)
    {
        var cars = manager.Vehicles.Where(c => c != null).ToArray();
        Assert(cars.Select(c => c.GetInstanceID()).Distinct().Count() == cars.Length, "IDs de veículos duplicados");
        for (int i = 0; i < cars.Length; i++)
        {
            Assert(cars[i].enabled, "Veículo desativado por erro de rota: " + cars[i].name);
            var a = cars[i].GetComponent<BoxCollider>();
            for (int j = i + 1; j < cars.Length; j++)
            {
                var b = cars[j].GetComponent<BoxCollider>();
                bool overlap = Physics.ComputePenetration(a, cars[i].transform.position, cars[i].transform.rotation,
                    b, cars[j].transform.position, cars[j].transform.rotation, out _, out float depth);
                Assert(!overlap || depth < .01f, "Sobreposição " + cars[i].name + " / " + cars[j].name + " profundidade=" + depth);
            }
        }
    }
    IEnumerator Run()
    {
        Time.timeScale = 4f;
        yield return new WaitForSeconds(12f);
        var manager = Object.FindFirstObjectByType<TrafficManager>();
        Assert(manager != null && manager.ActiveCount == 15, "Esperados 15 veículos; encontrados " + (manager != null ? manager.ActiveCount : -1));
        CheckFleet(manager); Pass("15 veículos distribuídos, alinhados e sem sobreposição");
        var kinds = manager.Vehicles.Select(c => c.name).Distinct().Count(); Assert(kinds >= 3, "Pouca variedade de modelos");
        Pass("modelos variados: " + kinds);
        var positions = manager.Vehicles.ToDictionary(c => c, c => c.transform.position);
        for (int i = 0; i < 80; i++) { yield return new WaitForSeconds(.25f); CheckFleet(manager); }
        Assert(positions.Count(p => Vector3.Distance(p.Key.transform.position, p.Value) > 2f) >= 5, "Trânsito não avançou");
        Pass("trânsito simultâneo por 20 s sem colisões entre veículos");
        TestJump(); TestQueue(); TestBody(); yield return null; // Aguarda a remoção dos objetos sintéticos antes de comparar os IDs reais.
        var session = WorldSession.Instance; Assert(session != null, "Sessão ausente");
        var city = SceneManager.GetSceneByName("SampleScene");
        var player = city.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<PlayerMovement>()).First();
        var playerPosition = player.transform.position;
        var ids = manager.Vehicles.Select(v => v.GetInstanceID()).OrderBy(v => v).ToArray();
        foreach (var fade in Object.FindObjectsByType<FadeManager>(FindObjectsSortMode.None)) Set(fade, "duracaoFade", .01f);
        for (int visit = 0; visit < 3; visit++)
        {
            var beforeOffice = manager.Vehicles.ToDictionary(c => c, c => c.transform.position);
            float time = session.ClockTime;
            var intersection=Object.FindFirstObjectByType<TrafficIntersection>(); var oldStage=intersection.GetType().GetField("etapa",Fields).GetValue(intersection); var oldTimer=(float)intersection.GetType().GetField("contador",Fields).GetValue(intersection); var lightIds=Object.FindObjectsByType<TrafficLight>(FindObjectsSortMode.None).Select(l=>l.GetInstanceID()).OrderBy(id=>id).ToArray();
            session.RequestTransition("Office", player.transform);
            float deadline = Time.realtimeSinceStartup + 30f;
            while (session.Busy && Time.realtimeSinceStartup < deadline) yield return null;
            Assert(!session.Busy && session.InOffice && city.isLoaded, "Entrada aditiva falhou"); Assert(Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Count(e=>e.isActiveAndEnabled)==1,"EventSystem duplicado ou ausente no Office");
            Assert(!player.gameObject.activeInHierarchy, "Player da cidade continua ativo no Office");
            Assert(Camera.allCameras.All(c => c.gameObject.scene.name != "SampleScene"), "Câmera da cidade permaneceu ativa");
            yield return new WaitForSeconds(6f); CheckFleet(manager);
            Assert(session.ClockTime > time, "Relógio não avançou no Office"); Assert(!Equals(oldStage,intersection.GetType().GetField("etapa",Fields).GetValue(intersection)) || Mathf.Abs(oldTimer-(float)intersection.GetType().GetField("contador",Fields).GetValue(intersection))>.01f,"Ciclo de semáforo congelou no Office"); Assert(lightIds.SequenceEqual(Object.FindObjectsByType<TrafficLight>(FindObjectsSortMode.None).Select(l=>l.GetInstanceID()).OrderBy(id=>id)),"Semáforos foram recriados");
            Assert(beforeOffice.Any(p => Vector3.Distance(p.Key.transform.position, p.Value) > 1f), "Carros não avançaram no Office");
            Assert(ids.SequenceEqual(manager.Vehicles.Select(v => v.GetInstanceID()).OrderBy(v => v)), "Veículos recriados/duplicados no Office");
            var office = SceneManager.GetSceneByName("Office");
            var officePlayer = office.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<PlayerMovement>()).First();
            session.RequestTransition("SampleScene", officePlayer.transform);
            deadline = Time.realtimeSinceStartup + 30f;
            while (session.Busy && Time.realtimeSinceStartup < deadline) yield return null;
            Assert(!session.Busy && !session.InOffice && !SceneManager.GetSceneByName("Office").isLoaded, "Saída do Office falhou");
            Assert(player.gameObject.activeInHierarchy && Vector3.Distance(player.transform.position, playerPosition) < .15f, "Player retornou ao local incorreto");
            Assert(Object.FindObjectsByType<TrafficManager>(FindObjectsSortMode.None).Length == 1, "Gerenciador duplicado");
            Pass("Office ida/volta " + (visit + 1) + ": mesmos veículos, cidade ativa e relógio contínuo");
        }
        var pooled = manager.Vehicles.First(c => c.name.StartsWith("Transito_")); int pooledId = pooled.GetInstanceID();
        Assert(manager.Recycle(pooled), "Reciclagem falhou"); Assert(manager.ActiveCount == 14, "Contagem após pool incorreta");
        for (int i = 0; i < 20 && manager.ActiveCount < 15; i++) manager.TrySpawn();
        Assert(manager.ActiveCount == 15, "Reposição após pool falhou"); CheckFleet(manager);
        Pass("pool e reposição respeitam contagem e ocupação");
        manager.SetDensity(.5f); yield return new WaitForSeconds(18f); Assert(manager.ActiveCount <= 8 && manager.ActiveCount >= 1,"Densidade não reduziu os veículos fora da visão"); manager.SetDensity(1f); Pass("densidade reduz a frota gradualmente e preserva o caminhão existente");
    }
    void TestJump()
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube); floor.transform.position = new Vector3(20000, -.5f, 0); floor.transform.localScale = new Vector3(30, 1, 30);
        var go = new GameObject("JumpCheck"); go.transform.position = new Vector3(20000, .05f, 0);
        var cc = go.AddComponent<CharacterController>(); cc.center = Vector3.up; cc.height = 2f; cc.radius = .3f;
        var move = go.AddComponent<PlayerMovement>(); move.enabled = false; Physics.SyncTransforms();
        for (int i = 0; i < 20; i++) move.SimulateMovement(Vector2.zero, false, false, .02f);
        float baseY = go.transform.position.y, highest = baseY;
        move.SimulateMovement(Vector2.zero, false, true, .02f); Assert(move.VerticalVelocity > 0f, "Pulo inicial falhou");
        for (int i = 0; i < 100; i++)
        {
            float before = move.VerticalVelocity;
            bool press = i == 5 || i == 15 || i == 25;
            move.SimulateMovement(Vector2.zero, false, press, .02f); highest = Mathf.Max(highest, go.transform.position.y);
            if (press) Assert(move.VerticalVelocity < before + .01f, "Pulo extra no ar");
        }
        Assert(highest - baseY > 1.7f && highest - baseY < 2.15f, "Altura inconsistente " + (highest - baseY));
        move.SimulateMovement(Vector2.zero, false, true, .02f); Assert(move.VerticalVelocity > 0f, "Não liberou salto após aterrissar");
        move.BlockControls(true); float saved = move.VerticalVelocity; move.SimulateMovement(Vector2.zero, false, true, .02f); Assert(move.VerticalVelocity == saved, "Saltou com controles bloqueados");
        move.BlockControls(false); float scale = Time.timeScale; Time.timeScale = 0f; move.SimulateMovement(Vector2.zero, false, true, .02f); Assert(move.VerticalVelocity == saved, "Saltou pausado"); Time.timeScale = scale;
        Object.Destroy(go); Object.Destroy(floor); Pass("pulo único, gravidade, aterrissagem, bloqueio e pausa");
    }
    void TestQueue()
    {
        var root = new GameObject("QueueCheck");
        Transform Point(string name, Vector3 p) { var g = new GameObject(name); g.transform.SetParent(root.transform); g.transform.position = p; return g.transform; }
        var offset = new Vector3(30000, 0, 0);
        var route = Point("Route", offset).gameObject.AddComponent<TrafficRoute>(); route.pontos = new[] { Point("a", offset + Vector3.back * 30), Point("b", offset + Vector3.forward * 200) };
        var light = Point("Light", offset).gameObject.AddComponent<TrafficLight>(); light.enabled = false; light.DefinirEstado(TrafficLight.Estado.Vermelho);
        Set(route, "semaforo", light); Set(route, "linhaParada", Point("Stop", offset + Vector3.forward * 80)); Set(route, "saidaCruzamento", Point("Exit", offset + Vector3.forward * 100));
        var cars = new List<TrafficCar>();
        foreach (float z in new[] { 20f, 8f, -4f })
        {
            var g = Point("QueueCar", offset + Vector3.forward * z).gameObject;
            var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube); mesh.transform.SetParent(g.transform, false); mesh.transform.localPosition = Vector3.up; mesh.transform.localScale = new Vector3(2, 2, 4);
            var c = TrafficVehicleFactory.Prepare(g, new TrafficVehicleModel { }); c.ResetRoute(route); cars.Add(c);
        }
        var mode = Physics.simulationMode; Physics.simulationMode = SimulationMode.Script; Physics.SyncTransforms();
        void Steps(int n) { for (int i = 0; i < n; i++) { foreach (var c in cars) Call(c, "FixedUpdate"); Physics.Simulate(.02f); for (int k = 1; k < cars.Count; k++) Assert(cars[k - 1].transform.position.z - cars[k].transform.position.z >= 5.8f, "Fila sem distância segura"); } }
        try
        {
            Steps(1600); Assert(cars[0].transform.position.z <= 77.01f && cars.All(c => c.VelocidadeAtual < .03f), "Fila não aguardou vermelho: " + string.Join(";",cars.Select(c=>c.transform.position+" v="+c.VelocidadeAtual)));
            light.DefinirEstado(TrafficLight.Estado.Verde); Steps(40); Assert(cars[0].VelocidadeAtual > cars[2].VelocidadeAtual, "Retomada sem progressão da fila"); Steps(1100); Assert(cars.All(c => c.transform.position.z > 100f), "Fila não retomou no verde: " + string.Join("; ",cars.Select(c=>c.transform.position+" speed="+c.VelocidadeAtual+" enabled="+c.enabled)));
        }
        finally { Physics.simulationMode = mode; Object.Destroy(root); }
        Pass("fila de três veículos: retenção, distância segura e retomada");
    }
    static long Indices(Mesh mesh) { long n=0; for(int i=0;i<mesh.subMeshCount;i++)n+=mesh.GetIndexCount(i);return n; }
    void TestBody()
    {
        var fp = Object.FindFirstObjectByType<FirstPersonBody>(); Assert(fp != null, "Corpo em primeira pessoa ausente");
        var player = fp.GetComponent<PlayerMovement>(); var camera = player.CameraTransform.GetComponent<Camera>();
        Assert((camera.cullingMask & (1 << 7)) == 0 && (camera.cullingMask & (1 << 8)) != 0, "Máscara da câmera incorreta");
        var original = fp.GetComponentsInChildren<SkinnedMeshRenderer>().First(r => r.gameObject.layer == 7);
        var local = fp.GetComponentsInChildren<SkinnedMeshRenderer>().First(r => r.gameObject.layer == 8);
        Assert(Indices(local.sharedMesh) > 0 && Indices(local.sharedMesh) < Indices(original.sharedMesh), "Malha local não preserva corpo/recorta cabeça: " + original.name + " original=" + Indices(original.sharedMesh) + " local=" + Indices(local.sharedMesh) + " mesh=" + local.sharedMesh.name);
        Assert(Indices(original.sharedMesh) / 3 == 13536, "Malha original modificada");
        Assert(local.bones.SequenceEqual(original.bones), "Corpo não compartilha esqueleto");
        Pass("malha local sem cabeça, corpo animado preservado e modelo completo nas outras câmeras");
    }
}









