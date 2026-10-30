using System;
using System.Collections;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
[InitializeOnLoad] public static class GridShiftV3Checks {
 static GridShiftV3Checks(){EditorApplication.playModeStateChanged+=Changed;}
 public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");Quiet();SessionState.SetBool("V3Checks",true);EditorApplication.isPlaying=true;}
 public static void BatchQueues(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SessionState.SetBool("V3Queues",true);EditorApplication.isPlaying=true;}
 public static void EditorBatch(){
  try{
   foreach(string name in new[]{"SampleScene","Office"}){EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)throw new Exception("Script ausente em "+name+"/"+t.name);}
   PedestrianCrossingChecks.Run();
   EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);TrafficBehaviourChecks.Run();
   File.WriteAllText("v3-editor-tests.txt","PASS referências de scripts nas duas cenas\nPASS 24 travessias individuais\nPASS 10 testes de trânsito: vermelho, verde, amarelo, obstáculos, saída, conexões e curvas\nRESULT SUCCESS");EditorApplication.Exit(0);
  }catch(Exception e){File.WriteAllText("v3-editor-tests.txt","RESULT FAILURE\n"+e);Debug.LogException(e);EditorApplication.Exit(1);}
 }
 static void Quiet(){foreach(var c in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))c.enabled=false;foreach(var c in UnityEngine.Object.FindObjectsByType<ClimaReal>(FindObjectsSortMode.None))c.enabled=false;}
 static void Changed(PlayModeStateChange s){
  if(s!=PlayModeStateChange.EnteredPlayMode)return;
  if(SessionState.GetBool("V3Queues",false)){SessionState.SetBool("V3Queues",false);new GameObject("QueueChecks").AddComponent<GridShiftV3QueueChecks>();return;}
  if(SessionState.GetBool("V3Checks",false)){SessionState.SetBool("V3Checks",false);new GameObject("V3Checks").AddComponent<GridShiftV3PlayChecks>();}
 }
}
public class GridShiftV3QueueChecks:MonoBehaviour {
 IEnumerator Start(){bool success=false;string result;
  try{var suite=new GameObject("QueueSuite").AddComponent<GridShiftV2PlayChecks>();suite.enabled=false;
   typeof(GridShiftV2PlayChecks).GetMethod("TestQueue",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(suite,null);
   success=true;result="PASS fila de três veículos: vermelho, distância segura e retomada progressiva no verde\nRESULT SUCCESS";
  }catch(Exception e){result="RESULT FAILURE\n"+e;}
  yield return null;File.WriteAllText("v3-queues.txt",result);Debug.Log(result);EditorApplication.Exit(success?0:1);
 }
}
public class GridShiftV3PlayChecks:MonoBehaviour {
 readonly System.Text.StringBuilder report=new System.Text.StringBuilder();
 readonly System.Collections.Generic.List<string> errors=new System.Collections.Generic.List<string>();
 void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)errors.Add(message);}
 void Check(bool ok,string message){if(!ok)throw new Exception(message);report.AppendLine("PASS "+message);}
 void Loaded(Scene s,LoadSceneMode m){foreach(var root in s.GetRootGameObjects()){foreach(var c in root.GetComponentsInChildren<Camera>(true))c.enabled=false;foreach(var c in root.GetComponentsInChildren<ClimaReal>(true))c.enabled=false;}}
 IEnumerator Start(){Application.logMessageReceived+=Log;SceneManager.sceneLoaded+=Loaded;var run=Run();while(true){bool next;try{next=run.MoveNext();}catch(Exception e){report.AppendLine("FAIL "+e);Finish(1);yield break;}if(!next)break;yield return run.Current;}Finish(errors.Count==0?0:1);}
 void Finish(int code){report.AppendLine("Errors: "+string.Join(";",errors));report.AppendLine("RESULT "+(code==0?"SUCCESS":"FAILURE"));File.WriteAllText("v3-tests.txt",report.ToString());Debug.Log(report.ToString());EditorApplication.Exit(code);}
 void Jump(PlayerMovement p){p.enabled=false;var start=p.transform.position;for(int i=0;i<30;i++)p.SimulateMovement(Vector2.zero,false,false,.02f);float floor=p.transform.position.y;
 p.SimulateMovement(Vector2.zero,false,true,.02f);Check(p.VerticalVelocity>0,p.gameObject.scene.name+" salto no piso real");float max=floor;
 for(int i=0;i<18;i++){float old=p.VerticalVelocity;p.SimulateMovement(Vector2.zero,false,true,.02f);Check(p.VerticalVelocity<=old+.01f,"sem impulso extra no ar "+i);max=Mathf.Max(max,p.transform.position.y);}
 for(int i=0;i<150;i++)p.SimulateMovement(Vector2.zero,false,false,.02f);Check(max>floor+.4f&&Mathf.Abs(p.transform.position.y-floor)<.15f,"subida e aterrissagem real");p.SimulateMovement(Vector2.zero,false,true,.02f);Check(p.VerticalVelocity>0,"novo salto após aterrissagem");
 for(int i=0;i<150;i++)p.SimulateMovement(Vector2.zero,false,false,.02f);p.enabled=true;}
 IEnumerator Run(){UnityEngine.Random.InitState(33003);Time.timeScale=4;yield return new WaitForSeconds(3);var p=UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();Jump(p);var held=HoldSpace(p);while(held.MoveNext())yield return held.Current;var manager=UnityEngine.Object.FindFirstObjectByType<TrafficManager>();var env=UnityEngine.Object.FindFirstObjectByType<CityEnvironment>();SetHour(12);yield return new WaitForSeconds(30);
 var population=UnityEngine.Object.FindFirstObjectByType<PedestrianManager>();Check(population!=null&&population.Agents.Count>=15,"população automática e grafo");
 Check(population.Entrances.Count>0,"portas reais com acesso navegável");
 var graph=population.graph;int home=population.Entrances.First();int sidewalk=-1;var search=new System.Collections.Generic.Queue<int>();var seen=new System.Collections.Generic.HashSet<int>();search.Enqueue(home);seen.Add(home);
 while(search.Count>0){int n=search.Dequeue();if(!graph.nodes[n].entrancePath){sidewalk=n;break;}foreach(int other in graph.nodes[n].neighbours)if(seen.Add(other))search.Enqueue(other);}
 Check(sidewalk>=0,"entrada conectada à calçada");
 var homeObject=UnityEngine.Object.Instantiate(population.models[0],population.transform);var homeAgent=homeObject.AddComponent<PedestrianAgent>();homeAgent.Initialize(population,sidewalk,1.4f);((System.Collections.Generic.List<PedestrianAgent>)population.Agents).Add(homeAgent);population.daytimeCount++;Check(homeAgent.WalkTo(home),"destino de casa selecionado");
 float homeDeadline=Time.time+35;while(homeAgent.gameObject.activeSelf&&homeAgent.CurrentActivity!=PedestrianAgent.Activity.EnteringBuilding&&Time.time<homeDeadline)yield return null;
 Check(homeAgent.CurrentActivity==PedestrianAgent.Activity.EnteringBuilding,"chegada à porta sem atravessar parede; posição="+homeAgent.transform.position+" destino="+graph.nodes[home].position+" próximo="+typeof(PedestrianAgent).GetField("next",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(homeAgent));population.daytimeCount--;
 var passing=TestPassing(population);while(passing.MoveNext())yield return passing.Current;
 var cars=manager.Vehicles.ToArray();var moved=cars.ToDictionary(c=>c,c=>c.transform.position);var counts=cars.ToDictionary(c=>c,c=>0);for(int k=0;k<180;k++){yield return new WaitForSeconds(1);foreach(var c in cars){if(c==null)continue;if(Vector3.Distance(c.transform.position,moved[c])>1){counts[c]++;moved[c]=c.transform.position;}if(!c.enabled)throw new Exception("Rota desativada "+c.name);}
 foreach(var car in manager.Vehicles)foreach(var pedestrian in population.Agents){var a=car.GetComponent<BoxCollider>();var b=pedestrian.GetComponent<CapsuleCollider>();if(Physics.ComputePenetration(a,car.transform.position,car.transform.rotation,b,pedestrian.transform.position,pedestrian.transform.rotation,out _,out var depth)&&depth>.015f)throw new Exception("Colisão veículo/pedestre "+depth);}}
 Check(counts.Count(k=>k.Value>10)>=Mathf.Min(8,cars.Length),"fluxo real por 180s: "+string.Join(",",counts.Select(c=>c.Key.name+"="+c.Value+" motivo="+c.Key.MotivoParada)));
 Check(population.Agents.Count(a=>a.DistanceWalked>5)>=10,"pedestres caminham na geometria real");
 Check(population.Agents.All(a=>a.GetComponent<Animator>().runtimeAnimatorController==population.controller),"Animator Idle/Walk em todos os pedestres");
 Check(population.Agents.Sum(a=>a.CrossingsCompleted)>0,"travessias concluídas: "+population.Agents.Sum(a=>a.CrossingsCompleted));
 report.AppendLine("OBS entrando em fachada "+population.Agents.Count(a=>a.CurrentActivity==PedestrianAgent.Activity.EnteringBuilding));
 SetHour(1);yield return new WaitForSeconds(50);Check(env.NightFactor>.95f&&env.lamps.Count(l=>l.light.enabled)>0&&env.lamps.Count(l=>l.light.enabled)<=env.lightBudget,"luzes noturnas e orçamento");Check(manager.TargetCount<=5&&population.TargetCount<=Mathf.CeilToInt(population.daytimeCount*.18f),"rotina de madrugada com população moderada");
 var session=WorldSession.Instance;session.RequestTransition("Office",p.transform);while(session.Busy)yield return null;var op=UnityEngine.Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(v=>v.gameObject.scene.name=="Office");Jump(op);held=HoldSpace(op);while(held.MoveNext())yield return held.Current;
 Check(UnityEngine.Object.FindObjectsByType<PedestrianManager>(FindObjectsSortMode.None).All(m=>m.gameObject.scene.name=="SampleScene"),"nenhum gerenciador pedestre no Office");
 var terminal=UnityEngine.Object.FindObjectsByType<ComputerInteraction>(FindObjectsSortMode.None).First(c=>c.gameObject.scene.name=="Office");var cam=terminal.playerCamera;var pos=cam.transform.localPosition;var rot=cam.transform.localRotation;float fov=cam.fieldOfView;var root=op.transform.position;
 for(int i=0;i<3;i++){terminal.EnterComputer();while(terminal.MovingCamera)yield return null;Check(op.ControlsBlocked&&terminal.UsingComputer&&terminal.screenContent.alpha==1,"terminal entra "+i);terminal.ExitComputer();while(terminal.MovingCamera)yield return null;Check(!op.ControlsBlocked&&!terminal.UsingComputer&&Vector3.Distance(cam.transform.localPosition,pos)<.0001f&&Quaternion.Angle(cam.transform.localRotation,rot)<.001f&&Mathf.Abs(cam.fieldOfView-fov)<.001f&&Vector3.Distance(op.transform.position,root)<.001f&&terminal.screenContent.alpha==0,"terminal restaura câmera, FOV, controles e posição "+i);}
 SetHour(7);float before=session.ClockTime;yield return new WaitForSeconds(35);Check(session.ClockTime>before&&manager.TargetCount>=15&&population.TargetCount>20,"relógio e aumento de densidade no Office");
 session.RequestTransition("SampleScene",op.transform);while(session.Busy)yield return null;Check(!session.InOffice,"retorno à cidade");yield return new WaitForSeconds(2);Check(env.NightFactor<.05f&&env.lamps.All(l=>!l.light.enabled),"postes desligados ao amanhecer");
 }
 void SetHour(float hour){var session=WorldSession.Instance;typeof(WorldSession).GetField("clockTime",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(session,hour/24*1920);}
 IEnumerator TestPassing(PedestrianManager manager){
  var nodes=manager.graph.nodes;int a=-1,b=-1;
  bool Pavement(Vector3 p){return Physics.Raycast(p+Vector3.up,Vector3.down,out var h,2,~(1<<9),QueryTriggerInteraction.Ignore)&&h.collider.name.StartsWith("Road ")&&Mathf.Abs(h.point.y+.025f-p.y)<.1f;}
  for(int i=0;i<nodes.Length&&a<0;i++){
   if(nodes[i].entrancePath||nodes[i].neighbours.Length<2||!manager.Clear(null,nodes[i].position,2))continue;
   for(int j=i+1;j<nodes.Length;j++){
    Vector3 d=nodes[j].position-nodes[i].position;float distance=d.magnitude;
    if(nodes[j].entrancePath||distance<4||distance>5||Mathf.Min(Mathf.Abs(d.x),Mathf.Abs(d.z))>.1f||!manager.Clear(null,nodes[j].position,2))continue;
    bool safe=true;for(int s=0;s<=10;s++)if(!Pavement(Vector3.Lerp(nodes[i].position,nodes[j].position,s/10f))){safe=false;break;}
    if(safe){a=i;b=j;break;}
   }
  }
  Check(a>=0,"corredor real para pedestres em sentidos opostos");
  PedestrianAgent Add(int node){var go=UnityEngine.Object.Instantiate(manager.models[0],manager.transform);var agent=go.AddComponent<PedestrianAgent>();agent.Initialize(manager,node,1.3f);((System.Collections.Generic.List<PedestrianAgent>)manager.Agents).Add(agent);return agent;}
  var first=Add(a);var second=Add(b);manager.daytimeCount+=2;Check(first.WalkTo(b)&&second.WalkTo(a),"destinos opostos configurados");
  var direction=(nodes[b].position-nodes[a].position).normalized;float length=Vector3.Distance(nodes[a].position,nodes[b].position);float deadline=Time.time+20;
  bool passed=false;while(Time.time<deadline){passed=Vector3.Dot(first.transform.position-nodes[a].position,direction)>length*.7f&&Vector3.Dot(second.transform.position-nodes[b].position,-direction)>length*.7f;if(passed)break;yield return null;}
 Check(passed,"pedestres em sentidos opostos se liberam sem atravessar um ao outro: "+first.transform.position+" / "+second.transform.position+" início="+nodes[a].position+" fim="+nodes[b].position);manager.Recycle(first);manager.Recycle(second);manager.daytimeCount-=2;
 }
 IEnumerator HoldSpace(PlayerMovement player){
  UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
  UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
  var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
  var updateMode=UnityEngine.InputSystem.InputSystem.settings.updateMode;
  UnityEngine.InputSystem.InputSystem.settings.updateMode=UnityEngine.InputSystem.InputSettings.UpdateMode.ProcessEventsManually;
  bool wasEnabled=player.enabled;player.enabled=false;
  float old=player.VerticalVelocity;int impulses=0;
  UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Space));
  var update=typeof(PlayerMovement).GetMethod("Update",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
  for(int i=0;i<180;i++){UnityEngine.InputSystem.InputSystem.Update();keyboard.MakeCurrent();
   if(i==0)report.AppendLine("INPUT edge="+keyboard.spaceKey.wasPressedThisFrame+" held="+keyboard.spaceKey.isPressed+" grounded="+player.GetComponent<CharacterController>().isGrounded+" blocked="+player.ControlsBlocked);
   update.Invoke(player,null);float velocity=player.VerticalVelocity;if(velocity>1&&old<=0)impulses++;old=velocity;yield return null;}
  UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());UnityEngine.InputSystem.InputSystem.Update();UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);
  UnityEngine.InputSystem.InputSystem.settings.updateMode=updateMode;player.enabled=wasEnabled;
  Check(impulses==1,player.gameObject.scene.name+" input Espaço mantido: exatamente um impulso ("+impulses+")");
 }
}
