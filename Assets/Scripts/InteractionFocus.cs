using System.Collections.Generic;
using UnityEngine;
public static class InteractionFocus {
 static readonly Dictionary<MonoBehaviour,float> targets=new Dictionary<MonoBehaviour,float>();
 static int consumed=-1;
 public static void Register(MonoBehaviour target,float range){targets[target]=range;}
 public static void Remove(MonoBehaviour target){targets.Remove(target);}
 public static bool Selected(MonoBehaviour target,Transform player){
 if(player==null)return false;float best=float.PositiveInfinity;MonoBehaviour selected=null;
 var movement=player.GetComponent<PlayerMovement>();var view=movement!=null?movement.CameraTransform:null;
 foreach(var item in targets){var t=item.Key;if(t==null||!t.isActiveAndEnabled||t.gameObject.scene!=player.gameObject.scene)continue;
 float d=Vector3.Distance(t.transform.position,player.position);if(d>item.Value)continue;
 var at=t.transform.position+Vector3.up*.7f;float facing=1;
 if(view!=null){var delta=at-view.position;facing=delta.sqrMagnitude<.01f?1:Vector3.Dot(view.forward,delta.normalized);if(facing<.1f||!GridShiftDeliveryAccess.ClearSight(movement,at,t.transform))continue;}
 float score=d+(1-facing)*2;if(score<best){best=score;selected=t;}}
 return selected==target;
 }
 public static bool Consume(){if(consumed==Time.frameCount)return false;consumed=Time.frameCount;return true;}
}
