using System.Collections.Generic;
using UnityEngine;
public static class DeliveryRouting {
 public static float Length(TrafficRoute route){float total=0;for(int i=1;i<route.pontos.Length;i++)total+=Vector3.Distance(route.pontos[i-1].position,route.pontos[i].position);return total;}
 // Dijkstra over the same directed connections accepted by the traffic driver.
 public static List<TrafficRoute> Find(TrafficRoute start,TrafficRoute goal,bool requireLoop=false){
  if(start==null||goal==null||!start.Valida||!goal.Valida)return null;if(start==goal&&!requireLoop)return new List<TrafficRoute>{start};
  var distance=new Dictionary<TrafficRoute,float>();var previous=new Dictionary<TrafficRoute,TrafficRoute>();var frontier=new List<TrafficRoute>();var closed=new HashSet<TrafficRoute>();
  foreach(var next in start.Connections()){distance[next]=Length(next);previous[next]=start;frontier.Add(next);}
  while(frontier.Count>0){int best=0;for(int i=1;i<frontier.Count;i++)if(distance[frontier[i]]<distance[frontier[best]])best=i;var current=frontier[best];frontier.RemoveAt(best);if(!closed.Add(current))continue;
   if(current==goal){var result=new List<TrafficRoute>{current};var at=current;while(previous.TryGetValue(at,out var before)){result.Add(before);if(before==start)break;at=before;}result.Reverse();return result;}
   foreach(var next in current.Connections()){if(closed.Contains(next))continue;float candidate=distance[current]+Length(next);if(distance.TryGetValue(next,out float old)&&old<=candidate)continue;distance[next]=candidate;previous[next]=current;if(!frontier.Contains(next))frontier.Add(next);}
  }return null;
 }
 public static bool Behind(TrafficCar driver,DeliveryPoint point){if(driver.RotaAtual!=point.route)return false;if(driver.NextPoint>point.segment+1)return true;var a=point.route.pontos[point.segment].position;var b=point.route.pontos[point.segment+1].position;return Vector3.Dot(driver.transform.position-point.Stop,(b-a).normalized)>.15f;}
 public static float Remaining(TrafficCar driver,DeliveryPoint point){if(driver.RotaAtual!=point.route||driver.NextPoint>point.segment+1)return float.PositiveInfinity;var r=point.route;var position=driver.transform.position;int next=driver.NextPoint;if(next==point.segment+1)return Mathf.Max(0,Vector3.Dot(point.Stop-position,(r.pontos[next].position-r.pontos[next-1].position).normalized));float result=Vector3.Distance(position,r.pontos[next].position);for(int i=next;i<point.segment;i++)result+=Vector3.Distance(r.pontos[i].position,r.pontos[i+1].position);return result+Vector3.Distance(r.pontos[point.segment].position,point.Stop);}
}
