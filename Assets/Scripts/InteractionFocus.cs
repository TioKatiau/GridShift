using System.Collections.Generic;
using UnityEngine;
public static class InteractionFocus {
 static readonly Dictionary<MonoBehaviour,float> targets=new Dictionary<MonoBehaviour,float>();
 static int consumed=-1;
 public static void Register(MonoBehaviour target,float range){targets[target]=range;}
 public static void Remove(MonoBehaviour target){targets.Remove(target);}
 public static bool Selected(MonoBehaviour target,Transform player){
 if(player==null)return false;float best=float.PositiveInfinity;MonoBehaviour selected=null;
 foreach(var item in targets){var t=item.Key;if(t==null||!t.isActiveAndEnabled||t.gameObject.scene!=player.gameObject.scene)continue;
 float d=Vector3.Distance(t.transform.position,player.position);if(d<=item.Value&&d<best){best=d;selected=t;}}
 return selected==target;
 }
 public static bool Consume(){if(consumed==Time.frameCount)return false;consumed=Time.frameCount;return true;}
}
