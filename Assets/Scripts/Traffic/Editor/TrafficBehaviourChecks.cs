using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

// Testes reais com Physics.Simulate, em cena isolada; disponíveis pelo menu e em batch.
public static class TrafficBehaviourChecks
{
    static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    static GameObject raiz;
    static int aprovados;
    static void Set(object o, string field, object value) => o.GetType().GetField(field, Flags).SetValue(o, value);
    static void Call(object o, string method) => o.GetType().GetMethod(method, Flags).Invoke(o, null);
    static void Assert(bool value, string message) { if (!value) throw new Exception(message); }
    static Transform P(string name, Vector3 position)
    { var g = new GameObject(name); g.transform.SetParent(raiz.transform); g.transform.position = position; return g.transform; }
    static TrafficRoute Route(string name, params Vector3[] points)
    {
        var r = P(name, Vector3.zero).gameObject.AddComponent<TrafficRoute>();
        r.pontos = new Transform[points.Length];
        for (int i = 0; i < points.Length; i++) r.pontos[i] = P(name + i, points[i]);
        return r;
    }
    static TrafficCar Car(TrafficRoute route)
    {
        var g = P("Truck", route.pontos[0].position).gameObject;
        g.transform.rotation = Quaternion.LookRotation(route.pontos[1].position - route.pontos[0].position);
        var box = g.AddComponent<BoxCollider>(); box.size = new Vector3(2f, 2f, 6f); box.center = Vector3.up * 1.1f;
        var car = g.AddComponent<TrafficCar>();
        Set(car, "rota", route); Set(car, "distanciaDoPonto", 0.25f);
        Call(car, "Start"); Physics.SyncTransforms(); return car;
    }
    static TrafficLight Signal(TrafficRoute route, TrafficLight.Estado state, float z = 20f)
    {
        var light = P("Light", Vector3.zero).gameObject.AddComponent<TrafficLight>();
        light.DefinirEstado(state);
        Set(route, "semaforo", light); Set(route, "linhaParada", P("Stop", Vector3.forward * z));
        Set(route, "saidaCruzamento", P("Exit", Vector3.forward * (z + 15f))); return light;
    }
    static void Steps(TrafficCar car, int count)
    {
        for (int i = 0; i < count; i++) { if (car.enabled) Call(car, "FixedUpdate"); Physics.Simulate(Time.fixedDeltaTime); }
    }
    static void Test(string name, Action body)
    {
        raiz = new GameObject("Traffic checks");
        try { body(); aprovados++; Debug.Log("TRAFFIC PASS: " + name); }
        finally { Object.DestroyImmediate(raiz); Physics.SyncTransforms(); }
    }
    [MenuItem("GridShift/Tráfego/Executar testes em cena isolada")]
    public static void Run()
    {
        if (EditorApplication.isPlaying) throw new Exception("Saia do Play Mode antes dos testes.");
        if (Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length > 0)
            throw new Exception("Abra uma cena vazia para executar os testes sem interferência dos Colliders do mapa.");
        SimulationMode oldMode = Physics.simulationMode;
        UnityEngine.Random.State oldRandom = UnityEngine.Random.state;
        aprovados = 0;
        try
        {
            Physics.simulationMode = SimulationMode.Script;
            Test("vermelho: frente antes da faixa e espera", () => {
                var r = Route("Straight", Vector3.zero, Vector3.forward * 80); Signal(r, TrafficLight.Estado.Vermelho);
                var c = Car(r); Steps(c, 1500);
                Assert(c.transform.position.z <= 16.01f && c.transform.position.z > 14f, "Posição de parada incorreta: " + c.transform.position + " speed=" + c.VelocidadeAtual + " enabled=" + c.enabled);
                Assert(c.VelocidadeAtual < .01f, "Não parou"); float z = c.transform.position.z; Steps(c, 100);
                Assert(Mathf.Abs(c.transform.position.z - z) < .001f, "Avançou no vermelho");
            });
            Test("verde: retoma progressivamente", () => {
                var r = Route("Straight", Vector3.zero, Vector3.forward * 80); var s = Signal(r, TrafficLight.Estado.Vermelho);
                var c = Car(r); Steps(c, 900); s.DefinirEstado(TrafficLight.Estado.Verde); Steps(c, 1);
                Assert(c.VelocidadeAtual <= .04f, "Aceleração instantânea"); Steps(c, 500);
                Assert(c.transform.position.z > 35f, "Não cruzou no verde");
            });
            Test("amarelo distante: para", () => {
                var r = Route("Straight", Vector3.zero, Vector3.forward * 80); Signal(r, TrafficLight.Estado.Amarelo);
                var c = Car(r); Steps(c, 1200); Assert(c.transform.position.z <= 16.01f, "Avançou amarelo distante");
            });
            Test("amarelo próximo: termina travessia após vermelho", () => {
                var r = Route("Straight", Vector3.zero, Vector3.forward * 80); var s = Signal(r, TrafficLight.Estado.Amarelo);
                var c = Car(r); c.GetComponent<Rigidbody>().position = Vector3.forward * 16.9f; Set(c, "velocidadeAtual", 6f);
                Physics.SyncTransforms(); Steps(c, 5); s.DefinirEstado(TrafficLight.Estado.Vermelho); Steps(c, 250);
                Assert(c.transform.position.z > 35f, "Parou dentro do cruzamento");
            });
            Test("reta longa: aceleração limitada", () => {
                var c = Car(Route("Straight", Vector3.zero, Vector3.forward * 200)); Steps(c, 1);
                Assert(c.VelocidadeAtual <= .031f, "Aceleração fora do limite"); Steps(c, 500);
                Assert(c.VelocidadeAtual > 7.9f && c.VelocidadeAtual <= 8f, "Velocidade máxima incorreta");
            });
            Test("obstáculo parado: freia e retoma", () => {
                var c = Car(Route("Straight", Vector3.zero, Vector3.forward * 100));
                var block = P("Obstacle", new Vector3(0, 1, 25)).gameObject.AddComponent<BoxCollider>(); block.size = new Vector3(3,2,2);
                Physics.SyncTransforms(); Steps(c, 1000);
                Assert(c.transform.position.z < 19.01f && c.VelocidadeAtual < .01f, "Atravessou obstáculo");
                block.enabled = false; Physics.SyncTransforms(); Steps(c, 300); Assert(c.transform.position.z > 30f, "Não retomou");
            });
            Test("saída ocupada: aguarda antes da faixa", () => {
                var r = Route("Straight", Vector3.zero, Vector3.forward * 80); Signal(r, TrafficLight.Estado.Verde);
                var block = P("Queue", new Vector3(0,1,40)).gameObject.AddComponent<BoxCollider>(); block.size = new Vector3(2,2,6);
                var c = Car(r); Steps(c, 1200); Assert(c.transform.position.z <= 16.01f, "Bloqueou cruzamento");
            });
            Test("conexões variadas: sem salto ou retorno", () => {
                var r = Route("Approach", Vector3.zero, Vector3.forward * 10);
                var a = Route("A", Vector3.forward * 10, Vector3.forward * 20);
                var b = Route("B", Vector3.forward * 10, new Vector3(5,0,15));
                Route("Invalid far", new Vector3(20,0,10), new Vector3(20,0,20));
                Route("U turn", Vector3.forward * 10, Vector3.zero);
                bool seenA = false, seenB = false;
                for (int i = 0; i < 100; i++) { var next = r.EncontrarProximaRota(); Assert(next == a || next == b, "Conexão inválida"); seenA |= next == a; seenB |= next == b; }
                Assert(seenA && seenB, "Sem variedade");
            });
            foreach (int side in new[] { -1, 1 }) Test("curva suave " + side, () => {
                var pts = new System.Collections.Generic.List<Vector3> { Vector3.zero, Vector3.forward * 10 };
                for (int i = 1; i <= 90; i += 3) { float t = i * Mathf.Deg2Rad; pts.Add(new Vector3(side * 12f * (1f - Mathf.Cos(t)), 0, 10f + 12f * Mathf.Sin(t))); }
                pts.Add(new Vector3(side * 12f, 0, 22)); pts.Add(new Vector3(side * 60f, 0, 22));
                var c = Car(Route("Turn", pts.ToArray())); Steps(c, 1600);
                Assert(c.enabled && Mathf.Abs(c.transform.position.x) > 30f && Mathf.Abs(c.transform.position.z - 22f) < .8f, "Não completou curva na faixa");
            });
            Debug.Log("TRAFFIC RESULT: " + aprovados + " testes aprovados.");
        }
        finally
        {
            Physics.simulationMode = oldMode;
            UnityEngine.Random.state = oldRandom;
        }
    }
    public static void RunBatch()
    {
        try { Run(); EditorApplication.Exit(0); }
        catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(1); }
    }
}

