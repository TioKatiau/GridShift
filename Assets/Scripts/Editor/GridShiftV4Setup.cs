using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;

public static class GridShiftV4Setup {
 const string Folder="Assets/Generated/TrafficSignals";
 static readonly List<string> report=new List<string>();
 [MenuItem("GridShift/Atualização 4.0/Configurar melhorias")]
 public static void Configure(){
  if(EditorApplication.isPlaying)throw new Exception("Saia do Play Mode.");
  if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
  var previous=EditorSceneManager.GetSceneManagerSetup();
  try{Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
   foreach(string name in new[]{"SampleScene","Office"}){
    var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");
    if(Object.FindFirstObjectByType<FadeManager>()==null)new GameObject("FadeManager").AddComponent<FadeManager>();
    foreach(var player in Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None)){
     var animator=player.GetComponent<Animator>();if(animator!=null)animator.applyRootMotion=false;
     player.GetComponent<CharacterController>().minMoveDistance=0;
     var so=new SerializedObject(player);if(so.FindProperty("moveSpeed").floatValue==5)so.FindProperty("moveSpeed").floatValue=5.4f;
     if(so.FindProperty("sprintSpeed").floatValue==8)so.FindProperty("sprintSpeed").floatValue=8.6f;so.ApplyModifiedPropertiesWithoutUndo();
     if(player.CameraTransform!=null){var motions=player.CameraTransform.GetComponents<PlayerCameraMotion>();for(int i=1;i<motions.Length;i++)Object.DestroyImmediate(motions[i]);var motion=player.CameraTransform.GetComponent<PlayerCameraMotion>();if(motion==null)motion=player.CameraTransform.gameObject.AddComponent<PlayerCameraMotion>();motion.player=player;motion.view=player.CameraTransform.GetComponent<Camera>();}
     foreach(var text in player.GetComponentsInChildren<TMPro.TMP_Text>(true)){
      // Mantém hierarquia, cores e posições; permite uma pequena redução em números longos.
      if(text.text.Contains("<")||text.fontSize<=0)continue;
      float original=text.enableAutoSizing?text.fontSizeMax:text.fontSize;text.enableAutoSizing=true;text.fontSizeMax=original;text.fontSizeMin=original*.9f;
     }
     foreach(var canvas in player.GetComponentsInChildren<Canvas>(true))if(canvas.renderMode!=RenderMode.WorldSpace){var scaler=canvas.GetComponent<UnityEngine.UI.CanvasScaler>();if(scaler!=null){scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;}}
    }
    if(name=="SampleScene"){
     int poles=0;foreach(var lamp in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Props_Street Light")))if(!lamp.GetComponentsInChildren<Collider>(true).Any(c=>c.enabled&&!c.isTrigger)){
      var shaft=lamp.gameObject.AddComponent<CapsuleCollider>();shaft.radius=.17f;shaft.height=6.2f;shaft.center=new Vector3(0,3.1f,0);poles++;
     }report.Add("Postes com colisor de haste acrescentado: "+poles);
     Signals();var population=Object.FindFirstObjectByType<PedestrianManager>();if(population!=null){if(population.daytimeCount==28)population.daytimeCount=34;population.excludedEntrances=Object.FindObjectsByType<CompanyEntrance>(FindObjectsSortMode.None);}
     if(population!=null&&population.graph!=null){Physics.SyncTransforms();typeof(GridShiftV3Setup).GetMethod("BuildGraph",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,new object[]{population.graph});report.Add("Grafo regenerado com margem de apoio de 24 cm junto às bordas.");}
     var env=Object.FindFirstObjectByType<CityEnvironment>();if(env!=null)foreach(var routine in env.routine)if(routine.pedestrians<=.121f)routine.pedestrians=.18f;
    }
    EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
   }
   AssetDatabase.SaveAssets();File.WriteAllLines("v4-setup.txt",report);
  }finally{if(!Application.isBatchMode)EditorSceneManager.RestoreSceneManagerSetup(previous);}
 }
 public static void Batch(){Configure();EditorApplication.Exit(0);}
 static Material Bulb(string label,Color color){var path=Folder+"/"+label+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Unlit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",color);EditorUtility.SetDirty(m);return m;}
 static void Signals(){
  var intersection=Object.FindFirstObjectByType<TrafficIntersection>();if(intersection==null)throw new Exception("Controlador de cruzamentos ausente.");
  var iso=new SerializedObject(intersection);var h=iso.FindProperty("grupoHorizontal").objectReferenceValue as TrafficLightGroup;var v=iso.FindProperty("grupoVertical").objectReferenceValue as TrafficLightGroup;
  if(h==null||v==null)throw new Exception("Grupos existentes inválidos.");
  var horizontal=new List<TrafficLight>();var vertical=new List<TrafficLight>();
  var existingHeads=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Props_Traffic Signal")).ToList();
  foreach(var route in Object.FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None)){
   if(route.Cruzamento==null||!route.GetParada(out var line,out var dir))continue;
   if(existingHeads.Any(r=>Vector3.Distance(r.transform.position,route.Cruzamento.transform.position)<22&&Vector3.Dot(-r.transform.forward,dir)>.7f))continue;
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimplePoly City - Low Poly Assets/Prefab/Props/Props_Traffic Signal_small.prefab");
   if(prefab==null)throw new Exception("Modelo de semáforo pequeno ausente.");var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.name="Props_Traffic Signal_small_Approach";go.transform.position=new Vector3(line.x,-4.4f,line.z)+Vector3.Cross(Vector3.up,dir)*5.6f;go.transform.rotation=Quaternion.LookRotation(-dir);
   existingHeads.Add(go.GetComponent<MeshRenderer>());report.Add("Cabeça adicionada na aproximação de "+route.name);
  }
  var mats=new[]{Bulb("Red",new Color(1,.025f,.015f)),Bulb("Amber",new Color(1,.68f,.015f)),Bulb("Green",new Color(.025f,1,.06f))};
  int added=0,repaired=0;
  foreach(var renderer in existingHeads){
   bool small=renderer.name.Contains("small");var light=renderer.GetComponent<TrafficLight>();if(light==null){light=renderer.gameObject.AddComponent<TrafficLight>();added++;}
   var so=new SerializedObject(light);string[] fields={"luzVermelha","luzAmarela","luzVerde"};
   for(int i=0;i<3;i++){
    var prop=so.FindProperty(fields[i]);var bulb=prop.objectReferenceValue as GameObject;
    if(bulb==null){bulb=GameObject.CreatePrimitive(PrimitiveType.Sphere);bulb.name="Signal_"+fields[i];bulb.transform.SetParent(renderer.transform,false);
     // Centros medidos na malha original: três discos verticais no modelo pequeno.
     bulb.transform.localPosition=small?new Vector3(0,5.38f-i*.435f,.205f):new Vector3(2.23f-i*.44f,5.28f,.27f);
     bulb.transform.localScale=small?new Vector3(.32f,.32f,.08f):new Vector3(.3f,.3f,.12f);prop.objectReferenceValue=bulb;
    }
    foreach(var c in bulb.GetComponentsInChildren<Collider>(true))c.enabled=false;
    foreach(var r in bulb.GetComponentsInChildren<Renderer>(true)){r.enabled=true;r.sharedMaterial=mats[i];r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;}
    foreach(var l in bulb.GetComponentsInChildren<Light>(true)){l.enabled=true;l.shadows=LightShadows.None;l.range=2;l.intensity=1;}
   }
   so.ApplyModifiedPropertiesWithoutUndo();light.DefinirEstado(TrafficLight.Estado.Vermelho);repaired++;
   (Mathf.Abs(renderer.transform.forward.x)>.7f?horizontal:vertical).Add(light);
  }
  SetGroup(h,horizontal);SetGroup(v,vertical);
  // Cada aproximação usa a cabeça voltada para seu motorista, no mesmo cruzamento.
  foreach(var route in Object.FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None)){
   if(route.Cruzamento==null||!route.GetParada(out var line,out var dir))continue;
   var candidates=horizontal.Concat(vertical).Where(l=>Vector3.Distance(l.transform.position,route.Cruzamento.transform.position)<22&&Vector3.Dot(-l.transform.forward,dir)>.7f).OrderBy(l=>(l.transform.position-line).sqrMagnitude).ToArray();
   if(candidates.Length==0){report.Add("SEM CABEÇA COMPATÍVEL: "+route.name);continue;}
   var so=new SerializedObject(route);so.FindProperty("semaforo").objectReferenceValue=candidates[0];so.ApplyModifiedPropertiesWithoutUndo();
  }
  report.Add("Semáforos: "+repaired+"; novos controladores: "+added+"; horizontal: "+horizontal.Count+"; vertical: "+vertical.Count+". Lâmpadas visíveis e sem colisão.");
 }
 static void SetGroup(TrafficLightGroup group,List<TrafficLight> lights){var so=new SerializedObject(group);var a=so.FindProperty("semaforos");a.arraySize=lights.Count;for(int i=0;i<lights.Count;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=lights[i];so.ApplyModifiedPropertiesWithoutUndo();}
}
