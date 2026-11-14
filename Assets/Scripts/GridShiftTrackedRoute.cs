using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class GridShiftTrackedRoute {
 static DeliveryManager personalOwner;static TrafficRoute[] personalRoads;static string personalKey;static float personalRefresh;
 static readonly List<Vector3> personalPoints=new List<Vector3>();
 public static bool BuildForMap(DeliveryManager manager,List<Vector3> points){
  if(manager==null||WorldSession.Instance?.InInterior==true){points.Clear();return false;}var ops=manager.GetComponent<GridShiftActiveOperations>();var order=ops?.Van?.Driving==true?manager.PersonalOrder:manager.Selected;
  if(order==null||order.mode!=DeliveryMode.Personal||order.status==DeliveryStatus.Completed)return Build(Vehicle(manager),points);
  if(personalOwner!=manager){personalOwner=manager;personalRoads=Object.FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None).Where(r=>r.gameObject.scene==manager.gameObject.scene).ToArray();personalRefresh=0;}
  string key=order.id+":"+order.status+":"+UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle;if(Time.unscaledTime>=personalRefresh||key!=personalKey){personalKey=key;personalRefresh=Time.unscaledTime+.5f;personalPoints.Clear();
   var target=manager.Point(order.status==DeliveryStatus.ToPickup?order.pickupId:order.destinationId);var player=InteractionFocus.PlayerFor(manager);var position=ops?.Van?.Driving==true?ops.Van.transform.position:player!=null?player.transform.position:ops!=null&&ops.Van!=null?ops.Van.transform.position:Vector3.zero;
   float best=float.PositiveInfinity;List<TrafficRoute> chosen=null;int next=1;Vector3 projection=position;
   if(target!=null&&target.route!=null)foreach(var road in personalRoads){if(road==null||!road.Valida)continue;for(int i=1;i<road.pontos.Length;i++){var a=road.pontos[i-1].position;var delta=road.pontos[i].position-a;float t=Mathf.Clamp01(Vector3.Dot(position-a,delta)/delta.sqrMagnitude);var at=a+delta*t;float score=Vector3.ProjectOnPlane(position-at,Vector3.up).sqrMagnitude;if(score>=best)continue;
     bool loop=road==target.route&&(target.segment<i-1||target.segment==i-1&&Vector3.Dot(target.Stop-at,delta)<-.01f);var path=DeliveryRouting.Find(road,target.route,loop);if(path==null)continue;best=score;chosen=path;next=i;projection=at;
   }}
   if(chosen!=null){
    // Off-road access is not invented: start at the nearest connected road when parked in a yard.
    if(best<16)personalPoints.Add(position);personalPoints.Add(projection);bool reached=false;
    for(int k=0;k<chosen.Count&&!reached;k++){var road=chosen[k];for(int i=k==0?next:0;i<road.pontos.Length;i++){if(k==chosen.Count-1&&road==target.route&&i>=target.segment+1){personalPoints.Add(target.Stop);reached=true;break;}personalPoints.Add(road.pontos[i].position);}}
   }
  }
  points.Clear();points.AddRange(personalPoints);return points.Count>1;
 }
 public static CompanyVehicle Vehicle(DeliveryManager m){if(m==null)return null;var order=m.Selected;if(order!=null&&order.status!=DeliveryStatus.Completed)return m.fleet.FirstOrDefault(v=>v!=null&&v.Order==order);return m.TrackedVehicle;}
 // A route can occur twice in a pickup loop: stop only on the final occurrence.
 public static bool Build(CompanyVehicle v,List<Vector3> points){points.Clear();if(v==null||v.Order==null||v.Order.status==DeliveryStatus.Completed||v.CurrentPath==null||v.Target==null||v.driver==null)return false;var path=v.CurrentPath;int first=-1;for(int i=0;i<path.Count;i++)if(path[i]==v.driver.RotaAtual){first=i;if(v.LoopDeparture)break;}if(first<0)return false;points.Add(v.transform.position);for(int k=first;k<path.Count;k++){var r=path[k];if(r==null||!r.Valida||(k>first&&!path[k-1].Connections().Contains(r))){points.Clear();return false;}int start=k==first?v.driver.NextPoint:0;for(int i=start;i<r.pontos.Length;i++){if(k==path.Count-1&&r==v.Target.route&&i>=v.Target.segment+1){points.Add(v.Target.Stop);return points.Count>1;}points.Add(r.pontos[i].position);}}return points.Count>1;}
}
