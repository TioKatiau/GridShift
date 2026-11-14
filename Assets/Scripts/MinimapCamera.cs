using UnityEngine;
public class MinimapCamera:MonoBehaviour {
 [SerializeField]Transform player;Camera cameraComponent;bool expanded,oldOrthographic;float oldSize;Vector3 center;float size;Quaternion oldRotation;
 public bool Expanded=>expanded;
 [Range(15,60)] public int refreshRate=30;
 float renderAccumulator;
 public void SetExpanded(bool value,Vector3 mapCenter,float mapSize){if(cameraComponent==null)cameraComponent=GetComponent<Camera>();if(value&&!expanded){oldOrthographic=cameraComponent.orthographic;oldSize=cameraComponent.orthographicSize;oldRotation=transform.localRotation;}expanded=value;center=mapCenter;size=mapSize;if(!value){cameraComponent.orthographic=oldOrthographic;cameraComponent.orthographicSize=oldSize;transform.localRotation=oldRotation;}else{cameraComponent.orthographic=true;cameraComponent.orthographicSize=size;}}
 void LateUpdate(){if(player==null)return;if(cameraComponent==null)cameraComponent=GetComponent<Camera>();
  // Presentation only. Traffic, deliveries and the simulation clock keep their normal cadence.
  if(WorldSession.Instance!=null&&WorldSession.Instance.InInterior&&gameObject.scene.name=="SampleScene"){cameraComponent.enabled=false;return;}
  float interval=1f/Mathf.Clamp(refreshRate,15,60);renderAccumulator+=Time.unscaledDeltaTime;bool render=renderAccumulator>=interval;cameraComponent.enabled=render;if(render)renderAccumulator%=interval;
  var target=expanded?center:player.position;transform.position=new Vector3(target.x,transform.position.y,target.z);if(expanded&&cameraComponent!=null){cameraComponent.orthographicSize=size;transform.rotation=Quaternion.Euler(90,0,0);}}
}
