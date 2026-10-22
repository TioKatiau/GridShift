using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficManager : MonoBehaviour
{
    [SerializeField] private TrafficVehicleModel[] modelos;
    [SerializeField, Min(0)] private int quantidadeInicial = 15;
    [SerializeField, Min(1)] private int quantidadeMaxima = 30;
    [SerializeField, Range(0f, 2f)] private float densidade = 1f;
    [SerializeField, Min(.2f)] private float intervaloGeracao = 2f;
    [SerializeField, Min(5f)] private float distanciaDoJogador = 18f;
    [SerializeField] private Transform jogador;
    private TrafficRoute[] routes;
    private readonly List<TrafficCar> active = new List<TrafficCar>();
    private readonly Dictionary<int, Queue<TrafficCar>> pools = new Dictionary<int, Queue<TrafficCar>>();
    private readonly Dictionary<TrafficCar, int> origins = new Dictionary<TrafficCar, int>();
    private readonly Collider[] occupants = new Collider[64];
    private Transform poolRoot;
    private Camera playerView;
    private int authoredCount;
    private float routineDensity = 1f;
    public void SetRoutineDensity(float value) { routineDensity = Mathf.Clamp(value, .05f, 2f); }
    private readonly Plane[] viewPlanes = new Plane[6];
    public int ActiveCount => active.Count;
    public int TargetCount => Mathf.Max(authoredCount, Mathf.Min(quantidadeMaxima, Mathf.RoundToInt(quantidadeInicial * densidade * routineDensity)));
    public IReadOnlyList<TrafficCar> Vehicles => active;
    private void Awake()
    {
        routes = FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None);
        poolRoot = new GameObject("PoolVeiculos").transform; poolRoot.SetParent(transform); poolRoot.gameObject.SetActive(false);
        foreach (TrafficCar car in FindObjectsByType<TrafficCar>(FindObjectsSortMode.None))
            if (car.gameObject.scene == gameObject.scene && car.gameObject.activeInHierarchy) active.Add(car);
        authoredCount = active.Count;
        var player = jogador != null ? jogador.GetComponent<PlayerMovement>() : null;
        if (player != null && player.CameraTransform != null) playerView = player.CameraTransform.GetComponent<Camera>();
    }
    private IEnumerator Start()
    {
        if (modelos == null || modelos.Length == 0) { Debug.LogError("Gerenciador sem modelos de veículos.", this); yield break; }
        while (active.Count < TargetCount)
        {
            if (!TrySpawn()) break;
            yield return new WaitForSeconds(Random.Range(.12f, .45f));
        }
        while (true)
        {
            yield return new WaitForSeconds(intervaloGeracao);
            active.RemoveAll(car => car == null);
            if (active.Count < TargetCount) TrySpawn();
            else if (active.Count > TargetCount) RecycleHidden();
        }
    }
    public bool TrySpawn()
    {
        if (active.Count >= quantidadeMaxima || modelos == null || modelos.Length == 0 || routes == null || routes.Length == 0) return false;
        int modelIndex = Random.Range(0, modelos.Length); var model = modelos[modelIndex];
        if (model.prefab == null) return false;
        if (!pools.TryGetValue(modelIndex, out var pool)) { pool = new Queue<TrafficCar>(); pools.Add(modelIndex, pool); }
        TrafficCar car;
        if (pool.Count > 0) car = pool.Dequeue();
        else
        {
            var go = Instantiate(model.prefab, poolRoot); go.name = "Transito_" + model.prefab.name;
            car = TrafficVehicleFactory.Prepare(go, model); origins[car] = modelIndex;
        }
        BoxCollider box = car.GetComponent<BoxCollider>(); Bounds body = new Bounds(box.center, box.size);
        for (int attempt = 0; attempt < 100; attempt++)
        {
            TrafficRoute route = routes[Random.Range(0, routes.Length)];
            if (route == null || !route.Valida || !route.isActiveAndEnabled) continue;
            Vector3 a = route.pontos[0].position, b = route.pontos[1].position;
            Vector3 direction = (b - a).normalized; float length = Vector3.Distance(a, b);
            // Os pontos iniciais ficam após a faixa anterior. Limita o spawn antes da próxima faixa.
            float available = length - body.size.z - 3f;
            if (route.Cruzamento != null)
                available = Mathf.Min(available, Vector3.Dot(route.Cruzamento.transform.position - a, direction) - 12f - body.extents.z - Mathf.Abs(body.center.z) - 4f);
            else if (route.GetParada(out Vector3 stop, out _))
                available = Mathf.Min(available, Vector3.Dot(stop - a, direction) - body.extents.z - Mathf.Abs(body.center.z) - 4f);
            if (available < .5f) continue;
            Vector3 position = a + direction * Random.Range(0f, available); Quaternion rotation = Quaternion.LookRotation(direction);
            if (jogador != null && Vector3.Distance(jogador.position, position) < distanciaDoJogador) continue;
            if (playerView != null && playerView.isActiveAndEnabled) {
                GeometryUtility.CalculateFrustumPlanes(playerView, viewPlanes);
                if (GeometryUtility.TestPlanesAABB(viewPlanes, new Bounds(position + rotation * body.center, body.size))) continue;
            }
            bool crowded = false;
            foreach (TrafficCar other in active)
                if (other != null && (other.transform.position - position).sqrMagnitude < 18f * 18f) { crowded = true; break; }
            if (crowded) continue;
            Vector3 extents = body.extents; extents.x += .15f; extents.z += 3f; extents.y = Mathf.Max(.05f, extents.y - .1f);
            Physics.SyncTransforms();
            int count = Physics.OverlapBoxNonAlloc(position + rotation * body.center, extents, occupants, rotation, ~0, QueryTriggerInteraction.Ignore);
            if (count > 0) continue;
            car.gameObject.SetActive(false); car.transform.SetParent(transform, true);
            car.transform.SetPositionAndRotation(position, rotation);
            car.ResetRoute(route); car.gameObject.SetActive(true); active.Add(car);
            return true;
        }
        car.gameObject.SetActive(false); car.transform.SetParent(poolRoot); pool.Enqueue(car); return false;
    }
    public void SetDensity(float value) { densidade = Mathf.Clamp(value, 0f, 2f); }
    public bool Recycle(TrafficCar car)
    {
        if (car == null || !origins.TryGetValue(car, out int model) || !active.Remove(car)) return false;
        car.gameObject.SetActive(false); car.transform.SetParent(poolRoot); pools[model].Enqueue(car); return true;
    }
    private void RecycleHidden()
    {
        bool viewActive = playerView != null && playerView.enabled && playerView.gameObject.activeInHierarchy;
        if (viewActive) GeometryUtility.CalculateFrustumPlanes(playerView, viewPlanes);
        for (int i = active.Count - 1; i >= 0; i--)
        {
            var car = active[i]; if (car == null || !origins.ContainsKey(car)) continue;
            bool visible = false;
            foreach (Renderer renderer in car.GetComponentsInChildren<Renderer>())
                if (viewActive && GeometryUtility.TestPlanesAABB(viewPlanes, renderer.bounds)) { visible = true; break; }
            if (!visible && (jogador == null || Vector3.Distance(jogador.position, car.transform.position) > 40f)) { Recycle(car); return; }
        }
    }
}
