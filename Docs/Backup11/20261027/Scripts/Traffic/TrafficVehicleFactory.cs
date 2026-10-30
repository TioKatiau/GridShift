using UnityEngine;

[System.Serializable]
public class TrafficVehicleModel
{
    public GameObject prefab;
    public bool pesado;
    public Vector2 velocidade = new Vector2(4.5f, 6.5f);
    public Vector2 aceleracao = new Vector2(1.1f, 1.8f);
    public Vector2 frenagem = new Vector2(3f, 4f);
}

public static class TrafficVehicleFactory
{
    public static Bounds Measure(GameObject go)
    {
        Bounds bounds = new Bounds(); bool first = true;
        foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>(true))
        {
            Bounds local = renderer.localBounds;
            for (int x = -1; x <= 1; x += 2) for (int y = -1; y <= 1; y += 2) for (int z = -1; z <= 1; z += 2)
            {
                Vector3 p = go.transform.InverseTransformPoint(renderer.transform.TransformPoint(local.center + Vector3.Scale(local.extents, new Vector3(x, y, z))));
                if (first) { bounds = new Bounds(p, Vector3.zero); first = false; } else bounds.Encapsulate(p);
            }
        }
        return bounds;
    }
    public static TrafficCar Prepare(GameObject go, TrafficVehicleModel profile)
    {
        Rigidbody body = go.GetComponent<Rigidbody>(); if (body == null) body = go.AddComponent<Rigidbody>();
        body.isKinematic = true; body.useGravity = false; body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        foreach (Collider collider in go.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        BoxCollider box = go.GetComponent<BoxCollider>(); if (box == null) box = go.AddComponent<BoxCollider>();
        Bounds bounds = Measure(go); box.center = bounds.center; box.size = bounds.size; box.enabled = true; box.isTrigger = false;
        int layer = LayerMask.NameToLayer("TrafficCar"); if (layer >= 0) go.layer = layer;
        foreach (Animator animator in go.GetComponentsInChildren<Animator>(true)) animator.applyRootMotion = false;
        TrafficCar driver = go.GetComponent<TrafficCar>(); if (driver == null) driver = go.AddComponent<TrafficCar>();
        driver.ConfigureProfile(box, profile.pesado, Random.Range(profile.velocidade.x, profile.velocidade.y),
            Random.Range(profile.aceleracao.x, profile.aceleracao.y), Random.Range(profile.frenagem.x, profile.frenagem.y));
        return driver;
    }
}
