using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class GridShiftTrackedRoute {
 public static CompanyVehicle Vehicle(DeliveryManager m){if(m==null)return null;var order=m.Selected;if(order!=null&&order.status!=DeliveryStatus.Completed)return m.fleet.FirstOrDefault(v=>v!=null&&v.Order==order);return m.TrackedVehicle;}
 // A route can occur twice in a pickup loop: stop only on the final occurrence.
 public static bool Build(CompanyVehicle v,List<Vector3> points){points.Clear();if(v==null||v.Order==null||v.Order.status==DeliveryStatus.Completed||v.CurrentPath==null||v.Target==null||v.driver==null)return false;var path=v.CurrentPath;int first=-1;for(int i=0;i<path.Count;i++)if(path[i]==v.driver.RotaAtual){first=i;if(v.LoopDeparture)break;}if(first<0)return false;points.Add(v.transform.position);for(int k=first;k<path.Count;k++){var r=path[k];if(r==null||!r.Valida||(k>first&&!path[k-1].Connections().Contains(r))){points.Clear();return false;}int start=k==first?v.driver.NextPoint:0;for(int i=start;i<r.pontos.Length;i++){if(k==path.Count-1&&r==v.Target.route&&i>=v.Target.segment+1){points.Add(v.Target.Stop);return points.Count>1;}points.Add(r.pontos[i].position);}}return points.Count>1;}
}
