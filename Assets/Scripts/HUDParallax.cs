using UnityEngine;
using UnityEngine.InputSystem;
public class HUDParallax:MonoBehaviour {
 [SerializeField]float horizontalAmount=.015f,verticalAmount=.008f,smoothSpeed=8,maxHorizontalOffset=.03f,maxVerticalOffset=.02f;
 public PlayerMovement player;
 [Header("Respiração e passos")]
 public float breathingIntensity=.002f,breathingFrequency=1.4f,walkingIntensity=.004f,runningIntensity=.007f,stepFrequency=7;
 Vector3 basePosition,lastWritten,offset;float phase;bool initialized;
 void Start(){basePosition=lastWritten=transform.localPosition;if(player==null)player=GetComponentInParent<PlayerMovement>();initialized=true;}
 void LateUpdate(){if(!initialized)return;if((transform.localPosition-lastWritten).sqrMagnitude>.00000001f)basePosition=transform.localPosition;
  bool allowed=Time.timeScale>0&&player!=null&&!player.ControlsBlocked&&(WorldSession.Instance==null||!WorldSession.Instance.Busy);Vector3 target=Vector3.zero;
  if(allowed){var mouse=Mouse.current!=null?Mouse.current.delta.ReadValue():Vector2.zero;target=new Vector3(Mathf.Clamp(-mouse.x*horizontalAmount,-maxHorizontalOffset,maxHorizontalOffset),Mathf.Clamp(-mouse.y*verticalAmount,-maxVerticalOffset,maxVerticalOffset),0);
   bool moving=player.MovementInput.sqrMagnitude>.05f&&player.IsGrounded;phase+=Time.deltaTime*(moving?stepFrequency*(player.IsRunning?1.45f:1):breathingFrequency);float amplitude=moving?(player.IsRunning?runningIntensity:walkingIntensity):breathingIntensity;target+=new Vector3(moving?Mathf.Sin(phase*.5f)*amplitude*.5f:0,Mathf.Sin(phase)*amplitude,Mathf.Cos(phase)*breathingIntensity*.2f);target*=GamePreferences.HudMotion;
  }
  offset=Vector3.Lerp(offset,target,1-Mathf.Exp(-smoothSpeed*Time.unscaledDeltaTime));offset.x=Mathf.Clamp(offset.x,-maxHorizontalOffset,maxHorizontalOffset);offset.y=Mathf.Clamp(offset.y,-maxVerticalOffset,maxVerticalOffset);lastWritten=basePosition+offset;transform.localPosition=lastWritten;
 }
 void OnDisable(){if(initialized){transform.localPosition=basePosition;lastWritten=basePosition;offset=Vector3.zero;}}
}
