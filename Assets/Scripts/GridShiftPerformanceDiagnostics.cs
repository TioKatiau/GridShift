using System.Text;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class GridShiftPerformanceDiagnostics : MonoBehaviour
{
    public bool visible;
    float nextRefresh, totalTime; int frames;
    string display = "";
    readonly StringBuilder text = new StringBuilder(512);
    ProfilerRecorder memory, gc;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        if (!Debug.isDebugBuild) return;
        var go = new GameObject("PerformanceDiagnostics"); DontDestroyOnLoad(go); go.AddComponent<GridShiftPerformanceDiagnostics>();
    }
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame && !GridShiftNativeOperations.Focused && !GridShiftControls.Capturing)
        {
            visible = !visible; totalTime = 0; frames = 0;
            if (visible) { memory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory"); gc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame"); }
            else { memory.Dispose(); gc.Dispose(); }
        }
        if (!visible) return;
        totalTime += Time.unscaledDeltaTime; frames++;
        if (Time.unscaledTime < nextRefresh) return; nextRefresh = Time.unscaledTime + .5f;
        text.Clear(); text.Append("GRIDSHIFT · DIAGNÓSTICO · F3\nFPS ").Append(Mathf.RoundToInt(frames / Mathf.Max(.001f, totalTime)))
            .Append("   Frame médio ").Append((1000 * totalTime / frames).ToString("F2")).Append(" ms\nVeículos ativos ").Append(TrafficCar.Active.Count);
        int waiting = 0; foreach (var car in TrafficCar.Active) if (car != null && car.MotivoParada != TrafficCar.StopReason.Nenhum) waiting++;
        int pedestrians = 0; foreach (var manager in FindObjectsByType<PedestrianManager>(FindObjectsSortMode.None)) pedestrians += manager.Agents.Count;
        int deliveries = 0; if (DeliveryManager.Instance != null) foreach (var order in DeliveryManager.Instance.orders) if (order.status != DeliveryStatus.Completed) deliveries++;
        int panels=FindObjectsByType<GridShiftNativeOperations>(FindObjectsSortMode.None).Length;
        text.Append("   Aguardando ").Append(waiting).Append("\nPedestres ").Append(pedestrians).Append("   Entregas ").Append(deliveries)
            .Append("\nPainéis nativos ativos ").Append(panels).Append("\nMemória Unity ").Append(memory.Valid ? (memory.LastValue / 1048576).ToString() + " MB" : "indisponível")
            .Append("   GC/frame ").Append(gc.Valid ? gc.LastValue.ToString() + " B" : "indisponível");
        var viewer=Camera.main;var origin=viewer!=null?viewer.transform.position:Vector3.zero;
        foreach(var car in TrafficCar.Active.Where(c=>c!=null).OrderBy(c=>(c.transform.position-origin).sqrMagnitude).Take(4))
            text.Append("\n").Append(car.name).Append(" · ").Append(car.Diagnostic).Append(" · ").Append(car.VelocidadeAtual.ToString("F1")).Append(" m/s · parado ").Append(car.StationarySeconds.ToString("F1")).Append("s · ").Append(car.DiagnosticDetail).Append(" · sinal ").Append(car.RotaAtual!=null&&car.RotaAtual.GetSemaforo()!=null?car.RotaAtual.GetSemaforo().name:"nenhum").Append(" · obstáculo ").Append(car.Obstacle!=null?car.Obstacle.name:"nenhum");
        foreach(var agent in FindObjectsByType<PedestrianAgent>(FindObjectsSortMode.None).OrderBy(c=>(c.transform.position-origin).sqrMagnitude).Take(3))
            text.Append("\nNPC ").Append(agent.name).Append(" · ").Append(agent.CurrentActivity).Append(" · ").Append(agent.CurrentSpeed.ToString("F1")).Append("m/s · obstáculo ").Append(agent.Obstacle!=null?agent.Obstacle.name:"nenhum").Append(" · destino ").Append(agent.DestinationNode).Append(" / nó ").Append(agent.NextNode).Append(" · sem progresso ").Append(agent.NoProgressSeconds.ToString("F1")).Append("s · recuperação ").Append(agent.RecoveryAttempts).Append(" · recentes ").Append(string.Join(",",agent.RecentDestinations));
        display = text.ToString(); totalTime = 0; frames = 0;
    }
    void OnGUI() { if (visible) GUI.Box(new Rect(15, 15, Mathf.Min(Screen.width-30,1100), 380), display); }
    void OnDestroy() { memory.Dispose(); gc.Dispose(); }
}

