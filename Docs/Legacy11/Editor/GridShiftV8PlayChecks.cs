using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object=UnityEngine.Object;

[InitializeOnLoad] public static class GridShiftV8PlayChecks
{
    static GridShiftV8PlayChecks(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("V8Play",false)){SessionState.SetBool("V8Play",false);new GameObject("V8PlayChecks").AddComponent<GridShiftV8PlayRunner>();}if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("V8House",false)){SessionState.SetBool("V8House",false);new GameObject("V8HouseChecks").AddComponent<GridShiftV8HouseRunner>();}};}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void Prepare(){if(SessionState.GetBool("V8Play",false)){DeliverySaveStore.OverridePath=Path.GetFullPath("v8-test-"+Guid.NewGuid().ToString("N")+".json");GridShiftTutorial.Prefix="GS.V8.Tests.";GridShiftTutorial.Skip();}}
    public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");foreach(var w in Object.FindObjectsByType<ClimaReal>(FindObjectsSortMode.None))w.enabled=false;SessionState.SetBool("V8Play",true);EditorApplication.isPlaying=true;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void SafeHouse(){if(SessionState.GetBool("V8House",false)){DeliverySaveStore.OverridePath=Path.GetFullPath("v8-house-"+Guid.NewGuid().ToString("N")+".json");GridShiftTutorial.Prefix="GS.V8.HouseTest.";GridShiftTutorial.Skip();}}
    public static void House(){EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");foreach(var w in Object.FindObjectsByType<ClimaReal>(FindObjectsSortMode.None))w.enabled=false;SessionState.SetBool("V8House",true);EditorApplication.isPlaying=true;}
}
public sealed class GridShiftV8HouseRunner:MonoBehaviour {
    readonly StringBuilder report=new StringBuilder();
    void Check(bool ok,string label){if(!ok)throw new Exception(label);report.AppendLine("PASS "+label);}
    IEnumerator Start(){Time.timeScale=3;Application.runInBackground=true;var run=Run();bool ok=true;while(true){bool more;object value=null;try{more=run.MoveNext();if(more)value=run.Current;}catch(Exception e){ok=false;report.AppendLine("FAIL "+e);break;}if(!more)break;yield return value;}Time.timeScale=1;report.AppendLine("RESULT "+(ok?"SUCCESS":"FAILURE"));File.WriteAllText("v8-house-boundary.txt",report.ToString());EditorApplication.Exit(ok?0:1);}
    IEnumerator Run(){yield return new WaitForSecondsRealtime(2);var manager=Object.FindFirstObjectByType<PedestrianManager>();manager.StopAllCoroutines();var nodes=manager.graph.nodes;int home=manager.Entrances.First(i=>nodes[i].position.x>435);int start=Enumerable.Range(0,nodes.Length).Where(i=>!nodes[i].entrancePath&&nodes[i].neighbours.Length>0&&manager.Clear(null,nodes[i].position,1)).OrderBy(i=>(nodes[i].position-nodes[home].position).sqrMagnitude).First();
        var camera=new GameObject("HouseObserver").AddComponent<Camera>();camera.transform.position=nodes[home].position+new Vector3(-5,2,0);camera.transform.LookAt(nodes[home].position+Vector3.up);manager.playerCamera=camera;
        var go=Object.Instantiate(manager.models[0],manager.transform);var agent=go.AddComponent<PedestrianAgent>();agent.Initialize(manager,start,1.2f);((List<PedestrianAgent>)manager.Agents).Add(agent);Check(agent.WalkTo(home),"rota real até a entrada residencial nova");
        float until=Time.realtimeSinceStartup+35;while(agent.CurrentActivity!=PedestrianAgent.Activity.EnteringBuilding&&Time.realtimeSinceStartup<until)yield return null;
        Check(agent.CurrentActivity==PedestrianAgent.Activity.EnteringBuilding&&agent.DistanceWalked>2,"pedestre chegou à porta caminhando");yield return new WaitForSeconds(.8f);Check(agent.gameObject.activeSelf,"transição não remove personagem diante do observador");camera.enabled=false;yield return new WaitForSeconds(1);Check(!agent.gameObject.activeSelf,"chegada fora de vista devolve personagem ao pool");Object.Destroy(camera.gameObject);
        var player=Object.FindFirstObjectByType<PlayerMovement>();player.BlockControls(true);var cc=player.GetComponent<CharacterController>();var boundary=Object.FindFirstObjectByType<GridShiftCityBoundary>();var b=boundary.playable;var original=player.transform.position;
        for(int side=0;side<4;side++){var at=new Vector3(side<2?(side==0?b.min.x+3:b.max.x-3):b.center.x,boundary.groundHeight+.2f,side<2?b.center.z:(side==2?b.min.z+3:b.max.z-3));cc.enabled=false;player.transform.position=at;cc.enabled=true;Physics.SyncTransforms();cc.Move((side==0?Vector3.left:side==1?Vector3.right:side==2?Vector3.back:Vector3.forward)*12);var p=player.transform.position;Check(p.x>=b.min.x&&p.x<=b.max.x&&p.z>=b.min.z&&p.z<=b.max.z,"limite físico bloqueia saída "+side);}
        cc.enabled=false;player.transform.position=original;cc.enabled=true;yield return new WaitForSecondsRealtime(.5f);cc.enabled=false;player.transform.position=original+Vector3.down*20;cc.enabled=true;yield return new WaitForSecondsRealtime(.5f);Check(player.transform.position.y>boundary.groundHeight-1,"queda anormal aciona recuperação de segurança do jogador");
    }
}
public sealed class GridShiftV8PlayRunner:MonoBehaviour
{
    readonly StringBuilder report=new StringBuilder();readonly List<string> errors=new List<string>();
    void Check(bool ok,string label){if(!ok)throw new Exception(label);report.AppendLine("PASS "+label);File.WriteAllText("v8-play-progress.txt",report.ToString());}
    void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)errors.Add(message);}
    IEnumerator Start(){Application.runInBackground=true;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;Application.logMessageReceived+=Log;var stack=new Stack<IEnumerator>();stack.Push(Run());bool ok=true;
        while(stack.Count>0){bool more;object value=null;try{more=stack.Peek().MoveNext();if(more)value=stack.Peek().Current;}catch(Exception e){ok=false;report.AppendLine("FAIL "+e);break;}if(!more){stack.Pop();continue;}if(value is IEnumerator nested&&!(value is CustomYieldInstruction)){stack.Push(nested);continue;}yield return value;}
        Time.timeScale=1;Application.logMessageReceived-=Log;if(errors.Count>0){ok=false;report.AppendLine("ERRORS "+string.Join("\n",errors));}report.AppendLine("RESULT "+(ok?"SUCCESS":"FAILURE"));File.WriteAllText("v8-play.txt",report.ToString());EditorApplication.Exit(ok?0:1);
    }
    IEnumerator Run(){
        yield return new WaitForSecondsRealtime(3);
        var player=Object.FindFirstObjectByType<PlayerMovement>();var manager=DeliveryManager.Instance;var ui=GridShiftOperationsUI.Instance;
        ui.Open("map");yield return new WaitForSecondsRealtime(.4f);
        Check(ui.Current=="map","mapa aberto com jogador bloqueado");var image=ui.GetComponentsInChildren<RawImage>().First(r=>r.name=="ExpandedCityMap");
        var panel=(RectTransform)ui.transform.Find("OperationOverlay/Panel");Check(image.rectTransform.rect.height>panel.rect.height*.72f,"mapa aproveita mais de 72% da altura do painel");
        var mini=player.GetComponentInChildren<MinimapCamera>();var cam=mini.GetComponent<Camera>();float initial=cam.orthographicSize;
        typeof(GridShiftOperationsUI).GetMethod("ZoomMap",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(ui,new object[]{.45f});yield return null;var center=cam.transform.position;
        ui.PanMap(new Vector2(60,25),image.rectTransform.rect.size);yield return null;Check(Vector3.Distance(center,cam.transform.position)>1,"arraste move câmera na escala do mapa");
        ui.PanMap(new Vector2(100000,100000),image.rectTransform.rect.size);yield return null;Check(cam.transform.position.x>70&&cam.transform.position.z>70,"arraste limitado à cidade");
        foreach(var button in ui.GetComponentsInChildren<Button>())if(button.name=="Ver toda a cidade")button.onClick.Invoke();yield return null;Check(Mathf.Abs(cam.orthographicSize-initial)<.1f,"enquadramento completo restaura zoom");yield return new WaitForSecondsRealtime(.15f);
        Capture(player.CameraTransform.GetComponent<Camera>(),"v8-map.png",true);ui.Close();yield return null;Check(!mini.Expanded&&!player.ControlsBlocked,"fechar mapa restaura controles e minimapa");
        var population=Object.FindFirstObjectByType<PedestrianManager>();var routes=Object.FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None);var newDest=manager.points.Where(p=>p.id.StartsWith("P8-")).ToArray();
        Check(newDest.Length>=3,"novos estabelecimentos disponíveis para entregas");
        var vehicle=manager.fleet.First(v=>v.Available);var pickup=manager.points.First(p=>p.Valid&&manager.Progression.Unlocked(p)&&DeliveryRouting.Find(vehicle.driver.RotaAtual,p.route,DeliveryRouting.Behind(vehicle.driver,p))!=null);
        var destination=newDest.First(p=>p.Valid&&DeliveryRouting.Find(pickup.route,p.route)!=null);
        Check(manager.TryCreate(new DeliveryRequest{goods="Regressão bairro leste",weightKg=50,pickupId=pickup.id,destinationId=destination.id},out var order,out var error),"criar entrega para expansão: "+error);manager.Dispatch();
        var start=new Dictionary<TrafficCar,Vector3>();foreach(var car in TrafficCar.Active)if(car!=null)start[car]=car.transform.position;
        float frames=0,total=0;float until=Time.realtimeSinceStartup+20;while(Time.realtimeSinceStartup<until){frames++;total+=Time.unscaledDeltaTime;yield return null;}
        report.AppendLine("MEASUREMENT cidade expandida, editor, média="+(frames/Mathf.Max(.001f,total)).ToString("F1")+" FPS; não comparável a build final");
        Check(population.Agents.Count>5,"população criada por gerenciador existente");Check(population.Agents.Sum(a=>a.DistanceWalked)>10,"pedestres progridem com animação");
        Check(TrafficCar.Active.Count(c=>c!=null&&c.enabled)>5,"veículos permanecem habilitados");
        Check(start.Count(k=>k.Key!=null&&Vector3.Distance(k.Key.transform.position,k.Value)>2)>2,"vários veículos progridem na simulação");
        Capture(player.CameraTransform.GetComponent<Camera>(),"v8-city.png",false);
        Time.timeScale=4;float deadline=Time.realtimeSinceStartup+180;float traceAt=0;while(order.status!=DeliveryStatus.Completed&&Time.realtimeSinceStartup<deadline){if(Time.realtimeSinceStartup>traceAt){traceAt=Time.realtimeSinceStartup+10;File.AppendAllText("v8-agent-trace.txt","ORDER "+order.status+" / "+order.travelledMeters+"\n"+string.Join("\n",TrafficCar.Active.Where(c=>c!=null).Select(c=>(c.GetComponent<CompanyVehicle>()?.id??c.name)+" "+c.transform.position+" "+c.Diagnostic+" "+c.DiagnosticDetail+" next="+c.NextPoint+" target="+(c.RotaAtual!=null&&c.NextPoint<c.RotaAtual.pontos.Length?c.RotaAtual.pontos[c.NextPoint].position.ToString():"none")+" forward="+c.transform.forward+" blocker="+(c.Obstacle!=null?c.Obstacle.name:"none")))+"\n");}yield return null;}
        Check(order.status==DeliveryStatus.Completed&&order.rewardPaid,"entrega ao bairro novo concluída e paga uma vez");
    }
    static void Capture(Camera view,string file,bool overlay){
        var camera=new GameObject("Capture8").AddComponent<Camera>();camera.CopyFrom(view);camera.enabled=false;camera.transform.SetPositionAndRotation(view.transform.position,view.transform.rotation);
        var canvases=overlay?Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray():Array.Empty<Canvas>();foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=.2f;}
        var target=new RenderTexture(1920,1080,24);camera.targetTexture=target;Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;var texture=new Texture2D(1920,1080,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1920,1080),0,0);texture.Apply();File.WriteAllBytes(file,texture.EncodeToPNG());RenderTexture.active=previous;
        foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}target.Release();Object.Destroy(target);Object.Destroy(texture);Object.Destroy(camera.gameObject);
    }
}
