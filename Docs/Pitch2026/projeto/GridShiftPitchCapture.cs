// Capture-only harness. Install ONLY in the isolated capture checkout.
// Does not change production scripts, scene assets, routes or save slots.
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object=UnityEngine.Object;

[InitializeOnLoad] public static class GridShiftPitchCapture {
 static GridShiftPitchCapture(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("GS.Pitch.Capture",false)){SessionState.SetBool("GS.Pitch.Capture",false);var go=new GameObject("Pitch capture only");Object.DontDestroyOnLoad(go);go.AddComponent<GridShiftPitchRunner>();}};}
 public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");SessionState.SetBool("GS.Pitch.Capture",true);EditorApplication.isPlaying=true;}
 public static void Detail(){SessionState.SetBool("GS.Pitch.Detail",true);Batch();}
 public static void Office(){SessionState.SetBool("GS.Pitch.Office",true);Batch();}
}
public sealed class GridShiftPitchRunner:MonoBehaviour {
 const string Output="C:/Users/Gb191/OneDrive/Desktop/GridShift/Docs/Pitch2026";
 const string Encoder="C:/Users/Gb191/AppData/Local/Temp/GridShiftPitchTools/ffmpeg/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe";
 Camera capture,view;RenderTexture target;Texture2D pixels;PlayerMovement player;DeliveryManager manager;
 Process encoder;int frame,frames;string shot;Action<float> move;bool recording,ui,finished;float timeout;
 readonly StringBuilder log=new StringBuilder();Vector3 streetCenter;TrafficCar truck,car;PedestrianAgent pedestrian;
 void Note(string s){log.AppendLine(s);File.WriteAllText(Output+"/capture-log.txt",log.ToString());}
 void Awake(){Directory.CreateDirectory(Output+"/clipes");Directory.CreateDirectory(Output+"/previas");DeliverySaveStore.OverridePath=Path.GetFullPath("Temp/pitch-capture-save.json");GridShiftProfiles.RootOverride=Path.GetFullPath("Temp/pitch-capture-profiles");GridShiftTutorial.Prefix="GS.Pitch.CaptureOnly.";GridShiftCompany.SuppressIntro=true;DeliverySaveStore.Pending=null;DeliverySaveStore.StartFresh=true;timeout=Time.realtimeSinceStartup+1800;Application.logMessageReceived+=Error;}
 void Error(string s,string stack,LogType type){if(type==LogType.Exception&&!finished){Note("ERROR "+s+"\n"+stack);Finish(false);}}
 void Update(){if(!finished&&Time.realtimeSinceStartup>timeout){Note("ERROR capture timeout");Finish(false);}}
 void Finish(bool ok){if(finished)return;finished=true;recording=false;if(encoder!=null){try{encoder.StandardInput.Close();encoder.WaitForExit(10000);}catch{}}Time.captureFramerate=0;Note(ok?"RESULT SUCCESS":"RESULT FAILURE");Application.logMessageReceived-=Error;EditorApplication.Exit(ok?0:1);}
 IEnumerator Start(){
  yield return new WaitForSecondsRealtime(4);GridShiftSavesUI.Instance?.Close();Object.FindFirstObjectByType<GridShiftMainMenu>().StartSlot(null,true);
  float until=Time.realtimeSinceStartup+90;while((DeliveryManager.Instance==null||!DeliveryManager.Instance.GetComponent<DeliveryPersistence>().Ready)&&Time.realtimeSinceStartup<until)yield return null;
  manager=DeliveryManager.Instance;if(manager==null)throw new Exception("City session did not initialize");GridShiftTutorial.Skip();yield return new WaitForSecondsRealtime(4);
  player=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(p=>p.gameObject.scene.name=="SampleScene");view=player.CameraTransform.GetComponent<Camera>();player.BlockControls(true);WorldSession.Instance.RestoreClock(WorldSession.Instance.SecondsPerDay*.44f);
  car=TrafficCar.Active.Where(c=>c!=null&&c.enabled&&c.BodySize.z<7).OrderByDescending(c=>c.VelocidadeAtual).First();truck=TrafficCar.Active.Where(c=>c!=null&&c.enabled&&c.BodySize.z>7).First();
  pedestrian=Object.FindObjectsByType<PedestrianAgent>(FindObjectsSortMode.None).OrderBy(p=>(p.transform.position-car.transform.position).sqrMagnitude).FirstOrDefault();streetCenter=car.transform.position;
  Note("REAL CITY traffic="+TrafficCar.Active.Count+" car="+car.name+" at="+car.transform.position+" truck="+truck.name+" at="+truck.transform.position+" pedestrian="+(pedestrian!=null?pedestrian.transform.position.ToString():"none"));
  capture=new GameObject("PitchRenderCamera").AddComponent<Camera>();capture.CopyFrom(view);capture.enabled=false;capture.fieldOfView=56;capture.nearClipPlane=.12f;capture.farClipPlane=1100;
  target=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGB32);target.Create();pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);capture.targetTexture=target;
  Time.captureFramerate=30;
  if(SessionState.GetBool("GS.Pitch.Office",false)){
   SessionState.SetBool("GS.Pitch.Office",false);yield return WorldSession.Instance.RequestTransition("Office",player.transform);yield return new WaitForSecondsRealtime(1);player=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(p=>p.gameObject.scene.name=="Office");view=player.CameraTransform.GetComponent<Camera>();player.BlockControls(true);var deskTerminal=Object.FindObjectsByType<ComputerInteraction>(FindObjectsSortMode.None).First(t=>t.gameObject.scene.name=="Office");var spot=deskTerminal.computerViewPoint;forceRecord=true;Note("OFFICE clean camera framing, own avatar renderers excluded only during capture");yield return Record("10_office",3.4f,false,t=>Look(spot.position-spot.forward*2.4f+Vector3.up*.3f+spot.right*t*.3f,spot.position+spot.forward*1.5f));yield return Record("15_office_detalhe",4.4f,false,t=>Look(spot.position-spot.forward*3+Vector3.up*.2f+spot.right*t*.4f,spot.position+spot.forward));Finish(true);yield break;
  }
  if(SessionState.GetBool("GS.Pitch.Detail",false)){
   SessionState.SetBool("GS.Pitch.Detail",false);
   for(int i=0;i<1800;i++){pedestrian=Object.FindObjectsByType<PedestrianAgent>(FindObjectsSortMode.None).FirstOrDefault(p=>p.CurrentActivity==PedestrianAgent.Activity.Crossing);if(pedestrian!=null)break;yield return null;}
   if(pedestrian==null)pedestrian=Object.FindObjectsByType<PedestrianAgent>(FindObjectsSortMode.None).First(p=>p.CurrentSpeed>.1f);
   Vector3 offset=ClearOffset(pedestrian);Note("DETAIL real pedestrian activity="+pedestrian.CurrentActivity+" at="+pedestrian.transform.position+" cameraOffset="+offset);
   forceRecord=true;yield return Record("04_pedestres",5.4f,false,t=>{var focus=pedestrian.transform.position+Vector3.up*.9f;Look(focus+offset,focus);});
   foreach(var offer in manager.Offers().Where(o=>o.weightKg>manager.truckThresholdKg).ToArray())if(manager.Accept(offer.id,out _))break;for(int i=0;i<45;i++)yield return null;var vehicle=manager.fleet.FirstOrDefault(v=>v.Order!=null);if(vehicle==null)throw new Exception("No real assignment for map detail");manager.TrackVehicle(vehicle.id);player.BlockControls(false);var device=player.GetComponent<PortableTablet>();if(!device.Open())throw new Exception("Tablet detail failed");yield return new WaitForSecondsRealtime(.7f);yield return Page(device.Operations,"Mapa");Press(device.Operations,"Centralizar no veículo");Press(device.Operations,"Zoom −");bool changed=false;Note("DETAIL real route vehicle="+vehicle.id+" state="+vehicle.state+" destination="+vehicle.Order.destinationId);
   yield return Record("08_mapa_rotas",4.4f,true,t=>{MatchView();if(t>.5f&&!changed){Press(device.Operations,"Ver destino");changed=true;}});Finish(true);yield break;
  }
  yield return Record("01_cidade_abertura",6.4f,false,t=>Orbit(streetCenter,21,29,-.6f+t*.18f));
  yield return Record("02_transito",5.4f,false,t=>Follow(car,new Vector3(-7,3,-13),1.1f,t));
  yield return Record("03_caminhao",5.4f,false,t=>Follow(truck,new Vector3(-7,3.5f,-15),1.7f,t));
  Vector3 crossing=pedestrian!=null?pedestrian.transform.position:streetCenter;
  yield return Record("04_pedestres",5.4f,false,t=>{Vector3 focus=pedestrian!=null?pedestrian.transform.position+Vector3.up:crossing;Look(focus+new Vector3(8,3,-12)+Vector3.right*t,focus);});
  // Actual contract acceptance: no fake order state, rewards, or forced traffic.
  foreach(var offer in manager.Offers().Where(o=>o.weightKg>manager.truckThresholdKg).ToArray()){if(manager.Accept(offer.id,out var error)){Note("REAL CONTRACT "+offer.id+" "+offer.goods);break;}}
  for(int i=0;i<90;i++)yield return null;
  var assigned=manager.fleet.FirstOrDefault(v=>v.Order!=null);if(assigned!=null){truck=assigned.driver;manager.TrackVehicle(assigned.id);Note("REAL ASSIGNMENT "+assigned.id+" state="+assigned.state+" order="+assigned.Order.id);}
  // Place only the recording viewpoint, never move simulated traffic.
  var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.position=streetCenter+Vector3.up*2;cc.enabled=true;player.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up));player.BlockControls(false);
  var tablet=player.GetComponent<PortableTablet>();if(!tablet.Open())throw new Exception("Tablet did not open");
  yield return Record("05_tablet_abre",4.4f,true,t=>MatchView());
  yield return Page(tablet.Operations,"Entregas");yield return Record("06_contratos",4.4f,true,t=>MatchView());
  yield return Page(tablet.Operations,"Frota");yield return Record("07_frota",4.4f,true,t=>MatchView());
  yield return Page(tablet.Operations,"Mapa");yield return Record("08_mapa_rotas",4.4f,true,t=>MatchView());
  tablet.Close();yield return new WaitForSecondsRealtime(.8f);player.BlockControls(true);
  yield return Record("09_caminhao_operacao",4.4f,false,t=>Follow(truck,new Vector3(8,3,-12),1.4f,t));
  // Enter the existing Office and interact with its real terminal.
  yield return WorldSession.Instance.RequestTransition("Office",player.transform);for(int i=0;i<20;i++)yield return null;
  player=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(p=>p.gameObject.scene.name=="Office");view=player.CameraTransform.GetComponent<Camera>();player.BlockControls(false);
  var terminal=Object.FindObjectsByType<ComputerInteraction>(FindObjectsSortMode.None).First(t=>t.gameObject.scene.name=="Office");
  Vector3 desk=terminal.computerViewPoint.position;
  yield return Record("10_office",3.4f,false,t=>Look(desk-terminal.computerViewPoint.forward*2.4f+Vector3.up*.3f+terminal.computerViewPoint.right*t*.3f,desk+terminal.computerViewPoint.forward*1.5f));
  terminal.EnterComputer();yield return new WaitForSecondsRealtime(2);if(!terminal.UsingComputer||terminal.Operations==null)throw new Exception("Office terminal unavailable: using="+terminal.UsingComputer+" moving="+terminal.MovingCamera+" blocked="+player.ControlsBlocked);
  yield return Page(terminal.Operations,"Financeiro");yield return Record("11_financeiro",5.4f,true,t=>MatchView());
  yield return Page(terminal.Operations,"Funcionários");yield return Record("12_funcionarios",4.4f,true,t=>MatchView());
  // Real purchases, using the starting balance. No injected funds/progression.
  bool hired=manager.Company.Employee("rui",true,out var hireError);Note("REAL HIRE "+hired+" "+hireError);
  yield return Page(terminal.Operations,"Upgrades");yield return Record("13_upgrades",4.4f,true,t=>MatchView());
  bool upgraded=manager.Progression.Buy("capacity",out var upgradeError);Note("REAL UPGRADE "+upgraded+" "+upgradeError);
  yield return Page(terminal.Operations,"Missões");yield return Record("14_missoes",5.4f,true,t=>MatchView());
  terminal.ExitComputer();yield return new WaitForSecondsRealtime(1.2f);
  yield return Record("15_office_detalhe",4.4f,false,t=>Look(desk-terminal.computerViewPoint.forward*3+Vector3.up*.2f+terminal.computerViewPoint.right*t*.4f,desk+terminal.computerViewPoint.forward));
  yield return WorldSession.Instance.RequestTransition("SampleScene",player.transform);for(int i=0;i<25;i++)yield return null;
  player=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(p=>p.gameObject.scene.name=="SampleScene");view=player.CameraTransform.GetComponent<Camera>();player.BlockControls(false);
  // A first-person walk uses the real movement controller and collisions.
  var driver=player.gameObject.AddComponent<GridShiftPitchWalk>();driver.player=player;
  yield return Record("16_primeira_pessoa",5.4f,false,t=>MatchView());Object.Destroy(driver);player.BlockControls(true);
  WorldSession.Instance.RestoreClock(WorldSession.Instance.SecondsPerDay*.88f);for(int i=0;i<60;i++)yield return null;
  yield return Record("17_cidade_noite",5.4f,false,t=>Orbit(streetCenter,16,24,.4f+t*.18f));
  yield return Record("18_transito_noite",5.4f,false,t=>Follow(car,new Vector3(-7,3,-13),1.1f,t));
  WorldSession.Instance.RestoreClock(WorldSession.Instance.SecondsPerDay*.71f);for(int i=0;i<45;i++)yield return null;
  yield return Record("19_cidade_final",5.4f,false,t=>Orbit(streetCenter,18,28,1.8f+t*.14f));
  Finish(true);
 }
 void MatchView(){capture.CopyFrom(view);capture.enabled=false;capture.targetTexture=target;capture.transform.SetPositionAndRotation(view.transform.position,view.transform.rotation);}
 bool forceRecord;
 void Press(GridShiftNativeOperations ops,string label){var b=ops.GetComponentsInChildren<Button>().First(x=>x.name==label);b.onClick.Invoke();}
 Vector3 ClearOffset(PedestrianAgent p){Vector3 focus=p.transform.position+Vector3.up*.9f;for(int i=0;i<12;i++){float angle=i*Mathf.PI/6;var offset=new Vector3(Mathf.Cos(angle)*6,2.1f,Mathf.Sin(angle)*6);if(!Physics.Linecast(focus,focus+offset,out var hit,~0,QueryTriggerInteraction.Ignore)||hit.transform.IsChildOf(p.transform))return offset;}return new Vector3(0,5,-6);}
 void Look(Vector3 at,Vector3 focus){capture.fieldOfView=56;capture.transform.SetPositionAndRotation(at,Quaternion.LookRotation(focus-at));}
 void Orbit(Vector3 center,float height,float radius,float angle){Look(center+new Vector3(Mathf.Sin(angle)*radius,height,Mathf.Cos(angle)*radius),center+Vector3.up*1.2f);}
 void Follow(TrafficCar vehicle,Vector3 offset,float height,float t){if(vehicle==null)return;Vector3 focus=vehicle.transform.position+Vector3.up*height;Vector3 at=vehicle.transform.position+vehicle.transform.TransformDirection(offset+Vector3.right*t*.8f);Look(at,focus);}
 IEnumerator Page(GridShiftNativeOperations ops,string page){ops.Show(page);yield return new WaitForSecondsRealtime(.3f);Canvas.ForceUpdateCanvases();var scroll=ops.GetComponentInChildren<ScrollRect>();if(scroll!=null){scroll.StopMovement();scroll.verticalNormalizedPosition=1;}yield return null;}
 IEnumerator Record(string name,float seconds,bool includeUI,Action<float> movement){
  GridShiftSavesUI.Instance?.Close();
  if(!forceRecord&&new[]{"01_cidade_abertura","02_transito","03_caminhao","04_pedestres","09_caminhao_operacao","10_office","15_office_detalhe","17_cidade_noite","18_transito_noite","19_cidade_final"}.Contains(name)&&File.Exists(Output+"/clipes/"+name+".mp4")){Note("REUSE previous real capture "+name);yield break;}
  shot=name;frames=Mathf.RoundToInt(seconds*30);frame=0;ui=includeUI;move=movement;
  var info=new ProcessStartInfo(Encoder,"-hide_banner -loglevel error -nostats -y -f rawvideo -pixel_format rgb24 -video_size 1920x1080 -framerate 30 -i pipe:0 -vf vflip -an -c:v libx264 -preset ultrafast -crf 18 -pix_fmt yuv420p \""+Output+"/clipes/"+name+".mp4\"");info.UseShellExecute=false;info.CreateNoWindow=true;info.RedirectStandardInput=true;info.RedirectStandardError=true;encoder=Process.Start(info);Note("START "+name+" frames="+frames+" scene="+SceneManager.GetActiveScene().name);recording=true;
  while(frame<frames)yield return null;recording=false;encoder.StandardInput.Close();string error=encoder.StandardError.ReadToEnd();encoder.WaitForExit();if(encoder.ExitCode!=0)throw new Exception("Encoder "+name+": "+error);encoder.Dispose();encoder=null;Note("DONE "+name);
 }
 void LateUpdate(){if(!recording||frame>=frames)return;move(frame/(float)Mathf.Max(1,frames-1));
  var renderers=player.GetComponentsInChildren<Renderer>(true);var rendererEnabled=renderers.Select(r=>r.enabled).ToArray();foreach(var r in renderers){bool tabletPart=false;for(var p=r.transform;p!=null&&p!=player.transform;p=p.parent)if(p.name=="PortableTabletViewModel"){tabletPart=true;break;}if(!ui||!tabletPart)r.enabled=false;}
  var canvases=Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);var enabled=canvases.Select(c=>c.enabled).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
  for(int i=0;i<canvases.Length;i++){var c=canvases[i];if(!ui){c.enabled=false;continue;}if(c.renderMode==RenderMode.ScreenSpaceOverlay){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=capture;c.planeDistance=.25f;}}
  Canvas.ForceUpdateCanvases();capture.targetTexture=target;capture.Render();var old=RenderTexture.active;RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1920,1080),0,0,false);pixels.Apply(false,false);RenderTexture.active=old;
  if(frame==frames/2)File.WriteAllBytes(Output+"/previas/"+shot+".png",pixels.EncodeToPNG());var data=pixels.GetRawTextureData<byte>();encoder.StandardInput.BaseStream.Write(data.ToArray(),0,data.Length);
  for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];canvases[i].enabled=enabled[i];}for(int i=0;i<renderers.Length;i++)renderers[i].enabled=rendererEnabled[i];frame++;
 }
}
[DefaultExecutionOrder(500)]public sealed class GridShiftPitchWalk:MonoBehaviour {public PlayerMovement player;void Update(){player.SimulateMovement(new Vector2(.12f,1),false,false,Time.deltaTime);}}
