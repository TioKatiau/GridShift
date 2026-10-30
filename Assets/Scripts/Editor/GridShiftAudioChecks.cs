using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Profiling;
using Object=UnityEngine.Object;

[InitializeOnLoad]public static class GridShiftAudioChecks {
 static GridShiftAudioChecks(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("GS.Audio13.Tests",false)){SessionState.SetBool("GS.Audio13.Tests",false);var go=new GameObject("Audio13Validation");Object.DontDestroyOnLoad(go);go.AddComponent<GridShiftAudioTestRunner>();}};}
 public static void Batch(){GridShiftAudioSetup.Install();EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");SessionState.SetBool("GS.Audio13.Tests",true);EditorApplication.isPlaying=true;}
}
public sealed class GridShiftAudioTestRunner:MonoBehaviour {
 readonly StringBuilder log=new StringBuilder();bool finished;float timeout;readonly bool[] prefExists=new bool[7];readonly float[] prefValues=new float[7];
 void Check(bool ok,string message){log.AppendLine((ok?"PASS ":"FAIL ")+message);Write();if(!ok)throw new Exception(message);}
 void Write(){Directory.CreateDirectory("Docs/Audio13");File.WriteAllText("Docs/Audio13/runtime-validation.txt",log.ToString());}
 void Restore(){for(int i=0;i<7;i++){string key="GS.Audio."+GridShiftAudio.Channels[i];if(prefExists[i])PlayerPrefs.SetFloat(key,prefValues[i]);else PlayerPrefs.DeleteKey(key);}PlayerPrefs.Save();}
 void Awake(){for(int i=0;i<7;i++){string key="GS.Audio."+GridShiftAudio.Channels[i];prefExists[i]=PlayerPrefs.HasKey(key);prefValues[i]=PlayerPrefs.GetFloat(key);}DeliverySaveStore.OverridePath=Path.GetFullPath("Temp/audio13-isolated-save.json");GridShiftCompany.SuppressIntro=true;timeout=Time.realtimeSinceStartup+220;Application.logMessageReceived+=Error;}
 void Error(string condition,string stack,LogType type){if(type==LogType.Exception){log.AppendLine("EXCEPTION "+condition+"\n"+stack);Write();Finish(false);}}
 void Update(){if(!finished&&Time.realtimeSinceStartup>timeout){log.AppendLine("FAIL timeout");Finish(false);}}
 void Finish(bool success){if(finished)return;finished=true;Restore();log.AppendLine(success?"RESULT SUCCESS":"RESULT FAILURE");Write();Application.logMessageReceived-=Error;EditorApplication.Exit(success?0:1);}
 IEnumerator Start(){yield return new WaitForSecondsRealtime(4);var a=GridShiftAudio.Instance;Check(a!=null&&a.Library!=null,"runtime bootstrap and persisted library");
  Check(a.Library.clips.Length==63&&a.Library.groups.Length==7,"63 real audio clips and seven mixer groups");log.AppendLine("DIAGNOSTIC LastAttempt="+GridShiftAudio.LastAttempt+", rejected="+a.LastRejected+", enabled="+a.enabled+", logoClip="+(a.Clip("Logo_GS")!=null));Check(a.Count("Logo_GS")==1,"logo effect fires once during real menu intro; count="+a.Count("Logo_GS")+", events="+a.PlayedEvents+", menus="+Object.FindObjectsByType<GridShiftMainMenu>(FindObjectsSortMode.None).Length);
  Check(a.GetComponentsInChildren<AudioSource>().Any(s=>s.isPlaying&&s.clip!=null&&s.clip.name=="Menu_CityInMotion"&&s.volume>.1f),"real menu starts instrumental loop with fade");
  foreach(var entry in a.Library.clips){Check(entry.clip.length>.02f,"clip imported: "+entry.id);if(entry.id.StartsWith("Car_Engine")||entry.id.StartsWith("Truck_Engine")){var samples=new float[1024];Check(entry.clip.GetData(samples,0)&&samples.Any(x=>Mathf.Abs(x)>.001f),"engine contains nonzero PCM: "+entry.id);}}
  Check(a.Clip("Car_Engine_Loop")!=a.Clip("Truck_Engine_Loop"),"cars and trucks reference different audio assets");
  foreach(var channel in GridShiftAudio.Channels){GridShiftAudio.SetVolume(channel,.5f);yield return new WaitForSecondsRealtime(.7f);Check(a.Library.mixer.GetFloat(channel+"Volume",out var db)&&Mathf.Abs(db+6.0206f)<.08f,"actual mixer linear-to-decibel mapping: "+channel);GridShiftAudio.SetVolume(channel,1);}
  GridShiftAudio.SetVolume("Music",0);yield return new WaitForSecondsRealtime(.7f);Check(a.Library.mixer.GetFloat("MusicVolume",out var mute)&&mute<=-79,"music independently muted");Check(GridShiftAudio.Play("UI_Confirm","UI",.3f,0),"UI remains available with music muted");
  PlayerPrefs.Save();Check(PlayerPrefs.GetFloat("GS.Audio.Music",-1)==0,"audio preferences saved to PlayerPrefs");GridShiftAudio.SetVolume("Music",.55f);
  var button=Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name=="Créditos");int clicks=a.Count("UI_Click");button.onClick.Invoke();yield return null;Check(a.Count("UI_Click")>clicks,"real button callback plays click");
  Object.FindFirstObjectByType<GridShiftMainMenu>().StartSlot(null,true);
  float until=Time.realtimeSinceStartup+75;while((DeliveryManager.Instance==null||!DeliveryManager.Instance.GetComponent<DeliveryPersistence>().Ready||WorldSession.Instance.Busy)&&Time.realtimeSinceStartup<until)yield return null;
  var m=DeliveryManager.Instance;Check(m!=null&&m.GetComponent<DeliveryPersistence>().Ready,"real city session initializes using isolated save path");yield return new WaitForSecondsRealtime(4);
  Check(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(l=>l.enabled)==1,"exactly one active listener in city");
  Check(TrafficCar.Active.Count>0&&TrafficCar.Active.All(c=>c.GetComponent<GridShiftVehicleAudio>()!=null),"all live traffic vehicles receive one audio component");
  var audios=Object.FindObjectsByType<GridShiftVehicleAudio>(FindObjectsSortMode.None);Check(audios.Any(v=>v.Heavy)&&audios.Any(v=>!v.Heavy),"real fleet includes separate truck and car sound profiles");
  var player=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(p=>p.gameObject.scene==SceneManager.GetActiveScene());var cc=player.GetComponent<CharacterController>();
  var near=audios.First(v=>!v.Heavy);Vector3 original=player.transform.position;cc.enabled=false;player.transform.position=near.transform.position+Vector3.right*7;cc.enabled=true;player.BlockControls(true);yield return new WaitForSecondsRealtime(2);
  Check(near.Engine.isPlaying&&near.Engine.spatialBlend==1&&near.Engine.maxDistance==65,"nearby real vehicle plays spatial engine with bounded attenuation");
  cc.enabled=false;player.transform.position=original+Vector3.up*200;cc.enabled=true;yield return new WaitForSecondsRealtime(2);Check(audios.All(v=>!v.Engine.isPlaying),"vehicles beyond cutoff stop playback");
  cc.enabled=false;player.transform.position=original;cc.enabled=true;player.BlockControls(false);yield return new WaitForSecondsRealtime(1);
  yield return Movement(player,a);yield return new WaitForSecondsRealtime(1);
  var tablet=player.GetComponent<PortableTablet>();int opened=a.Count("Tablet_Open");Check(tablet.Open(),"real tablet opens");yield return new WaitForSecondsRealtime(1);Check(a.Count("Tablet_Open")==opened+1,"tablet open event plays once");tablet.Close();yield return new WaitForSecondsRealtime(1);Check(a.Count("Tablet_Close")>0,"tablet close event preserves animation");
  int money=a.Count("Money_Received");Check(m.TryChangeMoney(1000,"audio13-credit","Teste áudio","Isolated test"),"real finance transaction accepted");Check(a.Count("Money_Received")>money,"finance transaction emits reward sound");yield return new WaitForSecondsRealtime(1);
  var offer=m.Offers().First();int accepted=a.Count("Delivery_Accepted");Check(m.Accept(offer.id,out _),"real delivery offer accepted");Check(a.Count("Delivery_Accepted")==accepted+1,"accepted delivery emits one sound");int events=a.Count("Delivery_Accepted");m.Accept(offer.id,out _);Check(a.Count("Delivery_Accepted")==events,"idempotent acceptance does not replay notification");
  yield return new WaitForSecondsRealtime(2);Check(m.Progression.Buy("capacity",out _),"real upgrade purchase");Check(a.Count("Upgrade_Purchased")>0,"upgrade completion emits sound");
  yield return new WaitForSecondsRealtime(2);var fixture=new DeliveryOrder{id=Guid.NewGuid().ToString("N"),requestId="mission:first-contract",goods="Audio event fixture",weightKg=5,reward=10,status=DeliveryStatus.Unloading};m.orders.Add(fixture);int completed=a.Count("Delivery_Completed");m.Complete(fixture);Check(a.Count("Delivery_Completed")==completed+1,"completion API emits delivery sound (isolated event fixture, not a driven route)");m.Complete(fixture);Check(a.Count("Delivery_Completed")==completed+1,"duplicate completion cannot replay reward sound");yield return new WaitForSecondsRealtime(2);
  m.Company.Missions.First.orderId=fixture.id;m.Company.Missions.First.step=11;Check(m.Company.Missions.Claim("first-contract",out _),"mission confirmation API accepts fulfilled isolated fixture");Check(a.Count("Mission_Completed")>0,"real mission claim event emits completion sound");yield return new WaitForSecondsRealtime(1);Check(a.Count("Act_Unlocked")>0,"actual campaign advancement emits act unlock sound");
  var pending=m.orders.First(o=>o!=fixture);pending.deadlineSeconds=.01f;pending.elapsedSeconds=1;yield return new WaitForSecondsRealtime(1);int late=a.Count("Delivery_Late");Check(late>0,"deadline crossing emits one warning");yield return new WaitForSecondsRealtime(1);Check(a.Count("Delivery_Late")==late,"overdue order does not repeat warning continuously");
  var special=m.Company.State.missions.First(x=>x.id=="city-top");string oldSelection=m.Company.State.selectedMission;special.status=CompanyMissionStatus.Active;special.orderId=pending.id;m.Company.State.selectedMission=special.id;yield return new WaitForSecondsRealtime(3);Check(a.GetComponentsInChildren<AudioSource>().Any(s=>s.clip!=null&&s.clip.name=="Mission_ForwardMotion"&&s.isPlaying&&s.volume>.1f),"active special mission selects dynamic music");m.Company.State.selectedMission=oldSelection;
  var cityRecorder=ProfilerRecorder.StartNew(ProfilerCategory.Scripts,"GridShift.Audio.Update",180);for(int i=0;i<180;i++){Check(a.ActiveVehicleVoices<=GridShiftAudio.VehicleBudget,"vehicle voice budget frame "+i);yield return null;}var citySamples=cityRecorder.ToArray();log.AppendLine("MEASURE Audio manager main-thread marker, city: "+(citySamples.Length==0?0:citySamples.Average(v=>(double)v.Value)/1e6).ToString("F4")+" ms mean; "+citySamples.Length+" samples. Excludes DSP, GPU and global FPS.");cityRecorder.Dispose();
  WorldSession.Instance.RestoreClock(WorldSession.Instance.SecondsPerDay*.5f);yield return new WaitForSecondsRealtime(4);var day=a.GetComponentsInChildren<AudioSource>().First(s=>s.name=="City_Ambience_Day");var night=a.GetComponentsInChildren<AudioSource>().First(s=>s.name=="City_Ambience_Night");Check(day.volume>night.volume,"day ambience follows actual clock");WorldSession.Instance.RestoreClock(0);yield return new WaitForSecondsRealtime(4);Check(night.volume>day.volume,"night ambience crossfades without restarting loops");
  yield return WorldSession.Instance.RequestTransition("Office",player.transform);yield return new WaitForSecondsRealtime(3);Check(SceneManager.GetActiveScene().name=="Office","real Office transition");Check(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(l=>l.enabled)==1,"single listener after additive Office transition");Check(a.GetComponentsInChildren<AudioSource>().Any(s=>s.clip!=null&&s.clip.name=="Office_Ventilation"&&s.volume>.1f),"Office ambience replaces outdoors");
  var terminal=Object.FindObjectsByType<ComputerInteraction>(FindObjectsSortMode.None).First(t=>t.gameObject.scene.name=="Office");terminal.EnterComputer();yield return new WaitForSecondsRealtime(2);Check(terminal.UsingComputer&&a.Count("Computer_Open")>0,"real Office computer opens with sound");terminal.ExitComputer();yield return new WaitForSecondsRealtime(2);Check(a.Count("Computer_Close")>0,"real computer exit emits sound");
  var recorder=ProfilerRecorder.StartNew(ProfilerCategory.Scripts,"GridShift.Audio.Update",180);for(int i=0;i<180;i++)yield return null;var readings=recorder.ToArray();double avg=readings.Length==0?0:readings.Average(v=>(double)v.Value)/1e6;log.AppendLine("MEASURE Audio manager main-thread marker, Office: "+avg.ToString("F4")+" ms mean; "+readings.Length+" samples. Excludes DSP, GPU and global FPS.");recorder.Dispose();
  Check(a.ActiveOneShots<=GridShiftAudio.OneShotBudget,"one-shot pool remains bounded");Check(m.GetComponent<DeliveryPersistence>().Save(),"save still writes successfully to isolated path");Finish(true);
 }
 int Steps(GridShiftAudio a){int n=0;foreach(var s in new[]{"Concrete","Asphalt","Grass","Office"})for(int i=1;i<=3;i++)n+=a.Count("Footstep_"+s+"_"+i);return n;}
 IEnumerator Movement(PlayerMovement p,GridShiftAudio a){
  var cc=p.GetComponent<CharacterController>();Vector3 saved=p.transform.position;Quaternion rotation=p.transform.rotation;bool enabled=p.enabled;
  var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="Audio13 isolated movement fixture";floor.transform.position=saved+Vector3.up*200;floor.transform.localScale=new Vector3(50,1,50);floor.AddComponent<GridShiftFootstepSurface>().surface=GridShiftFootstepSurface.Surface.Grass;
  cc.enabled=false;p.transform.position=floor.transform.position+Vector3.up*2;p.transform.rotation=Quaternion.identity;cc.enabled=true;
  var driver=p.gameObject.AddComponent<GridShiftAudioMovementDriver>();driver.player=p;
  yield return new WaitForSecondsRealtime(1);
  Check(p.IsGrounded,"movement fixture reaches actual physical floor");int still=Steps(a);yield return new WaitForSecondsRealtime(.3f);Check(Steps(a)==still,"standing player produces no footsteps");
  int walking=Steps(a);driver.input=Vector2.up;yield return new WaitForSecondsRealtime(1.4f);int walked=Steps(a)-walking;Check(walked>0,"walking footsteps follow real displacement on tagged grass");
  int running=Steps(a);driver.running=true;yield return new WaitForSecondsRealtime(1.4f);Check(Steps(a)-running>walked,"running produces faster footstep cadence than walking");
  int jump=a.Count("Jump"),land=a.Count("Land");driver.input=Vector2.zero;driver.jump=true;yield return null;yield return null;Check(a.Count("Jump")==jump+1,"real accepted jump triggers sound exactly once");int air=Steps(a);
  yield return new WaitForSecondsRealtime(.15f);Check(!p.IsGrounded&&Steps(a)==air,"airborne player produces no footsteps");
  yield return new WaitForSecondsRealtime(1.5f);Check(p.IsGrounded&&a.Count("Land")>land,"real ground contact triggers landing sound");
  Object.Destroy(driver);cc.enabled=false;p.transform.SetPositionAndRotation(saved,rotation);cc.enabled=true;p.enabled=enabled;Object.Destroy(floor);
 }
}
[DefaultExecutionOrder(500)] public sealed class GridShiftAudioMovementDriver:MonoBehaviour {
 public PlayerMovement player;public Vector2 input;public bool running,jump;
 void Update(){player.SimulateMovement(input,running,jump,Time.deltaTime);jump=false;}
}
