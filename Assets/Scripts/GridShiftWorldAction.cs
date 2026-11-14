using System;
using TMPro;
using UnityEngine;

// Every new physical station participates in the existing focus and E arbitration.
public sealed class GridShiftWorldAction:MonoBehaviour {
 public bool sceneAuthored;public string bindingId;public Transform sightRoot,focusOwner;public string title;public Func<PlayerMovement,bool> action;public Func<bool> available;
 public bool OwnerInRange(Vector3 at){if(focusOwner==null)return false;foreach(var r in focusOwner.GetComponentsInChildren<Renderer>())if(r is MeshRenderer||r is SkinnedMeshRenderer){var p=r.bounds.ClosestPoint(at+Vector3.up*.8f);if(Vector3.Distance(p,at+Vector3.up*.8f)<=range)return true;}return false;}
 public bool TryOwnerAim(Transform view,out Vector3 at,out float score){at=Vector3.zero;score=float.PositiveInfinity;if(focusOwner==null)return false;var ray=new Ray(view.position,view.forward);foreach(var r in focusOwner.GetComponentsInChildren<Renderer>()){if(!(r is MeshRenderer)||r.GetComponent<TMP_Text>()!=null)continue;var b=r.bounds;b.Expand(.1f);if(b.IntersectRay(ray,out float distance)&&distance>=0&&distance<range+1.8f&&distance<score){score=distance;at=ray.GetPoint(distance);}}if(FocusCollider!=null&&FocusCollider.Raycast(ray,out var exact,range+1.8f)&&exact.distance-.12f<score){at=exact.point;score=Mathf.Max(0,exact.distance-.12f);}return !float.IsPositiveInfinity(score);}
 public bool Reachable(PlayerMovement p){if(p==null||p.ControlsBlocked||Time.timeScale<=0||p.gameObject.scene!=UnityEngine.SceneManagement.SceneManager.GetActiveScene()||WorldSession.Instance?.Busy==true)return false;if(focusOwner!=null&&OwnerInRange(p.transform.position)&&TryOwnerAim(p.CameraTransform,out var at,out _))return GridShiftDeliveryAccess.ClearSight(p,at,sightRoot);return GridShiftDeliveryAccess.InReach(p,transform.position,range,sightRoot,sightRoot!=null&&FocusCollider!=null?(Vector3?)FocusCollider.bounds.center:null);}
 public Vector3 focusOffset=Vector3.up*.8f;public float focusRadius=.28f;public Vector3 focusSize;public Collider FocusCollider{get;private set;}
 public float range=2.6f;PlayerMovement player;TMP_Text label;
 void OnEnable(){FocusCollider=GetComponent<Collider>();if(FocusCollider!=null&&FocusCollider.isTrigger)FocusCollider.enabled=true;InteractionFocus.Register(this,range);}
 void OnDisable(){InteractionFocus.Remove(this);}
 void Start(){InteractionFocus.Register(this,range);FocusCollider=GetComponent<Collider>();if(FocusCollider!=null){FocusCollider.enabled=true;return;}if(focusSize.sqrMagnitude>0){var hit=gameObject.AddComponent<BoxCollider>();hit.isTrigger=true;hit.center=focusOffset;hit.size=focusSize;FocusCollider=hit;}else{var hit=gameObject.AddComponent<SphereCollider>();hit.isTrigger=true;hit.center=focusOffset;hit.radius=focusRadius;FocusCollider=hit;}}
 void Update(){
  player=InteractionFocus.PlayerFor(this);
  bool selected=player!=null&&(available==null||available())&&Reachable(player)&&InteractionFocus.Selected(this,player.transform);
  if(selected)GridShiftContextHUD.Offer(title);
  if(selected&&GridShiftControls.Down(GSAction.Interact)&&InteractionFocus.Consume()){if(action?.Invoke(player)==true)GridShiftTutorial.Record("interact");GridShiftControls.SuppressFrame();}
 }
 public static GridShiftWorldAction Create(Transform parent,string title,Vector3 local,Func<PlayerMovement,bool> action,string key=null){string binding=key??title;foreach(Transform child in parent){var existing=child.GetComponent<GridShiftWorldAction>();if(existing!=null&&existing.sceneAuthored&&(existing.bindingId==binding||(string.IsNullOrEmpty(existing.bindingId)&&existing.title==title))){existing.action=action;return existing;}}var go=new GameObject(title);go.transform.SetParent(parent,false);go.transform.localPosition=local;var component=go.AddComponent<GridShiftWorldAction>();component.title=title;component.bindingId=binding;component.action=action;return component;}
}
