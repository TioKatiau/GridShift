using UnityEngine;
using UnityEngine.InputSystem;

public sealed class GridShiftVanOrbit : MonoBehaviour {
 public float distance=6,height=1.4f,minDistance=3,maxDistance=10,sensitivity=.12f,recenterDelay=3,recenterSpeed=1.5f;
 public float minPitch=-12,maxPitch=65,collisionRadius=.25f;
 public Key perspectiveKey=Key.C;
 public Transform vehicle,driver;public Rigidbody body;
 readonly RaycastHit[] hits=new RaycastHit[64];float yaw,pitch=12,lastMouse,resolved;bool far;Vector3 velocity;
 float shake;public void Shake(float amount){shake=Mathf.Max(shake,amount);}
 public void ResetView(){yaw=vehicle.eulerAngles.y;pitch=15;resolved=distance;lastMouse=Time.unscaledTime;velocity=Vector3.zero;}
 void LateUpdate(){if(vehicle==null||!GetComponent<Camera>().enabled)return;
  bool free=Time.timeScale>0&&!GridShiftOperationsUI.ModalOpen&&!GridShiftCampaignUI.ModalOpen&&(PauseMenu.Instance==null||!PauseMenu.Instance.IsOpen);
  if(free){var mouse=Mouse.current;if(mouse!=null){var delta=mouse.delta.ReadValue();if(delta.sqrMagnitude>.01f){yaw+=delta.x*sensitivity;pitch=Mathf.Clamp(pitch-delta.y*sensitivity,minPitch,maxPitch);lastMouse=Time.unscaledTime;}float wheel=mouse.scroll.ReadValue().y;if(wheel!=0)distance=Mathf.Clamp(distance-Mathf.Sign(wheel)*.6f,minDistance,maxDistance);}
   if(Keyboard.current!=null&&Keyboard.current[perspectiveKey].wasPressedThisFrame)far=!far;
   if(Time.unscaledTime-lastMouse>recenterDelay){float reverse=body!=null&&Vector3.Dot(body.linearVelocity,vehicle.forward)<-.5f?180:0;yaw=Mathf.LerpAngle(yaw,vehicle.eulerAngles.y+reverse,1-Mathf.Exp(-recenterSpeed*Time.deltaTime));}
  }
  var anchor=vehicle.position+Vector3.up*height;var direction=Quaternion.Euler(pitch,yaw,0)*Vector3.back;float wanted=Mathf.Clamp(distance+(far?2:0),minDistance,maxDistance),allowed=wanted;
  int count=Physics.SphereCastNonAlloc(anchor,collisionRadius,direction,hits,wanted,~0,QueryTriggerInteraction.Ignore);
  for(int i=0;i<count;i++)if(!hits[i].transform.IsChildOf(vehicle)&&(driver==null||!hits[i].transform.IsChildOf(driver)))allowed=Mathf.Min(allowed,Mathf.Max(.3f,hits[i].distance-.15f));
  // Move inward immediately to prevent clipping; ease out only after clearance.
  resolved=allowed<resolved?allowed:Mathf.MoveTowards(resolved,allowed,Time.unscaledDeltaTime*4);
  var desired=anchor+direction*resolved;transform.position=Vector3.SmoothDamp(transform.position,desired,ref velocity,.12f,Mathf.Infinity,Time.unscaledDeltaTime);
  // Smoothing must never place the camera on the far side of the blocking surface.
  var actual=transform.position-anchor;count=Physics.SphereCastNonAlloc(anchor,collisionRadius,actual.normalized,hits,actual.magnitude,~0,QueryTriggerInteraction.Ignore);
  for(int i=0;i<count;i++)if(!hits[i].transform.IsChildOf(vehicle)&&(driver==null||!hits[i].transform.IsChildOf(driver)))transform.position=anchor+actual.normalized*Mathf.Min((transform.position-anchor).magnitude,Mathf.Max(.3f,hits[i].distance-.15f));
  transform.rotation=Quaternion.LookRotation(anchor-transform.position,Vector3.up);if(shake>0){shake=Mathf.MoveTowards(shake,0,Time.unscaledDeltaTime*.8f);transform.rotation*=Quaternion.Euler(Mathf.Sin(Time.unscaledTime*71)*shake*5,Mathf.Sin(Time.unscaledTime*53)*shake*3,0);}
 }
}
