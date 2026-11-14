using System.Linq;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Rigidbody),typeof(BoxCollider))]
public sealed partial class GridShiftPlayerVan:MonoBehaviour {
 public const string VehicleId="GS-VAN01";
 public float capacityKg=80,maxSpeed=19,reverseSpeed=5;
 [Header("Câmera orbital")]
 public float cameraDistance=6,cameraHeight=2.5f,cameraMinDistance=3,cameraMaxDistance=10,cameraSensitivity=.12f,cameraRecenterDelay=3;
 public Key cameraPerspectiveKey=Key.C;
 public bool Driving{get;private set;}public float Speed=>body!=null?Vector3.ProjectOnPlane(body.linearVelocity,Vector3.up).magnitude:0;
 public string CargoOrderId{get;private set;}public Vector3 SafeExit{get;private set;}
 public string LastError{get;private set;}public string LastObstacle{get;private set;}
 public Vector3 EntryPosition=>transform.TransformPoint(new Vector3(shape.center.x-shape.size.x*.5f-.3f,0,.5f));
 public Vector3 RightEntryPosition=>transform.TransformPoint(new Vector3(shape.center.x+shape.size.x*.5f+.3f,0,.5f));
 public Vector3 CargoPosition=>transform.TransformPoint(new Vector3(shape.center.x,0,shape.center.z-shape.size.z*.5f-.18f));
 Vector3 safePosition;Quaternion safeRotation;GridShiftCityBoundary boundary;
 public DeliveryManager Manager{get;private set;}
 public bool CargoOpen{get;private set;}public bool ParkingBrake=>brake;GridShiftVanOrbit orbit;Transform generalHud;Transform hudParent;Vector3 hudLocal;Quaternion hudRotation;float smoothSteer;float lastImpact=-10;[SerializeField]Transform hatch;[SerializeField]Renderer[] rearLights;[SerializeField]Material brakeMaterial,reverseMaterial;[SerializeField]bool sceneAuthored;Quaternion cargoRestRotation=Quaternion.identity;
 Rigidbody body;BoxCollider shape;PlayerMovement player;[SerializeField]Camera view;Camera oldCamera;bool oldBlocked;Renderer[] playerRenderers;bool[] rendererStates;Collider[] playerColliders;bool[] colliderStates;
 AudioSource engine;GameObject cargoBox,drivingHud;float throttle,steer;bool brake;Vector3 last;PhysicsMaterial friction;readonly RaycastHit[] cameraHits=new RaycastHit[16];
 public CompanyVehicleHealth Health=>Manager.Company.Health(VehicleId);
 void Awake(){body=GetComponent<Rigidbody>();shape=GetComponent<BoxCollider>();friction=new PhysicsMaterial("Van16 collision"){dynamicFriction=0,staticFriction=0,bounciness=0,bounceCombine=PhysicsMaterialCombine.Minimum,frictionCombine=PhysicsMaterialCombine.Minimum};shape.material=friction;body.mass=1200;body.isKinematic=false;body.useGravity=true;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;body.linearDamping=.1f;body.angularDamping=5;body.maxDepenetrationVelocity=2;body.solverIterations=12;body.solverVelocityIterations=4;BuildRoadContact();Park();}
 public void AuthorScene(){if(sceneAuthored)return;transform.localScale=Vector3.one*.78f;shape=GetComponent<BoxCollider>();BuildDetails();GridShiftWorldAction.Create(transform,"Entrar na van",transform.InverseTransformPoint(EntryPosition),null,"van-enter-left");GridShiftWorldAction.Create(transform,"Entrar na van",transform.InverseTransformPoint(RightEntryPosition),null,"van-enter-right");foreach(var door in GetComponentsInChildren<GridShiftWorldAction>().Where(a=>a.bindingId=="van-enter-left"||a.bindingId=="van-enter-right")){door.focusOffset=Vector3.up*1.3f;door.focusSize=new Vector3(.75f,2.4f,2);}MakeCamera();sceneAuthored=true;foreach(var action in GetComponentsInChildren<GridShiftWorldAction>(true))action.sceneAuthored=true;}
 void MakeCamera(){if(view!=null)return;var cameraObject=new GameObject("VanDrivingCamera");cameraObject.transform.SetParent(transform,false);view=cameraObject.AddComponent<Camera>();view.nearClipPlane=.15f;view.farClipPlane=800;view.fieldOfView=65;view.enabled=false;cameraObject.AddComponent<GridShiftVanOrbit>();}
 public void Initialize(DeliveryManager manager,GridShiftVanState state){
  if(!sceneAuthored)transform.localScale=Vector3.one*.78f;
  Manager=manager;CargoOrderId=state?.cargoOrderId;CargoOpen=state!=null&&state.cargoOpen;SafeExit=state!=null?state.safeExit:transform.position+transform.right*2;last=transform.position;
  var config=manager.GetComponent<GridShiftActiveOperations>().Config;safePosition=state!=null&&state.hasSafePosition?state.safePosition:config.vanPosition;safeRotation=state!=null&&state.hasSafePosition?state.safeRotation:config.vanRotation;boundary=FindObjectsByType<GridShiftCityBoundary>(FindObjectsSortMode.None).FirstOrDefault(b=>b.gameObject.scene==gameObject.scene);
  var leftDoor=GridShiftWorldAction.Create(transform,"Entrar na van",transform.InverseTransformPoint(EntryPosition),p=>Enter(p),"van-enter-left");leftDoor.available=()=>!Driving;if(!leftDoor.sceneAuthored){leftDoor.focusOffset=Vector3.up*1.3f;leftDoor.focusSize=new Vector3(.75f,2.4f,2);}
  var rightDoor=GridShiftWorldAction.Create(transform,"Entrar na van",transform.InverseTransformPoint(RightEntryPosition),p=>Enter(p),"van-enter-right");rightDoor.available=()=>!Driving;if(!rightDoor.sceneAuthored){rightDoor.focusOffset=Vector3.up*1.3f;rightDoor.focusSize=new Vector3(.75f,2.4f,2);}
  MakeCamera();orbit=view.GetComponent<GridShiftVanOrbit>();orbit.vehicle=transform;orbit.body=body;orbit.distance=Mathf.Clamp(Mathf.Max(cameraDistance,shape.size.z*.9f+1),cameraMinDistance,cameraMaxDistance);orbit.height=Mathf.Max(shape.bounds.size.y*.65f,cameraHeight-orbit.distance*Mathf.Sin(12*Mathf.Deg2Rad));orbit.minDistance=Mathf.Max(cameraMinDistance,shape.size.z*.55f);orbit.maxDistance=cameraMaxDistance;orbit.sensitivity=cameraSensitivity;orbit.recenterDelay=cameraRecenterDelay;orbit.perspectiveKey=cameraPerspectiveKey;
  drivingHud=new GameObject("Van HUD 17");drivingHud.transform.SetParent(Manager.transform,false);drivingHud.AddComponent<GridShiftVanHUD>().van=this;
  BuildDetails();
  engine=gameObject.AddComponent<AudioSource>();var library=Resources.Load<GridShiftAudioLibrary>("GridShiftAudioLibrary");if(library!=null){engine.clip=library.Find("Car_Engine_Loop");engine.outputAudioMixerGroup=library.Group("Vehicles");}engine.loop=true;engine.playOnAwake=false;engine.spatialBlend=1;engine.minDistance=3;engine.maxDistance=55;engine.rolloffMode=AudioRolloffMode.Linear;
  RefreshCargo();if(GetComponent<GridShiftRoadRules>()==null)gameObject.AddComponent<GridShiftRoadRules>();
 }
 void Park(){body.constraints=RigidbodyConstraints.FreezeRotation|RigidbodyConstraints.FreezePositionX|RigidbodyConstraints.FreezePositionZ;}
 public bool Enter(PlayerMovement candidate){
  if(Manager==null||Driving||TransferBusy||CargoAnimating||Speed>.3f||!(GridShiftDeliveryAccess.InReach(candidate,EntryPosition,3)||GridShiftDeliveryAccess.InReach(candidate,RightEntryPosition,3))||Health.fuel<=0||Health.condition<=5||Health.maintenanceUntil>Manager.Company.Day)return Fail("Aproxime-se da porta com a van parada, abastecida e em condições.");
  if(!FindExit(candidate,out var safe))return Fail("A saída da van está obstruída.");SafeExit=safe;player=candidate;oldBlocked=player.ControlsBlocked;oldCamera=player.CameraTransform.GetComponent<Camera>();
  if(Manager.PersonalOrder?.status==DeliveryStatus.Transporting&&CargoOrderId!=Manager.PersonalOrder.id)return Fail("Guarde a encomenda no compartimento antes de dirigir.");
  playerRenderers=player.GetComponentsInChildren<Renderer>(true).Where(r=>r.GetComponentInParent<Canvas>()==null&&!r.transform.IsChildOf(player.CameraTransform.Find("Hud")??player.CameraTransform)).ToArray();rendererStates=playerRenderers.Select(r=>r.enabled).ToArray();foreach(var r in playerRenderers)r.enabled=false;
  playerColliders=player.GetComponentsInChildren<Collider>(true);colliderStates=playerColliders.Select(c=>c.enabled).ToArray();foreach(var c in playerColliders)c.enabled=false;
  generalHud=player.CameraTransform.Find("Hud");if(generalHud!=null){hudParent=generalHud.parent;hudLocal=generalHud.localPosition;hudRotation=generalHud.localRotation;generalHud.SetParent(view.transform,false);generalHud.localPosition=hudLocal;generalHud.localRotation=hudRotation;}
  CargoOpen=false;player.BlockControls(true);player.GetComponent<CharacterController>().enabled=false;oldCamera.enabled=false;view.tag="MainCamera";view.enabled=true;Driving=true;body.constraints=RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
  throttle=steer=0;brake=true;orbit.driver=player.transform;view.transform.SetPositionAndRotation(oldCamera.transform.position,oldCamera.transform.rotation);orbit.ResetView();GridShiftAudio.Instance?.RefreshListener();GridShiftAudio.Play("UI_Confirm","UI",.25f);if(engine.clip!=null)engine.Play();return true;
 }
 public bool Exit(){
  if(!Driving)return false;if(Speed>.35f)return Fail("Pare a van antes de sair.");if(!FindExit(player,out var exit))return Fail("Não existe espaço livre para desembarcar.");
  SafeExit=exit;Driving=false;Park();body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;
  player.transform.SetPositionAndRotation(exit,Quaternion.Euler(0,transform.eulerAngles.y-90,0));player.GetComponent<CharacterController>().enabled=true;player.BlockControls(oldBlocked);oldCamera.enabled=true;view.enabled=false;view.tag="Untagged";if(generalHud!=null){generalHud.SetParent(hudParent,false);generalHud.localPosition=hudLocal;generalHud.localRotation=hudRotation;}GridShiftAudio.Instance?.RefreshListener();
  for(int i=0;i<playerRenderers.Length;i++)if(playerRenderers[i]!=null)playerRenderers[i].enabled=rendererStates[i];for(int i=0;i<playerColliders.Length;i++)if(playerColliders[i]!=null)playerColliders[i].enabled=colliderStates[i];engine.Stop();UpdateMission(8);Manager.GetComponent<DeliveryPersistence>()?.RequestSave();return true;
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
 bool Fail(string message){LastError=message;Manager?.Notify(message);return false;}
 public void SetInput(float acceleration,float steering,bool stopping){throttle=Mathf.Clamp(acceleration,-1,1);steer=Mathf.Clamp(steering,-1,1);brake=stopping;}
 public bool ScriptedInput{get;set;}
 void Update(){if(Manager==null)return;if(!string.IsNullOrEmpty(CargoOrderId)&&!Manager.orders.Any(o=>o.id==CargoOrderId&&o.mode==DeliveryMode.Personal&&o.status==DeliveryStatus.Transporting)){CargoOrderId=null;RefreshCargo();}
  AnimateCargo();
  if(rearLights!=null){bool reversing=Driving&&Vector3.Dot(body.linearVelocity,transform.forward)<-.15f;bool braking=Driving&&(brake||throttle<0&&!reversing);brakeMaterial.SetColor("_EmissionColor",new Color(braking?1.8f:.12f,.005f,.002f));foreach(var light in rearLights)light.sharedMaterial=reversing?reverseMaterial:brakeMaterial;}
  if(!Driving)return;if(Time.timeScale<=0||GridShiftOperationsUI.ModalOpen||GridShiftCampaignUI.ModalOpen){SetInput(0,0,true);return;}
  if(!ScriptedInput)SetInput((GridShiftControls.Held(GSAction.Forward)?1:0)-(GridShiftControls.Held(GSAction.Back)?1:0),(GridShiftControls.Held(GSAction.Right)?1:0)-(GridShiftControls.Held(GSAction.Left)?1:0),GridShiftControls.Held(GSAction.Jump));
  if(GridShiftControls.Down(GSAction.Interact)&&InteractionFocus.Consume()){Exit();GridShiftControls.SuppressFrame();}
  engine.pitch=Mathf.Lerp(.8f,1.5f,Speed/maxSpeed);engine.volume=Mathf.Lerp(.2f,.6f,Speed/maxSpeed);
 }
 void FixedUpdate(){
  if(Manager==null||!Manager.Company.Ready)return;StabilizeRoadContact();
  bool inside=boundary==null||body.position.x>=boundary.playable.min.x&&body.position.x<=boundary.playable.max.x&&body.position.z>=boundary.playable.min.z&&body.position.z<=boundary.playable.max.z;
  if(inside&&Physics.Raycast(body.position+Vector3.up*.5f,Vector3.down,out var ground,2,~((1<<6)|(1<<9)),QueryTriggerInteraction.Ignore)&&ground.normal.y>.75f&&Mathf.Abs(body.position.y-ground.point.y)<1){safePosition=body.position;safeRotation=body.rotation;}
  if(body.position.y<safePosition.y-8){body.position=safePosition+Vector3.up*.1f;body.rotation=safeRotation;body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;last=body.position;Manager.Notify("Van recuperada no último ponto seguro; encomenda preservada.");}
  float moved=Vector3.ProjectOnPlane(body.position-last,Vector3.up).magnitude;last=body.position;if(moved<10)Manager.Company.PersonalVehicleDistance(moved);
  if(!Driving)return;if(GridShiftOperationsUI.ModalOpen||GridShiftCampaignUI.ModalOpen||Time.timeScale<=0)SetInput(0,0,true);float forward=Vector3.Dot(body.linearVelocity,transform.forward);float target=brake||Health.fuel<=0||Health.condition<=5||Health.maintenanceUntil>Manager.Company.Day?0:throttle*(throttle>=0?maxSpeed:reverseSpeed)*Mathf.Lerp(.55f,1,Health.condition/100);
  if(player!=null)player.transform.SetPositionAndRotation(body.position,Quaternion.Euler(0,body.rotation.eulerAngles.y,0));
  if(!string.IsNullOrEmpty(CargoOrderId)&&moved<10){var order=Manager.orders.FirstOrDefault(o=>o.id==CargoOrderId);if(order!=null)order.personalDrivenMeters+=moved;}
  body.AddForce(transform.forward*Mathf.Clamp((target-forward)*3,-12,7),ForceMode.Acceleration);body.AddForce(-transform.right*Vector3.Dot(body.linearVelocity,transform.right)*6,ForceMode.Acceleration);
  smoothSteer=Mathf.MoveTowards(smoothSteer,steer,Time.fixedDeltaTime*3);if(Mathf.Abs(forward)>.15f)body.MoveRotation(body.rotation*Quaternion.Euler(0,smoothSteer*Mathf.Clamp(forward/4,-1,1)*Mathf.Lerp(65,34,Speed/maxSpeed)*Time.fixedDeltaTime,0));
 }
 public GridShiftVanState Capture(){if(Driving&&FindExit(player,out var safe))SafeExit=safe;return new GridShiftVanState{cargoOpen=CargoOpen,placed=true,position=body.position,rotation=body.rotation,safeExit=SafeExit,cargoOrderId=CargoOrderId,hasSafePosition=true,safePosition=safePosition,safeRotation=safeRotation};}
 void UpdateMission(int step){var order=Manager.PersonalOrder;if(order==null||!order.depotRequired||(step==8&&order.personalDrivenMeters<1))return;var mission=Manager.Company.State.missions.FirstOrDefault(m=>m.id=="depot-workflow"&&m.orderId==order.id&&!m.rewardClaimed);if(mission!=null){mission.step=Mathf.Max(mission.step,step);Manager.Changed();}}
 public static float ImpactDamage(float speed)=>speed<1.5f?0:Mathf.Clamp((speed-1.5f)*(speed-1.5f)*.85f,.2f,55);
 public bool RecoverToDepot(PlayerMovement candidate){var operations=Manager.GetComponent<GridShiftActiveOperations>();if(Driving||Speed>.3f||!GridShiftDeliveryAccess.InReach(candidate,operations.Config.depotPosition+operations.Config.depotRotation*new Vector3(2,0,.5f),3))return Fail("Estacione e solicite o guincho no terminal do depósito.");var config=operations.Config;if(Vector3.Distance(body.position,config.vanPosition)<.5f&&Quaternion.Angle(body.rotation,config.vanRotation)<5)return Fail("A van já está na vaga de recuperação.");var center=config.vanPosition+config.vanRotation*Vector3.Scale(shape.center,transform.lossyScale);var extents=Vector3.Scale(shape.size,transform.lossyScale)*.475f;var occupied=Physics.OverlapBox(center,extents,config.vanRotation,~((1<<7)|(1<<8)),QueryTriggerInteraction.Ignore).FirstOrDefault(c=>!c.transform.IsChildOf(transform));if(occupied!=null){LastObstacle=occupied.name;return Fail("A vaga está ocupada. Libere o espaço antes de solicitar o guincho.");}if(!Manager.TryChangeMoney(-15,"van-recovery:"+System.Guid.NewGuid().ToString("N"),"Serviços","Guincho da van para vaga segura"))return Fail("Saldo insuficiente para o guincho de R$ 15.");body.position=config.vanPosition;body.rotation=config.vanRotation;body.linearVelocity=body.angularVelocity=Vector3.zero;safePosition=last=body.position;safeRotation=body.rotation;Physics.SyncTransforms();Manager.Changed();Manager.GetComponent<DeliveryPersistence>()?.RequestSave();Manager.Notify("Van reposicionada na vaga. Combustível, condição e carga preservados.");return true;}
 void OnCollisionEnter(Collision collision){if(collision.contacts.Any(c=>Mathf.Abs(c.normal.y)<.7f))LastObstacle=collision.collider.name+" at "+collision.collider.bounds.center;if(Manager==null||!Manager.Company.Ready||Time.time-lastImpact<.3f)return;float impact=0;foreach(var contact in collision.contacts)if(Mathf.Abs(contact.normal.y)<.7f&&contact.point.y>transform.position.y+.23f)impact=Mathf.Max(impact,Mathf.Abs(Vector3.Dot(collision.relativeVelocity,contact.normal)));float damage=ImpactDamage(impact);if(damage<=0)return;lastImpact=Time.time;Health.condition=Mathf.Clamp(Health.condition-damage,0,100);Manager.Notify("Impacto · condição "+Health.condition.ToString("0")+"% · manutenção no depósito");ImpactSound();Manager.Changed();Manager.GetComponent<DeliveryPersistence>()?.RequestSave();}
 AudioSource impactAudio;AudioClip impactClip;
 void ImpactSound(){if(impactAudio==null){impactAudio=gameObject.AddComponent<AudioSource>();impactAudio.playOnAwake=false;impactAudio.spatialBlend=1;impactAudio.minDistance=3;impactAudio.maxDistance=35;impactAudio.rolloffMode=AudioRolloffMode.Linear;impactAudio.outputAudioMixerGroup=GridShiftAudio.Instance!=null?GridShiftAudio.Instance.Group("SFX"):engine.outputAudioMixerGroup;int rate=22050;var samples=new float[rate/4];var random=new System.Random(170);float state=0;for(int i=0;i<samples.Length;i++){float t=(float)i/rate;state=Mathf.Lerp(state,(float)random.NextDouble()*2-1,.3f);samples[i]=(state*.65f+Mathf.Sin(t*2*Mathf.PI*73)*.35f)*Mathf.Exp(-t*22);}impactClip=AudioClip.Create("GridShift original body impact",samples.Length,1,rate,false);impactClip.SetData(samples,0);}impactAudio.PlayOneShot(impactClip,.55f);orbit.Shake(.12f);}
 void BuildDetails(){if(sceneAuthored){cargoRestRotation=hatch.localRotation;BindAuthoredCargo();return;}
  var shader=Shader.Find("Universal Render Pipeline/Lit");var dark=new Material(shader){color=new Color(.05f,.08f,.1f)};
  brakeMaterial=new Material(shader){color=new Color(.8f,.04f,.025f)};reverseMaterial=new Material(shader){color=new Color(.9f,.92f,.78f)};brakeMaterial.EnableKeyword("_EMISSION");brakeMaterial.SetColor("_EmissionColor",new Color(.8f,.02f,.01f));
  rearLights=new Renderer[2];for(int i=0;i<2;i++){var lamp=GameObject.CreatePrimitive(PrimitiveType.Cube);lamp.name="Van rear lamps";lamp.transform.SetParent(transform,false);lamp.transform.localPosition=new Vector3(shape.center.x+(i==0?-1:1)*shape.size.x*.38f,.7f,shape.center.z-shape.size.z*.5f-.08f);lamp.transform.localScale=new Vector3(.16f,.25f,.06f);Destroy(lamp.GetComponent<Collider>());rearLights[i]=lamp.GetComponent<Renderer>();rearLights[i].sharedMaterial=brakeMaterial;}
  BuildCargoDetails(dark);
  foreach(float side in new[]{-1f,1f}){var logo=new GameObject("GS delivery livery",typeof(TextMeshPro));logo.transform.SetParent(transform,false);logo.transform.localPosition=new Vector3(shape.center.x+side*(shape.size.x*.5f+.015f),shape.center.y+.3f,shape.center.z-.5f);logo.transform.localRotation=Quaternion.Euler(0,-side*90,0);var text=logo.GetComponent<TextMeshPro>();text.text="<color=#FFD23F>GS</color>\nGRIDSHIFT";text.fontSize=1.1f;text.rectTransform.sizeDelta=new Vector2(1.8f,.8f);text.alignment=TextAlignmentOptions.Center;}
 }
 void OnDestroy(){if(Driving&&player!=null){if(generalHud!=null&&hudParent!=null){generalHud.SetParent(hudParent,false);generalHud.localPosition=hudLocal;generalHud.localRotation=hudRotation;}var controller=player.GetComponent<CharacterController>();if(controller!=null)controller.enabled=true;player.BlockControls(oldBlocked);if(oldCamera!=null)oldCamera.enabled=true;}if(impactClip!=null)Destroy(impactClip);if(friction!=null)Destroy(friction);}
}
