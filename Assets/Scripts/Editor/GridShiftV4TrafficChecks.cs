using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]public static class GridShiftV4TrafficChecks {
 static GridShiftV4TrafficChecks(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("V4Traffic",false)){SessionState.SetBool("V4Traffic",false);new GameObject("RecoveryChecks").AddComponent<GridShiftV4Recovery>();}};}
 public static void Batch(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SessionState.SetBool("V4Traffic",true);EditorApplication.isPlaying=true;}
}
public class GridShiftV4Recovery:MonoBehaviour {
 readonly StringBuilder report=new StringBuilder();
 void Check(bool ok,string text){if(!ok)throw new Exception(text);report.AppendLine("PASS "+text);}
 IEnumerator Start(){Time.timeScale=4;var run=Run();bool ok=true;while(true){bool more;try{more=run.MoveNext();}catch(Exception e){ok=false;report.AppendLine("FAIL "+e);break;}if(!more)break;yield return run.Current;}File.WriteAllText("v4-recovery-tests.txt",report+"\nRESULT "+(ok?"SUCCESS":"FAILURE"));EditorApplication.Exit(ok?0:1);}
 TrafficRoute Route(string label){var go=new GameObject(label);var route=go.AddComponent<TrafficRoute>();route.pontos=new Transform[2];for(int i=0;i<2;i++){var p=new GameObject("Point"+i).transform;p.SetParent(go.transform);p.position=Vector3.forward*i*30;route.pontos[i]=p;}route.Invalidate();return route;}
 IEnumerator Run(){var broken=Route("Broken");var alternate=Route("ContinuousAlternative");var go=new GameObject("RecoveryVehicle");var box=go.AddComponent<BoxCollider>();box.center=Vector3.up;box.size=new Vector3(1.8f,2,4);var car=go.AddComponent<TrafficCar>();car.ConfigureProfile(box,false,4,2,4);car.ResetRoute(broken);
  var block=new GameObject("RealObstacle");block.transform.position=new Vector3(0,1,12);var obstacle=block.AddComponent<BoxCollider>();obstacle.size=new Vector3(3,2,1);Physics.SyncTransforms();yield return new WaitForSeconds(.5f);float before=car.transform.position.z;Destroy(broken.pontos[1].gameObject);broken.Invalidate();yield return new WaitForSeconds(1);
  Check(car.RotaAtual==alternate&&car.transform.position.z>before,"rota danificada substituída por segmento contínuo da mesma faixa");Check(Mathf.Abs(car.transform.position.x)<.001f,"recuperação preservou faixa sem teleporte lateral");
  yield return new WaitForSeconds(6);Check(car.VelocidadeAtual<.05f&&car.transform.position.z<8,"obstáculo real manteve veículo parado após recuperação");Check(car.Diagnostic==TrafficCar.DiagnosticState.WAITING_OBSTACLE,"espera legítima não foi marcada como STUCK");float stopped=car.transform.position.z;Destroy(block);yield return new WaitForSeconds(3);Check(car.transform.position.z>stopped+1,"veículo retomou após liberação do obstáculo");
 }
}
