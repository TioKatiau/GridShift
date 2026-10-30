using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;

public static class GridShiftV3Setup {
 const string Generated="Assets/Generated/CityLife";
 static readonly List<string> report=new List<string>();
 [MenuItem("GridShift/Atualização 3.0/Configurar cidade e terminal")]
 public static void Configure(){
  if(EditorApplication.isPlaying)throw new Exception("Saia do Play Mode antes de configurar.");
  if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
  var previous=EditorSceneManager.GetSceneManagerSetup();
  try{
   Directory.CreateDirectory(Generated);AssetDatabase.Refresh();Layer();
   var minimap=AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/MinimapTexture.renderTexture");
   if(minimap!=null&&minimap.depth==0){minimap.depth=24;EditorUtility.SetDirty(minimap);}
   foreach(var name in new[]{"SampleScene","Office"}){
    var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");Physics.SyncTransforms();
    var player=Object.FindFirstObjectByType<PlayerMovement>();
    foreach(var listener in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))listener.enabled=listener.transform==player.CameraTransform;
    if(name=="SampleScene")City(player);else Office(player);
    EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
   }
   AssetDatabase.SaveAssets();File.WriteAllLines("v3-setup.txt",report);Debug.Log("V3 CONFIGURED\n"+string.Join("\n",report));
  }finally{if(!Application.isBatchMode)EditorSceneManager.RestoreSceneManagerSetup(previous);}
 }
 public static void Batch(){Configure();EditorApplication.Exit(0);}
 static void Layer(){var so=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);var p=so.FindProperty("layers").GetArrayElementAtIndex(9);if(!string.IsNullOrEmpty(p.stringValue)&&p.stringValue!="Pedestrian")throw new Exception("Layer 9 ocupada");p.stringValue="Pedestrian";so.ApplyModifiedPropertiesWithoutUndo();}
 static T Get<T>(string name)where T:Component{var result=Object.FindFirstObjectByType<T>();return result!=null?result:new GameObject(name).AddComponent<T>();}
  static Material Material(string name,Color color,bool emission=false){string path=Generated+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);var shader=Shader.Find(emission?"Universal Render Pipeline/Lit":"Universal Render Pipeline/Unlit");if(shader==null)shader=Shader.Find(emission?"Standard":"Unlit/Color");if(m==null){m=new Material(shader);AssetDatabase.CreateAsset(m,path);}else m.shader=shader;m.color=color;if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);if(emission){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color);}EditorUtility.SetDirty(m);return m;}
 static void City(PlayerMovement player){
  // O ComputerScreen da cidade é legado, não um segundo terminal utilizável.
  foreach(var c in Object.FindObjectsByType<ComputerInteraction>(FindObjectsSortMode.None))c.enabled=false;
  var graph=Get<PedestrianGraph>("PedestrianPaths");BuildGraph(graph);
  var manager=Get<PedestrianManager>("PedestrianManager");manager.graph=graph;manager.player=player.transform;manager.playerCamera=player.CameraTransform.GetComponent<Camera>();manager.controller=Animator();
  manager.models=AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Hodaart/CasualCharacterCollection3/Prefabs/Characters"}).Select(g=>AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g))).OrderBy(g=>g.name).ToArray();
  var environment=Get<CityEnvironment>("CityEnvironment");environment.traffic=Object.FindFirstObjectByType<TrafficManager>();environment.pedestrians=manager;environment.player=player.transform;
  string skyPath=Generated+"/CitySky.mat";var sky=AssetDatabase.LoadAssetAtPath<Material>(skyPath);var skyShader=Shader.Find("GridShift/City Sky");if(sky==null&&skyShader!=null){sky=new Material(skyShader);AssetDatabase.CreateAsset(sky,skyPath);}environment.sky=sky!=null?sky:RenderSettings.skybox;
  environment.nightIntensity=9;environment.nightAmbient=new Color(.18f,.23f,.34f);
  var lamps=new List<StreetLamp>();var bulbMat=Material("WarmLamp",new Color(1,.75f,.4f),true);
  foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Props_Street Light")).ToArray()){
   var t=r.transform.Find("GridShiftLamp");if(t==null){t=new GameObject("GridShiftLamp").transform;t.SetParent(r.transform,false);t.localPosition=new Vector3(2f,6.05f,0);}
   var light=t.GetComponent<Light>();if(light==null)light=t.gameObject.AddComponent<Light>();light.type=LightType.Spot;light.color=new Color(1,.78f,.5f);light.range=16;light.spotAngle=110;light.shadows=LightShadows.None;light.intensity=0;light.enabled=false;light.transform.rotation=Quaternion.LookRotation(Vector3.down,r.transform.forward);
   var bulb=t.Find("Bulb");if(bulb==null){var sphere=GameObject.CreatePrimitive(PrimitiveType.Sphere);sphere.name="Bulb";Object.DestroyImmediate(sphere.GetComponent<Collider>());bulb=sphere.transform;bulb.SetParent(t,false);bulb.localScale=new Vector3(.22f,.08f,.15f);}
   var br=bulb.GetComponent<Renderer>();br.sharedMaterial=bulbMat;br.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
   lamps.Add(new StreetLamp{light=light,bulb=br,offset=Mathf.Repeat(r.transform.position.x*.127f+r.transform.position.z*.319f,1)*.25f});
  }
  environment.lamps=lamps.ToArray();report.Add("Postes identificados pelo prefab Props_Street Light: "+lamps.Count+"; sem sombras; orçamento 24 luzes.");
  report.Add("Modelos pedestres: "+manager.models.Length+"; animações Idle/Walk originais em controlador independente.");
 }
 static RuntimeAnimatorController Animator(){string path=Generated+"/Pedestrian.controller";var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);if(controller!=null)return controller;
  controller=AnimatorController.CreateAnimatorControllerAtPath(path);controller.AddParameter("Speed",AnimatorControllerParameterType.Float);var state=controller.layers[0].stateMachine.AddState("Locomotion");var tree=new BlendTree{name="IdleWalk",blendParameter="Speed",blendType=BlendTreeType.Simple1D,useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(tree,controller);
  const string clips="Assets/Hodaart/CasualCharacterCollection3/Animations/Characters_Animations/";
  tree.AddChild(AssetDatabase.LoadAssetAtPath<AnimationClip>(clips+"A_Idle A.anim"),0);tree.AddChild(AssetDatabase.LoadAssetAtPath<AnimationClip>(clips+"A_Walk A.anim"),1);state.motion=tree;controller.layers[0].stateMachine.defaultState=state;return controller;
 }
 static bool Ground(Vector3 p,out Vector3 ground){
  ground=p;var hits=Physics.RaycastAll(new Vector3(p.x,-1.5f,p.z),Vector3.down,4,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance);
  foreach(var h in hits){if(h.collider is CharacterController)continue;if(!h.collider.name.StartsWith("Road "))return false;if(h.normal.y<.95f||h.point.y< -4.45f)return false;ground=h.point+Vector3.up*.025f;
   foreach(var offset in new[]{Vector3.right*.24f,Vector3.left*.24f,Vector3.forward*.24f,Vector3.back*.24f})if(!Physics.Raycast(ground+offset+Vector3.up,Vector3.down,out var edge,2,~0,QueryTriggerInteraction.Ignore)||!edge.collider.name.StartsWith("Road ")||Mathf.Abs(edge.point.y-h.point.y)>.1f)return false;
   return !Physics.CheckCapsule(ground+Vector3.up*.4f,ground+Vector3.up*1.55f,.3f,~0,QueryTriggerInteraction.Ignore);}return false;
 }
 static bool Segment(Vector3 a,Vector3 b){
  float distance=Vector3.Distance(a,b);int samples=Mathf.CeilToInt(distance/.6f);
  for(int s=1;s<samples;s++)if(!Ground(Vector3.Lerp(a,b,(float)s/samples),out _))return false;
  return !Physics.CapsuleCast(a+Vector3.up*.4f,a+Vector3.up*1.55f,.29f,(b-a).normalized,distance,~0,QueryTriggerInteraction.Ignore);
 }
 static void BuildGraph(PedestrianGraph graph){
  foreach(var child in graph.GetComponentsInChildren<Transform>().Where(t=>t!=graph.transform&&t.name.StartsWith("Entrance_")).ToArray())Object.DestroyImmediate(child.gameObject);
  var nodes=new List<PedestrianNode>();var links=new List<HashSet<int>>();var grid=new Dictionary<Vector2Int,int>();
  int Add(Vector3 p){var key=new Vector2Int(Mathf.RoundToInt(p.x*5),Mathf.RoundToInt(p.z*5));if(grid.TryGetValue(key,out int old))return old;int i=nodes.Count;nodes.Add(new PedestrianNode{position=p});links.Add(new HashSet<int>());grid[key]=i;return i;}
  var roads=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Road ")&&!r.name.Contains("Split Line")).ToArray();
  foreach(var r in roads){
   var bounds=r.bounds;
   // Amostra a geometria elevada real. Asfalto abaixo do meio-fio é rejeitado.
   for(float x=bounds.min.x+.5f;x<=bounds.max.x;x+=1.5f)for(float z=bounds.min.z+.5f;z<=bounds.max.z;z+=1.5f){var p=new Vector3(x,0,z);if(Ground(p,out var g))Add(g);}
  }
  for(int i=0;i<nodes.Count;i++)for(int j=i+1;j<nodes.Count;j++){float d=(nodes[i].position-nodes[j].position).sqrMagnitude;if(d>.2f&&d<2.3f*2.3f&&Segment(nodes[i].position,nodes[j].position)){links[i].Add(j);links[j].Add(i);}}
  var crossings=new List<PedestrianCrossing>();var routes=Object.FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None);
  foreach(var junction in Object.FindObjectsByType<TrafficJunction>(FindObjectsSortMode.None)){
   var connected=routes.Where(r=>r.Cruzamento==junction).ToArray();var signals=connected.Select(r=>r.GetSemaforo()).Where(s=>s!=null).Distinct().ToArray();
   foreach(var dir in new[]{Vector3.forward,Vector3.right,Vector3.back,Vector3.left}){
    // Só há faixa em braços de rua presentes na rede de movimentos.
    if(!connected.Any(r=>r.GetParada(out var line,out _)&&Vector3.Dot((line-junction.transform.position).normalized,dir)>.8f))continue;
    var across=Vector3.Cross(Vector3.up,dir);var center=junction.transform.position+dir*8.5f;
    int Nearest(Vector3 p){int best=-1;float dist=3.5f*3.5f;for(int i=0;i<nodes.Count;i++){var q=nodes[i].position-p;q.y=0;if(q.sqrMagnitude<dist){dist=q.sqrMagnitude;best=i;}}return best;}
    int a=Nearest(center+across*8.5f),b=Nearest(center-across*8.5f);if(a<0||b<0||a==b||Vector3.Distance(nodes[a].position,nodes[b].position)<12)continue;
    if(Physics.CapsuleCast(nodes[a].position+Vector3.up*.6f,nodes[a].position+Vector3.up*1.5f,.3f,(nodes[b].position-nodes[a].position).normalized,Vector3.Distance(nodes[a].position,nodes[b].position),~0,QueryTriggerInteraction.Ignore))continue;
    links[a].Add(b);links[b].Add(a);crossings.Add(new PedestrianCrossing{a=a,b=b,junction=junction,signals=signals});
   }
  }
  // Os quatro modelos possuem um painel de porta vertical de 1,52 x 2,64 m.
  // Extrai sua posição da malha, em vez de presumir que a porta fica no centro.
  foreach(var building in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Building_House")||r.name.StartsWith("Building House"))){
   var mesh=building.GetComponent<MeshFilter>()?.sharedMesh;if(mesh==null)continue;var v=mesh.vertices;var triangles=mesh.triangles;Vector3 door=Vector3.zero,normal=Vector3.zero;bool found=false;
   for(int t=0;t<triangles.Length;t+=3){var a=v[triangles[t]];var b=v[triangles[t+1]];var c=v[triangles[t+2]];var bounds=new Bounds(a,Vector3.zero);bounds.Encapsulate(b);bounds.Encapsulate(c);var n=Vector3.Cross(b-a,c-a).normalized;
    if(bounds.size.y>2.5f&&bounds.size.y<2.8f&&bounds.min.y<1.2f&&bounds.size.x>1.4f&&bounds.size.x<1.7f&&n.z>.95f){door=building.transform.TransformPoint(new Vector3(bounds.center.x,bounds.min.y,bounds.center.z));normal=building.transform.TransformDirection(n);found=true;break;}}
   if(!found)continue;Vector3 approach=door+normal*.65f;
   foreach(int start in Enumerable.Range(0,nodes.Count).Where(i=>!nodes[i].entrancePath&&links[i].Count>0&&(nodes[i].position-approach).sqrMagnitude<15*15).OrderBy(i=>(nodes[i].position-approach).sqrMagnitude).Take(30)){
    Vector3 from=nodes[start].position;int steps=Mathf.CeilToInt(Vector3.Distance(from,approach)/.35f);var samples=new List<Vector3>();bool valid=true;
    for(int s=1;s<=steps;s++){var point=Vector3.Lerp(from,approach,(float)s/steps);var previous=samples.Count==0?from:samples[samples.Count-1];
     if(!Physics.Raycast(point+Vector3.up*2,Vector3.down,out var hit,4,~0,QueryTriggerInteraction.Ignore)||hit.normal.y<.85f||Mathf.Abs(hit.point.y+.025f-previous.y)>.35f){valid=false;break;}
     point.y=hit.point.y+.025f;if(Physics.CheckCapsule(point+Vector3.up*.65f,point+Vector3.up*1.55f,.29f,~0,QueryTriggerInteraction.Ignore)){valid=false;break;}samples.Add(point);
    }
    if(!valid||samples.Count==0)continue;int last=start;
    foreach(var point in samples){int added=Add(point);if(added==last)continue;nodes[added].entrancePath=true;links[last].Add(added);links[added].Add(last);last=added;}nodes[last].entrance=true;
    var marker=new GameObject("Entrance_"+building.name).transform;marker.SetParent(graph.transform);marker.position=samples[samples.Count-1];marker.rotation=Quaternion.LookRotation(-normal);report.Add("Entrada navegável "+building.name+" porta="+door+" chegada="+marker.position);break;
   }
  }
  for(int i=0;i<nodes.Count;i++)nodes[i].neighbours=links[i].ToArray();graph.nodes=nodes.ToArray();graph.crossings=crossings.ToArray();
  report.Add("Grafo independente: "+nodes.Count+" pontos de calçada, "+links.Sum(l=>l.Count)/2+" ligações verificadas no piso; "+crossings.Count+" travessias; "+nodes.Count(n=>n.entrance)+" destinos junto às fachadas.");
 }
 static void Office(PlayerMovement player){
  var monitors=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Old Monitor")).OrderBy(r=>Vector3.Distance(r.transform.position,player.transform.position)).ToArray();
  if(monitors.Length==0)throw new Exception("Monitor existente não encontrado");
  var monitor=monitors[0];var terminal=monitor.GetComponent<ComputerInteraction>();if(terminal==null)terminal=monitor.gameObject.AddComponent<ComputerInteraction>();
  terminal.player=player.transform;terminal.playerMovement=player;terminal.playerCamera=player.CameraTransform.GetComponent<Camera>();terminal.interactionDistance=2;terminal.zoomDuration=.8f;
  var children=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.gameObject.scene==player.gameObject.scene).ToArray();
  terminal.minimap=children.FirstOrDefault(t=>t.name=="Minimap")?.gameObject;terminal.compass=children.FirstOrDefault(t=>t.name=="Compass")?.gameObject;terminal.Aim=children.FirstOrDefault(t=>t.name=="Aim")?.gameObject;
  terminal.hudRoot=children.FirstOrDefault(t=>t.name=="Hud")?.gameObject;terminal.hudBottom=children.FirstOrDefault(t=>t.name=="HudBottom")?.gameObject;
  if(Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>()==null){var events=new GameObject("OfficeEventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>();events.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();}
  var mf=monitor.GetComponent<MeshFilter>();var mesh=mf.sharedMesh;var indices=mesh.GetIndices(1);var vertices=mesh.vertices;var normals=mesh.normals;
  var center=Vector3.zero;var normal=Vector3.zero;foreach(int i in indices){center+=vertices[i];normal+=normals[i];}center/=indices.Length;normal.Normalize();
  Vector3 screen=monitor.transform.TransformPoint(center),outward=monitor.transform.TransformDirection(normal).normalized;
  if(Mathf.Abs(outward.y)>.5f||outward.sqrMagnitude<.5f)outward=-monitor.transform.forward;
  Vector3 up=Vector3.up;var right=Vector3.Cross(up,-outward).normalized;float minX=float.PositiveInfinity,maxX=float.NegativeInfinity,minY=minX,maxY=maxX;
  foreach(int i in indices){var p=monitor.transform.TransformPoint(vertices[i])-screen;float x=Vector3.Dot(p,right),y=Vector3.Dot(p,up);minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);}
  float depth=0;foreach(int i in indices)depth=Mathf.Max(depth,Vector3.Dot(monitor.transform.TransformPoint(vertices[i])-screen,outward));
  screen+=outward*(depth+.015f);
  var mats=monitor.sharedMaterials;mats[1]=Material("MonitorBlack",Color.black);monitor.sharedMaterials=mats;
  var view=monitor.transform.Find("TerminalView");if(view==null){view=new GameObject("TerminalView").transform;view.SetParent(monitor.transform);}
  view.position=screen+outward*.85f;view.rotation=Quaternion.LookRotation(-outward,up);terminal.computerViewPoint=view;
  var old=monitor.transform.Find("TerminalScreen");if(old!=null)Object.DestroyImmediate(old.gameObject);
  var canvas=new GameObject("TerminalScreen",typeof(RectTransform),typeof(Canvas),typeof(CanvasGroup),typeof(GraphicRaycaster));canvas.transform.SetParent(monitor.transform);canvas.transform.SetPositionAndRotation(screen+outward*.008f,Quaternion.LookRotation(-outward,up));canvas.transform.localScale=Vector3.one*.001f;
  var rect=canvas.GetComponent<RectTransform>();rect.sizeDelta=new Vector2((maxX-minX)*1000,(maxY-minY)*1000);
  canvas.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;canvas.GetComponent<Canvas>().worldCamera=terminal.playerCamera;terminal.screenContent=canvas.GetComponent<CanvasGroup>();terminal.screenContent.alpha=0;
  var text=new GameObject("TerminalStatus",typeof(TextMeshPro));text.transform.SetParent(canvas.transform,false);var tr=text.GetComponent<RectTransform>();tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;var tmp=text.GetComponent<TextMeshPro>();tmp.text="Navegador indisponível.\nESC para sair.";tmp.fontSize=150;tmp.alignment=TextAlignmentOptions.Center;tmp.color=Color.white;tmp.raycastTarget=false;
  string textMaterialPath=Generated+"/TerminalText.mat";var textMaterial=AssetDatabase.LoadAssetAtPath<Material>(textMaterialPath);if(textMaterial==null){textMaterial=new Material(tmp.fontSharedMaterial);AssetDatabase.CreateAsset(textMaterial,textMaterialPath);}textMaterial.SetFloat("_ZTestMode",8);textMaterial.SetFloat("_StencilComp",8);tmp.fontSharedMaterial=textMaterial;tmp.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;terminal.statusText=tmp;
  var prompt=monitor.transform.Find("TerminalPrompt");if(prompt==null){var go=new GameObject("TerminalPrompt",typeof(TextMeshPro));prompt=go.transform;prompt.SetParent(monitor.transform);}
  prompt.position=screen+up*.45f;prompt.rotation=terminal.playerCamera.transform.rotation;var label=prompt.GetComponent<TextMeshPro>();label.text="[E] Acessar";label.fontSize=2.5f;label.alignment=TextAlignmentOptions.Center;label.rectTransform.sizeDelta=new Vector2(2,.4f);terminal.interactionText=prompt.gameObject;
#if GRIDSHIFT_VUPLEX
  var browser=Vuplex.WebView.CanvasWebViewPrefab.Instantiate();browser.transform.SetParent(canvas.transform,false);var webRect=browser.GetComponent<RectTransform>();webRect.anchorMin=Vector2.zero;webRect.anchorMax=Vector2.one;webRect.offsetMin=webRect.offsetMax=Vector2.zero;browser.KeyboardEnabled=true;
  var adapter=monitor.GetComponent<VuplexTerminalWebView>();if(adapter==null)adapter=monitor.gameObject.AddComponent<VuplexTerminalWebView>();adapter.browser=browser;terminal.webView=adapter;browser.gameObject.SetActive(false);
#endif
  report.Add("Terminal no monitor existente "+monitor.name+" em "+monitor.transform.position+"; tela "+rect.sizeDelta+"; câmera "+view.position+"; WebView externo ainda ausente.");
 }
}
