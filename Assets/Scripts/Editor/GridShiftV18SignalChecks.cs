using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

[InitializeOnLoad]public static class GridShiftV18SignalChecks {
 static GridShiftV18SignalChecks(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("V18Signals",false)){var root=new GameObject("V17 signal checks");Object.DontDestroyOnLoad(root);root.AddComponent<GridShiftV18SignalRunner>();}};}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Prepare(){if(!SessionState.GetBool("V18Signals",false))return;DeliverySaveStore.OverridePath=Path.GetFullPath("Temp/V18Signals-"+Guid.NewGuid().ToString("N")+"/slot.json");GridShiftCompany.SuppressIntro=true;GridShiftTutorial.Prefix="GS.V17.Signals.";GridShiftTutorial.Skip();}
 public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");SessionState.SetBool("V18Signals",true);EditorApplication.isPlaying=true;}
}
public sealed class GridShiftV18SignalRunner:MonoBehaviour {
 readonly StringBuilder report=new StringBuilder();bool done;float timeout;
 void Check(bool ok,string text){report.AppendLine((ok?"PASS ":"FAIL ")+text);Write();if(!ok)Finish(false);}
 void Write(){Directory.CreateDirectory("Docs/Validacao18");File.WriteAllText("Docs/Validacao18/signals-factory.txt",report.ToString());}
 void Finish(bool success){if(done)return;done=true;SessionState.SetBool("V18Signals",false);Time.timeScale=1;report.AppendLine("RESULT "+(success?"SUCCESS":"FAILURE"));Write();EditorApplication.Exit(success?0:1);}
 void Update(){if(!done&&timeout>0&&Time.realtimeSinceStartup>timeout){report.AppendLine("FAIL timeout");Finish(false);}}
 static T Field<T>(object instance,string name)=>(T)instance.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(instance);
 IEnumerator Start(){timeout=Time.realtimeSinceStartup+150;while(DeliveryManager.Instance?.GetComponent<GridShiftActiveOperations>()?.Van==null)yield return null;
  GridShiftTutorial.Skip();GridShiftOperationsUI.Instance.Close();var controls=Object.FindObjectsByType<TrafficIntersection>(FindObjectsSortMode.None);Check(controls.Length>0,"existing intersection controller present");if(done)yield break;
  var horizontal=controls.Select(c=>Field<TrafficLight[]>(Field<TrafficLightGroup>(c,"grupoHorizontal"),"semaforos")).ToArray();var vertical=controls.Select(c=>Field<TrafficLight[]>(Field<TrafficLightGroup>(c,"grupoVertical"),"semaforos")).ToArray();
  report.AppendLine("DETAIL controllers="+controls.Length+" grouped_signals="+(horizontal.Sum(g=>g.Length)+vertical.Sum(g=>g.Length)));Write();
  var prior=controls.Select(c=>c.AllRedClearance).ToArray();var start=Enumerable.Repeat(-1f,controls.Length).ToArray();var cycles=new int[controls.Length];float until=Time.time+120;Time.timeScale=3;
  while(Time.time<until&&cycles.Any(n=>n<2)){for(int i=0;i<controls.Length;i++){bool a=horizontal[i].Any(s=>s!=null&&s.PodePassar()),b=vertical[i].Any(s=>s!=null&&s.PodePassar());if(a&&b){Check(false,"conflicting green phases");yield break;}bool red=controls[i].AllRedClearance;if(red&&!prior[i])start[i]=Time.time;if(!red&&prior[i]&&start[i]>=0){float duration=Time.time-start[i];Check(duration>=controls[i].AllRedDuration-.05f,"all-red clearance lasted "+duration.ToString("F2")+" simulation seconds");cycles[i]++;if(done)yield break;}prior[i]=red;}yield return null;}
  Time.timeScale=1;Check(cycles.All(n=>n>=2),"both conflict directions completed clearance and received green with pedestrians active");if(done)yield break;
  var session=WorldSession.Instance;var player=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(p=>p.gameObject.scene.name=="SampleScene");var original=controls[0];float clock=session.ClockTime;yield return session.Transition("FactoryInterior",player.transform);yield return new WaitForSeconds(1.2f);
  Check(session.InFactory&&Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(l=>l.isActiveAndEnabled)==1,"Factory transition retains exactly one active listener");Check(original!=null&&original==Object.FindFirstObjectByType<TrafficIntersection>(),"Factory keeps the original city signal controller alive");
  var office=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(p=>p.gameObject.scene.name=="FactoryInterior");yield return session.Transition("SampleScene",office.transform);yield return new WaitForSeconds(1.2f);Check(!session.InFactory&&Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(l=>l.isActiveAndEnabled)==1,"return to city retains exactly one active listener");Check(original==Object.FindFirstObjectByType<TrafficIntersection>()&&session.ClockTime!=clock,"city simulation continues without controller reset");Finish(true);
 }
}
