using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
[System.Serializable] public class CityRoutine {
 [Range(0,24)] public float hour;
 [Range(.05f,2)] public float traffic=1,pedestrians=1;
 public CityRoutine(float h,float t,float p){hour=h;traffic=t;pedestrians=p;}
}
[System.Serializable] public class StreetLamp {
 public Light light;public Renderer bulb;public float offset;
}
public class CityEnvironment:MonoBehaviour {
 public TrafficManager traffic;
 public PedestrianManager pedestrians;
 public Transform player;
 public StreetLamp[] lamps=new StreetLamp[0];
 public CityRoutine[] routine={new CityRoutine(0,.25f,.12f),new CityRoutine(5,.25f,.12f),new CityRoutine(6,1.3f,1.25f),new CityRoutine(9,1,1),new CityRoutine(17,1.3f,1.25f),new CityRoutine(20,.65f,.4f),new CityRoutine(23,.25f,.12f)};
 public float sunset=18,sunrise=6,lightDistance=65,nightIntensity=9;
 public int lightBudget=24;
 public Material sky;
 public Color nightAmbient=new Color(.18f,.23f,.34f),dayAmbient=new Color(.45f,.48f,.54f);
 public float NightFactor {get;private set;}
 public float TrafficDensity {get;private set;}=1;
 public float PedestrianDensity {get;private set;}=1;
 Material runtimeSky;MaterialPropertyBlock properties;float updateAt;
 public Vector2 DensityAt(float hour){
  if(routine==null||routine.Length==0)return Vector2.one;
  for(int i=0;i<routine.Length;i++){var a=routine[i];var b=routine[(i+1)%routine.Length];float duration=Mathf.Repeat(b.hour-a.hour,24);float elapsed=Mathf.Repeat(hour-a.hour,24);
   if(elapsed<=duration){float transition=Mathf.Min(.3f,duration);float t=Mathf.SmoothStep(0,1,Mathf.InverseLerp(duration-transition,duration,elapsed));return Vector2.Lerp(new Vector2(a.traffic,a.pedestrians),new Vector2(b.traffic,b.pedestrians),t);}}
  return Vector2.one;
 }
 void Start(){if(sky!=null)runtimeSky=new Material(sky);properties=new MaterialPropertyBlock();}
 void Update(){
  float hour=WorldSession.Instance!=null?WorldSession.Instance.ClockHour:12;
  NightFactor=1-Mathf.SmoothStep(0,1,Mathf.Min(Mathf.InverseLerp(sunrise-.6f,sunrise+.6f,hour),1-Mathf.InverseLerp(sunset-.6f,sunset+.6f,hour)));
  Vector2 desired=DensityAt(hour);TrafficDensity=Mathf.MoveTowards(TrafficDensity,desired.x,Time.deltaTime*.04f);PedestrianDensity=Mathf.MoveTowards(PedestrianDensity,desired.y,Time.deltaTime*.04f);
  if(traffic!=null)traffic.SetRoutineDensity(TrafficDensity);if(pedestrians!=null)pedestrians.routineDensity=PedestrianDensity;
  if(Time.time<updateAt)return;updateAt=Time.time+.25f;
  bool cityVisible=SceneManager.GetActiveScene()==gameObject.scene;
  if(cityVisible){RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=Color.Lerp(dayAmbient,nightAmbient,NightFactor);RenderSettings.ambientEquatorColor=Color.Lerp(dayAmbient*.8f,nightAmbient*.8f,NightFactor);RenderSettings.ambientGroundColor=Color.Lerp(dayAmbient*.5f,nightAmbient*.6f,NightFactor);
   RenderSettings.fogColor=Color.Lerp(new Color(.55f,.65f,.72f),new Color(.025f,.04f,.085f),NightFactor);
   if(runtimeSky!=null){RenderSettings.skybox=runtimeSky;
    if(runtimeSky.HasProperty("_Night")){runtimeSky.SetFloat("_Night",NightFactor);runtimeSky.SetVector("_SunDirection",RenderSettings.sun!=null?-RenderSettings.sun.transform.forward:Vector3.up);}
    else{if(runtimeSky.HasProperty("_SkyTint"))runtimeSky.SetColor("_SkyTint",Color.Lerp(new Color(.45f,.55f,.65f),new Color(.1f,.16f,.3f),NightFactor));if(runtimeSky.HasProperty("_Exposure"))runtimeSky.SetFloat("_Exposure",Mathf.Lerp(1.1f,.5f,NightFactor));}
   }
  }
  foreach(var lamp in lamps){if(lamp.light==null)continue;float factor=Mathf.SmoothStep(0,1,Mathf.InverseLerp(lamp.offset,.9f,NightFactor));
   lamp.light.intensity=nightIntensity*factor*(1+.015f*Mathf.Sin(Time.time*9+lamp.offset*100)*4*factor*(1-factor));
   bool nearby=cityVisible&&player!=null&&(player.position-lamp.light.transform.position).sqrMagnitude<lightDistance*lightDistance;int closer=0;
   if(nearby)foreach(var other in lamps)if(other.light!=null&&(other.light.transform.position-player.position).sqrMagnitude<(lamp.light.transform.position-player.position).sqrMagnitude)closer++;
   lamp.light.enabled=nearby&&factor>.001f&&closer<lightBudget;
   if(lamp.bulb!=null){properties.SetColor("_EmissionColor",new Color(1,.72f,.35f)*factor*2);properties.SetColor("_BaseColor",Color.Lerp(new Color(.15f,.14f,.1f),new Color(1,.8f,.45f),factor));lamp.bulb.SetPropertyBlock(properties);}
  }
 }
 void OnDestroy(){if(runtimeSky!=null)Destroy(runtimeSky);}
}
