using UnityEngine;

// Explicitly authored parking objectives. Each point is on an asphalt parking surface,
// with forward identifying the desired body orientation (also during reverse).
public sealed class TrafficParkingBay : MonoBehaviour,ITrafficGuidance
{
    [System.Serializable] public struct Step { public Transform point; public bool reverse; }
    public Step[] entry=new Step[0],exit=new Step[0];
    public Transform approach;
    public TrafficRoute accessRoute;
    [Range(0,1)] public float visitProbability=.12f;
    public Vector2 staySeconds=new Vector2(8,20);
    [Min(2)] public float length=7;
    [Min(1)] public float width=2.8f;
    [Min(3)] public float timeout=35;
    public TrafficCar Occupant {get;private set;}
    TrafficCar visitor;float nextCheck,leaveAt;bool leaving;
    readonly System.Collections.Generic.HashSet<int> considered=new System.Collections.Generic.HashSet<int>();
    public TrafficRoute NextRoute(TrafficRoute current)=>current.EncontrarProximaRota();
    public float DistanceToStop(TrafficRoute current,int nextPoint,Vector3 position)=>current==accessRoute&&approach!=null?Mathf.Max(0,Vector3.Dot(approach.position-position,approach.forward)):float.PositiveInfinity;
    void Update(){
        if(visitor!=null){
            if(Occupant==null&&visitor.Guidance!=this&&!visitor.HasParkingGoal){visitor=null;leaving=false;nextCheck=Time.time+30;return;}
            if(!visitor.isActiveAndEnabled){visitor=null;return;}
            if(Occupant==null){
                if(leaving){visitor=null;leaving=false;nextCheck=Time.time+30;return;}
                if(visitor.RotaAtual!=accessRoute){visitor.SetGuidance(null);visitor=null;return;}
                if(Vector3.Distance(visitor.transform.position,approach.position)<.45f&&visitor.VelocidadeAtual<.15f){visitor.SetGuidance(null);if(!visitor.RequestParking(this)){visitor=null;nextCheck=Time.time+30;}leaveAt=0;}
            }else if(!leaving&&visitor.Diagnostic==TrafficCar.DiagnosticState.PARKED){if(leaveAt==0)leaveAt=Time.time+Random.Range(staySeconds.x,staySeconds.y);if(Time.time>leaveAt)leaving=visitor.LeaveParking();}
            return;
        }
        if(Occupant!=null||accessRoute==null||approach==null||Time.time<nextCheck)return;nextCheck=Time.time+1;
        foreach(var car in TrafficCar.Active){if(car==null||car.BodySize.z>length||car.BodySize.x>width||car.Guidance!=null||car.RotaAtual!=accessRoute||considered.Contains(car.GetInstanceID()))continue;float distance=Vector3.Dot(approach.position-car.transform.position,approach.forward);if(distance<6||distance>25)continue;considered.Add(car.GetInstanceID());if(Random.value>visitProbability)continue;visitor=car;visitor.SetGuidance(this);break;}
    }
    public bool Reserve(TrafficCar car,Vector3 size)
    {
        if(car==null||Occupant!=null||approach==null||entry.Length==0||exit.Length==0||size.z>length||size.x>width)return false;
        foreach(var step in entry)if(step.point==null)return false;
        foreach(var step in exit)if(step.point==null)return false;
        Occupant=car;return true;
    }
    public void Release(TrafficCar car){if(Occupant==car)Occupant=null;}
    void OnDisable(){if(visitor!=null&&visitor.Guidance==this)visitor.SetGuidance(null);}
    void OnDrawGizmosSelected(){Gizmos.color=Color.cyan;Gizmos.matrix=transform.localToWorldMatrix;Gizmos.DrawWireCube(Vector3.zero,new Vector3(width,.1f,length));Gizmos.matrix=Matrix4x4.identity;foreach(var steps in new[]{entry,exit})for(int i=1;i<steps.Length;i++)if(steps[i-1].point!=null&&steps[i].point!=null)Gizmos.DrawLine(steps[i-1].point.position,steps[i].point.position);}
}
