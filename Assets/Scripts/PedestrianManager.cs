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
 public bool ResidenceAllowed(int index){if(!graph.nodes[index].entrance)return false;foreach(var door in excludedEntrances)if(door!=null&&Vector3.ProjectOnPlane(graph.nodes[index].position-door.transform.position,Vector3.up).sqrMagnitude<25)return false;return true;}
 public bool Path(int from,int target,Queue<int> path,int blockedFrom=-1,int blockedTo=-1){
  var nodes=graph.nodes;if(target<0||target>=nodes.Length||from<0||from>=nodes.Length)return false;
  if(nodes[target].entrance&&!ResidenceAllowed(target))return false;
  if(previous==null||previous.Length!=nodes.Length){previous=new int[nodes.Length];searchQueue=new int[nodes.Length];}
  for(int i=0;i<previous.Length;i++)previous[i]=-1;int first=0,last=0;searchQueue[last++]=from;previous[from]=from;
  while(first<last&&previous[target]<0){int node=searchQueue[first++];foreach(int other in nodes[node].neighbours){if((node==blockedFrom&&other==blockedTo)||(node==blockedTo&&other==blockedFrom))continue;if(previous[other]>=0)continue;
   if(blockedFrom>=0&&(nodes[node].position-nodes[blockedFrom].position).sqrMagnitude<36){var delta=nodes[other].position-nodes[node].position;if(Physics.CapsuleCast(nodes[node].position+Vector3.up*.6f,nodes[node].position+Vector3.up*1.55f,.29f,delta.normalized,delta.magnitude,~(1<<9),QueryTriggerInteraction.Ignore))continue;}
   previous[other]=node;searchQueue[last++]=other;}}
  if(previous[target]<0||target==from)return false;reversePath.Clear();for(int node=target;node!=from;node=previous[node])reversePath.Push(node);path.Clear();while(reversePath.Count>0)path.Enqueue(reversePath.Pop());return true;
 }
 public bool Supported(Vector3 point,float expectedY,bool entrance=false){
  // Verifica a pegada inteira, não apenas um raio no centro junto à borda.
  for(int i=0;i<5;i++){Vector3 offset=i==0?Vector3.zero:i==1?Vector3.right*.24f:i==2?Vector3.left*.24f:i==3?Vector3.forward*.24f:Vector3.back*.24f;
   if(!Physics.Raycast(point+offset+Vector3.up,Vector3.down,out var hit,2,~(1<<9),QueryTriggerInteraction.Ignore)||hit.normal.y<.85f||(!entrance&&!hit.collider.name.StartsWith("Road "))||Mathf.Abs(hit.point.y+.025f-expectedY)>(entrance?.35f:.11f))return false;
  }return true;
 }
 public IReadOnlyList<PedestrianAgent> Agents=>active;
 public int TargetCount=>Mathf.Clamp(Mathf.RoundToInt(daytimeCount*density*routineDensity),1,maximum);
 public float routineDensity=1;
 IEnumerator Start(){
  if(gameObject.scene.name!="SampleScene"){enabled=false;yield break;}
  if(graph==null||graph.nodes.Length==0||models==null||models.Length==0||controller==null){Debug.LogError("Pedestres sem grafo, modelos ou animações.",this);yield break;}
  poolRoot=new GameObject("PoolPedestres").transform;poolRoot.SetParent(transform);poolRoot.gameObject.SetActive(false);
  if(excludedEntrances.Length==0)excludedEntrances=FindObjectsByType<CompanyEntrance>(FindObjectsSortMode.None);
  for(int i=0;i<graph.nodes.Length;i++)if(ResidenceAllowed(i))Entrances.Add(i);
  while(true){active.RemoveAll(a=>a==null);if(active.Count<TargetCount)Spawn();else if(active.Count>TargetCount)foreach(var a in active.ToArray())if(!Visible(a.transform.position)){Recycle(a);break;}yield return new WaitForSeconds(.8f);}
 }
 public bool Visible(Vector3 position){if(playerCamera==null||!playerCamera.isActiveAndEnabled)return false;GeometryUtility.CalculateFrustumPlanes(playerCamera,planes);return GeometryUtility.TestPlanesAABB(planes,new Bounds(position+Vector3.up,Vector3.one*2));}
 public bool Clear(PedestrianAgent self,Vector3 position,float gap){
  foreach(var p in active)if(p!=null&&p!=self&&(p.transform.position-position).sqrMagnitude<gap*gap){
   if(self==null||(p.transform.position-position).sqrMagnitude<=(p.transform.position-self.transform.position).sqrMagnitude+.00001f)return false;
  }
  // A sonda ultrapassa degraus de meio-fio de 20 cm, sem ignorar paredes/carros.
  int n=Physics.OverlapCapsuleNonAlloc(position+Vector3.up*.6f,position+Vector3.up*1.55f,.27f,hits,~(1<<9),QueryTriggerInteraction.Ignore);
  if(n==hits.Length)return false;
  for(int i=0;i<n;i++)if(self==null||!hits[i].transform.IsChildOf(self.transform))return false;return true;
 }
 public bool TryYield(PedestrianAgent self,Vector3 direction,out Vector3 target){
  target=self.transform.position;bool nearPlayer=player!=null&&Vector3.Distance(player.position,self.transform.position)<2.4f;bool priority=nearPlayer;bool nearPedestrian=false;
  foreach(var other in active)if(other!=null&&other!=self&&Vector3.Distance(other.transform.position,self.transform.position)<separation+.5f){nearPedestrian=true;
      if(other.CurrentActivity==PedestrianAgent.Activity.Crossing||self.GetInstanceID()>other.GetInstanceID())priority=true;}
  if(!priority&&nearPedestrian)return false;
  if(!priority)priority=!Clear(self,self.transform.position+direction*.6f,separation);
  if(!priority)return false;
  foreach(float sign in new[]{1f,-1f}){
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
 public void Recycle(PedestrianAgent agent){if(!active.Remove(agent))return;agent.gameObject.SetActive(false);agent.transform.SetParent(poolRoot);pool.Enqueue(agent);}
}

