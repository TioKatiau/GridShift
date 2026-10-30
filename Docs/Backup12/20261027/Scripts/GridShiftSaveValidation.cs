using System;
using System.Collections.Generic;
using UnityEngine;
public static class GridShiftSaveValidation {
 static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);static bool Rotation(Quaternion q)=>Finite(q.x)&&Finite(q.y)&&Finite(q.z)&&Finite(q.w)&&Quaternion.Dot(q,q)>.001f;static bool Position(Vector3 p)=>Finite(p.x)&&Finite(p.y)&&Finite(p.z)&&p.sqrMagnitude<1e12f;
 public static bool Valid(DeliverySaveData d){if(d.slot<0||d.slot>5||double.IsNaN(d.playSeconds)||double.IsInfinity(d.playSeconds)||d.playSeconds<0||d.playSeconds>3155760000||!Position(d.activePlayerPosition)||!Rotation(d.activePlayerRotation)||d.vehicles==null||d.vehicles.Count>32||(d.currentScene!="SampleScene"&&d.currentScene!="Office"))return false;var ids=new HashSet<string>();foreach(var v in d.vehicles)if(v==null||string.IsNullOrEmpty(v.id)||!ids.Add(v.id)||!Enum.IsDefined(typeof(FleetKind),v.kind)||!Enum.IsDefined(typeof(FleetStatus),v.state)||!Position(v.position)||!Rotation(v.rotation)||!Finite(v.capacityKg)||v.capacityKg<=0||!Finite(v.serviceRemaining)||v.serviceRemaining<0||v.nextPoint<1)return false;return true;}
}



