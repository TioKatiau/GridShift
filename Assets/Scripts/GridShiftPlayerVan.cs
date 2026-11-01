using System.Linq;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Rigidbody),typeof(BoxCollider))]
public sealed class GridShiftPlayerVan:MonoBehaviour {
 public const string VehicleId="GS-VAN01";
 public float capacityKg=80,maxSpeed=12,reverseSpeed=4;
 public bool Driving{get;private set;}public float Speed=>body!=null?Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up).magnitude:0;
 public string CargoOrderId{get;private set;}public Vector3 SafeExit{get;private set;}
 public string LastError{get;private set;}
 public Vector3 EntryPosition=>transform.TransformPoint(new Vector3(shape.center.x-shape.size.x*.5f-.3f,0,.5f));
 public Vector3 RightEntryPosition=>transform.TransformPoint(new Vector3(shape.center.x+shape.size.x*.5f+.3f,0,.5f));
 public Vector3 CargoPosition=>transform.TransformPoint(new Vector3(0,0,shape.center.z-shape.size.z*.5f-.35f));
 Vector3 safePosition;Quaternion safeRotation;GridShiftCityBoundary boundary;
 public DeliveryManager Manager{get;private set;}
 Rigidbody body;BoxCollider shape;PlayerMovement player;Camera view,oldCamera;bool oldBlocked;Renderer[] playerRenderers;bool[] rendererStates;Collider[] playerColliders;bool[] colliderStates;
 AudioSource engine;TMP_Text instruments;GameObject cargoBox,drivingHud;float throttle,steer;bool brake;Vector3 last;CanvasGroup[] hud;float[] hudAlpha;PhysicsMaterial friction;readonly RaycastHit[] cameraHits=new RaycastHit[16];
 public CompanyVehicleHealth Health=>Manager.Company.Health(VehicleId);
 void Awake(){body=GetComponent<Rigidbody>();shape=GetComponent<BoxCollider>();friction=new PhysicsMaterial("Van16 collision"){dynamicFriction=.05f,staticFriction=.05f,frictionCombine=PhysicsMaterialCombine.Minimum};shape.material=friction;body.mass=1200;body.isKinematic=false;body.useGravity=true;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;body.linearDamping=.1f;body.angularDamping=5;Park();}
 public void Initialize(DeliveryManager manager,GridShiftVanState state){
  Manager=manager;CargoOrderId=state?.cargoOrderId;SafeExit=state!=null?state.safeExit:transform.position+transform.right*2;last=transform.position;
  var config=manager.GetComponent<GridShiftActiveOperations>().Config;safePosition=state!=null&&state.hasSafePosition?state.safePosition:config.vanPosition;safeRotation=state!=null&&state.hasSafePosition?state.safeRotation:config.vanRotation;boundary=FindObjectsByType<GridShiftCityBoundary>(FindObjectsSortMode.None).FirstOrDefault(b=>b.gameObject.scene==gameObject.scene);
  GridShiftWorldAction.Create(transform,"Entrar na van",transform.InverseTransformPoint(EntryPosition),p=>Enter(p)).available=()=>!Driving;
  GridShiftWorldAction.Create(transform,"Entrar na van",transform.InverseTransformPoint(RightEntryPosition),p=>Enter(p)).available=()=>!Driving;
  GridShiftWorldAction.Create(transform,"Carregar / retirar encomenda",transform.InverseTransformPoint(CargoPosition),p=>TransferCargo(p)).available=()=>!Driving;
  var cameraObject=new GameObject("VanDrivingCamera");cameraObject.transform.SetParent(transform,false);view=cameraObject.AddComponent<Camera>();view.nearClipPlane=.15f;view.farClipPlane=800;view.fieldOfView=65;view.enabled=false;
  var canvas=new GameObject("VanWorldHud",typeof(Canvas));canvas.transform.SetParent(view.transform,false);canvas.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;canvas.transform.localPosition=new Vector3(0,-.25f,1);canvas.transform.localScale=Vector3.one*.001f;var rect=canvas.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(800,160);instruments=GridShiftUI.Text(rect,"",24,150);drivingHud=canvas;drivingHud.SetActive(false);
  engine=gameObject.AddComponent<AudioSource>();var library=Resources.Load<GridShiftAudioLibrary>("GridShiftAudioLibrary");if(library!=null){engine.clip=library.Find("Car_Engine_Loop");engine.outputAudioMixerGroup=library.Group("Vehicles");}engine.loop=true;engine.playOnAwake=false;engine.spatialBlend=1;engine.minDistance=3;engine.maxDistance=55;engine.rolloffMode=AudioRolloffMode.Linear;
  RefreshCargo();
 }
 void Park(){body.constraints=RigidbodyConstraints.FreezeRotation|RigidbodyConstraints.FreezePositionX|RigidbodyConstraints.FreezePositionZ;}
 public bool Enter(PlayerMovement candidate){
  if(Manager==null||Driving||Speed>.3f||!(GridShiftDeliveryAccess.InReach(candidate,EntryPosition,3)||GridShiftDeliveryAccess.InReach(candidate,RightEntryPosition,3))||Health.fuel<=0||Health.condition<=5||Health.maintenanceUntil>Manager.Company.Day)return Fail("Aproxime-se da porta com a van parada, abastecida e em condições.");
  if(!FindExit(candidate,out var safe))return Fail("A saída da van está obstruída.");SafeExit=safe;player=candidate;oldBlocked=player.ControlsBlocked;oldCamera=player.CameraTransform.GetComponent<Camera>();
  if(Manager.PersonalOrder?.status==DeliveryStatus.Transporting&&CargoOrderId!=Manager.PersonalOrder.id)return Fail("Guarde a encomenda no compartimento antes de dirigir.");
  playerRenderers=player.GetComponentsInChildren<Renderer>(true);rendererStates=playerRenderers.Select(r=>r.enabled).ToArray();foreach(var r in playerRenderers)r.enabled=false;
  playerColliders=player.GetComponentsInChildren<Collider>(true);colliderStates=playerColliders.Select(c=>c.enabled).ToArray();foreach(var c in playerColliders)c.enabled=false;
  var groups=new System.Collections.Generic.List<CanvasGroup>();foreach(var canvas in player.GetComponentsInChildren<Canvas>(true)){var group=canvas.GetComponent<CanvasGroup>();if(group==null)group=canvas.gameObject.AddComponent<CanvasGroup>();groups.Add(group);}hud=groups.ToArray();hudAlpha=hud.Select(g=>g.alpha).ToArray();foreach(var g in hud)g.alpha=0;
  player.BlockControls(true);player.GetComponent<CharacterController>().enabled=false;oldCamera.enabled=false;view.tag="MainCamera";view.enabled=true;Driving=true;body.constraints=RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
  throttle=steer=0;brake=true;drivingHud.SetActive(true);GridShiftAudio.Play("UI_Confirm","UI",.25f);if(engine.clip!=null)engine.Play();return true;
 }
 public bool Exit(){
  if(!Driving)return false;if(Speed>.35f)return Fail("Pare a van antes de sair.");if(!FindExit(player,out var exit))return Fail("Não existe espaço livre para desembarcar.");
  SafeExit=exit;Driving=false;drivingHud.SetActive(false);Park();body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;
  player.transform.SetPositionAndRotation(exit,Quaternion.Euler(0,transform.eulerAngles.y-90,0));player.GetComponent<CharacterController>().enabled=true;player.BlockControls(oldBlocked);oldCamera.enabled=true;view.enabled=false;view.tag="Untagged";
  for(int i=0;i<playerRenderers.Length;i++)if(playerRenderers[i]!=null)playerRenderers[i].enabled=rendererStates[i];for(int i=0;i<playerColliders.Length;i++)if(playerColliders[i]!=null)playerColliders[i].enabled=colliderStates[i];for(int i=0;i<hud.Length;i++)if(hud[i]!=null)hud[i].alpha=hudAlpha[i];engine.Stop();UpdateMission(8);Manager.GetComponent<DeliveryPersistence>()?.RequestSave();return true;
 }
 public bool FindExit(PlayerMovement candidate,out Vector3 exit){
  exit=transform.position;var cc=candidate!=null?candidate.GetComponent<CharacterController>():null;float radius=cc!=null?cc.radius:.35f;float height=cc!=null?cc.height:1.8f;
  foreach(var local in new[]{new Vector3(-shape.size.x/2-1,0,.5f),new Vector3(shape.size.x/2+1,0,.5f),new Vector3(0,0,-shape.size.z/2-1.2f)}){
   var point=transform.TransformPoint(local);if(!Physics.Raycast(point+Vector3.up*2,Vector3.down,out var floor,4,~((1<<6)|(1<<9)),QueryTriggerInteraction.Ignore)||floor.normal.y<.8f)continue;point.y=floor.point.y+.08f;
   var hits=Physics.OverlapCapsule(point+Vector3.up*(radius+.05f),point+Vector3.up*(height-radius),radius,~0,QueryTriggerInteraction.Ignore);
   if(hits.Any(h=>!h.transform.IsChildOf(transform)&&(candidate==null||!h.transform.IsChildOf(candidate.transform))))continue;
   var from=transform.position+Vector3.up;var delta=point+Vector3.up-from;int count=Physics.RaycastNonAlloc(from,delta.normalized,cameraHits,delta.magnitude,~(1<<9),QueryTriggerInteraction.Ignore);bool blocked=count==cameraHits.Length;for(int i=0;i<count;i++)if(!cameraHits[i].transform.IsChildOf(transform)&&(candidate==null||!cameraHits[i].transform.IsChildOf(candidate.transform))){blocked=true;break;}if(blocked)continue;
   exit=point;return true;
  }return false;
 }
 public bool TransferCargo(PlayerMovement candidate){
  if(Driving||Speed>.3f||!GridShiftDeliveryAccess.InReach(candidate,CargoPosition,3))return Fail("Estacione e aproxime-se do compartimento de carga.");
  var order=Manager.PersonalOrder;if(order==null||order.status!=DeliveryStatus.Transporting)return Fail("Colete uma encomenda pessoal antes de usar a carga.");
  if(CargoOrderId==order.id)CargoOrderId=null;else{if(!string.IsNullOrEmpty(CargoOrderId)||order.weightKg>capacityKg)return Fail("Carga incompatível com a capacidade disponível.");CargoOrderId=order.id;}
  RefreshCargo();if(!string.IsNullOrEmpty(CargoOrderId))UpdateMission(7);Manager.Changed();Manager.GetComponent<DeliveryPersistence>()?.RequestSave();GridShiftAudio.Play("Cargo_Collected","SFX",.35f);return true;
 }
 void RefreshCargo(){if(cargoBox!=null)Destroy(cargoBox);if(string.IsNullOrEmpty(CargoOrderId))return;cargoBox=GridShiftDeliveryAccess.Package(transform);cargoBox.name="VanCargo-"+CargoOrderId;cargoBox.transform.localPosition=new Vector3(0,1,-1);}
 bool Fail(string message){LastError=message;Manager?.Notify(message);return false;}
 public void SetInput(float acceleration,float steering,bool stopping){throttle=Mathf.Clamp(acceleration,-1,1);steer=Mathf.Clamp(steering,-1,1);brake=stopping;}
 public bool ScriptedInput{get;set;}
 void Update(){if(Manager==null)return;if(!string.IsNullOrEmpty(CargoOrderId)&&!Manager.orders.Any(o=>o.id==CargoOrderId&&o.mode==DeliveryMode.Personal&&o.status==DeliveryStatus.Transporting)){CargoOrderId=null;RefreshCargo();}
  if(!Driving)return;if(Time.timeScale<=0||GridShiftOperationsUI.ModalOpen||GridShiftCampaignUI.ModalOpen){SetInput(0,0,true);return;}
  if(!ScriptedInput)SetInput((GridShiftControls.Held(GSAction.Forward)?1:0)-(GridShiftControls.Held(GSAction.Back)?1:0),(GridShiftControls.Held(GSAction.Right)?1:0)-(GridShiftControls.Held(GSAction.Left)?1:0),GridShiftControls.Held(GSAction.Jump));
  if(GridShiftControls.Down(GSAction.Interact)&&InteractionFocus.Consume()){Exit();GridShiftControls.SuppressFrame();}
  engine.pitch=Mathf.Lerp(.8f,1.5f,Speed/maxSpeed);instruments.text="VAN GRIDSHIFT  ·  "+(Speed*3.6f).ToString("0")+" km/h\nCombustível "+Health.fuel.ToString("0.0")+"%  ·  Condição "+Health.condition.ToString("0.0")+"%\n"+(string.IsNullOrEmpty(CargoOrderId)?"Compartimento vazio":"Encomenda a bordo")+"  ·  Espaço: frear  ·  E: sair parado";
 }
 void FixedUpdate(){
  if(Manager==null||!Manager.Company.Ready)return;
  bool inside=boundary==null||body.position.x>=boundary.playable.min.x&&body.position.x<=boundary.playable.max.x&&body.position.z>=boundary.playable.min.z&&body.position.z<=boundary.playable.max.z;
  if(inside&&Physics.Raycast(body.position+Vector3.up*.5f,Vector3.down,out var ground,2,~((1<<6)|(1<<9)),QueryTriggerInteraction.Ignore)&&ground.normal.y>.75f&&Mathf.Abs(body.position.y-ground.point.y)<1){safePosition=body.position;safeRotation=body.rotation;}
  if(body.position.y<safePosition.y-8){body.position=safePosition+Vector3.up*.1f;body.rotation=safeRotation;body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;last=body.position;Manager.Notify("Van recuperada no último ponto seguro; encomenda preservada.");}
  float moved=Vector3.ProjectOnPlane(body.position-last,Vector3.up).magnitude;last=body.position;if(moved<10)Manager.Company.PersonalVehicleDistance(moved);
  if(!Driving)return;float forward=Vector3.Dot(body.linearVelocity,transform.forward);float target=brake||Health.fuel<=0||Health.condition<=5||Health.maintenanceUntil>Manager.Company.Day?0:throttle*(throttle>=0?maxSpeed:reverseSpeed);
  if(player!=null)player.transform.SetPositionAndRotation(body.position,Quaternion.Euler(0,body.rotation.eulerAngles.y,0));
  if(!string.IsNullOrEmpty(CargoOrderId)&&moved<10){var order=Manager.orders.FirstOrDefault(o=>o.id==CargoOrderId);if(order!=null)order.personalDrivenMeters+=moved;}
  body.AddForce(transform.forward*Mathf.Clamp((target-forward)*3,-12,7),ForceMode.Acceleration);body.AddForce(-transform.right*Vector3.Dot(body.linearVelocity,transform.right)*6,ForceMode.Acceleration);
  if(Mathf.Abs(forward)>.15f)body.MoveRotation(body.rotation*Quaternion.Euler(0,steer*Mathf.Clamp(forward/5,-1,1)*65*Time.fixedDeltaTime,0));
 }
 void LateUpdate(){if(!Driving)return;var anchor=transform.position+Vector3.up*1.6f;var direction=(-transform.forward*5+Vector3.up).normalized;float length=5.1f;int count=Physics.SphereCastNonAlloc(anchor,.2f,direction,cameraHits,length,~0,QueryTriggerInteraction.Ignore);for(int i=0;i<count;i++)if(!cameraHits[i].transform.IsChildOf(transform)&&(player==null||!cameraHits[i].transform.IsChildOf(player.transform)))length=Mathf.Min(length,Mathf.Max(.4f,cameraHits[i].distance-.15f));view.transform.position=anchor+direction*length;view.transform.LookAt(anchor+transform.forward*2);}
 public GridShiftVanState Capture(){if(Driving&&FindExit(player,out var safe))SafeExit=safe;return new GridShiftVanState{placed=true,position=body.position,rotation=body.rotation,safeExit=SafeExit,cargoOrderId=CargoOrderId,hasSafePosition=true,safePosition=safePosition,safeRotation=safeRotation};}
 void UpdateMission(int step){var order=Manager.PersonalOrder;if(order==null||!order.depotRequired||(step==8&&order.personalDrivenMeters<1))return;var mission=Manager.Company.State.missions.FirstOrDefault(m=>m.id=="depot-workflow"&&m.orderId==order.id&&!m.rewardClaimed);if(mission!=null){mission.step=Mathf.Max(mission.step,step);Manager.Changed();}}
 void OnDestroy(){if(Driving&&player!=null){var controller=player.GetComponent<CharacterController>();if(controller!=null)controller.enabled=true;player.BlockControls(oldBlocked);if(oldCamera!=null)oldCamera.enabled=true;}if(friction!=null)Destroy(friction);}
}
