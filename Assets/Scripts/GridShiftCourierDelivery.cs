using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// The vehicle stays stopped while its driver walks to the receiving area and back.
public sealed class GridShiftCourierDelivery:MonoBehaviour {
 CompanyVehicle vehicle;DeliveryManager manager;PedestrianManager pedestrians;PedestrianAgent actor;GameObject box,visual;
 readonly Queue<int> path=new Queue<int>();int start=-1,end=-1;Vector3 exit,receiving;float retry,wait,stalled;Vector3 last;string activeOrder;
 public string Diagnostic{get;private set;}="Preparando descarga";
 void Awake(){vehicle=GetComponent<CompanyVehicle>();}
 public bool Step(float dt){
  manager=DeliveryManager.Instance;if(vehicle==null||vehicle.Order==null||manager==null)return false;
  var order=vehicle.Order;if(activeOrder!=order.id){Cleanup();activeOrder=order.id;start=end=-1;retry=wait=stalled=0;}if(order.courierPhase>=5)return true;
  if(pedestrians==null)pedestrians=FindObjectsByType<PedestrianManager>(FindObjectsSortMode.None).FirstOrDefault(p=>p.gameObject.scene==gameObject.scene);
  if(pedestrians==null||pedestrians.graph==null){Diagnostic="Sem grafo de passeio";return false;}
  if(start<0){if(Time.time<retry)return false;retry=Time.time+2;if(!Plan())return false;}
  if(actor==null){
   if(pedestrians.models.Length==0)return false;
   // Distant operations retain the real lightweight navigation/collider. The
   // character model is allocated once per vehicle, only when observable.
   var go=new GameObject("Entregador "+vehicle.id);go.transform.SetParent(pedestrians.transform,false);
   actor=go.GetComponent<PedestrianAgent>();if(actor==null)actor=go.AddComponent<PedestrianAgent>();
   int restoredNode=order.courierInitialized&&order.courierNode>=0&&order.courierNode<pedestrians.graph.nodes.Length?order.courierNode:start;
   actor.Initialize(pedestrians,restoredNode,1.2f);actor.ExternalControl=true;
   ((List<PedestrianAgent>)pedestrians.Agents).Add(actor);
   if(order.courierInitialized){actor.GetComponent<Rigidbody>().position=order.courierPosition;actor.transform.position=order.courierPosition;}
   else{actor.GetComponent<Rigidbody>().position=exit;actor.transform.position=exit;order.courierPosition=exit;order.courierInitialized=true;}
   last=actor.transform.position;
   box=GridShiftDeliveryAccess.Package(actor.transform);box.transform.localPosition=new Vector3(0,1.05f,.38f);
   int destination=order.courierPhase>=2?start:end;
   if((order.courierPhase==0||order.courierPhase==1||order.courierPhase==3)&&destination!=restoredNode&&!actor.WalkTo(destination)){Diagnostic="Percurso do entregador indisponível";Cleanup();start=-1;return false;}
   if(order.courierPhase==0)order.courierPhase=1;box.SetActive(order.courierPhase==1);manager.Changed();
  }
  UpdateVisual();
  order.courierPosition=actor.transform.position;order.courierNode=actor.CurrentNode;
  if(order.courierPhase==1){
   Diagnostic="Entregador a caminho do recebimento";
   if(actor.CurrentNode==end&&actor.NextNode<0&&Vector3.Distance(actor.transform.position,receiving)<.15f){order.courierPhase=2;wait=0;box.SetActive(false);GridShiftAudio.Play("Cargo_Collected","SFX",.25f);manager.Changed();}
  }else if(order.courierPhase==2){
   Diagnostic="Confirmando entrega";wait+=dt;
   if(wait>=1){if(actor.WalkTo(start)){order.courierPhase=3;manager.Changed();}else{start=-1;Cleanup();}}
  }else if(order.courierPhase==3){
   Diagnostic="Entregador retornando ao veículo";
   if(actor.CurrentNode==start&&actor.NextNode<0&&Vector3.Distance(actor.transform.position,pedestrians.graph.nodes[start].position)<.15f){order.courierPhase=4;actor.enabled=false;manager.Changed();}
  }else if(order.courierPhase==4){
   actor.enabled=false;Diagnostic="Embarcando no veículo";
   var position=Vector3.MoveTowards(actor.transform.position,exit,1.2f*dt);
   if(Physics.Raycast(position+Vector3.up*.5f,Vector3.down,out var floor,1.5f,~(1<<9),QueryTriggerInteraction.Ignore))position.y=floor.point.y+.025f;
   if(pedestrians.CanStep(actor,actor.transform.position,position)&&pedestrians.Supported(position,actor.transform.position.y,true))actor.GetComponent<Rigidbody>().MovePosition(position);
   if(Vector3.Distance(actor.transform.position,exit)<.1f){order.courierPhase=5;Diagnostic="Descarga confirmada";Cleanup();return true;}
  }
  if(actor!=null){float moved=Vector3.Distance(last,actor.transform.position);last=actor.transform.position;stalled=moved>.005f?0:stalled+dt;
   if(stalled>8){stalled=0;actor.WalkTo(order.courierPhase>=2?start:end);Diagnostic="Recalculando percurso bloqueado";}
  }
  return false;
 }
 bool Plan(){
  if(!GridShiftDeliveryAccess.TryGet(vehicle.Target,out receiving)){Diagnostic="Recebimento sem piso validado";return false;}
  var nodes=pedestrians.graph.nodes;end=-1;float nearest=1.5f;
  for(int i=0;i<nodes.Length;i++){float d=Vector3.Distance(nodes[i].position,receiving);if(d<nearest){nearest=d;end=i;}}
  if(end<0){Diagnostic="Recebimento sem nó próximo: "+receiving;return false;}receiving=nodes[end].position;
  float halfWidth=vehicle.driver.BodySize.x*.5f+.65f;
  foreach(float side in new[]{1f,-1f}){
   var candidate=transform.position+transform.right*halfWidth*side;
   int anchor=-1;nearest=8;
   for(int i=0;i<nodes.Length;i++){float d=Vector3.Distance(candidate,nodes[i].position);if(d<nearest&&i!=end&&pedestrians.Supported(nodes[i].position,nodes[i].position.y,nodes[i].entrancePath)&&pedestrians.Clear(null,nodes[i].position,.65f)){nearest=d;anchor=i;}}
   if(anchor<0){Diagnostic="Sem passeio livre junto ao veículo: "+candidate;continue;}candidate.y=nodes[anchor].position.y;
   if(!Physics.Raycast(candidate+Vector3.up*.5f,Vector3.down,out var floor,1.5f,~(1<<9),QueryTriggerInteraction.Ignore)||floor.normal.y<.85f){Diagnostic="Saída sem piso";continue;}
   candidate.y=floor.point.y+.025f;
   if(!pedestrians.Supported(candidate,candidate.y,true)){Diagnostic="Saída sem apoio: "+candidate;continue;}
   if(!pedestrians.Clear(null,candidate,.65f)){Diagnostic="Saída ocupada: "+candidate;continue;}
   if(!ServiceLink(candidate,nodes[anchor].position)){Diagnostic="Ligação com passeio obstruída: "+candidate+" -> "+nodes[anchor].position;continue;}
   if(!pedestrians.Path(anchor,end,path,allowServiceEntrance:true)){Diagnostic="Sem caminho dirigido: "+anchor+" -> "+end;continue;}
   start=anchor;exit=candidate;return true;
  }
  return false;
 }
 bool ServiceLink(Vector3 from,Vector3 to){
  int samples=Mathf.Max(4,Mathf.CeilToInt(Vector3.Distance(from,to)/.15f));float height=from.y;
  for(int i=1;i<=samples;i++){
   var point=Vector3.Lerp(from,to,(float)i/samples);
   if(!Physics.Raycast(point+Vector3.up*.5f,Vector3.down,out var floor,1.5f,~(1<<9),QueryTriggerInteraction.Ignore)||floor.normal.y<.85f)return false;
   point.y=floor.point.y+.025f;if(Mathf.Abs(point.y-height)>.3f||!pedestrians.Supported(point,point.y,true)||!pedestrians.Clear(null,point,.65f))return false;height=point.y;
  }
  return true;
 }
 void UpdateVisual(){
  if(actor==null)return;bool near=pedestrians.player!=null&&pedestrians.player.gameObject.activeInHierarchy&&(pedestrians.player.position-actor.transform.position).sqrMagnitude<65*65;
  if(near&&visual==null){visual=Instantiate(pedestrians.models[0],actor.transform,false);visual.name="CourierVisual";foreach(var collider in visual.GetComponentsInChildren<Collider>(true))collider.enabled=false;foreach(var t in visual.GetComponentsInChildren<Transform>(true))t.gameObject.layer=9;actor.BindVisualAnimator();}
  if(visual!=null){if(visual.transform.parent!=actor.transform){visual.transform.SetParent(actor.transform,false);actor.BindVisualAnimator();}visual.SetActive(near);var animator=visual.GetComponent<Animator>();if(animator!=null)animator.cullingMode=near?AnimatorCullingMode.AlwaysAnimate:AnimatorCullingMode.CullUpdateTransforms;}
  if(box!=null)box.SetActive(near&&vehicle.Order!=null&&vehicle.Order.courierPhase==1);
  if(visual!=null){var pose=visual.GetComponent<GridShiftCourierCarryPose>();if(pose==null)pose=visual.AddComponent<GridShiftCourierCarryPose>();pose.package=box!=null?box.transform:null;pose.actor=actor.transform;}
 }
 void Cleanup(){if(visual!=null){visual.transform.SetParent(transform,false);visual.SetActive(false);}if(actor!=null){((List<PedestrianAgent>)pedestrians.Agents).Remove(actor);Destroy(actor.gameObject);}actor=null;box=null;}
 void OnDestroy(){Cleanup();if(visual!=null)Destroy(visual);}
}

