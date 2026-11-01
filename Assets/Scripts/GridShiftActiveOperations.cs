using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

// World presentation and commands share DeliveryManager orders and company finance.
public sealed class GridShiftActiveOperations:MonoBehaviour {
 public GridShiftPlayerVan Van{get;private set;}public GridShiftActiveConfig Config{get;private set;}public Transform Depot{get;private set;}
 DeliveryManager manager;GridShiftVanState pending;bool restored;TMP_Text board;float refresh;
 public void Restore(GridShiftVanState state){pending=state;restored=true;}
 IEnumerator Start(){manager=GetComponent<DeliveryManager>();while(!restored||manager.Restoring||!manager.Company.Ready)yield return null;Config=Resources.Load<GridShiftActiveConfig>("GridShiftActiveConfig");if(Config==null||Config.vanPrefab==null)yield break;
  Depot=new GameObject("GridShiftDepot16").transform;Depot.SetParent(transform,false);Depot.SetPositionAndRotation(Config.depotPosition,Config.depotRotation);BuildDepot();
  var state=pending;Vector3 position=state!=null&&state.placed?state.position:Config.vanPosition;Quaternion rotation=state!=null&&state.placed?state.rotation:Config.vanRotation;
  var go=Instantiate(Config.vanPrefab,position,rotation,transform);go.name="GridShift Player Van";Van=go.GetComponent<GridShiftPlayerVan>();Van.Initialize(manager,state);
 }
 public Vector3 Origin(DeliveryOrder order){return Depot!=null&&order.depotRequired?Depot.TransformPoint(order.depotSorted?new Vector3(-2.8f,.5f,order.depotZone==0?1.4f:2.8f):new Vector3(-1,0,0)):manager.Point(order.pickupId).transform.position;}
 GameObject Block(string name,Vector3 at,Vector3 size,Material material,bool collision=true){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(Depot,false);go.transform.localPosition=at;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;go.GetComponent<Collider>().enabled=collision;return go;}
 void Sign(string name,string text,Vector3 local,int size=3){var go=new GameObject(name,typeof(TextMeshPro));go.transform.SetParent(Depot,false);go.transform.localPosition=local;var label=go.GetComponent<TextMeshPro>();label.text=text;label.fontSize=size;label.alignment=TextAlignmentOptions.Center;label.rectTransform.sizeDelta=new Vector2(5,2);label.color=new Color(1,.83f,.2f);go.AddComponent<InteractionPrompt>();if(name=="DepotBoard")board=label;}
 void BuildDepot(){
  Block("Receiving pad",new Vector3(-1,0,0),new Vector3(5,.08f,7),Config.floorMaterial);
  Block("Packing bench",new Vector3(-1,.42f,-1.5f),new Vector3(2.5f,.8f,.7f),Config.structureMaterial);
  Block("Scanner base",new Vector3(-1,.84f,-1.5f),new Vector3(.35f,.08f,.3f),Config.accentMaterial,false);Block("Label printer",new Vector3(0,.95f,-1.5f),new Vector3(.3f,.25f,.3f),Config.accentMaterial,false);
  foreach(float x in new[]{1.2f,4.3f})Block("Van bay marking",new Vector3(x,.015f,-.3f),new Vector3(.07f,.025f,5.8f),Config.accentMaterial,false);
  foreach(float z in new[]{1.4f,2.8f}){Block("Shelf",new Vector3(-2.8f,.35f,z),new Vector3(.6f,.7f,1),Config.structureMaterial);Block("Shelf top",new Vector3(-2.8f,.72f,z),new Vector3(.7f,.08f,1.1f),Config.accentMaterial);}
  Sign("DepotBoard","GRIDSHIFT · EXPEDIÇÃO",new Vector3(-1,2,-2.6f));Sign("Shelf labels","SETOR A                     SETOR B",new Vector3(-2.5f,1.4f,2.1f),2);
  Station("Consultar e aceitar pedido do depósito",new Vector3(-1,0,-2.6f),"accept");
  Station("Conferir e escanear etiqueta",new Vector3(-1,0,-.4f),"scan");
  Station("Embalar caixa pequena · até 5 kg",new Vector3(-2,0,-1),"pack-small");Station("Embalar caixa média · até 20 kg",new Vector3(0,0,-1),"pack-medium");
  Station("Imprimir etiqueta de destino",new Vector3(.4f,0,.2f),"label");Station("Separar no setor A",new Vector3(-2,0,1.4f),"sort-a");Station("Separar no setor B",new Vector3(-2,0,2.8f),"sort-b");
  Station("Liberar para expedição",new Vector3(.5f,0,1.6f),"release");Station("Delegar preparação ao operador contratado",new Vector3(.5f,0,2.6f),"delegate");
  Station("Inspecionar van",new Vector3(2.5f,0,-2.8f),"inspect");Station("Abastecer van · custo proporcional",new Vector3(3.8f,0,-2.8f),"refuel");Station("Autorizar manutenção da van",new Vector3(3.8f,0,2.8f),"repair");
 }
 void Station(string title,Vector3 at,string command){GridShiftWorldAction.Create(Depot,title,at,p=>Execute(p,command,command=="scan"?manager.GetComponent<GridShiftPersonalDelivery>().PackageIdentity?.packageId:manager.PersonalOrder?.id,out _));}
 public bool Execute(PlayerMovement player,string command,string packageId,out string error){
  error=null;Vector3 station=StationPosition(command);if(Depot==null||!GridShiftDeliveryAccess.InReach(player,station,3))return Reject("Aproxime-se da estação correta e mantenha acesso livre.",out error);
  if(command=="accept")return CreateOrder(out error);
  if(command=="inspect"){if(Van==null)return Reject("Van indisponível.",out error);manager.Notify("VAN · Combustível "+Van.Health.fuel.ToString("0.0")+"% · Condição "+Van.Health.condition.ToString("0.0")+"% · Capacidade "+Van.capacityKg+" kg");return true;}
  if(command=="refuel"||command=="repair")return manager.Company.Service(GridShiftPlayerVan.VehicleId,command=="repair",out error);
  var order=manager.PersonalOrder;if(order==null||!order.depotRequired||order.status!=DeliveryStatus.ToPickup||(command=="scan"?"PKG-"+order.id:order.id)!=packageId)return Reject("A encomenda não corresponde ao pedido em preparação.",out error);
  switch(command){
   case "scan":var parcel=manager.GetComponent<GridShiftPersonalDelivery>().PackageIdentity;if(parcel==null||parcel.packageId!=packageId||Vector3.Distance(parcel.transform.position,station)>2||!GridShiftDeliveryAccess.ClearSight(player,parcel.transform.position))return Reject("Posicione a encomenda correta ao alcance do leitor.",out error);order.depotScanned=true;break;
   case "pack-small":case "pack-medium":if(!order.depotScanned)return Reject("Confira o código da encomenda antes de embalar.",out error);if(order.weightKg>(command=="pack-small"?5:20))return Reject("Caixa incompatível com o peso. Utilize a caixa média.",out error);order.depotPacked=true;break;
   case "label":if(!order.depotPacked)return Reject("Embale a encomenda antes de imprimir a etiqueta.",out error);order.depotLabelled=true;manager.Notify("Etiqueta "+order.id.Substring(0,8)+" · "+manager.Point(order.destinationId).displayName+" · "+order.weightKg+" kg");break;
   case "sort-a":case "sort-b":if(!order.depotLabelled)return Reject("A etiqueta precisa estar pronta antes da separação.",out error);int zone=command=="sort-a"?0:1;if(zone!=RequiredZone(order))return Reject("Destino pertence ao setor "+(RequiredZone(order)==0?"A":"B")+".",out error);order.depotZone=zone;order.depotSorted=true;break;
   case "release":if(!order.depotScanned||!order.depotPacked||!order.depotLabelled||!order.depotSorted)return Reject("Conclua conferência, embalagem, etiqueta e separação.",out error);order.depotReleased=true;break;
   case "delegate":if(!manager.Company.State.employees.Any(e=>e.hired&&e.role=="Operador logístico"))return Reject("Contrate um operador logístico para delegar a preparação.",out error);if(order.depotAutomationRemaining>0||order.depotReleased)return Reject("Pedido já delegado ou liberado.",out error);if(!manager.TryChangeMoney(-3,"depot-operator:"+order.id,"Serviços","Preparação delegada da encomenda"))return Reject("Saldo insuficiente para o serviço de preparação.",out error);order.depotAutomationRemaining=8;break;
   default:return Reject("Estação desconhecida.",out error);
  }
  var mission=manager.Company.State.missions.FirstOrDefault(m=>m.id=="depot-workflow"&&m.orderId==order.id&&!m.rewardClaimed);if(mission!=null&&command!="delegate")mission.step=Mathf.Max(mission.step,command=="scan"?2:command.StartsWith("pack")?3:command=="label"?4:command.StartsWith("sort")?5:6);
  GridShiftAudio.Play("UI_Confirm","SFX",.3f);manager.Changed();manager.GetComponent<DeliveryPersistence>()?.RequestSave();return true;
 }
 public int RequiredZone(DeliveryOrder order)=>manager.Point(order.destinationId).transform.position.x>=Config.depotPosition.x?1:0;
 public Vector3 StationPosition(string command){Vector3 p=command=="accept"?new Vector3(-1,0,-2.6f):command=="scan"?new Vector3(-1,0,-.4f):command=="pack-small"?new Vector3(-2,0,-1):command=="pack-medium"?new Vector3(0,0,-1):command=="label"?new Vector3(.4f,0,.2f):command=="sort-a"?new Vector3(-2,0,1.4f):command=="sort-b"?new Vector3(-2,0,2.8f):command=="release"?new Vector3(.5f,0,1.6f):command=="delegate"?new Vector3(.5f,0,2.6f):command=="inspect"?new Vector3(2.5f,0,-2.8f):command=="refuel"?new Vector3(3.8f,0,-2.8f):new Vector3(3.8f,0,2.8f);return Depot!=null?Depot.TransformPoint(p):Vector3.zero;}
 bool CreateOrder(out string error){
  error=null;if(manager.PersonalOrder!=null)return Reject("Conclua a encomenda pessoal atual antes de aceitar outra.",out error);
  var source=manager.Point(Config.depotPointId);var destination=manager.points.Where(p=>p!=null&&p!=source&&!p.personalOnly&&p.Valid&&manager.Progression.Unlocked(p)&&GridShiftDeliveryAccess.TryGet(p,out _)).OrderBy(p=>(p.transform.position-Depot.position).sqrMagnitude).FirstOrDefault(p=>DeliveryRouting.Find(source.route,p.route,source.route==p.route)!=null);
  if(destination==null)return Reject("Nenhum destino acessível disponível.",out error);
  if(!manager.TryCreate(new DeliveryRequest{mode=DeliveryMode.Personal,requestId="depot:"+System.Guid.NewGuid().ToString("N"),goods="Encomenda conferida do depósito",weightKg=8,pickupId=source.id,destinationId=destination.id},out var order,out error))return false;
  order.depotRequired=true;var mission=manager.Company.State.missions.FirstOrDefault(m=>m.id=="depot-workflow"&&!m.rewardClaimed);if(mission!=null&&string.IsNullOrEmpty(mission.orderId)){mission.orderId=order.id;mission.step=1;manager.Company.State.selectedMission=mission.id;}manager.Select(order.id);manager.Changed();manager.GetComponent<DeliveryPersistence>()?.RequestSave();return true;
 }
 bool Reject(string text,out string error){error=text;manager.Notify(text);GridShiftAudio.Play("UI_Error","UI",.25f);return false;}
 void Update(){if(manager==null||manager.Restoring||Depot==null)return;var order=manager.PersonalOrder;
  if(order!=null&&order.depotRequired&&order.status==DeliveryStatus.ToPickup&&order.depotAutomationRemaining>0&&manager.Company.State.employees.Any(e=>e.hired&&e.role=="Operador logístico")){order.depotAutomationRemaining=Mathf.Max(0,order.depotAutomationRemaining-Time.deltaTime);if(order.depotAutomationRemaining==0){order.depotScanned=order.depotPacked=order.depotLabelled=order.depotSorted=order.depotReleased=true;order.depotZone=RequiredZone(order);var mission=manager.Company.State.missions.FirstOrDefault(m=>m.id=="depot-workflow"&&m.orderId==order.id&&!m.rewardClaimed);if(mission!=null)mission.step=6;manager.Notify("Operador preparou a encomenda para expedição.");manager.Changed();manager.GetComponent<DeliveryPersistence>()?.RequestSave();}}
  if(Time.unscaledTime<refresh)return;refresh=Time.unscaledTime+.5f;if(board!=null)board.text=order!=null&&order.depotRequired?"EXPEDIÇÃO · #"+order.id.Substring(0,8)+"\n"+manager.Point(order.destinationId)?.displayName+"\n"+(order.depotReleased?"LIBERADA PARA COLETA":"Conferir → embalar → etiquetar → separar")+"\nSetor "+(RequiredZone(order)==0?"A":"B"):"GRIDSHIFT · DEPÓSITO\nConsulte o quadro para preparar uma encomenda";
 }
 public GridShiftVanState Capture()=>Van!=null?Van.Capture():pending;
 public bool VanService(bool repair,out string error){
  error=null;if(Van==null||Van.Driving||Van.Speed>.3f)return Reject("Estacione e saia da van antes do serviço.",out error);var h=Van.Health;if(h.maintenanceUntil>manager.Company.Day)return Reject("Manutenção em andamento.",out error);
  float cost=repair?(100-h.condition)*manager.Company.Balance.repairPerPoint*(manager.Company.Mechanic?.75f:1):(100-h.fuel)/100*manager.Company.Balance.carRefuel;if(cost<.01f)return Reject("Serviço desnecessário.",out error);
  if(!manager.TryChangeMoney(-cost,"van-service:"+System.Guid.NewGuid().ToString("N"),repair?"Manutenção":"Combustível",GridShiftPlayerVan.VehicleId))return Reject("Saldo insuficiente.",out error);
  if(repair)h.maintenanceUntil=manager.Company.Day+manager.Company.Balance.maintenanceGameHours/24;else h.fuel=100;manager.Changed();manager.GetComponent<DeliveryPersistence>()?.RequestSave();return true;
 }
}
