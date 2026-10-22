using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad] public static class GridShiftV4Checks {
 static GridShiftV4Checks(){EditorApplication.playModeStateChanged+=Changed;}
 public static void PlayerBatch(){EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");foreach(var w in UnityEngine.Object.FindObjectsByType<ClimaReal>(FindObjectsSortMode.None))w.enabled=false;foreach(var c in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))c.enabled=false;SessionState.SetBool("V4Player",true);EditorApplication.isPlaying=true;}
 public static void CityBatch(){EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");foreach(var w in UnityEngine.Object.FindObjectsByType<ClimaReal>(FindObjectsSortMode.None))w.enabled=false;foreach(var c in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))c.enabled=false;SessionState.SetBool("V4City",true);EditorApplication.isPlaying=true;}
 public static void EditorBatch(){var report=new StringBuilder();try{
  foreach(var name in new[]{"SampleScene","Office"}){EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)throw new Exception("Script ausente em "+name+"/"+t.name);}report.AppendLine("PASS ambas as cenas sem scripts ausentes");
  EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
  var signals=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Props_Traffic Signal")).ToArray();
  foreach(var head in signals){var signal=head.GetComponent<TrafficLight>();if(signal==null)throw new Exception("Cabeça sem controlador: "+head.name);var so=new SerializedObject(signal);var lamps=new[]{"luzVermelha","luzAmarela","luzVerde"}.Select(f=>so.FindProperty(f).objectReferenceValue as GameObject).ToArray();
   if(lamps.Any(g=>g==null||g.GetComponent<Renderer>()==null||!g.GetComponent<Renderer>().enabled))throw new Exception("Lâmpada ausente/invisível em "+head.name);
   foreach(TrafficLight.Estado state in Enum.GetValues(typeof(TrafficLight.Estado))){signal.DefinirEstado(state);if(lamps.Count(g=>g.activeSelf)!=1)throw new Exception("Luzes simultâneas "+head.name);int expected=state==TrafficLight.Estado.Vermelho?0:state==TrafficLight.Estado.Amarelo?1:2;if(!lamps[expected].activeSelf)throw new Exception("Cor incorreta");}
  }report.AppendLine("PASS "+signals.Length+" cabeças: controladores, referências e três estados visíveis exclusivos");
  var intersection=UnityEngine.Object.FindFirstObjectByType<TrafficIntersection>();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
  for(int stage=0;stage<6;stage++){typeof(TrafficIntersection).GetField("etapa",flags).SetValue(intersection,stage);typeof(TrafficIntersection).GetMethod("AplicarEstado",flags).Invoke(intersection,null);foreach(var head in signals){var signal=head.GetComponent<TrafficLight>();bool horizontal=Mathf.Abs(head.transform.forward.x)>.7f;bool green=horizontal?stage==0:stage==3;bool amber=horizontal?stage==1:stage==4;if(signal.PodePassar()!=green||signal.EstaAmarelo()!=amber||signal.EstaVermelho()!=(!green&&!amber))throw new Exception("Fases conflitantes em "+head.name);}}
  report.AppendLine("PASS seis fases sincronizadas: direções conflitantes nunca recebem verde juntas; dois intervalos de vermelho total");
  foreach(var route in UnityEngine.Object.FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None))if(route.Cruzamento!=null&&route.GetParada(out _,out var dir)){if(route.GetSemaforo()==null||Vector3.Dot(-route.GetSemaforo().transform.forward,dir)<.7f)throw new Exception("Sinal de direção incorreta em "+route.name);}report.AppendLine("PASS referências de semáforos orientadas conforme as aproximações");
  var manager=UnityEngine.Object.FindFirstObjectByType<PedestrianManager>();if(manager.excludedEntrances.Length==0)throw new Exception("Exclusão do Office não configurada");for(int i=0;i<manager.graph.nodes.Length;i++)if(manager.ResidenceAllowed(i))foreach(var office in manager.excludedEntrances)if(Vector3.ProjectOnPlane(manager.graph.nodes[i].position-office.transform.position,Vector3.up).magnitude<5)throw new Exception("Office escolhido como residência");report.AppendLine("PASS exclusão do Office por referência a CompanyEntrance");
  PedestrianCrossingChecks.Run();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);TrafficBehaviourChecks.Run();report.AppendLine("PASS regressões de travessias e trânsito");report.AppendLine("RESULT SUCCESS");File.WriteAllText("v4-editor-tests.txt",report.ToString());EditorApplication.Exit(0);
 }catch(Exception e){File.WriteAllText("v4-editor-tests.txt",report+"\nFAIL "+e);EditorApplication.Exit(1);}}
 static void Changed(PlayModeStateChange s){if(s!=PlayModeStateChange.EnteredPlayMode)return;if(SessionState.GetBool("V4City",false)){SessionState.SetBool("V4City",false);new GameObject("V4CityProbe").AddComponent<GridShiftV4CityProbe>();}if(SessionState.GetBool("V4Player",false)){SessionState.SetBool("V4Player",false);new GameObject("V4PlayerChecks").AddComponent<GridShiftV4PlayerChecks>();}}
}
public class GridShiftV4CityProbe:MonoBehaviour {
 IEnumerator Start(){Time.timeScale=6;var output=new StringBuilder();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;yield return new WaitForSeconds(20);
 for(int k=0;k<24;k++){yield return new WaitForSeconds(5);foreach(var car in TrafficCar.Active.ToArray())if(car.VelocidadeAtual<.05f){var block=typeof(TrafficCar).GetField("bloqueador",flags).GetValue(car) as Collider;output.AppendLine("CAR "+car.name+" "+car.transform.position+" route="+car.RotaAtual?.name+" signal="+car.RotaAtual?.GetSemaforo()?.PodePassar()+" reason="+car.MotivoParada+" blocker="+block?.name+" wp="+typeof(TrafficCar).GetField("pontoAtual",flags).GetValue(car));}}
 File.WriteAllText("v4-city-probe.txt",output.ToString());EditorApplication.Exit(0);}
}
public class GridShiftV4PlayerChecks:MonoBehaviour {
 readonly StringBuilder report=new StringBuilder(),frames=new StringBuilder();Keyboard keyboard;
 void Assert(bool ok,string text){if(!ok)throw new Exception(text);report.AppendLine("PASS "+text);}
 void Loaded(Scene s,LoadSceneMode mode){foreach(var root in s.GetRootGameObjects()){foreach(var c in root.GetComponentsInChildren<Camera>(true))c.enabled=false;foreach(var w in root.GetComponentsInChildren<ClimaReal>(true))w.enabled=false;}}
 IEnumerator Start(){Application.runInBackground=true;QualitySettings.vSyncCount=0;Application.targetFrameRate=60;
  InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
  keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.onAfterUpdate+=Current;SceneManager.sceneLoaded+=Loaded;
  var run=Run();bool ok=true;while(true){bool more;try{more=run.MoveNext();}catch(Exception e){ok=false;report.AppendLine("FAIL "+e);break;}if(!more)break;yield return run.Current;}
  InputSystem.onAfterUpdate-=Current;InputSystem.RemoveDevice(keyboard);SceneManager.sceneLoaded-=Loaded;
  File.WriteAllText("v4-player.txt",report+"\nRESULT "+(ok?"SUCCESS":"FAILURE"));File.WriteAllText("v4-player-frames.csv",frames.ToString());EditorApplication.Exit(ok?0:1);
 }
 void Current(){keyboard.MakeCurrent();}
 IEnumerator TapEscape(){InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;yield return null;}
 IEnumerator Running(PlayerMovement player){var cc=player.GetComponent<CharacterController>();var previous=player.transform.position;var rotation=player.transform.rotation;var camera=player.CameraTransform.GetComponent<Camera>();float fov=camera.fieldOfView;var home=camera.transform.localPosition;
  float oldIntensity=GamePreferences.CameraIntensity,oldBob=GamePreferences.HeadBob;bool oldRun=GamePreferences.RunBob;GamePreferences.CameraIntensity=1;GamePreferences.HeadBob=1;GamePreferences.RunBob=true;
  var floor=new GameObject("RunningTestFloor");floor.transform.position=new Vector3(1000,-4.5f,1000);var box=floor.AddComponent<BoxCollider>();box.size=new Vector3(100,.2f,100);cc.enabled=false;player.transform.SetPositionAndRotation(new Vector3(1000,-4.3f,1000),Quaternion.identity);cc.enabled=true;Physics.SyncTransforms();yield return new WaitForSeconds(.5f);
  var start=player.transform.position;InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));yield return new WaitForSeconds(1);float walk=Vector3.Distance(start,player.transform.position);start=player.transform.position;
  InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W,Key.LeftShift));float bob=0;for(int i=0;i<60;i++){yield return null;bob=Mathf.Max(bob,Vector3.Distance(home,camera.transform.localPosition));}float run=Vector3.Distance(start,player.transform.position);
  Assert(walk>4&&run>walk*1.3f,"caminhada/corrida reais: "+walk+"m / "+run+"m");Assert(camera.fieldOfView>fov+1&&bob>.002f&&bob<.06f,"FOV gradual e head bob limitado durante corrida");
  InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return new WaitForSeconds(1);Assert(Vector3.Distance(home,camera.transform.localPosition)<.001f&&Mathf.Abs(camera.fieldOfView-fov)<.01f,"câmera retorna ao repouso sem movimento involuntário");
  GamePreferences.CameraIntensity=0;GamePreferences.RunBob=false;InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W,Key.LeftShift));yield return new WaitForSeconds(.5f);Assert(Vector3.Distance(home,camera.transform.localPosition)<.001f&&Mathf.Abs(camera.fieldOfView-fov)<.01f,"configurações desligam os efeitos de corrida");
  InputSystem.QueueStateEvent(keyboard,new KeyboardState());GamePreferences.CameraIntensity=oldIntensity;GamePreferences.HeadBob=oldBob;GamePreferences.RunBob=oldRun;cc.enabled=false;player.transform.SetPositionAndRotation(previous,rotation);cc.enabled=true;Destroy(floor);Physics.SyncTransforms();yield return new WaitForSeconds(.5f);
 }
 IEnumerator Run(){yield return new WaitForSeconds(2);var p=UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();var jump=Jump(p);while(jump.MoveNext())yield return jump.Current;var running=Running(p);while(running.MoveNext())yield return running.Current;
  var pause=PauseMenu.Instance;Assert(pause!=null,"menu instalado automaticamente");pause.Open();yield return null;Assert(pause.IsOpen&&Time.timeScale==0&&p.ControlsBlocked,"pausa bloqueia controles e tempo");pause.ShowSettings(true);var esc=TapEscape();while(esc.MoveNext())yield return esc.Current;Assert(pause.IsOpen&&!pause.SettingsOpen,"ESC fecha somente configurações");esc=TapEscape();while(esc.MoveNext())yield return esc.Current;Assert(!pause.IsOpen&&Time.timeScale==1&&!p.ControlsBlocked,"ESC continua jogo e restaura controles");
  WorldSession.Instance.RequestTransition("Office",p.transform);pause.Open();Assert(!pause.IsOpen,"pausa recusada durante transição");while(WorldSession.Instance.Busy)yield return null;
  p=UnityEngine.Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(x=>x.gameObject.scene.name=="Office");jump=Jump(p);while(jump.MoveNext())yield return jump.Current;
  var terminal=UnityEngine.Object.FindObjectsByType<ComputerInteraction>(FindObjectsSortMode.None).First(x=>x.gameObject.scene.name=="Office");terminal.EnterComputer();while(terminal.MovingCamera)yield return null;esc=TapEscape();while(esc.MoveNext())yield return esc.Current;while(terminal.MovingCamera)yield return null;Assert(!terminal.UsingComputer&&!pause.IsOpen&&!p.ControlsBlocked,"ESC sai do PC sem abrir pausa");
 }
 IEnumerator Jump(PlayerMovement p){yield return new WaitForSeconds(.5f);int count=p.JumpsExecuted;float floor=p.transform.position.y,max=floor;report.AppendLine("INFO "+p.gameObject.scene.name+" RootMotion="+p.GetComponent<Animator>().applyRootMotion);
  InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));
  for(int i=0;i<150;i++){yield return null;max=Mathf.Max(max,p.transform.position.y);frames.AppendLine(p.gameObject.scene.name+","+i+","+p.transform.position.y+","+p.VerticalVelocity+","+p.GetComponent<CharacterController>().isGrounded+","+keyboard.spaceKey.isPressed);}
  Assert(max-floor>1.2f,p.gameObject.scene.name+" altura real com Update/Animator normais: "+(max-floor));Assert(Mathf.Abs(p.transform.position.y-floor)<.15f,"aterrissagem após Espaço mantido");
  Assert(p.JumpsExecuted==count+1,"Espaço mantido executou somente um salto");
  InputSystem.QueueStateEvent(keyboard,new KeyboardState());for(int i=0;i<8;i++)yield return null;
  max=p.transform.position.y;InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));for(int i=0;i<35;i++){yield return null;max=Mathf.Max(max,p.transform.position.y);}
  Assert(max-floor>1.2f,"segundo salto após soltar e pressionar novamente");InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;yield return null;count=p.JumpsExecuted;InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));yield return null;yield return null;Assert(p.JumpsExecuted==count,"segundo comando no ar ignorado");InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return new WaitForSeconds(1);
 }
}
