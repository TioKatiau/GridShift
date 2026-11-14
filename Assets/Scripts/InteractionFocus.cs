using System.Collections.Generic;
using UnityEngine;
public static class InteractionFocus {
 sealed class ScenePlayer {public PlayerMovement player;public float retryAt;}
 static readonly Dictionary<int,ScenePlayer> players=new Dictionary<int,ScenePlayer>();
 static InteractionFocus(){UnityEngine.SceneManagement.SceneManager.sceneUnloaded+=scene=>players.Remove(scene.handle);}
 public static PlayerMovement PlayerFor(MonoBehaviour target){int handle=(target is DeliveryManager||target is GridShiftPersonalDelivery||target is GridShiftActiveOperations)?UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle:target.gameObject.scene.handle;if(!players.TryGetValue(handle,out var entry)){entry=new ScenePlayer();players[handle]=entry;}if(entry.player!=null&&entry.player.gameObject.activeInHierarchy)return entry.player;if(Time.unscaledTime<entry.retryAt)return null;entry.retryAt=Time.unscaledTime+.5f;foreach(var candidate in Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))if(candidate.gameObject.scene.handle==handle){entry.player=candidate;return candidate;}return null;}
 static readonly Dictionary<MonoBehaviour,float> targets=new Dictionary<MonoBehaviour,float>();
 static int consumed=-1,focusFrame=-1;static Transform focusPlayer;static MonoBehaviour focused;
 public static void Invalidate(){focusFrame=-1;focused=null;focusPlayer=null;players.Clear();}
 public static void Register(MonoBehaviour target,float range){targets[target]=range;focusFrame=-1;}
 public static void Remove(MonoBehaviour target){targets.Remove(target);focusFrame=-1;}
 public static bool Selected(MonoBehaviour target,Transform player){
 if(player==null)return false;if(focusFrame==Time.frameCount&&focusPlayer==player)return focused==target;focusFrame=Time.frameCount;focusPlayer=player;float best=float.PositiveInfinity;MonoBehaviour selected=null;
 var movement=player.GetComponent<PlayerMovement>();var view=movement!=null?movement.CameraTransform:null;
 foreach(var item in targets){var t=item.Key;if(t==null||!t.isActiveAndEnabled||t.gameObject.scene!=player.gameObject.scene)continue;
 float d=Vector3.Distance(t.transform.position,player.position);float reach=t is GridShiftWorldAction wa?wa.range:item.Value;if(d>reach&&!(t is GridShiftWorldAction ownerAction&&ownerAction.OwnerInRange(player.position)))continue;
 var action=t as GridShiftWorldAction;if(action!=null&&action.available!=null&&!action.available())continue;
 var hitCollider=action!=null?action.FocusCollider:t.GetComponent<Collider>();var at=hitCollider!=null?hitCollider.bounds.center:t.transform.position+Vector3.up*.7f;float facing=1,score=d;
 if(view!=null){if(action!=null&&action.TryOwnerAim(view,out var ownerAt,out var ownerScore)){at=ownerAt;score=ownerScore;}else if(hitCollider!=null){if(hitCollider.bounds.Contains(view.position)){var toward=Vector3.ProjectOnPlane(hitCollider.bounds.center-view.position,Vector3.up);if(toward.sqrMagnitude>.0001f&&Vector3.Dot(Vector3.ProjectOnPlane(view.forward,Vector3.up).normalized,toward.normalized)<.15f)continue;at=view.position+view.forward*.01f;score=0;}else{if(!hitCollider.Raycast(new Ray(view.position,view.forward),out var rayHit,item.Value+1.8f))continue;at=rayHit.point;score=rayHit.distance;}}else{var delta=at-view.position;facing=delta.sqrMagnitude<.01f?1:Vector3.Dot(view.forward,delta.normalized);if(facing<.985f)continue;score=d+(1-facing)*2;}if(!GridShiftDeliveryAccess.ClearSight(movement,at,action!=null&&action.sightRoot!=null?action.sightRoot:t.transform))continue;}
 if(score<best){best=score;selected=t;}}
 focused=selected;return selected==target;
 }
 public static bool Consume(){if(consumed==Time.frameCount)return false;consumed=Time.frameCount;return true;}
}