// A compatible humanoid rig can support the parcel without introducing a new
// animation controller. Legs retain the existing walk; the arm pose is visual.
public sealed class GridShiftCourierCarryPose:MonoBehaviour {
 public Transform package,actor;Animator animator;Transform[] rig;
 public bool Applied{get;private set;}
 void LateUpdate(){ApplyPose();}
 public void ApplyPose(){
  Applied=false;
  if(package==null||!package.gameObject.activeInHierarchy||actor==null)return;
  if(animator==null)animator=GetComponentInChildren<Animator>();if(animator==null)return;
  Arm(HumanBodyBones.LeftUpperArm,HumanBodyBones.LeftLowerArm,HumanBodyBones.LeftHand,-1);
  Arm(HumanBodyBones.RightUpperArm,HumanBodyBones.RightLowerArm,HumanBodyBones.RightHand,1);
 }
 void Arm(HumanBodyBones upperBone,HumanBodyBones lowerBone,HumanBodyBones handBone,float side){
  var upper=Bone(upperBone);var lower=Bone(lowerBone);var hand=Bone(handBone);if(upper==null||lower==null||hand==null)return;
  var target=package.position+actor.right*(side*.145f)-actor.up*.015f;
  float a=Vector3.Distance(upper.position,lower.position),b=Vector3.Distance(lower.position,hand.position);var delta=target-upper.position;
  if(a<.001f||b<.001f||delta.sqrMagnitude<.00001f)return;
  float distance=Mathf.Clamp(delta.magnitude,Mathf.Abs(a-b)+.001f,a+b-.001f);var direction=delta.normalized;
  var bend=Vector3.ProjectOnPlane(-actor.up*.7f-actor.forward*.2f+actor.right*(side*.4f),direction).normalized;
  float cosine=Mathf.Clamp((a*a+distance*distance-b*b)/(2*a*distance),-1,1);
  var elbow=upper.position+direction*(a*cosine)+bend*(a*Mathf.Sqrt(1-cosine*cosine));
  upper.rotation=Quaternion.FromToRotation(lower.position-upper.position,elbow-upper.position)*upper.rotation;
  lower.rotation=Quaternion.FromToRotation(hand.position-lower.position,target-lower.position)*lower.rotation;
  Applied=true;
 }
 Transform Bone(HumanBodyBones bone){
  if(rig==null){rig=GetComponentsInChildren<SkinnedMeshRenderer>(true).SelectMany(s=>s.bones).Where(t=>t!=null).Distinct().ToArray();if(rig.Length==0)rig=GetComponentsInChildren<Transform>(true);}
  string name=bone==HumanBodyBones.LeftUpperArm?"LeftArm":bone==HumanBodyBones.LeftLowerArm?"LeftForeArm":bone==HumanBodyBones.LeftHand?"LeftHand":bone==HumanBodyBones.RightUpperArm?"RightArm":bone==HumanBodyBones.RightLowerArm?"RightForeArm":"RightHand";
  foreach(var part in rig)if(part.name==name)return part;return animator.isHuman?animator.GetBoneTransform(bone):null;
 }
}
