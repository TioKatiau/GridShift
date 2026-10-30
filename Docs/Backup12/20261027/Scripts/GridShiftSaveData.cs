using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]public sealed class FleetSaveData {public string id,model,routeId,orderId;public FleetKind kind;public FleetStatus state;public float capacityKg,serviceRemaining;public int nextPoint;public Vector3 position;public Quaternion rotation=Quaternion.identity;public bool carrying;}
[Serializable]public sealed class SimulationSaveData {public int preset;public float pedestrians=1,traffic=1;}
[Serializable]public sealed class TutorialSaveData {public int step;public bool complete,skipped;}
public static class GridShiftRouteIdentity {
 public static string Of(TrafficRoute route){if(route==null)return null;if(!string.IsNullOrEmpty(route.stableId))return route.stableId;var names=new Stack<string>();for(var t=route.transform;t!=null;t=t.parent)names.Push(t.name+"["+t.GetSiblingIndex()+"]");return string.Join("/",names);}
}

