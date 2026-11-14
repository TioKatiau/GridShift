using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Physical entrances are distinct from the vehicle's road stopping position.
public static class GridShiftDeliveryAccess {
 static readonly RaycastHit[] sightHits=new RaycastHit[24];
 public static bool ClearSight(PlayerMovement player,Vector3 at,Transform target=null){
  var eye=player.CameraTransform!=null?player.CameraTransform.position:player.transform.position+Vector3.up*1.5f;
  var delta=at-eye;int count=Physics.RaycastNonAlloc(eye,delta.normalized,sightHits,delta.magnitude,~(1<<9),QueryTriggerInteraction.Ignore);
  if(count==sightHits.Length)return false;
  for(int i=0;i<count;i++){var t=sightHits[i].transform;if(t.IsChildOf(player.transform)||(target!=null&&t.IsChildOf(target)))continue;return false;}return true;
 }
 public static bool TryGet(DeliveryPoint point,out Vector3 at){
  at=point!=null?(point.pedestrianAccess!=null?point.pedestrianAccess.position:point.transform.position):Vector3.zero;
  if(point==null||!point.accessible||(point.pedestrianAccess==null&&point.accessNeedsReview))return false;
  if(!Physics.Raycast(at+Vector3.up*.35f,Vector3.down,out var floor,1.2f,~(1<<9),QueryTriggerInteraction.Ignore)||floor.normal.y<.85f)return false;
  at.y=floor.point.y+.025f;
  return true;
 }
 public static bool InReach(PlayerMovement player,Vector3 at,float range=2.5f,Transform target=null,Vector3? sightPoint=null){
  if(player==null||player.ControlsBlocked||Time.timeScale<=0||player.gameObject.scene!=SceneManager.GetActiveScene()||(WorldSession.Instance!=null&&WorldSession.Instance.Busy))return false;
  if(Vector3.Distance(player.transform.position,at)>range)return false;
  return ClearSight(player,sightPoint??at+Vector3.up*.65f,target);
 }
 public static GameObject Package(Transform parent){
  var prefab=GridShiftSceneTemplates.Find(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"parcel")??Resources.Load<GameObject>("DeliveryPackage");
  if(prefab!=null)return Object.Instantiate(prefab,parent,false);
  var box=GameObject.CreatePrimitive(PrimitiveType.Cube);box.name="DeliveryPackage";box.transform.SetParent(parent,false);box.transform.localScale=new Vector3(.34f,.25f,.28f);
  box.GetComponent<Collider>().enabled=false;
  var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
  var material=new Material(shader){color=new Color(.56f,.34f,.15f)};box.GetComponent<Renderer>().sharedMaterial=material;
  return box;
 }
}

