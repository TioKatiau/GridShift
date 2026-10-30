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
    float nextCrossing,groupStarted;
    int groupFrom=-1,groupTo=-1;
    TrafficLight[] crossingSignals;
    public bool VehicleOccupied=>owner!=null&&owner.isActiveAndEnabled;
    public bool PedestrianPhaseOpen {
        get {
            if(!TrafficIntersection.PedestrianAdmissionOpen)return false;
            if(crossingSignals==null){var lights=new List<TrafficLight>();foreach(var route in FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None))if(route.Cruzamento==this&&route.GetSemaforo()!=null&&!lights.Contains(route.GetSemaforo()))lights.Add(route.GetSemaforo());crossingSignals=lights.ToArray();}
            foreach(var light in crossingSignals)if(light!=null&&!light.EstaVermelho())return false;
            return true;
        }
    }
    public bool PedestrianOccupied {get {foreach(var agent in crossingGroup)if(agent!=null&&agent.isActiveAndEnabled)return true;return pedestrian!=null&&pedestrian.isActiveAndEnabled;}}
    public bool CanJoin(PedestrianAgent agent,int from,int to)=>PedestrianOccupied&&from>=0&&from==groupFrom&&to==groupTo&&crossingGroup.Count<3&&Time.time-groupStarted<8f&&(owner==null||!owner.isActiveAndEnabled);
    public bool TryCross(PedestrianAgent agent)=>TryCross(agent,-1,-1);
    public bool TryCross(PedestrianAgent agent,int from,int to)
    {
        if(PedestrianOccupied){if(crossingGroup.Contains(agent)||pedestrian==agent)return true;if(!CanJoin(agent,from,to))return false;crossingGroup.Add(agent);return true;}
        if(Time.time<nextCrossing)return false;
        if(owner!=null&&owner.isActiveAndEnabled)return false;
        crossingGroup.Clear();crossingGroup.Add(agent);pedestrian=agent;groupFrom=from;groupTo=to;groupStarted=Time.time;return true;
    }
    public void LeaveCrossing(PedestrianAgent agent)
    {
        bool member=crossingGroup.Remove(agent)||pedestrian==agent;if(!member)return;
        if(pedestrian==agent){pedestrian=null;foreach(var other in crossingGroup)if(other!=null&&other.isActiveAndEnabled){pedestrian=other;break;}}
        if(!PedestrianOccupied){crossingGroup.Clear();pedestrian=null;nextCrossing=Time.time+vehicleReleaseWindow;}
    }
    public bool TryEnter(TrafficCar vehicle)
    {
        if(vehicle==null||PedestrianOccupied)return false;
        if(owner!=null&&owner.isActiveAndEnabled&&owner!=vehicle)return false;
        owner=vehicle;return true;
    }
    public bool Available(TrafficCar vehicle){return !PedestrianOccupied&&(owner==null||!owner.isActiveAndEnabled||owner==vehicle);}
    public void Release(TrafficCar vehicle){if(owner==vehicle)owner=null;}
}
