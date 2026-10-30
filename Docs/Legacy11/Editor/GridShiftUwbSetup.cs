using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using VoltstroStudios.UnityWebBrowser;
using VoltstroStudios.UnityWebBrowser.Communication;
using VoltstroStudios.UnityWebBrowser.Core.Engines;
using VoltstroStudios.UnityWebBrowser.Shared.Popups;
using Object=UnityEngine.Object;
public static class GridShiftUwbSetup {
 const string Folder="Assets/Generated/Terminal";
 [MenuItem("GridShift/Computador/Configurar UWB no monitor")]
 public static void Configure(){
  if(EditorApplication.isPlaying)throw new Exception("Saia do Play Mode antes de gravar a configuração.");
  if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
  Directory.CreateDirectory(Folder);AssetDatabase.Refresh();var scene=EditorSceneManager.OpenScene("Assets/Scenes/Office.unity");
  var terminal=Object.FindObjectsByType<ComputerInteraction>(FindObjectsSortMode.None).FirstOrDefault(t=>t.enabled&&t.screenContent!=null);
  if(terminal==null)throw new Exception("Computador e Canvas da tela não encontrados.");
  var canvas=terminal.screenContent.GetComponent<Canvas>();var screen=canvas.GetComponent<RectTransform>();FitScreen(terminal,screen);
  canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=terminal.playerCamera;if(canvas.GetComponent<GraphicRaycaster>()==null)canvas.gameObject.AddComponent<GraphicRaycaster>();if(canvas.GetComponent<RectMask2D>()!=null)Object.DestroyImmediate(canvas.GetComponent<RectMask2D>());
  var viewport=screen.Find("BrowserViewport") as RectTransform;if(viewport==null){viewport=new GameObject("BrowserViewport",typeof(RectTransform),typeof(RectMask2D)).GetComponent<RectTransform>();viewport.SetParent(screen,false);}viewport.anchorMin=Vector2.zero;viewport.anchorMax=Vector2.one;viewport.offsetMin=viewport.offsetMax=Vector2.zero;
  var browsers=Object.FindObjectsByType<WebBrowserUIBasic>(FindObjectsInactive.Include,FindObjectsSortMode.None);var browser=browsers.FirstOrDefault(b=>b.transform.IsChildOf(screen));if(browser==null)browser=browsers.FirstOrDefault();
  if(browser==null){var go=new GameObject("UrbanLinkBrowser",typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage));browser=go.AddComponent<WebBrowserUIBasic>();}
  foreach(var duplicate in browsers.Where(b=>b!=browser)){duplicate.enabled=false;duplicate.gameObject.SetActive(false);}
  browser.transform.SetParent(viewport,false);browser.name="UrbanLinkBrowser";var rect=browser.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;rect.localPosition=Vector3.zero;rect.localRotation=Quaternion.identity;rect.localScale=Vector3.one;
  var image=browser.GetComponent<RawImage>();image.color=Color.white;image.uvRect=new Rect(0,0,1,-1);image.raycastTarget=false;image.enabled=false;
  var input=AssetDatabase.LoadAssetAtPath<GridShiftUwbInput>(Folder+"/Input.asset");if(input==null){input=ScriptableObject.CreateInstance<GridShiftUwbInput>();AssetDatabase.CreateAsset(input,Folder+"/Input.asset");}
  var communication=AssetDatabase.LoadAssetAtPath<TCPCommunicationLayer>(Folder+"/Communication.asset");if(communication==null){communication=ScriptableObject.CreateInstance<TCPCommunicationLayer>();AssetDatabase.CreateAsset(communication,Folder+"/Communication.asset");}
  var engine=AssetDatabase.LoadAssetAtPath<Engine>("Packages/dev.voltstro.unitywebbrowser.engine.cef/Resources/Cef Engine Configuration.asset");if(engine==null)throw new Exception("Configuração CEF do pacote instalado não encontrada.");
  browser.browserClient.engine=engine;browser.browserClient.communicationLayer=communication;browser.browserClient.initialUrl=terminal.url;browser.browserClient.engineStartupTimeout=15000;browser.browserClient.backgroundColor=new Color32(11,23,39,255);browser.browserClient.windowlessFrameRate=30;browser.browserClient.popupAction=PopupAction.Redirect;
  browser.inputHandler=input;browser.disableMouseInputs=browser.disableKeyboardInputs=true;browser.enabled=false;
  var adapter=terminal.GetComponent<GridShiftUwbTerminal>();if(adapter==null)adapter=terminal.gameObject.AddComponent<GridShiftUwbTerminal>();adapter.browser=browser;adapter.inputTemplate=input;adapter.terminal=terminal;
  var router=browser.GetComponent<GridShiftUwbInputRouter>();if(router==null)router=browser.gameObject.AddComponent<GridShiftUwbInputRouter>();router.browser=browser;router.input=input;router.eventCamera=terminal.playerCamera;adapter.inputRouter=router;terminal.webView=adapter;
  var size=adapter.TargetResolution();var so=new SerializedObject(browser);var resolution=so.FindProperty("browserClient").FindPropertyRelative("resolution");resolution.FindPropertyRelative("Width").intValue=size.x;resolution.FindPropertyRelative("Height").intValue=size.y;so.ApplyModifiedPropertiesWithoutUndo();
  terminal.screenContent.alpha=0;terminal.screenContent.blocksRaycasts=false;terminal.screenContent.interactable=false;
  foreach(var graphic in canvas.GetComponentsInChildren<Graphic>(true))graphic.raycastTarget=graphic==image;if(terminal.statusText!=null){terminal.statusText.raycastTarget=false;terminal.statusText.text="";terminal.statusText.enableAutoSizing=true;terminal.statusText.fontSizeMax=48;terminal.statusText.fontSizeMin=24;}
  var events=Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);var active=events.FirstOrDefault();if(active==null)active=new GameObject("OfficeEventSystem").AddComponent<EventSystem>();foreach(var extra in events.Where(e=>e!=active))extra.enabled=false;
  var module=active.GetComponent<InputSystemUIInputModule>();if(module==null){module=active.gameObject.AddComponent<InputSystemUIInputModule>();module.AssignDefaultActions();}module.enabled=true;foreach(var old in active.GetComponents<BaseInputModule>())if(old!=module)old.enabled=false;
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  File.WriteAllText("uwb-setup.txt","Office: "+terminal.name+"\nCanvas: "+screen.rect.size+"; escala mundial="+screen.lossyScale+"\nChromium: "+size.x+"x"+size.y+"; 30 FPS; proporção física preservada\nInput System: GridShiftUwbInput + GridShiftUwbInputRouter\nZoom e TerminalView preservados. EventSystem ativo: "+active.name);
 }
 static void FitScreen(ComputerInteraction terminal,RectTransform screen){var filter=terminal.GetComponent<MeshFilter>();if(filter==null||filter.sharedMesh.subMeshCount<2)return;var mesh=filter.sharedMesh;var vertices=mesh.vertices;var ids=mesh.GetIndices(1);var rotation=screen.rotation;var right=rotation*Vector3.right;var up=rotation*Vector3.up;var outward=-(rotation*Vector3.forward);var origin=terminal.transform.TransformPoint(vertices[ids[0]]);float minX=float.PositiveInfinity,maxX=float.NegativeInfinity,minY=minX,maxY=maxX,depth=float.NegativeInfinity;
  foreach(int id in ids){var delta=terminal.transform.TransformPoint(vertices[id])-origin;float x=Vector3.Dot(delta,right),y=Vector3.Dot(delta,up);minX=Mathf.Min(minX,x);maxX=Mathf.Max(maxX,x);minY=Mathf.Min(minY,y);maxY=Mathf.Max(maxY,y);depth=Mathf.Max(depth,Vector3.Dot(delta,outward));}
  screen.SetPositionAndRotation(origin+right*(minX+maxX)*.5f+up*(minY+maxY)*.5f+outward*(depth+.003f),rotation);var scale=screen.parent.lossyScale;screen.localScale=new Vector3(.001f/Mathf.Abs(scale.x),.001f/Mathf.Abs(scale.y),.001f/Mathf.Abs(scale.z));screen.sizeDelta=new Vector2((maxX-minX)*1000,(maxY-minY)*1000);
 }
 public static void Batch(){Configure();EditorApplication.Exit(0);}
}
