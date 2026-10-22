using System.Collections.Generic;
using UnityEngine;
public class PedestrianAgent:MonoBehaviour {
 public enum Activity { Idle,Walking,WaitingToCross,Crossing,EnteringBuilding }
 [SerializeField] Activity activity;
 public Activity CurrentActivity=>activity;
 public float DistanceWalked {get;private set;}
 public int CrossingsCompleted {get;private set;}
 public PedestrianManager Manager {get;private set;}
 Animator animator;Rigidbody body;CapsuleCollider capsule;
 readonly Queue<int> path=new Queue<int>();
 int current,next=-1;float speed,pause;PedestrianCrossing crossing;
 Vector3 lastPosition;
 bool yielding;Vector3 yieldTarget;float yieldTime;
 Vector3 yieldDirection;int yieldStage;
 int goal=-1;float blockedTime,replanAt;
 int progressNode=-1;float bestDistance,progressAt;
 public void Initialize(PedestrianManager manager,int node,float walkSpeed){
  Manager=manager;current=node;next=-1;path.Clear();speed=walkSpeed;pause=Random.Range(.5f,3);activity=Activity.Idle;yielding=false;goal=-1;blockedTime=0;replanAt=0;progressNode=-1;
  if(body==null){
   foreach(var c in GetComponentsInChildren<Collider>())c.enabled=false;
   body=GetComponent<Rigidbody>();if(body==null)body=gameObject.AddComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;
   capsule=gameObject.AddComponent<CapsuleCollider>();capsule.center=Vector3.up*.95f;capsule.height=1.8f;capsule.radius=.28f;
   animator=GetComponentInChildren<Animator>();if(animator!=null){animator.runtimeAnimatorController=manager.controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;}
  }
  foreach(var t in GetComponentsInChildren<Transform>())t.gameObject.layer=9;
  body.position=transform.position=manager.graph.nodes[node].position;lastPosition=transform.position;
 }
 void OnDisable(){if(crossing!=null)crossing.junction.LeaveCrossing(this);crossing=null;}
 public bool WalkTo(int target){if(crossing!=null)return false;path.Clear();next=-1;pause=0;return Destination(target);}
 bool Destination(int target=-1){
  var nodes=Manager.graph.nodes;
  if(target<0){target=Random.Range(0,nodes.Length);if(Random.value<Manager.buildingProbability){var homes=Manager.Entrances;if(homes.Count>0)target=homes[Random.Range(0,homes.Count)];}}
  goal=target;return Manager.Path(current,target,path);
 }
 void FixedUpdate(){
  if(Manager==null)return;float dt=Time.fixedDeltaTime;
  if(next>=0&&crossing==null){float distance=Vector3.Distance(body.position,Manager.graph.nodes[next].position);
   if(progressNode!=next){progressNode=next;bestDistance=distance;progressAt=Time.time;}
   if(distance<bestDistance-.1f){bestDistance=distance;progressAt=Time.time;}
   if(Time.time-progressAt>3&&Time.time>=replanAt)Replan();
  }
  if(yielding){
   var step=Vector3.MoveTowards(body.position,yieldTarget,speed*dt);
   bool moving=Vector3.Distance(body.position,yieldTarget)>.05f;
   if(!moving&&yieldStage==0&&crossing==null){var ahead=body.position+yieldDirection*2.4f;if(Manager.SidewalkSegment(this,body.position,ahead)){yieldTarget=ahead;yieldStage=1;yieldTime=3;progressAt=Time.time;return;}}
   if(!moving&&yieldStage==1){yielding=false;
    // Após contornar o obstáculo, retoma o corredor sem voltar contra o poste.
    while(next>=0&&path.Count>0&&Vector3.Dot(Manager.graph.nodes[next].position-body.position,yieldDirection)<-.1f&&Manager.graph.Crossing(next,path.Peek())<0&&Manager.SidewalkSegment(this,body.position,Manager.graph.nodes[path.Peek()].position)){current=next;next=path.Dequeue();progressNode=-1;}
    return;
   }
   if(moving&&Manager.Supported(step,body.position.y)&&Manager.Clear(this,step,Manager.separation)){body.MovePosition(step);Animate(speed);yieldTime-=dt;}
   else {Animate(0);yieldTime-=dt;}
   if(yieldTime<=0)yielding=false;return;
  }
  if(pause>0){pause-=dt;Animate(0);return;}
  if(next<0){if(path.Count==0&&!Destination()){pause=1;Animate(0);return;}next=path.Dequeue();int index=Manager.graph.Crossing(current,next);
   if(index>=0)crossing=Manager.graph.crossings[index];}
  if(crossing!=null&&activity!=Activity.Crossing){
   activity=Activity.WaitingToCross;
   if(!crossing.CanStart||!crossing.junction.TryCross(this)){
    // Aguarda atrás da borda; deixa a chegada livre para quem atravessa no sentido oposto.
    Vector3 away=(Manager.graph.nodes[current].position-Manager.graph.nodes[next].position).normalized;
    Vector3 waiting=Manager.graph.nodes[current].position+away*.9f;
    Vector3 step=Vector3.MoveTowards(body.position,waiting,speed*dt);
    bool moving=Vector3.Distance(step,body.position)>.001f&&Manager.Supported(step,body.position.y)&&Manager.Clear(this,step,Manager.separation);
    if(moving){body.MoveRotation(Quaternion.RotateTowards(body.rotation,Quaternion.LookRotation(away),180*dt));body.MovePosition(step);}
    else if(Vector3.Distance(waiting,body.position)>.05f&&Manager.TryYield(this,away,out yieldTarget)){BeginYield(away);}
    Animate(moving?speed:0);return;
   }
   activity=Activity.Crossing;
  }
  Vector3 target=Manager.graph.nodes[next].position;Vector3 delta=target-body.position;delta.y=0;
  if(delta.magnitude<.08f){
   current=next;next=-1;
   if(crossing!=null){crossing.junction.LeaveCrossing(this);crossing=null;CrossingsCompleted++;}
   if(path.Count==0){
    if(Manager.ResidenceAllowed(current)){activity=Activity.EnteringBuilding;StartCoroutine(EnterBuilding());return;}
    pause=Random.Range(Manager.destinationPause.x,Manager.destinationPause.y);activity=Activity.Idle;
   }
   Animate(0);return;
  }
  Vector3 direction=delta.normalized;
  // Deslocamento lateral pequeno dentro do corredor permite pedestres em sentidos opostos.
  Vector3 desired=Vector3.MoveTowards(body.position,target,Mathf.Min(speed*dt,delta.magnitude));
  if(Physics.Raycast(desired+Vector3.up*1.5f,Vector3.down,out var floor,3,~(1<<9),QueryTriggerInteraction.Ignore)) {
      bool entrancePath=Manager.graph.nodes[current].entrancePath||Manager.graph.nodes[next].entrancePath;
      if(floor.normal.y<.8f||(!entrancePath&&!floor.collider.name.StartsWith("Road "))||floor.collider.GetComponentInParent<TrafficCar>()!=null||Mathf.Abs(floor.point.y+.025f-body.position.y)>.4f||(crossing==null&&!Manager.Supported(desired,body.position.y,entrancePath))) { Blocked(direction,dt);return; }
      desired.y=floor.point.y+.025f;
  } else { Blocked(direction,dt);return; }
  bool playerApproaching=Manager.player!=null&&Vector3.Distance(Manager.player.position,body.position)<(Manager.player.GetComponent<PlayerMovement>()?.IsRunning==true?2.4f:1.5f)&&Vector3.Dot(Manager.player.position-body.position,direction)>0;
  if(crossing==null&&playerApproaching&&Manager.TryYield(this,direction,out yieldTarget)){BeginYield(direction);Animate(0);return;}
  if(!Manager.Clear(this,desired,Manager.separation)){
   Blocked(direction,dt);return;
  }
  var rotation=Quaternion.RotateTowards(body.rotation,Quaternion.LookRotation(direction),180*dt);
  body.MoveRotation(rotation);body.MovePosition(desired);blockedTime=0;activity=crossing==null?Activity.Walking:Activity.Crossing;
  DistanceWalked+=Vector3.Distance(lastPosition,desired);lastPosition=desired;Animate(speed);
 }
 void Blocked(Vector3 direction,float dt){Animate(0);blockedTime+=dt;if(crossing!=null)return;
  if(Manager.TryYield(this,direction,out yieldTarget)){BeginYield(direction);return;}
  if(blockedTime<2||Time.time<replanAt)return;replanAt=Time.time+3;
  // A nova rota parte do último nó alcançado; o retorno também é caminhado e validado.
  Replan();
 }
 void Replan(){replanAt=Time.time+3;int blocked=next;var replacement=new Queue<int>();if(goal>=0&&Manager.Path(current,goal,replacement,current,blocked)){path.Clear();foreach(int n in replacement)path.Enqueue(n);next=current;yielding=false;progressNode=-1;}}
 void BeginYield(Vector3 direction){yielding=true;yieldTime=1.4f;yieldDirection=direction;yieldStage=0;}
 void Animate(float velocity){if(animator!=null){animator.SetFloat("Speed",velocity,.15f,Time.fixedDeltaTime);animator.speed=Mathf.Lerp(1,speed/1.25f,Mathf.Clamp01(velocity));}}
 System.Collections.IEnumerator EnterBuilding(){
  enabled=false;Animate(0);yield return new WaitForSeconds(.6f);
  // A remoção só ocorre fora da visão: nunca atravessa a fachada ou desaparece à vista.
  while(Manager!=null&&Manager.Visible(transform.position))yield return new WaitForSeconds(.5f);
  if(Manager!=null)Manager.Recycle(this);
 }
}
