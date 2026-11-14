using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

// World presentation and commands share DeliveryManager orders and company finance.
public sealed class GridShiftActiveOperations:MonoBehaviour {
 public GridShiftPlayerVan Van{get;private set;}public GridShiftActiveConfig Config{get;private set;}public Transform Depot{get;private set;}
 [SerializeField]GridShiftPlayerVan sceneVan;
 public void AuthorScene(){Config=Resources.Load<GridShiftActiveConfig>("GridShiftActiveConfig");GridShiftFactoryEntrance.Install(this,Config);BuildParking();if(sceneVan==null){var go=Instantiate(Config.vanPrefab,Config.vanPosition,Config.vanRotation,transform);go.name="GridShift Player Van";sceneVan=go.GetComponent<GridShiftPlayerVan>();sceneVan.AuthorScene();}}
 void BuildParking(){if(transform.Find("Factory rear parking bay")!=null)return;var bay=new GameObject("Factory rear parking bay");bay.transform.SetParent(transform,false);bay.transform.SetPositionAndRotation(Config.vanPosition-Vector3.up*.07f,Config.vanRotation);foreach(float side in new[]{-1.65f,1.65f}){var line=GameObject.CreatePrimitive(PrimitiveType.Cube);line.name="Rear bay marking";line.transform.SetParent(bay.transform,false);line.transform.localPosition=new Vector3(side,0,0);line.transform.localScale=new Vector3(.06f,.015f,6);line.GetComponent<Collider>().enabled=false;line.GetComponent<Renderer>().sharedMaterial=Config.accentMaterial;}var pad=GameObject.CreatePrimitive(PrimitiveType.Cube);pad.name="Rear parking paving";pad.transform.SetParent(bay.transform,false);pad.transform.localPosition=Vector3.down*.015f;pad.transform.localScale=new Vector3(4,.02f,7);pad.GetComponent<Collider>().enabled=false;pad.GetComponent<Renderer>().sharedMaterial=Config.floorMaterial;}
 DeliveryManager manager;GridShiftVanState pending;bool restored;TMP_Text board;float refresh;
 public void Restore(GridShiftVanState state){pending=state;restored=true;}
 IEnumerator Start(){manager=GetComponent<DeliveryManager>();while(!restored||manager.Restoring||!manager.Company.Ready)yield return null;Config=Resources.Load<GridShiftActiveConfig>("GridShiftActiveConfig");if(Config==null||Config.vanPrefab==null)yield break;
  GridShiftFactoryEntrance.Install(this);
  var state=pending;if(state!=null&&state.placed&&Vector3.Distance(state.position,new Vector3(322.97372f,-4.5f,187.56693f))<.6f){state.position=Config.vanPosition;state.rotation=Config.vanRotation;state.safePosition=state.position;state.safeRotation=state.rotation;}BuildParking();Vector3 position=state!=null&&state.placed?state.position:Config.vanPosition;Quaternion rotation=state!=null&&state.placed?state.rotation:Config.vanRotation;
  var go=sceneVan!=null?sceneVan.gameObject:Instantiate(Config.vanPrefab,position,rotation,transform);if(sceneVan==null||state!=null&&state.placed)go.transform.SetPositionAndRotation(position,rotation);go.name="GridShift Player Van";Van=go.GetComponent<GridShiftPlayerVan>();Van.Initialize(manager,state);
 }
 public void BindInterior(Transform interior){Depot=interior;}
 public Vector3 Origin(DeliveryOrder order){if(order.depotRequired&&Depot!=null)return Depot.TransformPoint(new Vector3(-8.5f,.6f,order.weightKg<=5?4:8));return order.depotRequired?Config.depotPosition:manager.Point(order.pickupId).transform.position;}
 public bool Execute(PlayerMovement player,string command,string packageId,out string error){
  error=null;Vector3 station=StationPosition(command);if(!WorldSession.Instance.InFactory||Depot==null||(!GridShiftDeliveryAccess.InReach(player,station,3)&&!(command=="accept"&&GridShiftDeliveryAccess.InReach(player,GridShiftFactoryInterior.Instance.attendantPoint.position,3))))return Reject("Aproxime-se da estação correta e mantenha acesso livre.",out error);
  if(command=="accept")return CreateOrder(out error);
  if(command=="inspect"){if(Van==null)return Reject("Van indisponível.",out error);manager.Notify("VAN · Combustível "+Van.Health.fuel.ToString("0.0")+"% · Condição "+Van.Health.condition.ToString("0.0")+"% · Capacidade "+Van.capacityKg+" kg");return true;}
  if(command=="refuel"||command=="repair")return manager.Company.Service(GridShiftPlayerVan.VehicleId,command=="repair",out error);
  var order=manager.PersonalOrder;if(order==null||!order.depotRequired||order.status!=DeliveryStatus.ToPickup||(command=="scan"?"PKG-"+order.id:order.id)!=packageId)return Reject("A encomenda não corresponde ao pedido em preparação.",out error);
  switch(command){
   case "scan":var parcel=manager.GetComponent<GridShiftPersonalDelivery>().PackageIdentity;if(parcel==null||parcel.packageId!=packageId||Vector3.Distance(parcel.transform.position,station)>2||!GridShiftDeliveryAccess.ClearSight(player,parcel.transform.position))return Reject("Posicione a encomenda correta ao alcance do leitor.",out error);order.depotScanned=true;break;
   case "pack-small":case "pack-medium":if(!order.depotScanned)return Reject("Confira o código da encomenda antes de embalar.",out error);if(order.weightKg>(command=="pack-small"?5:20))return Reject("Caixa incompatível com o peso. Utilize a caixa média.",out error);order.depotPacked=true;if(order.depotTutorial){order.depotLabelled=order.depotSorted=order.depotReleased=true;order.depotZone=0;order.status=DeliveryStatus.Transporting;}break;
   case "label":if(!order.depotPacked)return Reject("Embale a encomenda antes de imprimir a etiqueta.",out error);order.depotLabelled=true;manager.Notify("Etiqueta "+order.id.Substring(0,8)+" · "+manager.Point(order.destinationId).displayName+" · "+order.weightKg+" kg");break;
   case "sort-a":case "sort-b":if(!order.depotLabelled)return Reject("A etiqueta precisa estar pronta antes da separação.",out error);int zone=command=="sort-a"?0:1;if(zone!=RequiredZone(order))return Reject("Destino pertence ao setor "+(RequiredZone(order)==0?"A":"B")+".",out error);order.depotZone=zone;order.depotSorted=true;break;
   case "release":if(!order.depotScanned||!order.depotPacked||!order.depotLabelled||!order.depotSorted)return Reject("Conclua conferência, embalagem, etiqueta e separação.",out error);order.depotReleased=true;if(order.depotPicked)order.status=DeliveryStatus.Transporting;break;
   case "delegate":if(!manager.Company.State.employees.Any(e=>e.hired&&e.role=="Operador logístico"))return Reject("Contrate um operador logístico para delegar a preparação.",out error);if(order.depotAutomationRemaining>0||order.depotReleased)return Reject("Pedido já delegado ou liberado.",out error);if(!manager.TryChangeMoney(-3,"depot-operator:"+order.id,"Serviços","Preparação delegada da encomenda"))return Reject("Saldo insuficiente para o serviço de preparação.",out error);order.depotAutomationRemaining=8;break;
   default:return Reject("Estação desconhecida.",out error);
  }
  var mission=manager.Company.State.missions.FirstOrDefault(m=>m.id=="depot-workflow"&&m.orderId==order.id&&!m.rewardClaimed);if(mission!=null&&command!="delegate")mission.step=Mathf.Max(mission.step,command=="scan"?2:command.StartsWith("pack")?3:command=="label"?4:command.StartsWith("sort")?5:6);
  GridShiftAudio.Play("UI_Confirm","SFX",.3f);manager.Changed();manager.GetComponent<DeliveryPersistence>()?.RequestSave();return true;
 }
 public int RequiredZone(DeliveryOrder order)=>order.weightKg<=5?0:1;
 public Vector3 StationPosition(string command){var station=GridShiftFactoryInterior.Instance?.Station(command);return station!=null?station.position:Config!=null?Config.depotPosition:Vector3.zero;}
 bool CreateOrder(out string error){
  error=null;if(manager.PersonalOrder!=null)return Reject("Conclua a encomenda pessoal atual antes de aceitar outra.",out error);
  var source=manager.Point(Config.depotPointId);var destination=manager.points.Where(p=>p!=null&&p!=source&&p.id!=Config.factoryPointId&&(p.transform.position-Config.depotPosition).sqrMagnitude>100&&!p.personalOnly&&p.Valid&&manager.Progression.Unlocked(p)&&GridShiftDeliveryAccess.TryGet(p,out _)).OrderBy(p=>(p.transform.position-Config.depotPosition).sqrMagnitude).FirstOrDefault(p=>DeliveryRouting.Find(source.route,p.route,source.route==p.route)!=null);
  if(destination==null)return Reject("Nenhum destino acessível disponível.",out error);
  if(!manager.TryCreate(new DeliveryRequest{mode=DeliveryMode.Personal,requestId="depot:"+System.Guid.NewGuid().ToString("N"),goods="Encomenda conferida do depósito",weightKg=GridShiftTutorial.Active&&GridShiftTutorial.Step<=6?3:8,pickupId=source.id,destinationId=destination.id},out var order,out error))return false;
  order.depotRequired=true;order.depotTutorial=GridShiftTutorial.Active&&GridShiftTutorial.Step<=6;if(order.depotTutorial)GridShiftTutorial.BindOrder(order.id);var first=manager.Company.State.missions.FirstOrDefault(m=>m.id=="factory-first");if(first!=null&&!first.rewardClaimed&&string.IsNullOrEmpty(first.orderId)){first.orderId=order.id;first.step=1;manager.Company.State.selectedMission=first.id;}var mission=manager.Company.State.missions.FirstOrDefault(m=>m.id=="depot-workflow"&&!m.rewardClaimed);if(mission!=null&&string.IsNullOrEmpty(mission.orderId)){mission.orderId=order.id;mission.step=1;if(!order.depotTutorial)manager.Company.State.selectedMission=mission.id;}manager.Select(order.id);manager.Changed();manager.GetComponent<DeliveryPersistence>()?.RequestSave();return true;
 }
 bool Reject(string text,out string error){error=text;manager.Notify(text);GridShiftAudio.Play("UI_Error","UI",.25f);return false;}
 void Update(){if(manager==null||manager.Restoring)return;var order=manager.PersonalOrder;
  if(order!=null&&order.depotRequired&&order.status==DeliveryStatus.ToPickup&&order.depotAutomationRemaining>0&&manager.Company.State.employees.Any(e=>e.hired&&e.role=="Operador logístico")){order.depotAutomationRemaining=Mathf.Max(0,order.depotAutomationRemaining-Time.deltaTime);if(order.depotAutomationRemaining==0){order.depotScanned=order.depotPacked=order.depotLabelled=order.depotSorted=order.depotReleased=true;order.depotZone=RequiredZone(order);if(order.depotPicked)order.status=DeliveryStatus.Transporting;var mission=manager.Company.State.missions.FirstOrDefault(m=>m.id=="depot-workflow"&&m.orderId==order.id&&!m.rewardClaimed);if(mission!=null)mission.step=6;manager.Notify("Operador preparou a encomenda para expedição.");manager.Changed();manager.GetComponent<DeliveryPersistence>()?.RequestSave();}}

 }
 public GridShiftVanState Capture()=>Van!=null?Van.Capture():pending;
 public bool VanService(bool repair,out string error){
  error=null;if(Van==null||Van.Driving||Van.Speed>.3f)return Reject("Estacione e saia da van antes do serviço.",out error);var h=Van.Health;if(h.maintenanceUntil>manager.Company.Day)return Reject("Manutenção em andamento.",out error);
  float cost=repair?(100-h.condition)*manager.Company.Balance.repairPerPoint*(manager.Company.Mechanic?.75f:1):(100-h.fuel)/100*manager.Company.Balance.carRefuel;if(cost<.01f)return Reject("Serviço desnecessário.",out error);
  if(!manager.TryChangeMoney(-cost,"van-service:"+System.Guid.NewGuid().ToString("N"),repair?"Manutenção":"Combustível",GridShiftPlayerVan.VehicleId))return Reject("Saldo insuficiente.",out error);
  if(repair)h.maintenanceUntil=manager.Company.Day+manager.Company.Balance.maintenanceGameHours/24;else h.fuel=100;manager.Changed();manager.GetComponent<DeliveryPersistence>()?.RequestSave();return true;
 }
}