public sealed partial class DeliveryManager {
 public DeliveryOrder PersonalOrder=>orders.FirstOrDefault(o=>o.mode==DeliveryMode.Personal&&o.status!=DeliveryStatus.Completed);
 public bool AcceptPersonal(string id,out string error){
  var old=orders.FirstOrDefault(o=>o.requestId=="offer:"+id);if(old!=null){error="Este contrato já foi aceito.";return false;}
  var offer=Offers().FirstOrDefault(o=>o.id==id);if(offer==null){error="Oferta indisponível.";return false;}
  bool ok=TryCreate(new DeliveryRequest{mode=DeliveryMode.Personal,requestId="offer:"+id,goods=offer.goods,weightKg=offer.weightKg,kind=offer.kind,pickupId=offer.pickupId,destinationId=offer.destinationId},out var order,out error);
  if(ok){Select(order.id);GetComponent<DeliveryPersistence>()?.RequestSave();}return ok;
 }
 public bool PersonalInteract(PlayerMovement player,string pointId,out string error){
  error=null;var order=PersonalOrder;
  if(order==null){error="Nenhuma entrega pessoal ativa.";return false;}
  string expected=order.status==DeliveryStatus.ToPickup?order.pickupId:order.destinationId;
  var operations=GetComponent<GridShiftActiveOperations>();
  if(operations?.Van!=null&&operations.Van.CargoOrderId==order.id){error="Estacione e retire a encomenda do compartimento da van.";return false;}
  if(order.status==DeliveryStatus.ToPickup&&order.depotRequired&&!WorldSession.Instance.InFactory){error="Entre na Factory para retirar a encomenda do setor indicado.";return false;}
  bool access=GridShiftDeliveryAccess.TryGet(Point(expected),out var at);if(order.depotRequired&&order.status==DeliveryStatus.ToPickup&&operations?.Depot!=null)at=operations.Origin(order);
  if(order.depotRequired&&order.status==DeliveryStatus.ToPickup)access=operations?.Depot!=null;
  if(pointId!=expected||!access||!GridShiftDeliveryAccess.InReach(player,at)){error="Aproxime-se do acesso correto, sem obstáculos.";return false;}
  var mission=Company.State.missions.FirstOrDefault(m=>m.id=="personal-express"&&m.orderId==order.id);
  if(order.status==DeliveryStatus.ToPickup&&order.depotRequired&&!order.depotReleased){if(order.depotPicked){error="Leve a caixa à mesa de conferência.";return false;}order.depotPicked=true;Changed();GetComponent<DeliveryPersistence>()?.RequestSave();GridShiftAudio.Play("Cargo_Collected","SFX",.4f);return true;}
  if(order.status==DeliveryStatus.ToPickup){if(order.depotRequired)order.depotPicked=true;order.status=DeliveryStatus.Transporting;if(mission!=null)mission.step=2;GridShiftAudio.Play("Cargo_Collected","SFX",.4f);Notify("Encomenda coletada · leve até "+Point(order.destinationId).displayName);}
  else if(order.status==DeliveryStatus.Transporting){int reputation=Company.State.reputation;Complete(order);if(mission!=null)mission.step=3;var depotMission=Company.State.missions.FirstOrDefault(m=>m.id=="depot-workflow"&&m.orderId==order.id&&!m.rewardClaimed);if(depotMission!=null)depotMission.step=9;int delta=Company.State.reputation-reputation;Notify("ENTREGA CONCLUÍDA · R$ "+order.reward.ToString("N2")+" · Reputação "+(delta>=0?"+":"")+delta);}
  else{error="Etapa de entrega inválida.";return false;}
  Changed();GetComponent<DeliveryPersistence>()?.RequestSave();return true;
 }
}

