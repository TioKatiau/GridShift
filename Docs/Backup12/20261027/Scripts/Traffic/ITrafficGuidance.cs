using UnityEngine;
// Optional dispatch instructions; driving and all safety checks remain in TrafficCar.
public interface ITrafficGuidance {
 TrafficRoute NextRoute(TrafficRoute current);
 float DistanceToStop(TrafficRoute current,int nextPoint,Vector3 position);
}
