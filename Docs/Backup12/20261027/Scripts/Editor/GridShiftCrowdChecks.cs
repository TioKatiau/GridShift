using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

[InitializeOnLoad] public static class GridShiftCrowdChecks {
 static GridShiftCrowdChecks(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("CrowdChecks",false)){SessionState.SetBool("CrowdChecks",false);new GameObject("CrowdChecks").AddComponent<GridShiftCrowdRunner>();}};}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void Prepare(){if(SessionState.GetBool("CrowdChecks",false)){DeliverySaveStore.OverridePath=Path.GetFullPath("crowd-test-"+Guid.NewGuid().ToString("N")+".json");GridShiftTutorial.Prefix="GS.Crowd.Test.";GridShiftTutorial.Skip();}}
 public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");foreach(var w in Object.FindObjectsByType<ClimaReal>(FindObjectsSortMode.None))w.enabled=false;SessionState.SetBool("CrowdChecks",true);EditorApplication.isPlaying=true;}
}
public sealed class GridShiftCrowdRunner:MonoBehaviour {
 IEnumerator Start(){Time.timeScale=3;yield return new WaitForSeconds(2);var manager=Object.FindFirstObjectByType<PedestrianManager>();manager.StopAllCoroutines();foreach(var a in manager.Agents.ToArray())manager.Recycle(a);
  var road=new GameObject("Road Crowd Regression");road.transform.position=new Vector3(1000,-.1f,1000);road.AddComponent<BoxCollider>().size=new Vector3(20,.2f,3.2f);
  var graph=road.AddComponent<PedestrianGraph>();var nodes=new List<PedestrianNode>();for(int x=0;x<=12;x++)for(int lane=0;lane<2;lane++){var links=new List<int>();if(x>0)links.Add((x-1)*2+lane);if(x<12)links.Add((x+1)*2+lane);links.Add(x*2+1-lane);nodes.Add(new PedestrianNode{position=new Vector3(994+x,.025f,999.25f+lane*1.5f),neighbours=links.ToArray()});}graph.nodes=nodes.ToArray();manager.graph=graph;Physics.SyncTransforms();
  var agents=new List<PedestrianAgent>();var goals=new Dictionary<PedestrianAgent,Vector3>();foreach(int side in new[]{0,1})for(int i=0;i<6;i++){int node=(side==0?i/2:12-i/2)*2+i%2;int target=(side==0?12:0)*2+i%2;var go=Object.Instantiate(manager.models[i%manager.models.Length],manager.transform);var a=go.AddComponent<PedestrianAgent>();a.Initialize(manager,node,1.25f);((List<PedestrianAgent>)manager.Agents).Add(a);a.WalkTo(target);agents.Add(a);goals[a]=nodes[target].position;}
  float deadline=Time.time+100,minGap=100;int completed=0;string failure="";while(Time.time<deadline&&agents.Count>0){foreach(var a in agents.ToArray()){if(Vector3.Distance(a.transform.position,goals[a])<.35f){agents.Remove(a);manager.Recycle(a);completed++;continue;}if(!manager.Supported(a.transform.position,a.transform.position.y))failure="pedestre saiu da calçada";foreach(var b in agents)if(a!=b)minGap=Mathf.Min(minGap,Vector3.Distance(a.transform.position,b.transform.position));}yield return null;}
  bool ok=completed==12&&minGap>.55f&&failure=="";File.WriteAllText("crowd-result.txt","opposing_pedestrians=12 completed="+completed+" minimum_gap="+minGap+" failure="+failure+"\nRESULT "+(ok?"SUCCESS":"FAILURE"));EditorApplication.Exit(ok?0:1);
 }
}
