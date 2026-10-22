using UnityEditor;
using UnityEngine;

public static class TrafficSetup
{
    [MenuItem("GridShift/Tráfego/Preparar veículo selecionado")]
    private static void Preparar()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null || go.GetComponent<TrafficCar>() == null)
        { Debug.LogError("Selecione o objeto que contém TrafficCar."); return; }
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = Undo.AddComponent<Rigidbody>(go);
        Undo.RecordObject(rb, "Configurar tráfego");
        rb.isKinematic = true; rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        BoxCollider box = go.GetComponent<BoxCollider>();
        if (box == null)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) { Debug.LogError("Veículo sem renderers para medir carroceria."); return; }
            Bounds bounds = new Bounds(); bool primeiro = true;
            foreach (Renderer renderer in renderers)
            {
                Bounds local = renderer.localBounds;
                for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 p = go.transform.InverseTransformPoint(renderer.transform.TransformPoint(local.center + Vector3.Scale(local.extents, new Vector3(x, y, z))));
                    if (primeiro) { bounds = new Bounds(p, Vector3.zero); primeiro = false; }
                    else bounds.Encapsulate(p);
                }
            }
            box = Undo.AddComponent<BoxCollider>(go);
            box.center = bounds.center; box.size = bounds.size;
        }
        Undo.RecordObject(box, "Configurar carroceria");
        box.enabled = true; box.isTrigger = false;
        SerializedObject car = new SerializedObject(go.GetComponent<TrafficCar>());
        car.FindProperty("carroceria").objectReferenceValue = box;
        car.ApplyModifiedProperties();
        PrefabUtility.RecordPrefabInstancePropertyModifications(rb);
        PrefabUtility.RecordPrefabInstancePropertyModifications(box);
        Debug.Log("Carroceria preparada. Confira limites, eixo +Z e altura; salve a cena.", go);
    }

    [MenuItem("GridShift/Tráfego/Validar cena")]
    private static void Validar()
    {
        int problemas = 0;
        foreach (TrafficRoute route in Object.FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None))
        {
            if (!route.Valida) { Debug.LogError("Pontos ausentes/repetidos: " + route.name, route); problemas++; continue; }
            if (route.GetSemaforo() != null && (!route.GetParada(out _, out _) || !route.TemSaidaCruzamento))
            { Debug.LogError("Configure linhaParada (ou pontoDoSemaforo) e saidaCruzamento: " + route.name, route); problemas++; }
            Random.State estado = Random.state;
            TrafficRoute proxima = route.EncontrarProximaRota();
            Random.state = estado;
            if (proxima == null) { Debug.LogWarning("Sem conexão contínua de saída: " + route.name, route); problemas++; }
            for (int i = 1; i < route.pontos.Length - 1; i++)
            {
                Vector3 a = route.pontos[i].position - route.pontos[i - 1].position;
                Vector3 b = route.pontos[i + 1].position - route.pontos[i].position;
                if (Vector3.Angle(a, b) > 25f)
                { Debug.LogWarning("Revise raio da curva em " + route.name + ", ponto [" + i + "] " + route.pontos[i].name, route.pontos[i]); problemas++; }
            }
        }
        foreach (TrafficCar car in Object.FindObjectsByType<TrafficCar>(FindObjectsSortMode.None))
            if (car.GetComponent<BoxCollider>() == null || car.GetComponent<Rigidbody>() == null)
            { Debug.LogError("Prepare este veículo pelo menu GridShift/Tráfego: " + car.name, car); problemas++; }
        Debug.Log("Validação de tráfego: " + problemas + " pendências. Clique nas mensagens para localizar os objetos. Valide também em Play Mode.");
    }
}
