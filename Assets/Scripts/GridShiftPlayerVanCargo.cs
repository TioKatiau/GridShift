using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// One rear target owns the contextual action; the existing order remains the inventory authority.
public sealed partial class GridShiftPlayerVan {
 [Header("Compartimento traseiro")][Range(80,110)]public float cargoOpeningAngle=95;
 [Min(.25f)]public float cargoDoorDuration=.65f;
 [field:SerializeField]public Transform CargoAnchor{get;private set;}public Transform CargoHinge=>hatch;
 public bool CargoAnimating=>Mathf.Abs(doorProgress-(CargoOpen?1:0))>.001f;
 public bool TransferBusy{get;private set;}public float UsedCapacityKg=>Manager?.orders.FirstOrDefault(o=>o.id==CargoOrderId)?.weightKg??0;
 public GridShiftWorldAction CargoAction{get;private set;}public GridShiftWorldAction CargoCloseAction{get;private set;}
 float doorProgress;Vector3 hingeAxis=Vector3.right;
 public bool HasHeldCargo(PlayerMovement candidate){var o=Manager.PersonalOrder;var parcel=Manager.GetComponent<GridShiftPersonalDelivery>().PackageIdentity;return o!=null&&o.status==DeliveryStatus.Transporting&&string.IsNullOrEmpty(CargoOrderId)&&parcel!=null&&parcel.orderId==o.id&&candidate!=null&&parcel.transform.IsChildOf(candidate.CameraTransform);}
 public string CargoCaption(PlayerMovement candidate){if(!CargoOpen)return "Abrir compartimento";if(!string.IsNullOrEmpty(CargoOrderId))return "Retirar encomenda";if(HasHeldCargo(candidate))return Manager.PersonalOrder.weightKg>capacityKg?"Compartimento cheio":"Guardar encomenda";return "Fechar compartimento";}
 bool RearReach(PlayerMovement candidate){return candidate!=null&&SceneManager.GetActiveScene()==gameObject.scene&&GridShiftDeliveryAccess.InReach(candidate,CargoPosition,2.6f);}
 public bool ToggleCargo(PlayerMovement candidate){if(Driving||Speed>.3f||TransferBusy||CargoAnimating||!RearReach(candidate))return Fail("Aproxime-se da traseira com a van parada e aguarde a tampa.");CargoOpen=!CargoOpen;GridShiftAudio.At(CargoOpen?"Tablet_Open":"Tablet_Close",CargoPosition,"SFX",.35f);Manager.GetComponent<DeliveryPersistence>()?.RequestSave();return true;}
 public bool TransferCargo(PlayerMovement candidate){
  if(Driving||Speed>.3f||TransferBusy||CargoAnimating||!RearReach(candidate))return Fail("Estacione e aproxime-se da abertura traseira.");
  if(!CargoOpen)return Fail("Abra o compartimento antes de transferir a encomenda.");
  var order=Manager.PersonalOrder;if(order==null||order.status!=DeliveryStatus.Transporting)return Fail("Nenhuma encomenda ativa disponível para transporte.");
  bool storing=string.IsNullOrEmpty(CargoOrderId);if(storing&&(!HasHeldCargo(candidate)||order.weightKg>capacityKg))return Fail(order.weightKg>capacityKg?"Compartimento cheio":"Você precisa estar segurando a caixa deste pedido.");
  if(!storing&&CargoOrderId!=order.id)return Fail("A carga não pertence ao pedido ativo.");
  TransferBusy=true;if(!Manager.GetComponent<GridShiftPersonalDelivery>().MoveCargo(this,candidate,storing)){TransferBusy=false;return Fail("A representação da encomenda ainda não está disponível.");}return true;
 }
 public void FinishCargoTransfer(string orderId,bool storing){TransferBusy=false;var order=Manager.PersonalOrder;if(order==null||order.id!=orderId||order.status!=DeliveryStatus.Transporting)return;CargoOrderId=storing?orderId:null;if(storing)UpdateMission(7);Manager.Changed();Manager.GetComponent<DeliveryPersistence>()?.RequestSave();GridShiftAudio.Play("Cargo_Collected","SFX",.4f);Manager.Notify(storing?"ENCOMENDA CARREGADA · Pedido armazenado na van. Carga: 1 encomenda":"ENCOMENDA RETIRADA · Leve a caixa até a entrada do cliente.");}
 void RefreshCargo(){/* GridShiftPersonalDelivery moves the same identified package to CargoAnchor. */}
 void AnimateCargo(){if(hatch==null)return;doorProgress=Mathf.MoveTowards(doorProgress,CargoOpen?1:0,Time.deltaTime/Mathf.Max(.25f,cargoDoorDuration));hatch.localRotation=cargoRestRotation*Quaternion.AngleAxis(cargoOpeningAngle*Mathf.SmoothStep(0,1,doorProgress),hingeAxis);if(CargoAction!=null)CargoAction.title=CargoCaption(InteractionFocus.PlayerFor(this));}
 void BindAuthoredCargo(){doorProgress=CargoOpen?1:0;hatch.localRotation=cargoRestRotation*Quaternion.AngleAxis(CargoOpen?cargoOpeningAngle:0,hingeAxis);CargoCloseAction=GridShiftWorldAction.Create(hatch,"Fechar compartimento",Vector3.zero,p=>ToggleCargo(p));CargoCloseAction.available=()=>CargoOpen&&!Driving&&!CargoAnimating&&!TransferBusy;CargoAction=GridShiftWorldAction.Create(transform,"Abrir compartimento",Vector3.zero,p=>{if(CargoOpen&&(!string.IsNullOrEmpty(CargoOrderId)||HasHeldCargo(p)))return TransferCargo(p);return ToggleCargo(p);});CargoAction.available=()=>!Driving&&Speed<.3f&&!CargoAnimating&&!TransferBusy;}
 void BuildCargoDetails(Material dark){
  var enclosure=GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="Closed cargo body");var center=enclosure!=null?enclosure.localPosition:shape.center;var size=enclosure!=null?enclosure.localScale:new Vector3(shape.size.x*.85f,shape.size.y*.55f,shape.size.z*.42f);var shell=enclosure!=null?enclosure.GetComponent<Renderer>().sharedMaterial:dark;
  if(enclosure!=null)foreach(var r in enclosure.GetComponentsInChildren<Renderer>())r.enabled=false;
  float rear=center.z-size.z*.5f,floor=center.y-size.y*.5f,top=center.y+size.y*.5f;
  CargoPart("Cargo floor",new Vector3(center.x,floor,center.z),new Vector3(size.x,.08f,size.z),dark);
  CargoPart("Cargo roof",new Vector3(center.x,top,center.z),new Vector3(size.x,.06f,size.z),shell);
  foreach(float side in new[]{-1f,1f}){CargoPart("Cargo side",center+Vector3.right*side*size.x*.5f,new Vector3(.06f,size.y,size.z),shell);CargoPart("Cargo aperture trim",new Vector3(center.x+side*(size.x*.5f-.04f),center.y,rear-.02f),new Vector3(.08f,size.y,.1f),dark);CargoPart("Cargo side rail",new Vector3(center.x+side*(size.x*.5f-.07f),floor+.23f,center.z),new Vector3(.04f,.06f,size.z*.85f),dark);}
  var hinge=new GameObject("CargoDoorHinge");hinge.transform.SetParent(transform,false);hinge.transform.localPosition=new Vector3(center.x,top,rear-.045f);hatch=hinge.transform;
  // Model-space right is the hinge edge. +95 degrees lifts the lower edge toward the rear and above the roof.
  hingeAxis=transform.InverseTransformDirection(transform.right).normalized;
  var door=CargoPart("CargoDoorMesh",Vector3.zero,new Vector3(size.x-.08f,size.y-.05f,.055f),shell);door.transform.SetParent(hatch,false);door.transform.localPosition=Vector3.down*(size.y*.5f);
  var trim=CargoPart("Cargo door lower trim",Vector3.zero,new Vector3(size.x-.2f,.06f,.07f),dark);trim.transform.SetParent(hatch,false);trim.transform.localPosition=new Vector3(0,-size.y+.08f,-.025f);
  var handle=CargoPart("Cargo handle",Vector3.zero,new Vector3(.3f,.05f,.06f),dark);handle.transform.SetParent(hatch,false);handle.transform.localPosition=new Vector3(0,-size.y+.24f,-.06f);
  CargoCloseAction=GridShiftWorldAction.Create(hatch,"Fechar compartimento",new Vector3(0,-size.y+.24f,-.06f),p=>ToggleCargo(p));CargoCloseAction.focusOffset=Vector3.zero;CargoCloseAction.focusSize=new Vector3(.8f,.35f,.35f);CargoCloseAction.available=()=>CargoOpen&&!Driving&&!CargoAnimating&&!TransferBusy;
  var anchor=new GameObject("CargoPackageAnchor");anchor.transform.SetParent(transform,false);anchor.transform.localPosition=new Vector3(center.x,floor+.04f,rear+size.z*.35f);CargoAnchor=anchor.transform;
  doorProgress=CargoOpen?1:0;hatch.localRotation=Quaternion.AngleAxis(CargoOpen?cargoOpeningAngle:0,hingeAxis);
  CargoAction=GridShiftWorldAction.Create(transform,"Abrir compartimento",transform.InverseTransformPoint(CargoPosition),p=>{if(CargoOpen&&(!string.IsNullOrEmpty(CargoOrderId)||HasHeldCargo(p)))return TransferCargo(p);return ToggleCargo(p);});CargoAction.gameObject.name="CargoLoadingInteractable";
  // The volume sits outside the solid vehicle collider, covers the real aperture, and never ignores external walls.
  CargoAction.focusOffset=new Vector3(0,Mathf.Clamp(center.y,1.3f,1.8f),-.13f);CargoAction.focusSize=new Vector3(size.x*.9f,Mathf.Max(size.y,1.4f),.5f);CargoAction.available=()=>!Driving&&Speed<.3f&&!CargoAnimating&&!TransferBusy;
 }
 GameObject CargoPart(string name,Vector3 at,Vector3 size,Material material){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(transform,false);go.transform.localPosition=at;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;go.GetComponent<Collider>().enabled=false;return go;}
}
