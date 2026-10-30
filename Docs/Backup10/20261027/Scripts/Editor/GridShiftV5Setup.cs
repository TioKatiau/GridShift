using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using VoltstroStudios.UnityWebBrowser;
using Object=UnityEngine.Object;
public static class GridShiftV5Setup {
 const string Folder="Assets/Generated/Experience5";
 public static void Audit(){EditorSceneManager.OpenScene("Assets/Scenes/Office.unity");File.WriteAllLines("v5-geometry.txt",Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.bounds.size.x>2||r.bounds.size.z>2||r.name.ToLower().Contains("wall")).Select(r=>r.name+" | "+r.bounds+" | root "+r.transform.root.name));EditorApplication.Exit(0);}
 [MenuItem("GridShift/Atualização 5.0/Configurar experiência")]
 public static void Configure(){Directory.CreateDirectory(Folder);AssetDatabase.Refresh();GridShiftUwbSetup.Configure();var terminal=Object.FindFirstObjectByType<ComputerInteraction>();var adapter=(GridShiftUwbTerminal)terminal.webView;adapter.pageZoom=150;
  var browserPrefab=BrowserPrefab(adapter);var model=TabletModel();Office();ConfigurePlayer(model,browserPrefab);EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");ConfigurePlayer(model,browserPrefab);
  // Reuse the existing visual repair without changing route graphs or simulation parameters.
  typeof(GridShiftV4Setup).GetMethod("Signals",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,null);
  foreach(var name in new[]{"Red","Amber","Green"}){var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Generated/TrafficSignals/"+name+".mat");mat.SetColor("_BaseColor",mat.GetColor("_BaseColor")*2.2f);EditorUtility.SetDirty(mat);}
  EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();File.WriteAllText("v5-setup.txt","Office e SampleScene configuradas: zoom 150%, tablet e referências, HUD e corrida, cobertura e luminárias locais. Semáforos padronizados pelo reparo visual existente.");
 }
 public static void Batch(){Configure();EditorApplication.Exit(0);}
 static Material Mat(string name,Color color,bool unlit=false){var path=Folder+"/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}mat.SetColor("_BaseColor",color);if(!unlit){mat.SetFloat("_Smoothness",.25f);mat.SetFloat("_Metallic",name=="Graphite"?.35f:0);}EditorUtility.SetDirty(mat);return mat;}
 static GameObject Cube(string name,Transform parent,Vector3 position,Vector3 size,Material mat,bool collider=false){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=mat;if(!collider)Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
 static GameObject TabletModel(){var go=new GameObject("TabletViewModel");var graphite=Mat("Graphite",new Color(.035f,.045f,.06f));var glass=Mat("TabletGlass",new Color(.006f,.012f,.02f),true);Cube("Body",go.transform,Vector3.zero,new Vector3(.74f,.76f,.035f),graphite);Cube("Glass",go.transform,new Vector3(0,0,-.02f),new Vector3(.7f,.7f,.008f),glass);Cube("Camera",go.transform,new Vector3(0,.364f,-.023f),new Vector3(.012f,.006f,.003f),glass);
  var screen=new GameObject("TabletScreen",typeof(RectTransform),typeof(Canvas),typeof(CanvasGroup),typeof(GraphicRaycaster));screen.transform.SetParent(go.transform,false);screen.transform.localPosition=new Vector3(0,0,-.026f);screen.transform.localScale=Vector3.one*.0015f;screen.GetComponent<RectTransform>().sizeDelta=new Vector2(459.25f,459.25f);screen.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;screen.GetComponent<CanvasGroup>().alpha=0;
  var prefab=PrefabUtility.SaveAsPrefabAsset(go,Folder+"/TabletViewModel.prefab");Object.DestroyImmediate(go);return prefab;}
 static GridShiftUwbTerminal BrowserPrefab(GridShiftUwbTerminal source){var root=new GameObject("PortableBrowser");var host=root.AddComponent<GridShiftUwbTerminal>();host.resolution=new Vector2Int(1600,1000);host.pageZoom=150;host.inputTemplate=source.inputTemplate;var viewport=new GameObject("BrowserViewport",typeof(RectTransform),typeof(RectMask2D));viewport.transform.SetParent(root.transform,false);var child=new GameObject("UrbanLinkBrowser",typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage));child.transform.SetParent(viewport.transform,false);var image=child.GetComponent<RawImage>();image.uvRect=new Rect(0,0,1,-1);var browser=child.AddComponent<WebBrowserUIBasic>();EditorUtility.CopySerialized(source.browser,browser);browser.enabled=false;host.browser=browser;var router=child.AddComponent<GridShiftUwbInputRouter>();router.browser=browser;router.input=source.inputTemplate;host.inputRouter=router;var r=child.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
  var prefab=PrefabUtility.SaveAsPrefabAsset(root,Folder+"/PortableBrowser.prefab");Object.DestroyImmediate(root);return prefab.GetComponent<GridShiftUwbTerminal>();}
 static void ConfigurePlayer(GameObject model,GridShiftUwbTerminal browser){foreach(var player in Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None)){var tablet=player.GetComponent<PortableTablet>();if(tablet==null)tablet=player.gameObject.AddComponent<PortableTablet>();tablet.player=player;tablet.view=player.CameraTransform.GetComponent<Camera>();tablet.modelPrefab=model;tablet.browserPrefab=browser;
  var so=new SerializedObject(player);var speed=so.FindProperty("moveSpeed");var run=so.FindProperty("sprintSpeed");if(speed.floatValue<=5.4f||Mathf.Approximately(speed.floatValue,5.8f))speed.floatValue=5.4f;if(run.floatValue<=8.6f||Mathf.Approximately(run.floatValue,9.6f))run.floatValue=9;so.ApplyModifiedPropertiesWithoutUndo();
  var camera=player.CameraTransform;var motion=camera.GetComponent<PlayerCameraMotion>();if(motion==null)motion=camera.gameObject.AddComponent<PlayerCameraMotion>();motion.player=player;motion.view=tablet.view;motion.amplitude=.042f;motion.frequency=12;motion.runFov=4;
  foreach(var hud in player.GetComponentsInChildren<HUDParallax>(true)){
   if(hud.name=="HudMotion"){hud.player=player;continue;}var original=hud.transform;var parent=original.parent;var wrapper=parent is RectTransform?new GameObject("HudMotion",typeof(RectTransform)):new GameObject("HudMotion");wrapper.transform.SetParent(parent,false);if(wrapper.transform is RectTransform rect){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;}original.SetParent(wrapper.transform,false);var combined=wrapper.AddComponent<HUDParallax>();EditorUtility.CopySerialized(hud,combined);combined.player=player;Object.DestroyImmediate(hud);
  }
 }}
 static void Office(){
  var old=GameObject.Find("OfficeCoverage5");if(old!=null)Object.DestroyImmediate(old);var root=new GameObject("OfficeCoverage5");var floors=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("floor_tile")).ToArray();if(floors.Length==0)throw new Exception("Pisos do Office não encontrados.");var bounds=floors[0].bounds;foreach(var floor in floors)bounds.Encapsulate(floor.bounds);
  var walls=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("wall_standard")).ToArray();float height=walls.Max(r=>r.bounds.max.y);var ceiling=Mat("OfficeCeiling",new Color(.68f,.64f,.57f));Cube("OfficeRoof",root.transform,new Vector3(bounds.center.x,height+.13f,bounds.center.z),new Vector3(bounds.size.x+.55f,.26f,bounds.size.z+.55f),ceiling,true);
  var rim=Mat("FixtureRim",new Color(.19f,.2f,.22f));var glow=Mat("FixtureGlow",new Color(.95f,.88f,.73f)*1.3f,true);var lights=new List<Light>();
  for(float x=bounds.min.x+1.9f;x<bounds.max.x;x+=3.8f)for(float z=bounds.min.z+1.9f;z<bounds.max.z;z+=3.8f){var at=new Vector3(x,height-.08f,z);Cube("CeilingFixture",root.transform,at,new Vector3(.78f,.08f,.37f),rim);Cube("Diffuser",root.transform,at+Vector3.down*.045f,new Vector3(.7f,.012f,.29f),glow);var go=new GameObject("InteriorLight");go.transform.SetParent(root.transform);go.transform.position=at+Vector3.down*.12f;go.transform.rotation=Quaternion.Euler(90,0,0);var light=go.AddComponent<Light>();light.renderMode=LightRenderMode.Auto;light.shadowResolution=UnityEngine.Rendering.LightShadowResolution.Low;var data=go.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();var so=new SerializedObject(data);so.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=0;so.ApplyModifiedPropertiesWithoutUndo();lights.Add(light);}
  var controller=root.AddComponent<OfficeInteriorLighting>();controller.fixtures=lights.ToArray();controller.intensity=6;controller.range=4.7f;controller.Apply();
  foreach(var sun in Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l=>l.type==LightType.Directional)){sun.intensity=Mathf.Min(sun.intensity,.4f);sun.shadows=LightShadows.Soft;}
  File.WriteAllText("v5-office-geometry.txt","Piso: "+bounds+"\nCobertura inferior: "+height+" m; luminárias: "+lights.Count+"; spots locais com sombras duras de baixa resolução; ambiente global inalterado.");
 }
}


