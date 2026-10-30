using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]public static class GridShiftV4PedChecks {
 static GridShiftV4PedChecks(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("V4Ped",false)){SessionState.SetBool("V4Ped",false);new GameObject("PedestrianSafetyChecks").AddComponent<GridShiftV4PedSafety>();}};}
 public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");foreach(var c in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))c.enabled=false;foreach(var w in UnityEngine.Object.FindObjectsByType<ClimaReal>(FindObjectsSortMode.None))w.enabled=false;SessionState.SetBool("V4Ped",true);EditorApplication.isPlaying=true;}
}
public class GridShiftV4PedSafety:MonoBehaviour {
 readonly StringBuilder report=new StringBuilder();
 void Check(bool value,string label){if(!value)throw new Exception(label);report.AppendLine("PASS "+label);}
 IEnumerator Start(){Time.timeScale=4;var run=Run();bool ok=true;while(true){bool more;try{more=run.MoveNext();}catch(Exception e){report.AppendLine("FAIL "+e);ok=false;break;}if(!more)break;yield return run.Current;}File.WriteAllText("v4-pedestrian-tests.txt",report+"\nRESULT "+(ok?"SUCCESS":"FAILURE"));EditorApplication.Exit(ok?0:1);}
 IEnumerator Run(){yield return new WaitForSeconds(2);var manager=UnityEngine.Object.FindFirstObjectByType<PedestrianManager>();manager.StopAllCoroutines();foreach(var existing in manager.Agents.ToArray())manager.Recycle(existing);
  var nodes=manager.graph.nodes;int a=-1,b=-1;
  for(int i=0;i<nodes.Length&&a<0;i++){if(nodes[i].entrancePath)continue;for(int j=i+1;j<nodes.Length;j++){if(nodes[j].entrancePath)continue;Vector3 delta=nodes[j].position-nodes[i].position;float distance=delta.magnitude;if(distance<5||distance>6||Mathf.Abs(delta.y)>.05f||Mathf.Min(Mathf.Abs(delta.x),Mathf.Abs(delta.z))>.1f)continue;var side=Vector3.Cross(Vector3.up,delta.normalized);bool safe=true;
   for(int s=0;s<=12&&safe;s++){var point=Vector3.Lerp(nodes[i].position,nodes[j].position,s/12f);foreach(float offset in new[]{0f,.95f,-.95f})if(!manager.Supported(point+side*offset,point.y)||!manager.Clear(null,point+side*offset,1)){safe=false;break;}}
   if(safe){a=i;b=j;break;}
  }}Check(a>=0,"corredor de calçada real com espaço lateral seguro");
  PedestrianAgent Add(){var go=UnityEngine.Object.Instantiate(manager.models[0],manager.transform);var agent=go.AddComponent<PedestrianAgent>();agent.Initialize(manager,a,1.3f);((List<PedestrianAgent>)manager.Agents).Add(agent);Check(agent.WalkTo(b),"rota de teste existente");return agent;}
  var pole=new GameObject("V4TestPole");pole.transform.position=Vector3.Lerp(nodes[a].position,nodes[b].position,.5f);var collider=pole.AddComponent<CapsuleCollider>();collider.radius=.16f;collider.height=3;collider.center=Vector3.up*1.5f;Physics.SyncTransforms();
  var first=Add();var test=Pass(first,manager,nodes[b].position,pole.transform.position,.43f,"poste");while(test.MoveNext())yield return test.Current;manager.Recycle(first);Destroy(pole);yield return new WaitForFixedUpdate();
  var player=manager.player.GetComponent<PlayerMovement>();var cc=player.GetComponent<CharacterController>();var old=player.transform.position;player.BlockControls(true);cc.enabled=false;player.transform.position=Vector3.Lerp(nodes[a].position,nodes[b].position,.5f)+Vector3.up*.09f;cc.enabled=true;Physics.SyncTransforms();
  var second=Add();test=Pass(second,manager,nodes[b].position,player.transform.position,.75f,"jogador");while(test.MoveNext())yield return test.Current;manager.Recycle(second);cc.enabled=false;player.transform.position=old;cc.enabled=true;player.BlockControls(false);
  bool edgeFound=false;foreach(var node in nodes){if(node.entrancePath)continue;for(int i=1;i<=4;i++){var offset=Vector3.right*i*.25f;if(Physics.Raycast(node.position+offset+Vector3.up,Vector3.down,out var hit,2,~(1<<9),QueryTriggerInteraction.Ignore)&&hit.collider.name.StartsWith("Road ")&&hit.point.y+.025f<node.position.y-.15f){Check(!manager.Supported(node.position+offset,node.position.y),"asfalto abaixo da borda rejeitado pela margem de apoio");edgeFound=true;break;}}if(edgeFound)break;}Check(edgeFound,"borda real de calçada localizada");
 }
 IEnumerator Pass(PedestrianAgent agent,PedestrianManager manager,Vector3 destination,Vector3 obstacle,float clearance,string label){float until=Time.time+35,min=float.PositiveInfinity,trace=0;bool near=false;while(Time.time<until&&Vector3.Distance(agent.transform.position,destination)>.3f){float d=Vector3.ProjectOnPlane(agent.transform.position-obstacle,Vector3.up).magnitude;min=Mathf.Min(min,d);near|=d<2;
   if(Time.time>trace){trace=Time.time+2;var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;int next=(int)typeof(PedestrianAgent).GetField("next",flags).GetValue(agent);report.AppendLine("TRACE "+label+" pos="+agent.transform.position+" goal="+destination+" next="+(next<0?"NONE":manager.graph.nodes[next].position.ToString())+" current="+typeof(PedestrianAgent).GetField("current",flags).GetValue(agent)+" yield="+typeof(PedestrianAgent).GetField("yielding",flags).GetValue(agent));}
   CheckSilent(d>=clearance,label+" atravessado; distância="+d);CheckSilent(manager.Supported(agent.transform.position,agent.transform.position.y),"desvio saiu da calçada");yield return null;}
  Check(near&&Vector3.Distance(agent.transform.position,destination)<.3f,"desvio de "+label+" concluído sem teleporte, colisão ou saída da calçada; folga mínima="+min);
 }
 void CheckSilent(bool ok,string label){if(!ok)throw new Exception(label);}
}
