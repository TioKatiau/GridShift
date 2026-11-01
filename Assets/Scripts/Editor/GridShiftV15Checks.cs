using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object=UnityEngine.Object;

[InitializeOnLoad] public static class GridShiftV15Checks {
 static GridShiftV15Checks(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("V15",false)){new GameObject("V15Checks").AddComponent<GridShiftV15Runner>();}};}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Prepare(){if(!SessionState.GetBool("V15",false))return;DeliverySaveStore.OverridePath=Path.GetFullPath("Temp/V15-"+Guid.NewGuid().ToString("N")+"/slot.json");GridShiftCompany.SuppressIntro=true;GridShiftTutorial.Prefix="GS.V15.Test.";GridShiftTutorial.Skip();}
 public static void Batch(){GridShiftV15Setup.Configure();EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");SessionState.SetBool("V15",true);EditorApplication.isPlaying=true;}
 public static void VisualBatch(){SessionState.SetBool("V15Visual",true);Batch();}
}
public sealed class GridShiftV15Runner:MonoBehaviour {
 readonly StringBuilder report=new StringBuilder();float deadline;bool done;
 void Check(bool ok,string text){report.AppendLine((ok?"PASS ":"FAIL ")+text);Write();if(!ok)Finish(false);}
 void Write(){Directory.CreateDirectory("Docs/Validacao15");File.WriteAllText("Docs/Validacao15/runtime.txt",report.ToString());}
 void Finish(bool ok){if(done)return;done=true;SessionState.SetBool("V15",false);report.AppendLine("RESULT "+(ok?"SUCCESS":"FAILURE"));Write();EditorApplication.Exit(ok?0:1);}
 void Update(){if(!done&&deadline>0&&Time.realtimeSinceStartup>deadline){report.AppendLine("FAIL timeout de execução");Finish(false);}}
 static void Place(PlayerMovement player,Vector3 at){var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.position=at+Vector3.up*.15f;Physics.SyncTransforms();cc.enabled=true;}
 IEnumerator Walk(PlayerMovement player,Vector3 destination){
  var pedestrians=Object.FindFirstObjectByType<PedestrianManager>();var nodes=pedestrians.graph.nodes;
  int Nearest(Vector3 at)=>Enumerable.Range(0,nodes.Length).OrderBy(i=>(nodes[i].position-at).sqrMagnitude).First();
  var path=new Queue<int>();Check(pedestrians.Path(Nearest(player.transform.position),Nearest(destination),path,allowServiceEntrance:true),"percurso de passeio conecta coleta ao destino pessoal");if(done)yield break;
  player.enabled=false;float until=Time.realtimeSinceStartup+90;deadline=until+30;
  while(path.Count>0&&Time.realtimeSinceStartup<until){var target=nodes[path.Peek()].position;var delta=Vector3.ProjectOnPlane(target-player.transform.position,Vector3.up);if(delta.magnitude<.22f){path.Dequeue();continue;}
   player.transform.rotation=Quaternion.RotateTowards(player.transform.rotation,Quaternion.LookRotation(delta),240*Time.deltaTime);
   var direction=delta.normalized;var input=new Vector2(Vector3.Dot(direction,player.transform.right),Vector3.Dot(direction,player.transform.forward));player.SimulateMovement(input,false,false,Time.deltaTime);yield return null;
  }
  player.enabled=true;Check(path.Count==0&&Vector3.ProjectOnPlane(destination-player.transform.position,Vector3.up).magnitude<1,"jogador percorre caminho real carregando caixa com CharacterController");
 }
 IEnumerator Start(){Object.DontDestroyOnLoad(gameObject);deadline=Time.realtimeSinceStartup+150;
  while(DeliveryManager.Instance==null||!DeliveryManager.Instance.GetComponent<DeliveryPersistence>().Ready)yield return null;
  var manager=DeliveryManager.Instance;var player=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(p=>p.gameObject.scene.name=="SampleScene");
  foreach(var weather in Object.FindObjectsByType<ClimaReal>(FindObjectsSortMode.None))weather.enabled=false;
  player.BlockControls(false);manager.Dispatch();
  var offer=manager.Offers().FirstOrDefault(o=>o.weightKg<=manager.lightThresholdKg&&GridShiftDeliveryAccess.TryGet(manager.Point(o.pickupId),out _)&&GridShiftDeliveryAccess.TryGet(manager.Point(o.destinationId),out _));
  Check(offer!=null,"oferta leve real possui dois acessos físicos");if(done)yield break;
  Check(manager.AcceptPersonal(offer.id,out var error),"aceitar entrega pessoal: "+error);if(done)yield break;
  var order=manager.PersonalOrder;manager.Dispatch();
  Check(order!=null&&order.status==DeliveryStatus.ToPickup&&order.vehicleId==null,"pedido pessoal não é designado à frota");
  Check(!manager.TryCreate(new DeliveryRequest{mode=DeliveryMode.Personal,goods="Carga pesada incompatível",weightKg=500,pickupId=order.pickupId,destinationId=order.destinationId},out _,out _),"carga pesada não pode ser assumida pelo jogador");
  Check(!manager.PersonalInteract(player,order.destinationId,out _),"destino não permite coletar encomenda da origem");
  GridShiftDeliveryAccess.TryGet(manager.Point(order.pickupId),out var pickup);Place(player,pickup);yield return null;
  var obstacle=GameObject.CreatePrimitive(PrimitiveType.Cube);obstacle.name="V15 sight blocker";obstacle.transform.position=(player.CameraTransform.position+pickup+Vector3.up*.65f)*.5f;obstacle.transform.localScale=new Vector3(.7f,.15f,.7f);Physics.SyncTransforms();
  Check(!manager.PersonalInteract(player,order.pickupId,out _),"obstáculo físico impede coleta através de parede");Object.Destroy(obstacle);yield return null;
  report.AppendLine("TRACE pickup="+pickup+" player="+player.transform.position+" camera="+player.CameraTransform.position+" blocked="+player.ControlsBlocked+" scene="+SceneManager.GetActiveScene().name+" reach="+GridShiftDeliveryAccess.InReach(player,pickup));Write();
  Check(manager.PersonalInteract(player,order.pickupId,out error),"coleta física no acesso de origem: "+error);if(done)yield break;
  yield return new WaitForSeconds(.4f);
  Check(order.status==DeliveryStatus.Transporting&&player.CameraTransform.GetComponentsInChildren<Renderer>().Any(r=>r.name.StartsWith("Encomenda ")),"caixa real fica vinculada à câmera após coletar");
  GridShiftV11Capture.Save("v15-carrying",player.CameraTransform.GetComponent<Camera>());
  var store=manager.GetComponent<DeliveryPersistence>();Check(store.Save(),"salvar operação pessoal carregando caixa");
  Check(DeliverySaveStore.Read(out var saved)&&saved.version==4&&saved.orders.Any(o=>o.id==order.id&&o.mode==DeliveryMode.Personal&&o.status==DeliveryStatus.Transporting),"save v4 preserva modalidade e caixa carregada");
  DeliverySaveStore.Pending=saved;yield return SceneManager.LoadSceneAsync("SampleScene",LoadSceneMode.Single);
  while(DeliveryManager.Instance==null||!DeliveryManager.Instance.GetComponent<DeliveryPersistence>().Ready)yield return null;
  manager=DeliveryManager.Instance;store=manager.GetComponent<DeliveryPersistence>();player=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(p=>p.gameObject.scene.name=="SampleScene");player.BlockControls(false);order=manager.PersonalOrder;yield return new WaitForSeconds(.5f);
  Check(order!=null&&order.status==DeliveryStatus.Transporting&&player.CameraTransform.GetComponentsInChildren<Renderer>().Count(r=>r.name.StartsWith("Encomenda "))==1,"recarregar a cena restaura exatamente uma caixa e a operação pessoal");
  Check(!manager.PersonalInteract(player,order.pickupId,out _),"origem não recebe encomenda destinada a outro local");
  GridShiftDeliveryAccess.TryGet(manager.Point(order.destinationId),out var destination);yield return Walk(player,destination);if(done)yield break;yield return null;
  float before=manager.Money;Check(manager.PersonalInteract(player,order.destinationId,out error),"entrega física confirmada no destino: "+error);if(done)yield break;
  Check(order.status==DeliveryStatus.Completed&&order.rewardPaid&&manager.Money>before,"entrega libera pagamento real e conclui pedido");
  float after=manager.Money;manager.Complete(order);Check(manager.Money==after,"repetir conclusão não duplica pagamento");
  Check(manager.PersonalOrder==null&&manager.SelectedId==null,"conclusão limpa encomenda e acompanhamento");
  Check(manager.Company.Missions.RequirementsMet(manager.Company.State.missions.First(m=>m.id=="personal-express")),"missão expressa exige entrega pessoal realmente concluída");
  Check(store.Save()&&DeliverySaveStore.Read(out saved)&&saved.orders.Single(o=>o.id==order.id).rewardPaid,"histórico pago persiste no mesmo armazenamento");
  var tablet=player.GetComponent<PortableTablet>();Check(tablet.Open(),"tablet real abre após entrega pessoal");yield return new WaitForSeconds(1);
  var ops=tablet.Operations;ops.Show("Entregas");yield return new WaitForSeconds(.5f);
  Check(ops.GetComponentsInChildren<TMPro.TMP_Text>().Any(t=>t.text=="ENTREGAS CONCLUÍDAS"),"interface possui seção separada de concluídas");
  Check(!ops.GetComponentsInChildren<Button>().Any(b=>b.name=="Acompanhar entrega"),"histórico concluído não recebe botão de acompanhamento");
  ops.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition=0;yield return new WaitForSeconds(.3f);GridShiftV11Capture.Save("v15-tablet-history",player.CameraTransform.GetComponent<Camera>());
  tablet.Close();yield return new WaitForSeconds(1);
  if(SessionState.GetBool("V15Visual",false)){SessionState.SetBool("V15Visual",false);Finish(true);yield break;}
  var heavy=manager.Offers().FirstOrDefault(o=>o.weightKg>manager.truckThresholdKg);
  Check(heavy!=null&&manager.Accept(heavy.id,out _),"aceitar contrato pesado pela frota existente");if(done)yield break;
  var fleetOrder=manager.orders.Last();Time.timeScale=6;deadline=Time.realtimeSinceStartup+240;
  float traceAt=0;bool observedDriver=false,observedReturn=false,restoredCourier=false,capturedDriver=false;
  while(fleetOrder.status!=DeliveryStatus.Completed&&Time.realtimeSinceStartup<deadline-2){
   observedDriver|=fleetOrder.courierPhase==1;observedReturn|=fleetOrder.courierPhase>=3;
   if(fleetOrder.courierPhase==1&&restoredCourier&&!capturedDriver){
    player=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(p=>p.gameObject.scene.name=="SampleScene");var v=manager.fleet.First(x=>x!=null&&x.Order==fleetOrder);var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.enabled=false;player.transform.position=v.transform.position+new Vector3(-5,0,-5);
    yield return new WaitForSeconds(.5f);var actor=Object.FindObjectsByType<PedestrianAgent>(FindObjectsSortMode.None).FirstOrDefault(a=>a.ExternalControl);
    if(actor!=null){
     WorldSession.Instance.RestoreClock(960);yield return new WaitForSeconds(.5f);
     var look=actor.transform.position+Vector3.up;
     foreach(var side in new[]{Vector3.forward,Vector3.back,Vector3.right,Vector3.left}){var eye=look+side*3+Vector3.up*1.2f;if(!Physics.Linecast(eye,look,~(1<<9),QueryTriggerInteraction.Ignore)){player.CameraTransform.position=eye;break;}}
     player.CameraTransform.rotation=Quaternion.LookRotation(look-player.CameraTransform.position);var pose=actor.GetComponentInChildren<GridShiftCourierCarryPose>();pose.ApplyPose();Check(pose.Applied,"pose de transporte encontra e posiciona os braços do modelo real");GridShiftV11Capture.Save("v15-courier",player.CameraTransform.GetComponent<Camera>());Check(actor.GetComponentsInChildren<Animator>().Length>0,"aproximar observador ativa modelo animado do entregador");
    }
    capturedDriver=true;
   }
   if(fleetOrder.courierPhase==1&&!restoredCourier){
    string id=fleetOrder.id;store=manager.GetComponent<DeliveryPersistence>();Check(store.Save()&&DeliverySaveStore.Read(out saved),"salvar descarga com entregador em percurso");if(done)yield break;
    DeliverySaveStore.Pending=saved;yield return SceneManager.LoadSceneAsync("SampleScene",LoadSceneMode.Single);
    while(DeliveryManager.Instance==null||!DeliveryManager.Instance.GetComponent<DeliveryPersistence>().Ready)yield return null;
    manager=DeliveryManager.Instance;fleetOrder=manager.orders.Single(o=>o.id==id);restoredCourier=true;
    Check(fleetOrder.courierInitialized&&fleetOrder.courierPhase==1&&!fleetOrder.rewardPaid,"recarregar mantém descarga pendente e posição lógica do entregador");
   }
   if(Time.realtimeSinceStartup>=traceAt){traceAt=Time.realtimeSinceStartup+10;var v=manager.fleet.FirstOrDefault(x=>x!=null&&x.Order==fleetOrder);report.AppendLine("TRACE fleet="+fleetOrder.status+" phase="+fleetOrder.courierPhase+" vehicle="+v?.id+" stop="+v?.driver.MotivoParada+" courier="+v?.GetComponent<GridShiftCourierDelivery>()?.Diagnostic);Write();}
   yield return null;
  }
  Time.timeScale=1;
  Check(fleetOrder.status==DeliveryStatus.Completed&&fleetOrder.courierPhase==5&&observedDriver&&observedReturn,"caminhão percorre rota, entregador entrega e retorna antes do pagamento");if(done)yield break;
  // The start is positioned at the pickup fixture. Travel to the destination
  // uses the real controller and graph, not position replacement.
  Finish(true);
 }
}
