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
 readonly Queue<int> crossingApproach=new Queue<int>();bool approachPlanned;int approachGoal=-1;
 readonly Queue<int> replacementPath=new Queue<int>();
 static readonly int SpeedParameter=Animator.StringToHash("Speed");
 int current,next=-1;float speed,pause;PedestrianCrossing crossing;
 Vector3 lastPosition;
 bool crossingRetreat,crossingCommitted;float approachBlocked,approachRetryAt;
 bool yielding;Vector3 yieldTarget;float yieldTime;
 Vector3 yieldDirection;int yieldStage;
 int goal=-1;float blockedTime,replanAt;
 readonly Queue<int> recentDestinations=new Queue<int>();
 int recoveryAttempts;float actualSpeed,currentVelocity;
 public Collider Obstacle {get;set;}
 public int DestinationNode=>goal;
 public int NextNode=>next;
 public float NoProgressSeconds=>next<0?0:Mathf.Max(0,Time.time-progressAt);
 public int RecoveryAttempts=>recoveryAttempts;
 public float CurrentSpeed=>currentVelocity;
 public IEnumerable<int> RecentDestinations=>recentDestinations;
 int progressNode=-1;float bestDistance,progressAt;
 public void Initialize(PedestrianManager manager,int node,float walkSpeed){
  Manager=manager;current=node;next=-1;path.Clear();speed=walkSpeed;pause=Random.Range(.5f,3);activity=Activity.Idle;yielding=false;crossingRetreat=crossingCommitted=false;approachBlocked=0;approachPlanned=false;crossingApproach.Clear();recentDestinations.Clear();recoveryAttempts=0;actualSpeed=0;goal=-1;blockedTime=0;replanAt=0;progressNode=-1;
  if(body==null){
   foreach(var c in GetComponentsInChildren<Collider>())c.enabled=false;
   body=GetComponent<Rigidbody>();if(body==null)body=gameObject.AddComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;
   capsule=gameObject.AddComponent<CapsuleCollider>();capsule.center=Vector3.up*.95f;capsule.height=1.8f;capsule.radius=.28f;
   animator=GetComponentInChildren<Animator>();if(animator!=null){animator.runtimeAnimatorController=manager.controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.CullUpdateTransforms;}
  }
  foreach(var t in GetComponentsInChildren<Transform>())t.gameObject.layer=9;
  body.position=transform.position=manager.graph.nodes[node].position;lastPosition=transform.position;
 }
 void OnDisable(){Manager?.LeaveCrossingQueue(this);if(crossing!=null)crossing.junction.LeaveCrossing(this);crossing=null;}
 public bool WalkTo(int target){if(crossingCommitted)return false;Manager?.LeaveCrossingQueue(this);crossing=null;path.Clear();next=-1;pause=0;return Destination(target);}
 bool Destination(int target=-1){
  var nodes=Manager.graph.nodes;
  if(target>=0){goal=target;return Manager.Path(current,target,path);}
  for(int attempt=0;attempt<12;attempt++){
   int candidate=Random.Range(0,nodes.Length);
   if(Random.value<Manager.buildingProbability&&Manager.Entrances.Count>0)candidate=Manager.Entrances[Random.Range(0,Manager.Entrances.Count)];
   if(candidate==current||recentDestinations.Contains(candidate)||nodes[candidate].neighbours.Length==0||(!nodes[candidate].entrance&&Vector3.Distance(nodes[current].position,nodes[candidate].position)<12))continue;
   if(!Manager.Path(current,candidate,path))continue;
   goal=candidate;recentDestinations.Enqueue(candidate);while(recentDestinations.Count>6)recentDestinations.Dequeue();recoveryAttempts=0;return true;
  }
  return false;
 }

 void FixedUpdate(){
  if(Manager==null)return;float dt=Time.fixedDeltaTime;
  // The animator and footstep observers consume displacement, not requested speed.
  float measured=Vector3.ProjectOnPlane(body.position-lastPosition,Vector3.up).magnitude;
  DistanceWalked+=measured;lastPosition=body.position;
  Animate(measured/Mathf.Max(dt,.0001f));
  // A waiting person can occupy a graph waypoint without blocking the whole sidewalk.
  // Continue to the following waypoint only when the entire bypass is supported and clear.
  if(!yielding&&crossing==null&&next>=0&&path.Count>0&&!Manager.graph.nodes[next].entrance&&Manager.graph.Crossing(current,next)<0&&Manager.graph.Crossing(next,path.Peek())<0&&Vector3.Distance(body.position,Manager.graph.nodes[next].position)<1.6f&&!Manager.Clear(this,Manager.graph.nodes[next].position,Manager.separation)&&Manager.SidewalkSegment(this,body.position,Manager.graph.nodes[path.Peek()].position)){
   current=next;next=path.Dequeue();progressNode=-1;
  }
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
   if(moving&&Manager.Supported(step,body.position.y)&&Manager.CanStep(this,body.position,step)){body.MoveRotation(Quaternion.RotateTowards(body.rotation,Quaternion.LookRotation(Vector3.ProjectOnPlane(step-body.position,Vector3.up)),120*dt));body.MovePosition(step);yieldTime-=dt;}
   else {Animate(0);yieldTime-=dt;}
   if(yieldTime<=0)yielding=false;return;
  }
  if(pause>0){pause-=dt;Animate(0);return;}
  if(next<0){if(path.Count==0&&!Destination()){pause=1;Animate(0);return;}next=path.Dequeue();int index=Manager.graph.Crossing(current,next);
   if(index>=0)crossing=Manager.graph.crossings[index];}
  if(crossing!=null&&!crossingCommitted){
   activity=Activity.WaitingToCross;
   bool turn=Manager.CrossingTurn(this,crossing,current,out var waitingNode);
   var departure=Manager.graph.nodes[current].position;
   bool near=Vector3.ProjectOnPlane(body.position-departure,Vector3.up).magnitude<.07f;
   bool join=crossing.junction.CanJoin(this,current,next);
   if(turn&&near&&((crossing.CanStart&&crossing.junction.PedestrianPhaseOpen)||join)&&Manager.CrossingClear(this,current,next)&&crossing.junction.TryCross(this,current,next)){
    crossingCommitted=true;crossingRetreat=false;approachBlocked=0;approachPlanned=false;crossingApproach.Clear();Manager.LeaveCrossingQueue(this);activity=Activity.Crossing;
   }else{
    // Follow the verified sidewalk to the departure anchor before reserving the road.
    // Other queue members wait in separate slots, clear of both crossing exits.
    int targetNode=turn&&(!crossing.junction.PedestrianOccupied||join)?current:waitingNode;
    if(approachGoal!=targetNode){approachGoal=targetNode;approachPlanned=false;crossingApproach.Clear();}
    if(!approachPlanned&&Time.time>=approachRetryAt){approachRetryAt=Time.time+.75f;approachPlanned=Manager.CrossingApproach(this,targetNode,crossingApproach);}
    while(crossingApproach.Count>0&&Vector3.ProjectOnPlane(body.position-Manager.graph.nodes[crossingApproach.Peek()].position,Vector3.up).magnitude<.07f)crossingApproach.Dequeue();
    var targetWait=approachPlanned?(crossingApproach.Count>0?Manager.graph.nodes[crossingApproach.Peek()].position:Manager.graph.nodes[targetNode].position):body.position;
    if(!approachPlanned)approachBlocked+=dt;
    var deltaWait=targetWait-body.position;deltaWait.y=0;
    if(deltaWait.magnitude>.05f){var step=Vector3.MoveTowards(body.position,targetWait,speed*dt);
     if(Manager.Supported(step,body.position.y)&&Manager.CanStep(this,body.position,step)){
      body.MoveRotation(Quaternion.RotateTowards(body.rotation,Quaternion.LookRotation(deltaWait),180*dt));body.MovePosition(step);approachBlocked=0;return;
     }
     approachBlocked+=dt;

     if(approachBlocked>1){approachPlanned=false;crossingApproach.Clear();}
    }
    if(approachBlocked>5){Manager.LeaveCrossingQueue(this);crossing=null;crossingCommitted=false;path.Clear();next=-1;goal=-1;pause=1;approachBlocked=0;approachPlanned=false;crossingApproach.Clear();}
    Animate(0);return;
   }
  }

  Vector3 target=Manager.graph.nodes[next].position;Vector3 delta=target-body.position;delta.y=0;
  if(delta.magnitude<.08f){
   current=next;next=-1;
   if(crossing!=null){crossing.junction.LeaveCrossing(this);Manager.LeaveCrossingQueue(this);if(!crossingRetreat)CrossingsCompleted++;crossing=null;crossingRetreat=crossingCommitted=false;activity=Activity.Walking;}
   if(path.Count==0){
    if(Manager.ResidenceAllowed(current)){activity=Activity.EnteringBuilding;StartCoroutine(EnterBuilding());return;}
    pause=Random.Range(Manager.destinationPause.x,Manager.destinationPause.y);activity=Activity.Idle;
   }
   Animate(0);return;
  }
  Vector3 direction=delta.normalized;
  // Deslocamento lateral pequeno dentro do corredor permite pedestres em sentidos opostos.
  float angle=Vector3.Angle(body.rotation*Vector3.forward,direction);
  float turnSpeed=speed*Mathf.Lerp(.12f,1,Mathf.Clamp01(1-angle/100));
  actualSpeed=Mathf.MoveTowards(actualSpeed,turnSpeed,2*dt);
  float travelSpeed=actualSpeed*Mathf.Lerp(.05f,1,Mathf.Clamp01((110-angle)/70));
  Vector3 desired=Vector3.MoveTowards(body.position,target,Mathf.Min(travelSpeed*dt,delta.magnitude));
  if(Physics.Raycast(desired+Vector3.up*1.5f,Vector3.down,out var floor,3,~(1<<9),QueryTriggerInteraction.Ignore)) {
      bool entrancePath=Manager.graph.nodes[current].entrancePath||Manager.graph.nodes[next].entrancePath;
      if(floor.normal.y<.8f||(!entrancePath&&!floor.collider.name.StartsWith("Road "))||floor.collider.GetComponentInParent<TrafficCar>()!=null||Mathf.Abs(floor.point.y+.025f-body.position.y)>.4f||(crossing==null&&!Manager.Supported(desired,body.position.y,entrancePath))) { Blocked(direction,dt);return; }
      desired.y=floor.point.y+.025f;
  } else { Blocked(direction,dt);return; }
  bool playerApproaching=Manager.player!=null&&Vector3.Distance(Manager.player.position,body.position)<(Manager.player.GetComponent<PlayerMovement>()?.IsRunning==true?2.4f:1.5f)&&Vector3.Dot(Manager.player.position-body.position,direction)>0;
  Vector3 avoided=desired;bool crowdStep=crossing==null&&Manager.CrowdStep(this,target,Mathf.Min(speed*dt,delta.magnitude),out avoided);
  if(crowdStep){desired=avoided;direction=(desired-body.position).normalized;travelSpeed=speed;}
  if(!crowdStep&&crossing==null&&playerApproaching&&Manager.TryYield(this,direction,out yieldTarget)){BeginYield(direction);Animate(0);return;}
  if(!Manager.CanStep(this,body.position,desired)){
   Blocked(direction,dt);return;
  }
  var rotation=Quaternion.RotateTowards(body.rotation,Quaternion.LookRotation(direction),180*dt);
  body.MoveRotation(rotation);body.MovePosition(desired);blockedTime=0;activity=crossing==null?Activity.Walking:Activity.Crossing;
 }
 void Blocked(Vector3 direction,float dt){Animate(0);blockedTime+=dt;if(crossing!=null){
   if(blockedTime>3&&!crossingRetreat){var back=Manager.graph.nodes[current].position-body.position;back.y=0;var step=body.position+back.normalized*.15f;if(back.sqrMagnitude>.01f&&Manager.Clear(this,step,Manager.separation)){next=current;path.Clear();goal=-1;crossingRetreat=true;blockedTime=0;}}
   return;
  }
  if(Manager.TryYield(this,direction,out yieldTarget)){BeginYield(direction);return;}
  if(blockedTime<2||Time.time<replanAt)return;replanAt=Time.time+3;
  // A nova rota parte do último nó alcançado; o retorno também é caminhado e validado.
  Replan();
 }
 void Replan(){
  replanAt=Time.time+3;int blocked=next;replacementPath.Clear();recoveryAttempts++;
  bool found=goal>=0&&recoveryAttempts<=3&&Manager.Path(current,goal,replacementPath,current,blocked);
  if(!found){path.Clear();found=Destination();if(found){foreach(int n in path)replacementPath.Enqueue(n);}}
  if(found){path.Clear();foreach(int n in replacementPath)path.Enqueue(n);next=current;yielding=false;progressNode=-1;}
  else {pause=3;progressAt=Time.time;}
 }

 void BeginYield(Vector3 direction){yielding=true;yieldTime=1.4f;yieldDirection=direction;yieldStage=0;}
 void Animate(float velocity){currentVelocity=velocity;if(animator!=null){animator.SetFloat(SpeedParameter,velocity,.15f,Time.fixedDeltaTime);animator.speed=Mathf.Lerp(1,speed/1.25f,Mathf.Clamp01(velocity));}}
 System.Collections.IEnumerator EnterBuilding(){
  enabled=false;Animate(0);
  Transform door=null;float nearest=2;
  foreach(Transform t in Manager.graph.transform)if(t.name.StartsWith("Entrance_")){float d=Vector3.Distance(t.position,body.position);if(d<nearest){nearest=d;door=t;}}
  float end=Time.time+.65f;
  while(Time.time<end){if(door!=null)body.rotation=Quaternion.RotateTowards(body.rotation,door.rotation,120*Time.deltaTime);yield return null;}
  // Wait briefly for a concealed transition; a visible door stays occupied only briefly.
  end=Time.time+4;
  while(Manager!=null&&Manager.Visible(transform.position)&&Time.time<end)yield return new WaitForSeconds(.3f);
  if(Manager==null)yield break;
  if(!Manager.Visible(transform.position)){Manager.Recycle(this);yield break;}
  activity=Activity.Idle;pause=1;enabled=true;Destination();
 }
}
