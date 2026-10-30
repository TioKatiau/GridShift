using System.Collections.Generic;
using UnityEngine;

// The road stays reserved until every member of a crossing group has reached the sidewalk.
public class TrafficJunction : MonoBehaviour
{
    [SerializeField] private TrafficCar owner;
    [SerializeField] private PedestrianAgent pedestrian;
    [SerializeField,Min(0)] float vehicleReleaseWindow=4;
    [SerializeField,Min(10)] float crossingExit=12;
    public Vector3 ClearancePoint(Vector3 direction)=>transform.position+direction.normalized*crossingExit;
    readonly HashSet<PedestrianAgent> crossingGroup=new HashSet<PedestrianAgent>();
    readonly Dictionary<TrafficCar,TrafficRoute> vehicles=new Dictionary<TrafficCar,TrafficRoute>();
    readonly List<TrafficCar> expired=new List<TrafficCar>();
    float nextCrossing,groupStarted;
    int groupFrom=-1,groupTo=-1;
    TrafficLight[] crossingSignals;
    public bool VehicleOccupied {get{Clean();return owner!=null&&owner.isActiveAndEnabled||vehicles.Count>0;}}
    void Clean(){expired.Clear();foreach(var entry in vehicles)if(entry.Key==null||!entry.Key.isActiveAndEnabled)expired.Add(entry.Key);foreach(var key in expired)vehicles.Remove(key);if(owner==null||!owner.isActiveAndEnabled){owner=null;foreach(var key in vehicles.Keys){owner=key;break;}}}
    public int VehicleCount{get{Clean();return vehicles.Count;}}
    public bool MovingLeader(TrafficCar candidate,TrafficCar follower){Clean();return candidate!=null&&follower!=null&&vehicles.TryGetValue(candidate,out var lane)&&lane==follower.RotaAtual&&candidate.VelocidadeAtual>.5f&&Vector3.Dot(candidate.transform.forward,follower.transform.forward)>.9f&&Vector3.Dot(candidate.transform.position-follower.transform.position,follower.transform.forward)>0;}
    public bool PedestrianPhaseOpen {
        get {
            if(!TrafficIntersection.PedestrianAdmissionOpen)return false;
            if(crossingSignals==null){var lights=new List<TrafficLight>();foreach(var route in FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None))if(route.Cruzamento==this&&route.GetSemaforo()!=null&&!lights.Contains(route.GetSemaforo()))lights.Add(route.GetSemaforo());crossingSignals=lights.ToArray();}
            foreach(var light in crossingSignals)if(light!=null&&!light.EstaVermelho())return false;
            return true;
        }
    }
    public bool PedestrianOccupied {get {foreach(var agent in crossingGroup)if(agent!=null&&agent.isActiveAndEnabled)return true;return pedestrian!=null&&pedestrian.isActiveAndEnabled;}}
    public bool CanJoin(PedestrianAgent agent,int from,int to)=>PedestrianOccupied&&from>=0&&from==groupFrom&&to==groupTo&&crossingGroup.Count<3&&Time.time-groupStarted<8f&&!VehicleOccupied;
    public bool TryCross(PedestrianAgent agent)=>TryCross(agent,-1,-1);
    public bool TryCross(PedestrianAgent agent,int from,int to)
    {
        if(PedestrianOccupied){if(crossingGroup.Contains(agent)||pedestrian==agent)return true;if(!CanJoin(agent,from,to))return false;crossingGroup.Add(agent);return true;}
        if(Time.time<nextCrossing)return false;
        if(VehicleOccupied)return false;
        crossingGroup.Clear();crossingGroup.Add(agent);pedestrian=agent;groupFrom=from;groupTo=to;groupStarted=Time.time;return true;
    }
    public void LeaveCrossing(PedestrianAgent agent)
    {
        bool member=crossingGroup.Remove(agent)||pedestrian==agent;if(!member)return;
        if(pedestrian==agent){pedestrian=null;foreach(var other in crossingGroup)if(other!=null&&other.isActiveAndEnabled){pedestrian=other;break;}}
        if(!PedestrianOccupied){crossingGroup.Clear();pedestrian=null;nextCrossing=Time.time+vehicleReleaseWindow;}
    }
    public bool TryEnter(TrafficCar vehicle){if(!Available(vehicle))return false;vehicles[vehicle]=vehicle.RotaAtual;if(owner==null)owner=vehicle;return true;}
    public bool Available(TrafficCar vehicle){Clean();if(vehicle==null||PedestrianOccupied)return false;if(vehicles.ContainsKey(vehicle))return true;if(owner!=null&&!vehicles.ContainsKey(owner)&&owner!=vehicle)return false;foreach(var entry in vehicles)if(entry.Key!=vehicle&&(entry.Value==null||entry.Value!=vehicle.RotaAtual))return false;return true;}
    public void Release(TrafficCar vehicle){vehicles.Remove(vehicle);if(owner==vehicle)owner=null;Clean();}
}