public sealed class GridShiftPersonalDelivery:MonoBehaviour {
 public bool CanExitFactory(out string reason){reason=null;var order=manager.PersonalOrder;if(order==null||!order.depotRequired||!order.depotPicked||order.status!=DeliveryStatus.ToPickup||order.depotReleased)return true;reason=!order.depotScanned?"Confira a caixa no leitor antes de sair.":!order.depotPacked?"Embale a caixa antes de sair.":!order.depotLabelled?"Imprima a etiqueta antes de sair.":!order.depotSorted?"Separe a caixa no setor correto antes de sair.":"Libere a expedição antes de sair com a caixa.";return false;}
 public void PrepareSceneTransition(){if(package!=null){package.transform.SetParent(transform,true);package.SetActive(false);}if(target!=null){target.transform.SetParent(transform,true);target.gameObject.SetActive(false);}measuring=false;heldVelocity=Vector3.zero;}
 public GridShiftPackageIdentity PackageIdentity=>package!=null?package.GetComponent<GridShiftPackageIdentity>():null;
 DeliveryManager manager;PlayerMovement player;GameObject package;GridShiftPackageInteraction target;string shownOrder;Vector3 heldVelocity,lastWalk;bool measuring,finishing;readonly RaycastHit[] walls=new RaycastHit[16];
 void Awake(){manager=GetComponent<DeliveryManager>();}
 void Update(){
  if(manager==null||manager.Restoring||finishing||WorldSession.Instance?.Busy==true)return;var order=manager.PersonalOrder;
  player=InteractionFocus.PlayerFor(this);
  if(order==null){Clear();return;}
  if(shownOrder!=order.id){Clear();shownOrder=order.id;}
  var point=manager.Point(order.status==DeliveryStatus.ToPickup?order.pickupId:order.destinationId);
  if(!GridShiftDeliveryAccess.TryGet(point,out var at))return;
  var operations=manager.GetComponent<GridShiftActiveOperations>();if(order.depotRequired&&order.status==DeliveryStatus.ToPickup&&operations?.Depot!=null)at=operations.Origin(order);
  if(order.depotRequired&&order.status==DeliveryStatus.ToPickup&&!order.depotPicked&&!WorldSession.Instance.InFactory){if(package!=null)package.SetActive(false);if(target!=null)target.gameObject.SetActive(false);return;}
  if(target==null){var go=new GameObject("PersonalDeliveryInteraction");go.transform.SetParent(transform,false);target=go.AddComponent<GridShiftPackageInteraction>();target.owner=this;}
  target.transform.SetParent(null,true);if(target.gameObject.scene!=SceneManager.GetActiveScene())SceneManager.MoveGameObjectToScene(target.gameObject,SceneManager.GetActiveScene());target.gameObject.SetActive(!order.depotPicked||order.status==DeliveryStatus.Transporting);
  target.transform.position=at;target.pointId=point.id;target.caption=order.status==DeliveryStatus.ToPickup?"Pegar encomenda":"Entregar encomenda";
  if(package==null){package=GridShiftDeliveryAccess.Package(transform);package.name="Encomenda "+order.id;var identity=package.GetComponent<GridShiftPackageIdentity>();if(identity==null)identity=package.AddComponent<GridShiftPackageIdentity>();identity.orderId=order.id;identity.packageId="PKG-"+order.id;}
  PackageIdentity.ShowLabel(order.depotRequired&&!order.depotLabelled?"Conferência pendente":manager.Point(order.destinationId).displayName);
  bool city=SceneManager.GetActiveScene().name=="SampleScene"||SceneManager.GetActiveScene().name=="FactoryInterior";
  bool carrying=order.status==DeliveryStatus.Transporting||order.depotPicked;
  if(carrying&&operations?.Van!=null&&operations.Van.CargoOrderId==order.id){RestInCargo(operations.Van);package.SetActive(city&&SceneManager.GetActiveScene().name=="SampleScene");if(player!=null)player.CarrySpeedMultiplier=1;measuring=false;return;}
  if(carrying&&city&&player!=null){if(measuring){float moved=Vector3.ProjectOnPlane(player.transform.position-lastWalk,Vector3.up).magnitude;if(moved<3)order.travelledMeters+=moved;}lastWalk=player.transform.position;measuring=true;}else measuring=false;
  if(player!=null)player.CarrySpeedMultiplier=carrying?Mathf.Lerp(1,.85f,order.weightKg/manager.lightThresholdKg):1;
  if(carrying&&player!=null&&player.CameraTransform!=null){
   var camera=player.CameraTransform;if(package.transform.parent!=camera){package.transform.SetParent(camera,false);package.transform.localPosition=new Vector3(.08f,-.5f,.55f);heldVelocity=Vector3.zero;}
   float distance=.65f;int hits=Physics.SphereCastNonAlloc(camera.position,.2f,camera.forward,walls,distance,~(1<<9),QueryTriggerInteraction.Ignore);for(int i=0;i<hits;i++)if(!walls[i].transform.IsChildOf(player.transform))distance=Mathf.Min(distance,Mathf.Max(.15f,walls[i].distance-.1f));
   float bob=player.MovementInput.sqrMagnitude>.01f?Mathf.Sin(Time.time*(player.IsRunning?13:8))*.012f:0;
   var position=new Vector3(.08f,-.25f+bob,distance);package.transform.localPosition=Vector3.SmoothDamp(package.transform.localPosition,position,ref heldVelocity,.14f,Mathf.Infinity,Time.deltaTime);
   package.transform.localRotation=Quaternion.Slerp(package.transform.localRotation,Quaternion.Euler(-6,0,0),Time.deltaTime*10);
   package.SetActive(city&&!player.ControlsBlocked&&Time.timeScale>0&&distance>.22f);
  }else{package.transform.SetParent(transform,true);package.transform.position=at+Vector3.up*.3f;package.SetActive(city);}
 }
 public bool MoveCargo(GridShiftPlayerVan van,PlayerMovement candidate,bool storing){if(finishing||package==null||PackageIdentity==null||PackageIdentity.orderId!=manager.PersonalOrder?.id)return false;StartCoroutine(MoveCargoRoutine(van,candidate,storing));return true;}
 void RestInCargo(GridShiftPlayerVan van){if(package.transform.parent!=van.CargoAnchor){package.transform.SetParent(van.CargoAnchor,true);package.transform.localRotation=Quaternion.identity;package.transform.localPosition=Vector3.zero;var bounds=new Bounds();bool found=false;foreach(var r in package.GetComponentsInChildren<MeshRenderer>()){if(r.GetComponent<TMP_Text>()!=null)continue;var b=r.localBounds;for(int i=0;i<8;i++){var corner=b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));var point=van.CargoAnchor.InverseTransformPoint(r.transform.TransformPoint(corner));if(!found){bounds=new Bounds(point,Vector3.zero);found=true;}else bounds.Encapsulate(point);}}if(found)package.transform.localPosition=new Vector3(-bounds.center.x,-bounds.min.y,-bounds.center.z);}}
 System.Collections.IEnumerator MoveCargoRoutine(GridShiftPlayerVan van,PlayerMovement candidate,bool storing){finishing=true;string id=PackageIdentity.orderId;var from=package.transform.position;var rotation=package.transform.rotation;var target=storing?van.CargoAnchor:candidate.CameraTransform;Vector3 at;if(storing){RestInCargo(van);at=package.transform.position;package.transform.SetParent(transform,true);package.transform.position=from;}else{at=target.TransformPoint(new Vector3(.08f,-.25f,.55f));package.transform.SetParent(transform,true);}var endRotation=storing?van.transform.rotation:target.rotation*Quaternion.Euler(-6,0,0);package.SetActive(true);for(float t=0;t<.28f;t+=Time.deltaTime){if(package==null||van==null){finishing=false;yield break;}float k=Mathf.SmoothStep(0,1,t/.28f);package.transform.position=Vector3.Lerp(from,at,k);package.transform.rotation=Quaternion.Slerp(rotation,endRotation,k);yield return null;}if(storing)RestInCargo(van);else{package.transform.SetParent(target,true);package.transform.localPosition=new Vector3(.08f,-.25f,.55f);heldVelocity=Vector3.zero;}van.FinishCargoTransfer(id,storing);finishing=false;}
 public bool Interact(string pointId){var order=manager.PersonalOrder;bool ok=manager.PersonalInteract(player,pointId,out var error);if(!ok)manager.Notify(error);else if(order!=null&&order.status==DeliveryStatus.Completed&&package!=null&&GridShiftDeliveryAccess.TryGet(manager.Point(pointId),out var at))StartCoroutine(PlacePackage(at));return ok;}
 System.Collections.IEnumerator PlacePackage(Vector3 at){finishing=true;package.transform.SetParent(transform,true);var from=package.transform.position;var rotation=package.transform.rotation;for(float t=0;t<.24f;t+=Time.deltaTime){if(package==null)yield break;float k=Mathf.SmoothStep(0,1,t/.24f);package.transform.position=Vector3.Lerp(from,at+Vector3.up*.15f,k);package.transform.rotation=Quaternion.Slerp(rotation,Quaternion.identity,k);yield return null;}Clear();}
 void Clear(){finishing=false;measuring=false;if(player!=null)player.CarrySpeedMultiplier=1;if(package!=null)Destroy(package);if(target!=null)Destroy(target.gameObject);package=null;target=null;shownOrder=null;}
 void OnDestroy(){Clear();}
}

public sealed class GridShiftPackageInteraction:MonoBehaviour {
 public GridShiftPersonalDelivery owner;public string pointId,caption;TMP_Text label;PlayerMovement player;BoxCollider focus;
 void OnEnable(){InteractionFocus.Register(this,2.5f);}void OnDisable(){InteractionFocus.Remove(this);}
 void Start(){focus=gameObject.AddComponent<BoxCollider>();focus.isTrigger=true;focus.center=Vector3.up*.3f;focus.size=new Vector3(.65f,.6f,.65f);}
 void Update(){if(focus!=null){bool delivery=caption=="Entregar encomenda";focus.center=Vector3.up*(delivery?1.1f:.3f);focus.size=delivery?new Vector3(.9f,1.6f,.6f):new Vector3(.65f,.6f,.65f);}player=InteractionFocus.PlayerFor(this);
  bool near=GridShiftDeliveryAccess.InReach(player,transform.position)&&InteractionFocus.Selected(this,player.transform);
  if(near)GridShiftContextHUD.Offer(caption);
  if(near&&GridShiftControls.Down(GSAction.Interact)&&InteractionFocus.Consume()){owner.Interact(pointId);GridShiftControls.SuppressFrame();}
 }
}
