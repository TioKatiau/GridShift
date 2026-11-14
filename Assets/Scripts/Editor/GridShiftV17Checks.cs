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
using Object=UnityEngine.Object;

[InitializeOnLoad]public static class GridShiftV17Checks {
 static GridShiftV17Checks(){EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("V17",false)){var root=new GameObject("V16Checks");Object.DontDestroyOnLoad(root);root.AddComponent<GridShiftV17Runner>();}};}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Prepare(){if(!SessionState.GetBool("V17",false))return;DeliverySaveStore.OverridePath=Path.GetFullPath("Temp/V17-"+Guid.NewGuid().ToString("N")+"/slot.json");GridShiftCompany.SuppressIntro=true;GridShiftTutorial.Prefix="GS.V17.Test.";GridShiftTutorial.Skip();}
 public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");SessionState.SetBool("V17",true);EditorApplication.isPlaying=true;}
}
public sealed class GridShiftV17Runner:MonoBehaviour {
 readonly StringBuilder report=new StringBuilder();bool done;float deadline;
 void Check(bool ok,string text){report.AppendLine((ok?"PASS ":"FAIL ")+text);Write();if(!ok)Finish(false);}
 void Write(){Directory.CreateDirectory("Docs/Validacao17");File.WriteAllText("Docs/Validacao17/runtime.txt",report.ToString());}
 void Finish(bool ok){if(done)return;done=true;SessionState.SetBool("V17",false);report.AppendLine("RESULT "+(ok?"SUCCESS":"FAILURE"));Write();EditorApplication.Exit(ok?0:1);}
 void Update(){if(!done&&deadline>0&&Time.realtimeSinceStartup>deadline){report.AppendLine("FAIL timeout");Finish(false);}}
 static void Place(PlayerMovement player,Vector3 at){var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.position=at+Vector3.up*.15f;Physics.SyncTransforms();cc.enabled=true;player.BlockControls(false);}
 IEnumerator Ready(){while(DeliveryManager.Instance==null||!DeliveryManager.Instance.GetComponent<DeliveryPersistence>().Ready||DeliveryManager.Instance.GetComponent<GridShiftActiveOperations>()?.Van==null)yield return null;}
 IEnumerator Start(){deadline=Time.realtimeSinceStartup+180;yield return Ready();
  var manager=DeliveryManager.Instance;var operations=manager.GetComponent<GridShiftActiveOperations>();var van=operations.Van;var player=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(p=>p.gameObject.scene.name=="SampleScene");WorldSession.Instance.RestoreClock(960);
  foreach(var weather in Object.FindObjectsByType<ClimaReal>(FindObjectsSortMode.None)){weather.StopAllCoroutines();weather.enabled=false;}
  Check(van.GetComponent<TrafficCar>()==null&&van.GetComponent<CompanyVehicle>()==null,"van pessoal não possui controle autônomo concorrente");
  Place(player,operations.StationPosition("inspect"));Check(operations.Execute(player,"inspect",null,out _),"inspeção presencial consulta combustível e condição reais da van");
  Place(player,operations.StationPosition("accept"));Check(operations.Execute(player,"accept",null,out var error),"pedido real aceito no quadro do depósito: "+error);if(done)yield break;yield return null;
  var order=manager.PersonalOrder;string orderId=order.id;Check(order.depotRequired&&!order.depotReleased,"pedido aguarda preparação física");
  Check(!manager.PersonalInteract(player,order.pickupId,out _),"coleta recusada antes da liberação");
  Place(player,operations.StationPosition("pack-small"));Check(!operations.Execute(player,"pack-small",order.id,out _),"embalagem exige conferência anterior");
  Place(player,operations.StationPosition("scan"));Check(!operations.Execute(player,"scan","PKG-inexistente",out _),"leitor recusa código que não pertence à encomenda");
  var identity=manager.GetComponent<GridShiftPersonalDelivery>().PackageIdentity;Check(identity!=null&&identity.orderId==order.id,"caixa física possui identificador próprio e pedido vinculado");if(done)yield break;
  Check(operations.Execute(player,"scan",identity.packageId,out error),"leitor confere código da caixa real: "+error);if(done)yield break;
  Place(player,operations.StationPosition("pack-small"));Check(!operations.Execute(player,"pack-small",order.id,out _),"caixa pequena rejeita carga de oito quilos");
  Place(player,operations.StationPosition("pack-medium"));Check(operations.Execute(player,"pack-medium",order.id,out _),"caixa média compatível embala o pedido");
  Place(player,operations.StationPosition("label"));Check(operations.Execute(player,"label",order.id,out _),"etiqueta registra endereço e peso reais");
  DeliverySaveData saved=null;var persistence=manager.GetComponent<DeliveryPersistence>();Check(persistence.Save()&&DeliverySaveStore.Read(out saved),"salvar preparação intermediária");if(done)yield break;
  Check(saved.version==5&&saved.orders.Single(o=>o.id==orderId).depotLabelled,"save v5 inclui preparação e van");
  DeliverySaveStore.Pending=saved;yield return SceneManager.LoadSceneAsync("SampleScene",LoadSceneMode.Single);yield return Ready();
  manager=DeliveryManager.Instance;operations=manager.GetComponent<GridShiftActiveOperations>();van=operations.Van;player=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(p=>p.gameObject.scene.name=="SampleScene");order=manager.PersonalOrder;yield return new WaitForSeconds(.3f);
  Check(order.id==orderId&&order.depotScanned&&order.depotPacked&&order.depotLabelled&&!order.depotSorted,"recarregar preserva exatamente a etapa de preparação");
  Check(manager.GetComponent<GridShiftPersonalDelivery>().PackageIdentity.GetComponentInChildren<TMPro.TMP_Text>().text.Contains(orderId.Substring(0,8)),"etiqueta física da caixa restaura o identificador do pedido");
  int zone=operations.RequiredZone(order);string wrong=zone==0?"sort-b":"sort-a",correct=zone==0?"sort-a":"sort-b";
  Place(player,operations.StationPosition(wrong));Check(!operations.Execute(player,wrong,order.id,out _),"separação recusa setor de destino errado");
  Place(player,operations.StationPosition(correct));Check(operations.Execute(player,correct,order.id,out _),"encomenda separada no setor correto");
  Place(player,operations.StationPosition("release"));Check(operations.Execute(player,"release",order.id,out _),"expedição libera somente pedido preparado");
  Place(player,operations.StationPosition(correct));Check(manager.PersonalInteract(player,order.pickupId,out error),"retirar pacote preparado da prateleira: "+error);if(done)yield break;yield return new WaitForSeconds(.3f);
  Place(player,van.CargoPosition);if(!van.CargoOpen)van.ToggleCargo(player);Check(van.TransferCargo(player),"guardar encomenda real no compartimento da van");if(done)yield break;yield return null;
  Check(van.CargoOrderId==order.id&&!manager.PersonalInteract(player,order.destinationId,out _),"carga na van não pode ser entregue remotamente");
  Check(van.FindExit(player,out var entry),"existe posição de acesso com espaço para o corpo do jogador");Place(player,entry);yield return null;Check(van.Enter(player),"entrar na van por acesso livre");if(done)yield break;
  Check(van.GetComponentInChildren<GridShiftVanOrbit>()!=null,"orbital camera installed");
  Check(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(l=>l.isActiveAndEnabled)==1,"exactly one listener at vehicle entry");
  GridShiftOperationsUI.Instance.Open("map");yield return null;Check(GridShiftOperationsUI.Instance.Current=="map","map opens while driving");var personalRoute=new System.Collections.Generic.List<Vector3>();Check(GridShiftTrackedRoute.BuildForMap(manager,personalRoute)&&personalRoute.Count>1&&Vector3.Distance(personalRoute.Last(),manager.Point(order.destinationId).Stop)<.1f,"rota pessoal usa grafo viário real e termina no destino correto");yield return new WaitForSeconds(.5f);GridShiftV11Capture.Save("v17-active-personal-map",Camera.main);van.SetInput(1,1,false);yield return new WaitForSeconds(.1f);GridShiftOperationsUI.Instance.Close();
  Check(!Object.FindObjectsByType<TMPro.TextMeshPro>(FindObjectsSortMode.None).Any(t=>t.name=="ActionPrompt"||t.name=="PackagePrompt"),"no per-object giant interaction labels");
  van.ScriptedInput=true;var start=van.transform.position;float yaw=van.transform.eulerAngles.y;float fuel=van.Health.fuel;van.SetInput(1,.3f,false);yield return new WaitForSeconds(.65f);
  Check(Camera.allCameras.Count(c=>c.CompareTag("MainCamera"))==1,"condução mantém uma câmera principal ativa");Check(van.GetComponents<AudioSource>().Any(s=>s.clip!=null&&s.isPlaying&&s.outputAudioMixerGroup!=null&&s.outputAudioMixerGroup.name=="Vehicles"),"motor reproduz áudio existente no canal Vehicles");
  van.SetInput(0,0,true);PauseMenu.Instance.Open();var paused=van.transform.position;yield return new WaitForSecondsRealtime(.3f);Check(Time.timeScale==0&&Vector3.Distance(paused,van.transform.position)<.01f,"pausa congela condução sem perder estado do motorista");PauseMenu.Instance.Close();van.SetInput(1,0,false);
  float travelled=Vector3.ProjectOnPlane(van.transform.position-start,Vector3.up).magnitude;report.AppendLine("TRACE van_moved="+travelled+" speed="+van.Speed+" fuel="+van.Health.fuel+" obstacle="+van.LastObstacle+" position="+van.transform.position);Write();
  Check(travelled>.3f&&van.Speed>.2f&&van.Health.fuel<fuel,"condução move Rigidbody real e consome combustível empresarial");
  Check(Mathf.Abs(Mathf.DeltaAngle(yaw,van.transform.eulerAngles.y))>.3f,"direção altera orientação física da van em movimento");
  Check(!van.Exit(),"não desembarca com veículo em movimento");
  var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="V16 collision fixture";var bodyBounds=van.GetComponent<BoxCollider>().bounds;wall.transform.position=bodyBounds.center+van.transform.forward*(bodyBounds.extents.z+1.1f);wall.transform.rotation=van.transform.rotation;wall.transform.localScale=new Vector3(bodyBounds.size.x+2,4,.2f);Physics.SyncTransforms();yield return new WaitForSeconds(.8f);
  Check(Vector3.Dot(van.GetComponent<BoxCollider>().bounds.center-wall.transform.position,wall.transform.forward)<-.5f,"colisão real impede atravessar obstáculo estático");Object.Destroy(wall);var reverseStart=van.transform.position;van.SetInput(-1,0,false);yield return new WaitForSeconds(.5f);Check(Vector3.Dot(van.transform.position-reverseStart,van.transform.forward)<-.1f,"ré desloca o veículo para trás sem controle autônomo");van.SetInput(0,0,true);float until=Time.realtimeSinceStartup+8;while(van.Speed>.25f&&Time.realtimeSinceStartup<until)yield return null;
  Check(van.Exit(),"frear, estacionar e desembarcar em ponto seguro");if(done)yield break;
  Check(!player.ControlsBlocked&&player.GetComponent<CharacterController>().enabled,"saída restaura controlador e câmera do jogador");
  persistence=manager.GetComponent<DeliveryPersistence>();Check(persistence.Save()&&DeliverySaveStore.Read(out saved)&&saved.personalVan.cargoOrderId==order.id,"carga e posição da van persistem no save");
  float savedCondition=van.Health.condition;var parked=van.transform.position;Check(van.Enter(player),"retomar o assento após desembarque seguro: "+van.LastError);if(done)yield break;Check(persistence.Save()&&DeliverySaveStore.Read(out saved)&&Vector3.Distance(saved.activePlayerPosition,saved.personalVan.position)>.5f,"save ao volante registra desembarque seguro para restauração");
  DeliverySaveStore.Pending=saved;yield return SceneManager.LoadSceneAsync("SampleScene",LoadSceneMode.Single);yield return Ready();
  manager=DeliveryManager.Instance;operations=manager.GetComponent<GridShiftActiveOperations>();van=operations.Van;player=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(p=>p.gameObject.scene.name=="SampleScene");order=manager.PersonalOrder;yield return new WaitForSeconds(.3f);
  Check(Object.FindObjectsByType<GridShiftPlayerVan>(FindObjectsSortMode.None).Length==1&&van.CargoOrderId==order.id&&Vector3.Distance(van.transform.position,parked)<.5f,"carregar restaura uma única van, posição e carga corretas");
  Check(Mathf.Abs(van.Health.condition-savedCondition)<.1f,"condição mecânica persiste ao recarregar o slot");Check(!van.Driving&&!player.ControlsBlocked&&player.GetComponent<CharacterController>().enabled,"carregar save ao volante restaura jogador fora da van com controles válidos");
  var safeVan=van.transform.position;van.GetComponent<Rigidbody>().position=safeVan+Vector3.down*20;yield return new WaitForSeconds(.2f);Check(van.transform.position.y>safeVan.y-1&&van.CargoOrderId==order.id,"queda fora do mundo recupera van sem perder encomenda");
  Place(player,van.CargoPosition);if(!van.CargoOpen)van.ToggleCargo(player);Check(van.TransferCargo(player)&&van.CargoOrderId==null,"retirar encomenda após carregar a partida");if(done)yield break;
  GridShiftDeliveryAccess.TryGet(manager.Point(order.destinationId),out var destination);Place(player,destination);yield return null;float before=manager.Money;Check(manager.PersonalInteract(player,order.destinationId,out _),"pedido preparado e transportado é entregue no destino real");if(done)yield break;
  float paid=manager.Money;manager.Complete(order);Check(paid>before&&manager.Money==paid&&order.rewardPaid,"pagamento permanece único após fluxo depósito e van");
  var mission=manager.Company.State.missions.First(m=>m.id=="depot-workflow");Check(manager.Company.Missions.RequirementsMet(mission),"missão operacional verifica preparação, condução e entrega concluídas");Check(manager.Company.Missions.Claim(mission.id,out _),"bônus da missão operacional é liberado após ações reais");float rewarded=manager.Money;Check(!manager.Company.Missions.Claim(mission.id,out _)&&manager.Money==rewarded,"missão operacional não paga bônus duas vezes");
  van.Health.fuel=50;Place(player,operations.StationPosition("refuel"));manager.TryChangeMoney(50,"test-service-capital","Capital","Capital isolado de teste");float balance=manager.Money;Check(operations.Execute(player,"refuel",null,out _)&&van.Health.fuel==100&&manager.Money<balance,"abastecimento utiliza despesa real e restaura combustível");
  yield return new WaitForSeconds(.6f);var tablet=player.GetComponent<PortableTablet>();Check(tablet.Open(),"tablet permanece funcional após condução e entrega");yield return new WaitForSeconds(1);var ops=tablet.Operations;ops.Show("Equipamentos");yield return new WaitForSeconds(.3f);Check(ops.GetComponentsInChildren<Button>().Any(b=>b.name=="Localizar depósito no mapa"),"equipamentos mostram van e acesso real ao depósito");ops.Show("Entregas");yield return new WaitForSeconds(.3f);ops.GetComponentsInChildren<Button>().First(b=>b.name=="Ver comprovante").onClick.Invoke();Check(ops.GetComponentsInChildren<TMPro.TMP_Text>().Any(t=>t.text.Contains("delivery:"+order.id)),"comprovante apresenta a transação real da entrega");GridShiftV11Capture.Save("v17-receipt",player.CameraTransform.GetComponent<Camera>());tablet.Close();yield return new WaitForSeconds(1);
  var observer=new GameObject("DepotObservation").AddComponent<Camera>();observer.CopyFrom(player.CameraTransform.GetComponent<Camera>());observer.enabled=false;observer.cullingMask&=~((1<<5)|(1<<7)|(1<<8)|(1<<10)|(1<<11));observer.transform.position=operations.Depot.position+new Vector3(-7,4,6);observer.transform.LookAt(operations.Depot.position+Vector3.up);foreach(var text in operations.Depot.GetComponentsInChildren<TMPro.TMP_Text>())text.transform.rotation=observer.transform.rotation;GridShiftV11Capture.Save("v17-depot",observer);Object.Destroy(observer.gameObject);
  Place(player,operations.StationPosition("accept"));Check(operations.Execute(player,"accept",null,out _),"aceitar nova operação após concluir a primeira");order=manager.PersonalOrder;string delegatedId=order.id;
  Place(player,operations.StationPosition("delegate"));Check(!operations.Execute(player,"delegate",order.id,out _),"delegação exige operador realmente contratado");Check(manager.Company.Employee("elisa",true,out _),"contratação existente habilita operador logístico");Check(operations.Execute(player,"delegate",order.id,out _),"operador assume preparação com custo financeiro real");
  persistence=manager.GetComponent<DeliveryPersistence>();Check(persistence.Save()&&DeliverySaveStore.Read(out saved)&&saved.orders.Single(o=>o.id==delegatedId).depotAutomationRemaining>0,"preparação delegada em andamento é salva");DeliverySaveStore.Pending=saved;yield return SceneManager.LoadSceneAsync("SampleScene",LoadSceneMode.Single);yield return Ready();manager=DeliveryManager.Instance;order=manager.PersonalOrder;
  Time.timeScale=4;until=Time.realtimeSinceStartup+10;while(!order.depotReleased&&Time.realtimeSinceStartup<until)yield return null;Time.timeScale=1;
  Check(order.depotReleased&&order.depotScanned&&order.depotPacked&&order.depotLabelled&&order.depotSorted,"operador contratado retoma e conclui preparação após carregar");Check(manager.Company.State.ledger.Count(t=>t.id=="depot-operator:"+delegatedId)==1,"serviço delegado não é cobrado duas vezes após carregar");
  Finish(true);
 }
}
