using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object = UnityEngine.Object;

public static class GridShiftV2Setup
{
    const string Models = "Assets/SimplePoly City - Low Poly Assets/Prefab/Vehicles/Vehicle with Separated Wheels";
    static void Reference(Object o, string name, Object value) { var s = new SerializedObject(o); s.FindProperty(name).objectReferenceValue = value; s.ApplyModifiedPropertiesWithoutUndo(); }
    [MenuItem("GridShift/Atualização 2.0/Configurar cidade e jogador")]
    public static void Configure()
    {
        EditorSceneManager.SaveOpenScenes();
        var previous = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            ConfigureLayers();
            foreach (string sceneName in new[] { "SampleScene", "Office" })
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity");
                var player = Object.FindFirstObjectByType<PlayerMovement>();
                if (player == null) throw new Exception("PlayerMovement ausente: " + sceneName);
                ConfigurePlayer(player, sceneName);
                if (sceneName == "SampleScene") ConfigureCity(player);
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets(); Debug.Log("GRIDSHIFT V2 CONFIGURED");
        }
        finally { if (!Application.isBatchMode) EditorSceneManager.RestoreSceneManagerSetup(previous); }
    }
    static void ConfigureLayers()
    {
        var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tags.FindProperty("layers");
        foreach (var pair in new[] { (7, "PlayerFullBody"), (8, "PlayerLocalBody") })
        {
            var entry = layers.GetArrayElementAtIndex(pair.Item1);
            if (!string.IsNullOrEmpty(entry.stringValue) && entry.stringValue != pair.Item2) throw new Exception("Layer já utilizada: " + pair.Item1);
            entry.stringValue = pair.Item2;
        }
        tags.ApplyModifiedPropertiesWithoutUndo();
    }
    static void ConfigureCity(PlayerMovement player)
    {
        var manager = Object.FindFirstObjectByType<TrafficManager>();
        if (manager == null) manager = new GameObject("TrafficManager").AddComponent<TrafficManager>();
        var session = Object.FindFirstObjectByType<WorldSession>();
        if (session == null) new GameObject("WorldSession").AddComponent<WorldSession>();
        Reference(manager, "jogador", player.transform);
        var models = new List<TrafficVehicleModel>();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { Models }).OrderBy(g => AssetDatabase.GUIDToAssetPath(g)))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            Bounds b = TrafficVehicleFactory.Measure(prefab);
            if (b.size.x > 3.7f || b.size.z > 9.2f || b.size.y > 4.5f || b.size.z < 1f) continue;
            bool heavy = b.size.z > 7f;
            models.Add(new TrafficVehicleModel { prefab = prefab, pesado = heavy,
                velocidade = heavy ? new Vector2(4.8f, 6f) : new Vector2(5.2f, 7f),
                aceleracao = heavy ? new Vector2(1f, 1.4f) : new Vector2(1.3f, 2f), frenagem = new Vector2(3f, 4f) });
        }
        var ms = new SerializedObject(manager); var array = ms.FindProperty("modelos"); array.arraySize = models.Count;
        for (int i = 0; i < models.Count; i++)
        {
            var entry = array.GetArrayElementAtIndex(i); var model = models[i];
            entry.FindPropertyRelative("prefab").objectReferenceValue = model.prefab;
            entry.FindPropertyRelative("pesado").boolValue = model.pesado;
            entry.FindPropertyRelative("velocidade").vector2Value = model.velocidade;
            entry.FindPropertyRelative("aceleracao").vector2Value = model.aceleracao;
            entry.FindPropertyRelative("frenagem").vector2Value = model.frenagem;
        }
        ms.ApplyModifiedPropertiesWithoutUndo();
        var junctionRoot = GameObject.Find("TrafficJunctions"); if (junctionRoot == null) junctionRoot = new GameObject("TrafficJunctions");
        var centers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r => r.name.StartsWith("Road ") && (r.name.Contains("Intersection") || r.name.Contains("Corner"))).Select(r => r.transform.position).ToArray();
        var junctions = new Dictionary<string, TrafficJunction>();
        foreach (TrafficRoute route in Object.FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None))
        {
            if (!route.Valida) continue;
            Vector3 end = route.pontos[route.pontos.Length - 1].position;
            Vector3 center = centers.OrderBy(p => Vector3.Distance(p, end)).First(); center.y = end.y;
            string key = $"Cruzamento_{center.x:F1}_{center.z:F1}";
            if (!junctions.TryGetValue(key, out TrafficJunction j))
            {
                var t = junctionRoot.transform.Find(key);
                if (t == null) { t = new GameObject(key).transform; t.SetParent(junctionRoot.transform); }
                t.position = center; j = t.GetComponent<TrafficJunction>(); if (j == null) j = t.gameObject.AddComponent<TrafficJunction>(); junctions.Add(key, j);
            }
            Reference(route, "cruzamento", j);
            Vector3 incoming = (route.pontos[1].position - route.pontos[0].position).normalized;
            Vector3 outgoing = (end - route.pontos[route.pontos.Length - 2].position).normalized;
            var rs = new SerializedObject(route);
            foreach (var item in new[] { ("linhaParada", center - incoming * 12f + Vector3.Cross(Vector3.up, incoming) * 3.2f, incoming),
                ("saidaCruzamento", center + outgoing * 12f + Vector3.Cross(Vector3.up, outgoing) * 3.2f, outgoing) })
            {
                var field = rs.FindProperty(item.Item1);
                if (field.objectReferenceValue == null)
                { var marker = new GameObject(item.Item1).transform; marker.SetParent(route.transform); marker.position = item.Item2; marker.rotation = Quaternion.LookRotation(item.Item3); field.objectReferenceValue = marker; }
            }
            rs.ApplyModifiedPropertiesWithoutUndo();
        }
        Debug.Log("V2 TRAFFIC: " + models.Count + " modelos; " + junctions.Count + " cruzamentos; alvo inicial 15 veículos incluindo caminhão existente.");
    }
    static void ConfigurePlayer(PlayerMovement player, string sceneName)
    {
        var camera = player.CameraTransform.GetComponent<Camera>();
        camera.transform.SetParent(player.transform, true); camera.nearClipPlane = .03f;
        // Mantém a altura de olhos e a sensibilidade já configuradas; retira somente os polígonos da cabeça.
        foreach (var collider in player.GetComponents<Collider>()) if (!(collider is CharacterController)) { collider.enabled = false; PrefabUtility.RecordPrefabInstancePropertyModifications(collider); }
        var head = player.GetComponentsInChildren<Transform>(true).First(t => t.name == "Head");
        float cutoff = head.position.y - .02f;
        var renderers = player.GetComponentsInChildren<Renderer>(true).Where(r => r is SkinnedMeshRenderer || (r is MeshRenderer && r.GetComponent<MeshFilter>() != null && AssetDatabase.Contains(r.GetComponent<MeshFilter>().sharedMesh))).ToArray();
        if (!AssetDatabase.IsValidFolder("Assets/Generated")) AssetDatabase.CreateFolder("Assets", "Generated");
        if (!AssetDatabase.IsValidFolder("Assets/Generated/FirstPerson")) AssetDatabase.CreateFolder("Assets/Generated", "FirstPerson");
        var fp = player.GetComponent<FirstPersonBody>(); if (fp == null) fp = player.gameObject.AddComponent<FirstPersonBody>();
        var serialized = new SerializedObject(fp); serialized.FindProperty("localCamera").objectReferenceValue = camera;
        var parts = serialized.FindProperty("parts"); parts.arraySize = renderers.Length;
        for (int index = 0; index < renderers.Length; index++)
        {
            Renderer renderer = renderers[index];
            Mesh original = renderer is SkinnedMeshRenderer skin ? skin.sharedMesh : renderer.GetComponent<MeshFilter>().sharedMesh;
            var body = Object.Instantiate(original); body.name = original.name + "_SemCabeca";
            Vector3[] vertices;
            Mesh baked = null;
            if (renderer is SkinnedMeshRenderer skinned) { baked = new Mesh(); skinned.BakeMesh(baked); vertices = baked.vertices; }
            else vertices = original.vertices;
            int before = 0, after = 0;
            for (int sub = 0; sub < original.subMeshCount; sub++)
            {
                var source = original.GetTriangles(sub); var keep = new List<int>(); before += source.Length;
                for (int i = 0; i < source.Length; i += 3)
                    if (renderer.transform.TransformPoint(vertices[source[i]]).y < cutoff && renderer.transform.TransformPoint(vertices[source[i + 1]]).y < cutoff && renderer.transform.TransformPoint(vertices[source[i + 2]]).y < cutoff)
                    { keep.Add(source[i]); keep.Add(source[i + 1]); keep.Add(source[i + 2]); }
                body.SetTriangles(keep, sub); after += keep.Count;
            }
            if (baked != null) Object.DestroyImmediate(baked);
            string assetPath = $"Assets/Generated/FirstPerson/{sceneName}_{index:00}.asset";
            Mesh asset = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (asset == null) { AssetDatabase.CreateAsset(body, assetPath); asset = body; }
            else { EditorUtility.CopySerialized(body, asset); Object.DestroyImmediate(body); EditorUtility.SetDirty(asset); }
            parts.GetArrayElementAtIndex(index).FindPropertyRelative("source").objectReferenceValue = renderer;
            parts.GetArrayElementAtIndex(index).FindPropertyRelative("bodyMesh").objectReferenceValue = asset;
            renderer.gameObject.layer = 7; PrefabUtility.RecordPrefabInstancePropertyModifications(renderer.gameObject);
            Debug.Log($"V2 BODY {sceneName} {renderer.name}: {before / 3} -> {after / 3} triângulos");
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
        foreach (var other in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            other.cullingMask = other == camera ? (other.cullingMask & ~(1 << 7)) | (1 << 8) : (other.cullingMask | (1 << 7)) & ~(1 << 8);
            PrefabUtility.RecordPrefabInstancePropertyModifications(other);
        }
        PrefabUtility.RecordPrefabInstancePropertyModifications(camera.transform); PrefabUtility.RecordPrefabInstancePropertyModifications(camera);
    }
    public static void Batch()
    {
        try { Configure(); EditorApplication.Exit(0); }
        catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }
}
