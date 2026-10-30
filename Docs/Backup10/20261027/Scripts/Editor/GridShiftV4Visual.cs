using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
[InitializeOnLoad]public static class GridShiftV4Visual {
 static GridShiftV4Visual(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("V4Visual",false)){SessionState.SetBool("V4Visual",false);new GameObject("V4VisualChecks").AddComponent<GridShiftV4Capture>();}};}
 public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");foreach(var w in UnityEngine.Object.FindObjectsByType<ClimaReal>(FindObjectsSortMode.None))w.enabled=false;SessionState.SetBool("V4Visual",true);EditorApplication.isPlaying=true;}
}
public class GridShiftV4Capture:MonoBehaviour {
 readonly System.Text.StringBuilder report=new System.Text.StringBuilder();int noCamera;
 void Update(){if(Camera.allCameras.All(c=>!c.isActiveAndEnabled||c.targetTexture!=null||c.targetDisplay!=0))noCamera++;}
 IEnumerator Shot(string name){Directory.CreateDirectory("v4-images");var source=Camera.allCameras.Where(c=>c.isActiveAndEnabled&&c.targetTexture==null&&c.targetDisplay==0).OrderByDescending(c=>c.depth).First();var capture=new GameObject("CaptureCamera");var camera=capture.AddComponent<Camera>();camera.CopyFrom(source);camera.enabled=false;camera.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);
  var overlays=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isActiveAndEnabled&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var oldWorld=overlays.Select(c=>c.worldCamera).ToArray();var oldDistance=overlays.Select(c=>c.planeDistance).ToArray();
  var rt=new RenderTexture(1280,720,24);var oldTarget=camera.targetTexture;int oldMask=camera.cullingMask;camera.targetTexture=rt;camera.cullingMask=~0;
  for(int i=0;i<overlays.Length;i++){overlays[i].renderMode=RenderMode.ScreenSpaceCamera;overlays[i].worldCamera=camera;overlays[i].planeDistance=camera.nearClipPlane+.5f;}
  Canvas.ForceUpdateCanvases();camera.Render();var oldActive=RenderTexture.active;RenderTexture.active=rt;var picture=new Texture2D(1280,720,TextureFormat.RGB24,false);picture.ReadPixels(new Rect(0,0,1280,720),0,0);picture.Apply();File.WriteAllBytes("v4-images/"+name+".png",picture.EncodeToPNG());RenderTexture.active=oldActive;camera.targetTexture=oldTarget;camera.cullingMask=oldMask;
  for(int i=0;i<overlays.Length;i++){overlays[i].renderMode=RenderMode.ScreenSpaceOverlay;overlays[i].worldCamera=oldWorld[i];overlays[i].planeDistance=oldDistance[i];}Canvas.ForceUpdateCanvases();Destroy(picture);rt.Release();Destroy(rt);Destroy(capture);yield return new WaitForSecondsRealtime(.2f);
 }
 void Loaded(Scene scene,LoadSceneMode mode){foreach(var root in scene.GetRootGameObjects())foreach(var w in root.GetComponentsInChildren<ClimaReal>(true))w.enabled=false;}
 IEnumerator Start(){SceneManager.sceneLoaded+=Loaded;Application.runInBackground=true;yield return new WaitForSeconds(3);noCamera=0;
  var p=UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();p.BlockControls(true);var camera=p.CameraTransform.GetComponent<Camera>();camera.transform.position=new Vector3(239,-1.1f,158);camera.transform.LookAt(new Vector3(236.54f,.65f,149.01f));yield return new WaitForSeconds(1);var shot=Shot("signals-street");while(shot.MoveNext())yield return shot.Current;
  PauseMenu.Instance.Open();yield return new WaitForSecondsRealtime(.4f);shot=Shot("pause");while(shot.MoveNext())yield return shot.Current;PauseMenu.Instance.ShowSettings(true);yield return new WaitForSecondsRealtime(.2f);shot=Shot("settings");while(shot.MoveNext())yield return shot.Current;PauseMenu.Instance.Close();yield return new WaitForSecondsRealtime(.3f);
  var traffic=UnityEngine.Object.FindFirstObjectByType<TrafficManager>();int trafficID=traffic.GetInstanceID();var city=WorldSession.Instance;float clock=city.ClockTime;
  for(int k=0;k<3;k++){
   city.RequestTransition("Office",p.transform);while(!FadeManager.Instance.HasTransitionCamera)yield return null;yield return null;shot=Shot("loading-"+k);while(shot.MoveNext())yield return shot.Current;while(city.Busy)yield return null;
   var terminal=UnityEngine.Object.FindObjectsByType<ComputerInteraction>(FindObjectsSortMode.None).First(c=>c.gameObject.scene.name=="Office");terminal.EnterComputer();while(terminal.MovingCamera)yield return null;if(k==0){shot=Shot("terminal");while(shot.MoveNext())yield return shot.Current;}terminal.ExitComputer();while(terminal.MovingCamera)yield return null;
   city.RequestTransition("SampleScene",terminal.player);while(city.Busy)yield return null;
   if(traffic==null||traffic.GetInstanceID()!=trafficID)throw new Exception("Simulação recriada");report.AppendLine("PASS entrada/saída Office e PC "+k+"; gerente de trânsito preservado");
  }
  report.AppendLine((noCamera==0?"PASS":"FAIL")+" frames sem câmera de display: "+noCamera);report.AppendLine("Relógio avançou "+(city.ClockTime-clock));File.WriteAllText("v4-visual-tests.txt",report.ToString());EditorApplication.Exit(noCamera==0?0:1);
 }
}
