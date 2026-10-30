using System.Collections.Generic;
using UnityEngine;
public sealed class CompanyVehicle:MonoBehaviour,ITrafficGuidance {
 public string id;public FleetKind kind;public float capacityKg=200;
 public FleetStatus state=FleetStatus.Available;
 public TrafficCar driver;
 public DeliveryOrder Order{get;private set;}
 public DeliveryPoint Target=>target;
 public bool Carrying=>picked;
 public bool CanDrive=>manager==null||manager.Company==null||manager.Company.CanDrive(this);public bool Operational=>manager==null||manager.Company==null||manager.Company.CanOperate(this);public bool Available=>Operational&&Order==null&&state==FleetStatus.Available&&driver!=null&&driver.enabled&&gameObject.activeInHierarchy;
 DeliveryManager manager;DeliveryPoint target;List<TrafficRoute> path;float until,retry;Vector3 last;bool picked,loopDeparture;
 public void Configure(DeliveryManager owner,string identifier,FleetKind type,float capacity){manager=owner;id=identifier;kind=type;capacityKg=capacity;driver=GetComponent<TrafficCar>();driver.SetGuidance(this);last=transform.position;}
 public bool Assign(DeliveryOrder order){if(!Available||order.weightKg>capacityKg)return false;Order=order;picked=false;order.vehicleId=id;last=transform.position;Go(manager.Point(order.pickupId));return true;}
 void Go(DeliveryPoint point){target=point;loopDeparture=point!=null&&DeliveryRouting.Behind(driver,point);path=point!=null?DeliveryRouting.Find(driver.RotaAtual,point.route,loopDeparture):null;driver.RefreshGuidance();Set(path==null?FleetStatus.WaitingRoute:picked?FleetStatus.Transporting:FleetStatus.ToPickup);retry=Time.time+2;}
 void Set(FleetStatus value){state=value;if(Order==null)return;Order.status=value==FleetStatus.ToPickup?DeliveryStatus.ToPickup:value==FleetStatus.Loading?DeliveryStatus.Loading:value==FleetStatus.Transporting?DeliveryStatus.Transporting:value==FleetStatus.Unloading?DeliveryStatus.Unloading:DeliveryStatus.WaitingRoute;manager.Changed();}
 void Update(){if(driver==null||manager==null)return;if(!driver.enabled){if(Order!=null&&state!=FleetStatus.Unavailable)Set(FleetStatus.Unavailable);return;}float traveled=Vector3.Distance(last,transform.position);if(traveled<30)manager.Company.Distance(this,traveled);if(Order==null){last=transform.position;return;}if(state==FleetStatus.Unavailable){Go(target);return;}if(loopDeparture&&driver.RotaAtual!=target?.route)loopDeparture=false;
  float delta=Vector3.Distance(last,transform.position);if(delta<3)Order.travelledMeters+=delta;last=transform.position;
  if(state==FleetStatus.Loading||state==FleetStatus.Unloading){if(Time.time<until)return;if(state==FleetStatus.Loading){picked=true;manager.Notify("Carga coletada · "+id);Go(manager.Point(Order.destinationId));}else{var completed=Order;manager.Complete(completed);Order=null;target=null;path=null;state=FleetStatus.Available;driver.RefreshGuidance();}return;}
  if(state==FleetStatus.WaitingRoute){if(Time.time>=retry)Go(target);return;}
  if(target==null||!target.Valid){Set(FleetStatus.WaitingRoute);retry=Time.time+2;return;}
  if(driver.RotaAtual==target.route&&driver.NextPoint<=target.segment+1&&Vector3.ProjectOnPlane(transform.position-target.Stop,Vector3.up).magnitude<.45f&&driver.VelocidadeAtual<.12f){Set(picked?FleetStatus.Unloading:FleetStatus.Loading);until=Time.time+(picked?manager.unloadingSeconds:manager.loadingSeconds)*manager.Progression.Handling*manager.Company.Handling;manager.Notify(picked?"Descarregando · "+id:"Carregando · "+id);}
 }
 public TrafficRoute NextRoute(TrafficRoute current){if(Order==null)return current.EncontrarProximaRota();if(path==null)return null;int index=path.IndexOf(current);if(index>=0&&index+1<path.Count&&path[index+1].Valida)return path[index+1];if(current==target?.route)return null;
  path=target!=null?DeliveryRouting.Find(current,target.route):null;return path!=null&&path.Count>1?path[1]:null;
 }
 public float DistanceToStop(TrafficRoute current,int nextPoint,Vector3 position){if(Order==null||target==null||loopDeparture||current!=target.route||nextPoint>target.segment+1)return float.PositiveInfinity;return DeliveryRouting.Remaining(driver,target);}
 public void RestoreOrder(DeliveryOrder order,bool collected){Order=order;picked=collected;last=transform.position;Go(manager.Point(collected?order.destinationId:order.pickupId));}
 void OnDestroy(){if(driver!=null)driver.SetGuidance(null);}
}


