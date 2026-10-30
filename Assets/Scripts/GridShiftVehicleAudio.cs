using UnityEngine;

// Observes the real traffic state; never modifies speed, routes or reservations.
[DisallowMultipleComponent] public sealed class GridShiftVehicleAudio:MonoBehaviour {
 public bool Audible {get;set;}
 public bool Heavy {get;private set;}
 public AudioSource Engine {get;private set;}
 public AudioSource Road {get;private set;}
 TrafficCar car;float previousSpeed,reverseAt,brakeAt,hornAt;
 void Start(){car=GetComponent<TrafficCar>();var box=GetComponent<BoxCollider>();Heavy=(car!=null&&car.BodySize.z>7)||(box!=null&&Mathf.Abs(box.size.z*transform.lossyScale.z)>7);var a=GridShiftAudio.Instance;if(a==null){enabled=false;return;}string type=Heavy?"Truck":"Car";
  Engine=a.Source(type+"-Engine","Vehicles",true,transform);Engine.clip=a.Clip(type+"_Engine_Loop");Road=a.Source(type+"-Road","Vehicles",true,transform);Road.clip=a.Clip(type+"_Road_Loop");
  foreach(var s in new[]{Engine,Road}){s.spatialBlend=1;s.rolloffMode=AudioRolloffMode.Linear;s.minDistance=Heavy?4:3;s.maxDistance=65;s.priority=150;s.dopplerLevel=.35f;}
  hornAt=Time.time+20+(GetInstanceID()&31);
 }
 void Update(){if(car==null||Engine==null||Road==null)return;float speed=car.VelocidadeAtual;var fleet=car.Guidance as CompanyVehicle;var audio=GridShiftAudio.Instance;bool running=audio!=null&&Audible&&car.isActiveAndEnabled&&Time.timeScale>0&&car.Diagnostic!=TrafficCar.DiagnosticState.PARKED&&(fleet==null||fleet.CanDrive)&&audio.ClaimVehicle(this);
  float normalized=Mathf.Clamp01(speed/Mathf.Max(1,car.MaximumSpeed));float gain=running?Mathf.Lerp(.19f,.55f,normalized):0;
  Engine.volume=Mathf.MoveTowards(Engine.volume,gain,Time.unscaledDeltaTime*1.3f);Engine.pitch=Mathf.MoveTowards(Engine.pitch,(Heavy?.78f:.88f)+normalized*(Heavy?.55f:.85f),Time.deltaTime*.9f);
  Road.volume=Mathf.MoveTowards(Road.volume,running?normalized*.3f:0,Time.unscaledDeltaTime);Road.pitch=.8f+normalized*.4f;
  Run(Engine);Run(Road);if(!Engine.isPlaying&&!Road.isPlaying&&audio!=null)audio.ReleaseVehicle(this);
  if(running){string type=Heavy?"Truck":"Car";if(previousSpeed-speed>Time.deltaTime*3&&previousSpeed>2&&Time.time>brakeAt){GridShiftAudio.At(type+"_Brake",transform.position,"Vehicles",.25f,.4f);brakeAt=Time.time+3;}
   if(Heavy&&car.Diagnostic==TrafficCar.DiagnosticState.REVERSING&&Time.time>reverseAt){GridShiftAudio.At("Truck_Reverse",transform.position,"Vehicles",.28f,.5f);reverseAt=Time.time+1.2f;}
   if(car.MotivoParada==TrafficCar.StopReason.Obstaculo&&car.StationarySeconds>14&&Time.time>hornAt){GridShiftAudio.At(type+"_Horn",transform.position,"Vehicles",.22f,12);hornAt=Time.time+45;}
  }previousSpeed=speed;
 }
 static void Run(AudioSource source){if(source.volume>.001f){if(!source.isPlaying&&source.clip!=null)source.Play();}else if(source.isPlaying)source.Stop();}
 void OnDisable(){Audible=false;if(Engine!=null)Engine.Stop();if(Road!=null)Road.Stop();if(GridShiftAudio.Instance!=null)GridShiftAudio.Instance.ReleaseVehicle(this);}
 void OnDestroy(){if(Engine!=null)Destroy(Engine.gameObject);if(Road!=null)Destroy(Road.gameObject);}
}
