using UnityEngine;
public class PlayerCameraMotion:MonoBehaviour {
 public PlayerMovement player;
 public Camera view;
 public float amplitude=.035f,frequency=11,runFov=3;
 Vector3 home;float normalFov,phase;bool initialized;
 void Start(){if(player==null)player=GetComponent<PlayerMovement>();if(view==null&&player!=null)view=player.CameraTransform.GetComponent<Camera>();if(view!=null){home=view.transform.localPosition;normalFov=view.fieldOfView;initialized=true;}}
 void LateUpdate(){
  if(!initialized||player==null||view==null||!player.enabled||player.ControlsBlocked||Time.timeScale<=0||(WorldSession.Instance!=null&&WorldSession.Instance.Busy))return;
  bool active=player.IsRunning&&player.IsGrounded&&player.MovementInput.sqrMagnitude>.05f&&GamePreferences.RunBob;
  float intensity=GamePreferences.CameraIntensity;Vector3 target=home;
  if(active){phase+=Time.deltaTime*frequency;target+=new Vector3(Mathf.Sin(phase*.5f)*.35f,Mathf.Sin(phase),0)*amplitude*intensity*GamePreferences.HeadBob;}
  float t=1-Mathf.Exp(-12*Time.deltaTime);view.transform.localPosition=Vector3.Lerp(view.transform.localPosition,target,t);
  view.fieldOfView=Mathf.Lerp(view.fieldOfView,normalFov+(active?runFov*intensity:0),t);
 }
}
