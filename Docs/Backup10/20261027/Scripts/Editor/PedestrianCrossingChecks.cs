using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class PedestrianCrossingChecks {
 static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
 public static void Batch(){
  try{Run();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
 }
 public static void Run(){
   EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");foreach(var c in UnityEngine.Object.FindObjectsByType<TrafficCar>(FindObjectsSortMode.None))c.gameObject.SetActive(false);
   var manager=UnityEngine.Object.FindFirstObjectByType<PedestrianManager>();foreach(var light in UnityEngine.Object.FindObjectsByType<TrafficLight>(FindObjectsSortMode.None))light.DefinirEstado(TrafficLight.Estado.Vermelho);
   var report=new System.Text.StringBuilder();int failed=0;var mode=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
   try{foreach(var crossing in manager.graph.crossings){
    var go=UnityEngine.Object.Instantiate(manager.models[0]);var agent=go.AddComponent<PedestrianAgent>();agent.Initialize(manager,crossing.a,1.3f);
    typeof(PedestrianAgent).GetField("pause",Flags).SetValue(agent,0f);typeof(PedestrianAgent).GetField("next",Flags).SetValue(agent,crossing.b);typeof(PedestrianAgent).GetField("crossing",Flags).SetValue(agent,crossing);
    Physics.SyncTransforms();var step=typeof(PedestrianAgent).GetMethod("FixedUpdate",Flags);
    for(int k=0;k<1500&&agent.CrossingsCompleted==0;k++){step.Invoke(agent,null);Physics.Simulate(.02f);}
    report.AppendLine(crossing.junction.name+" "+manager.graph.nodes[crossing.a].position+" -> "+manager.graph.nodes[crossing.b].position+" result="+agent.CrossingsCompleted+" pos="+agent.transform.position+" state="+agent.CurrentActivity);
    if(agent.CrossingsCompleted==0)failed++;
    UnityEngine.Object.DestroyImmediate(go);
   }}finally{Physics.simulationMode=mode;}
   File.WriteAllText("v3-crossings.txt",report.ToString());Debug.Log(report.ToString());if(failed>0)throw new Exception("Travessias bloqueadas: "+failed);
 }
}
