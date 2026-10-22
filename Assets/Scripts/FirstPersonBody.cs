using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Malhas locais sem cabeça compartilham os ossos do avatar completo; outras câmeras mantêm o modelo original.
public class FirstPersonBody : MonoBehaviour
{
    [Serializable] public class BodyPart { public Renderer source; public Mesh bodyMesh; }
    [SerializeField] private Camera localCamera;
    [SerializeField] private BodyPart[] parts;
    [SerializeField] private int fullBodyLayer = 7;
    [SerializeField] private int localBodyLayer = 8;
    private readonly List<SkinnedMeshRenderer> originals = new List<SkinnedMeshRenderer>();
    private readonly List<SkinnedMeshRenderer> copies = new List<SkinnedMeshRenderer>();
    private readonly List<GameObject> generated = new List<GameObject>();
    private static bool HasTriangles(Mesh mesh)
    {
        if (mesh == null) return false;
        for (int i = 0; i < mesh.subMeshCount; i++) if (mesh.GetIndexCount(i) >= 3) return true;
        return false;
    }
    private void Awake()
    {
        if (localCamera == null || parts == null) return;
        localCamera.cullingMask = (localCamera.cullingMask & ~(1 << fullBodyLayer)) | (1 << localBodyLayer);
        foreach (BodyPart part in parts)
        {
            if (part.source == null) continue;
            part.source.gameObject.layer = fullBodyLayer;
            if (!HasTriangles(part.bodyMesh)) continue;
            GameObject go = new GameObject(part.source.name + "_CorpoLocal");
            go.layer = localBodyLayer; go.transform.SetParent(part.source.transform.parent, false);
            go.transform.localPosition = part.source.transform.localPosition; go.transform.localRotation = part.source.transform.localRotation; go.transform.localScale = part.source.transform.localScale;
            Renderer copy;
            if (part.source is SkinnedMeshRenderer skin)
            {
                var target = go.AddComponent<SkinnedMeshRenderer>(); target.sharedMesh = part.bodyMesh;
                target.bones = skin.bones; target.rootBone = skin.rootBone; target.localBounds = skin.localBounds;
                target.quality = skin.quality; target.updateWhenOffscreen = true;
                originals.Add(skin); copies.Add(target); copy = target;
            }
            else
            {
                go.AddComponent<MeshFilter>().sharedMesh = part.bodyMesh; copy = go.AddComponent<MeshRenderer>();
            }
            copy.sharedMaterials = part.source.sharedMaterials; copy.shadowCastingMode = ShadowCastingMode.Off;
            copy.receiveShadows = part.source.receiveShadows; copy.enabled = part.source.enabled;
            generated.Add(go);
        }
    }
    private void LateUpdate()
    {
        for (int i = 0; i < originals.Count; i++)
        {
            if (originals[i] == null || copies[i] == null) continue;
            copies[i].enabled = originals[i].enabled;
            for (int j = 0; j < originals[i].sharedMesh.blendShapeCount; j++) copies[i].SetBlendShapeWeight(j, originals[i].GetBlendShapeWeight(j));
        }
    }
    private void OnDestroy() { foreach (var go in generated) if (go != null) Destroy(go); }
}
