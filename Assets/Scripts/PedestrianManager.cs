using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PedestrianManager:MonoBehaviour {
 public PedestrianGraph graph;
 public GameObject[] models;
 public RuntimeAnimatorController controller;
 public Transform player;
 public Camera playerCamera;
 [Min(1)] public int maximum=50;
 [Min(1)] public int daytimeCount=28;
 [Range(.05f,2)] public float density=1;
 public Vector2 speeds=new Vector2(.9f,1.5f),destinationPause=new Vector2(2,7);
 [Range(.01f,.5f)] public float buildingProbability=.12f;
 [Range(.4f,1)] public float separation=.62f;
 readonly List<PedestrianAgent> active=new List<PedestrianAgent>();readonly Queue<PedestrianAgent> pool=new Queue<PedestrianAgent>();
 readonly Collider[] hits=new Collider[32];readonly Plane[] planes=new Plane[6];
 public readonly List<int> Entrances=new List<int>();Transform poolRoot;
 public CompanyEntrance[] excludedEntrances=new CompanyEntrance[0];
 int[] previous,searchQueue;readonly Stack<int> reversePath=new Stack<int>();
 sealed class CrossingWait { public PedestrianCrossing crossing;public long ticket;public int node;public bool approaching;public float bestDistance=float.PositiveInfinity,progressAt; }
 readonly Dictionary<PedestrianAgent,CrossingWait> crossingWaits=new Dictionary<PedestrianAgent,CrossingWait>();long crossingTicket;
 static readonly float[] steeringAngles={0f,30f,-30f,60f,-60f,90f,-90f,120f,-120f,160f,-160f};
 public void LeaveCrossingQueue(PedestrianAgent agent){crossingWaits.Remove(agent);}
 public bool CrossingTurn(PedestrianAgent agent,PedestrianCrossing crossing,int from,out int waitingNode){
  if(!crossingWaits.TryGetValue(agent,out var slot)){
   var departure=graph.nodes[from].position;int other=crossing.a==from?crossing.b:crossing.a;var away=(departure-graph.nodes[other].position).normalized;var inner=Vector3.ProjectOnPlane(departure-crossing.junction.transform.position,away).normalized;
   waitingNode=from;float best=float.PositiveInfinity;
   for(int node=0;node<graph.nodes.Length;node++){
    var n=graph.nodes[node];var candidate=n.position;var delta=candidate-departure;
    if(n.entrancePath||n.neighbours.Length==0||delta.sqrMagnitude>100||Vector3.Dot(delta,away)<1.1f||Vector3.Dot(delta,inner)<-.1f||Vector3.Dot(delta,inner)>2.1f||Mathf.Abs(delta.y)>.1f)continue;
    bool exit=false;foreach(var c in graph.crossings)if(c.junction==crossing.junction&&((graph.nodes[c.a].position-candidate).sqrMagnitude<1.21f||(graph.nodes[c.b].position-candidate).sqrMagnitude<1.21f)){exit=true;break;}if(exit)continue;
    bool reserved=false;foreach(var s in crossingWaits.Values)if((graph.nodes[s.node].position-candidate).sqrMagnitude<.8f*.8f){reserved=true;break;}
    if(reserved||!Clear(agent,candidate,separation)||!Supported(candidate,departure.y))continue;
    float score=(Vector3.Dot(delta,inner)<.8f?10:0)+Mathf.Abs(delta.magnitude-2.5f)+(agent.transform.position-candidate).sqrMagnitude*.1f;
    if(score<best){best=score;waitingNode=node;}
   }
   slot=new CrossingWait{crossing=crossing,ticket=++crossingTicket,node=waitingNode};crossingWaits[agent]=slot;
  }
  // A head which cannot approach its departure must not indefinitely hold
  // every other crossing. Rotate only a stalled approach, never a person on the road.
  float remaining=Vector3.ProjectOnPlane(agent.transform.position-graph.nodes[from].position,Vector3.up).magnitude;
  if(slot.approaching){if(remaining<slot.bestDistance-.12f){slot.bestDistance=remaining;slot.progressAt=Time.time;}if(remaining>.15f&&Time.time-slot.progressAt>6&&!crossing.junction.PedestrianOccupied){slot.ticket=++crossingTicket;slot.approaching=false;slot.bestDistance=float.PositiveInfinity;}}
  waitingNode=slot.node;int to=crossing.a==from?crossing.b:crossing.a;if(crossing.junction.CanJoin(agent,from,to))return true;
  if(!CrossingClear(agent,from,to))return false;
  foreach(var pair in crossingWaits)if(pair.Key!=agent&&pair.Key!=null&&pair.Key.isActiveAndEnabled&&pair.Value.crossing.junction==crossing.junction&&pair.Value.ticket<slot.ticket){
   var prior=pair.Value.crossing;int priorTo=pair.Key.NextNode,priorFrom=prior.a==priorTo?prior.b:prior.a;
   // A blocked landing must not prevent a different, clear crossing from advancing.
   if(priorTo>=0&&CrossingClear(pair.Key,priorFrom,priorTo))return false;
  }
  if(!slot.approaching){slot.approaching=true;slot.bestDistance=remaining;slot.progressAt=Time.time;}return true;
 }
 public bool CrossingClear(PedestrianAgent agent,int from,int to){
  var a=graph.nodes[from].position;var b=graph.nodes[to].position;
  if(!Clear(agent,b,separation+.2f))return false;
  return !Physics.CapsuleCast(a+Vector3.up*.6f,a+Vector3.up*1.55f,.29f,(b-a).normalized,Vector3.Distance(a,b),~(1<<9),QueryTriggerInteraction.Ignore);
 }
 public bool ResidenceAllowed(int index){if(!graph.nodes[index].entrance)return false;foreach(var door in excludedEntrances)if(door!=null&&Vector3.ProjectOnPlane(graph.nodes[index].position-door.transform.position,Vector3.up).sqrMagnitude<25)return false;return true;}
 public bool Path(int from,int target,Queue<int> path,int blockedFrom=-1,int blockedTo=-1,bool sidewalkOnly=false,PedestrianAgent avoidAgent=null,bool allowServiceEntrance=false){
  var nodes=graph.nodes;if(target<0||target>=nodes.Length||from<0||from>=nodes.Length)return false;
  if(nodes[target].entrance&&!ResidenceAllowed(target)&&!allowServiceEntrance)return false;
  if(previous==null||previous.Length!=nodes.Length){previous=new int[nodes.Length];searchQueue=new int[nodes.Length];}
  for(int i=0;i<previous.Length;i++)previous[i]=-1;int first=0,last=0;searchQueue[last++]=from;previous[from]=from;
  while(first<last&&previous[target]<0){int node=searchQueue[first++];foreach(int other in nodes[node].neighbours){if(sidewalkOnly&&graph.Crossing(node,other)>=0)continue;if((node==blockedFrom&&other==blockedTo)||(node==blockedTo&&other==blockedFrom))continue;if(previous[other]>=0)continue;if(avoidAgent!=null&&(nodes[other].position-nodes[target].position).sqrMagnitude>100)continue;if(avoidAgent!=null&&!Clear(avoidAgent,nodes[other].position,separation+.03f))continue;
   if(blockedFrom>=0&&(nodes[node].position-nodes[blockedFrom].position).sqrMagnitude<36){var delta=nodes[other].position-nodes[node].position;if(Physics.CapsuleCast(nodes[node].position+Vector3.up*.6f,nodes[node].position+Vector3.up*1.55f,.29f,delta.normalized,delta.magnitude,~(1<<9),QueryTriggerInteraction.Ignore))continue;}
   previous[other]=node;searchQueue[last++]=other;}}
  if(previous[target]<0||target==from)return false;reversePath.Clear();for(int node=target;node!=from;node=previous[node])reversePath.Push(node);path.Clear();while(reversePath.Count>0)path.Enqueue(reversePath.Pop());return true;
 }
 public bool CrossingApproach(PedestrianAgent self,int departure,Queue<int> path){
  path.Clear();var candidates=new List<int>();
  for(int i=0;i<graph.nodes.Length;i++)if(!graph.nodes[i].entrancePath&&(graph.nodes[i].position-self.transform.position).sqrMagnitude<16)candidates.Add(i);
  candidates.Sort((a,b)=>(graph.nodes[a].position-self.transform.position).sqrMagnitude.CompareTo((graph.nodes[b].position-self.transform.position).sqrMagnitude));
  int count=0;foreach(int start in candidates){if(count++>=16)break;if(!SidewalkSegment(self,self.transform.position,graph.nodes[start].position))continue;
   if(start==departure){path.Enqueue(start);return true;}
   if(Path(start,departure,path,departure,-1,true,self)){var tail=path.ToArray();path.Clear();path.Enqueue(start);foreach(int node in tail)path.Enqueue(node);return true;}
  }return false;
 }
 public bool Supported(Vector3 point,float expectedY,bool entrance=false){
  // Verifica a pegada inteira, não apenas um raio no centro junto à borda.
  for(int i=0;i<5;i++){Vector3 offset=i==0?Vector3.zero:i==1?Vector3.right*.24f:i==2?Vector3.left*.24f:i==3?Vector3.forward*.24f:Vector3.back*.24f;
   if(!Physics.Raycast(point+offset+Vector3.up,Vector3.down,out var hit,2,~(1<<9),QueryTriggerInteraction.Ignore)||hit.normal.y<.85f||(!entrance&&!hit.collider.name.StartsWith("Road "))||Mathf.Abs(hit.point.y+.025f-expectedY)>(entrance?.35f:.11f))return false;
  }return true;
 }
 public IReadOnlyList<PedestrianAgent> Agents=>active;
 public int TargetCount=>Mathf.Clamp(Mathf.RoundToInt(daytimeCount*density*routineDensity*GridShiftPerformanceSettings.PedestrianDensity),1,maximum);
 public float routineDensity=1;
 IEnumerator Start(){
  if(gameObject.scene.name!="SampleScene"){enabled=false;yield break;}
  if(graph==null||graph.nodes.Length==0||models==null||models.Length==0||controller==null){Debug.LogError("Pedestres sem grafo, modelos ou animações.",this);yield break;}
  poolRoot=new GameObject("PoolPedestres").transform;poolRoot.SetParent(transform);poolRoot.gameObject.SetActive(false);
  if(excludedEntrances.Length==0)excludedEntrances=FindObjectsByType<CompanyEntrance>(FindObjectsSortMode.None);
  for(int i=0;i<graph.nodes.Length;i++)if(ResidenceAllowed(i))Entrances.Add(i);
  while(true){active.RemoveAll(a=>a==null);if(active.Count<TargetCount)Spawn();else if(active.Count>TargetCount)foreach(var a in active.ToArray())if(!a.ExternalControl&&!Visible(a.transform.position)){Recycle(a);break;}yield return new WaitForSeconds(.8f);}
 }
 public bool Visible(Vector3 position){if(playerCamera==null||!playerCamera.isActiveAndEnabled)return false;GeometryUtility.CalculateFrustumPlanes(playerCamera,planes);return GeometryUtility.TestPlanesAABB(planes,new Bounds(position+Vector3.up,Vector3.one*2));}
 public bool Clear(PedestrianAgent self,Vector3 position,float gap){
  if(self!=null)self.Obstacle=null;
  // Keep the landing free while someone is already committed to the road.
  if(self!=null&&self.CurrentActivity!=PedestrianAgent.Activity.Crossing)foreach(var crossingAgent in active){
   if(crossingAgent==null||crossingAgent==self||crossingAgent.CurrentActivity!=PedestrianAgent.Activity.Crossing||crossingAgent.NextNode<0)continue;
   var exit=graph.nodes[crossingAgent.NextNode].position;float distance=(position-exit).sqrMagnitude;
   if(distance<1.1f*1.1f&&distance<(self.transform.position-exit).sqrMagnitude-.00001f)return false;
  }
  foreach(var p in active)if(p!=null&&p!=self&&(p.transform.position-position).sqrMagnitude<gap*gap){
   if(self==null||(p.transform.position-position).sqrMagnitude<=(p.transform.position-self.transform.position).sqrMagnitude+.00001f){if(self!=null)self.Obstacle=p.GetComponent<Collider>();return false;}
  }
  // A sonda ultrapassa degraus de meio-fio de 20 cm, sem ignorar paredes/carros.
  int n=Physics.OverlapCapsuleNonAlloc(position+Vector3.up*.6f,position+Vector3.up*1.55f,.27f,hits,~(1<<9),QueryTriggerInteraction.Ignore);
  if(n==hits.Length)return false;
  for(int i=0;i<n;i++)if(self==null||!hits[i].transform.IsChildOf(self.transform)){if(self!=null)self.Obstacle=hits[i];return false;}return true;
 }
 // Validate the swept body as well as the endpoint: thin walls and moving
 // vehicles must not be skipped by a kinematic MovePosition.
 public bool CanStep(PedestrianAgent self,Vector3 from,Vector3 to){
  if(!Clear(self,to,separation))return false;
  var delta=to-from;float distance=delta.magnitude;if(distance<.00001f)return true;
  if(Physics.CapsuleCast(from+Vector3.up*.6f,from+Vector3.up*1.55f,.27f,delta/distance,out var hit,distance,~(1<<9),QueryTriggerInteraction.Ignore)){
   if(self!=null)self.Obstacle=hit.collider;return false;
  }
  return true;
 }
 // Pick a free walking direction before two bodies meet at the same graph node.
 // A shared right-hand preference separates opposing flows without leaving the sidewalk.
 public bool CrowdStep(PedestrianAgent self,Vector3 target,float distance,out Vector3 step){
  var origin=self.transform.position;var forward=Vector3.ProjectOnPlane(target-origin,Vector3.up).normalized;step=origin;
  float prediction=Mathf.Min(.9f,Vector3.Distance(origin,target));
  bool crowd=false;foreach(var p in active)if(p!=null&&p!=self){var delta=Vector3.ProjectOnPlane(p.transform.position-origin,Vector3.up);float ahead=Vector3.Dot(delta,forward);if(ahead>0&&(delta-forward*Mathf.Clamp(ahead,0,prediction)).sqrMagnitude<(separation+.12f)*(separation+.12f)){crowd=true;break;}}
  if(!crowd)return false;
  float best=float.NegativeInfinity;var right=Vector3.Cross(Vector3.up,forward);
  foreach(float angle in steeringAngles){
   var direction=Quaternion.AngleAxis(angle,Vector3.up)*forward;var candidate=origin+direction*distance;
   var look=origin+direction*Mathf.Min(.8f,Vector3.Distance(origin,target));
   if(!Supported(candidate,origin.y)||!Clear(self,candidate,separation+.08f)||!Supported(look,origin.y))continue;
   float clearance=1.5f;foreach(var p in active)if(p!=null&&p!=self)clearance=Mathf.Min(clearance,Vector3.ProjectOnPlane(p.transform.position-look,Vector3.up).magnitude);
   float score=Vector3.Dot(direction,forward)*.65f+Mathf.Min(clearance,1.2f)*1.8f+Vector3.Dot(direction,right)*.18f;
   if(score>best){best=score;step=candidate;}
  }
  return best>float.NegativeInfinity;
 }
 public bool TryYield(PedestrianAgent self,Vector3 direction,out Vector3 target){
  target=self.transform.position;bool nearPlayer=player!=null&&Vector3.Distance(player.position,self.transform.position)<2.4f;bool priority=nearPlayer;bool nearPedestrian=false;
  foreach(var other in active)if(other!=null&&other!=self&&Vector3.Distance(other.transform.position,self.transform.position)<separation+.5f){nearPedestrian=true;
      if(other.CurrentActivity==PedestrianAgent.Activity.Crossing||self.GetInstanceID()>other.GetInstanceID())priority=true;}
  if(!priority&&nearPedestrian)return false;
  if(!priority)priority=!Clear(self,self.transform.position+direction*.6f,separation);
  if(!priority)return false;
  for(int side=0;side<2;side++){float sign=side==0?1f:-1f;
   Vector3 candidate=self.transform.position+Vector3.Cross(Vector3.up,direction)*((nearPlayer?.95f:.7f)*sign);bool safe=true;
   for(int i=1;i<=4;i++){Vector3 point=Vector3.Lerp(self.transform.position,candidate,i/4f);
    if(!Supported(point,self.transform.position.y)||!Clear(self,point,separation)){safe=false;break;}
   }
   if(safe&&Clear(self,candidate,separation)){target=candidate;return true;}
  }
  return false;
 }
 public bool SidewalkSegment(PedestrianAgent self,Vector3 from,Vector3 to){int samples=Mathf.Max(4,Mathf.CeilToInt(Vector3.Distance(from,to)/.2f));for(int i=1;i<=samples;i++){var point=Vector3.Lerp(from,to,(float)i/samples);if(!Supported(point,from.y)||!Clear(self,point,separation))return false;}return true;}
 public bool Spawn(){for(int attempt=0;attempt<80;attempt++){
  int n=Random.Range(0,graph.nodes.Length);if(graph.nodes[n].neighbours.Length==0||graph.nodes[n].entrance||graph.nodes[n].entrancePath)continue;Vector3 pos=graph.nodes[n].position;
  if((player!=null&&Vector3.Distance(player.position,pos)<18)||Visible(pos)||!Clear(null,pos,1.5f))continue;
  PedestrianAgent agent;if(pool.Count>0)agent=pool.Dequeue();else{var go=Instantiate(models[Random.Range(0,models.Length)],poolRoot);agent=go.AddComponent<PedestrianAgent>();}
  agent.gameObject.SetActive(false);agent.transform.SetParent(transform);agent.enabled=true;agent.Initialize(this,n,Random.Range(speeds.x,speeds.y));agent.gameObject.SetActive(true);active.Add(agent);return true;
 }return false;}
 public void Recycle(PedestrianAgent agent){if(agent.ExternalControl)return;if(!active.Remove(agent))return;agent.gameObject.SetActive(false);agent.transform.SetParent(poolRoot);pool.Enqueue(agent);}
}

