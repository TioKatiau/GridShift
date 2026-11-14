using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-10000)]
public sealed class GridShiftAudio : MonoBehaviour {
 public static GridShiftAudio Instance {get;private set;}
 public static readonly string[] Channels={"Master","Music","Ambience","Vehicles","Footsteps","UI","SFX"};
 public GridShiftAudioLibrary Library {get;private set;}
 public int PlayedEvents {get;private set;}
 readonly Dictionary<string,int> played=new Dictionary<string,int>();
 public int Count(string id)=>played.TryGetValue(id,out var n)?n:0;
 static readonly Unity.Profiling.ProfilerMarker marker=new Unity.Profiling.ProfilerMarker("GridShift.Audio.Update");
 public string LastEvent {get;private set;}
 public static string LastAttempt {get;private set;}
 public string LastRejected {get;private set;}
 struct Request {public string id,channel;public float gain,interval;}
 static readonly Queue<Request> pending=new Queue<Request>();
 readonly HashSet<GridShiftVehicleAudio> vehicleSlots=new HashSet<GridShiftVehicleAudio>();
 public int ActiveVehicleVoices=>vehicleSlots.Count;
 public bool ClaimVehicle(GridShiftVehicleAudio v){if(vehicleSlots.Contains(v))return true;if(vehicleSlots.Count>=VehicleBudget)return false;vehicleSlots.Add(v);return true;}
 public void ReleaseVehicle(GridShiftVehicleAudio v){vehicleSlots.Remove(v);}
 public int ActiveOneShots {get{int n=0;foreach(var s in shots)if(s.isPlaying)n++;return n;}}
 public const int VehicleBudget=6,OneShotBudget=10;
 readonly AudioSource[] shots=new AudioSource[OneShotBudget];
 readonly Dictionary<string,float> cooldown=new Dictionary<string,float>();
 readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
 readonly Dictionary<string,AudioMixerGroup> groups=new Dictionary<string,AudioMixerGroup>();
 readonly float[] volumes=new float[7];
 readonly TrafficCar[] nearest=new TrafficCar[VehicleBudget];
 readonly float[] distances=new float[VehicleBudget];
 readonly List<GridShiftVehicleAudio> vehicles=new List<GridShiftVehicleAudio>();
 readonly List<GridShiftVehicleAudio> attached=new List<GridShiftVehicleAudio>();
 AudioSource musicA,musicB,day,night,office,green;
 AudioListener fallback,listener;PlayerMovement player;PedestrianAgent[] pedestrians=new PedestrianAgent[0];
 float scanAt,worldAt,nextChirp,nextMurmur;string musicId;float musicMix,indoor,dayMix;int stepVariant,jumps;
 Vector3 lastPlayer;bool grounded,havePlayer;float stride;
 DeliveryManager observedManager;readonly HashSet<string> lateOrders=new HashSet<string>();
 public static float ToDecibels(float linear)=>linear<=.0001f?-80:20*Mathf.Log10(Mathf.Clamp01(linear));
 public static float GetVolume(string channel)=>PlayerPrefs.GetFloat("GS.Audio."+channel,channel=="Master"?PlayerPrefs.GetFloat("GS.Volume",1):channel=="Music"?.55f:1);
 public static void SetVolume(string channel,float value){if(float.IsNaN(value)||float.IsInfinity(value))return;PlayerPrefs.SetFloat("GS.Audio."+channel,Mathf.Clamp01(value));}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset(){Instance=null;pending.Clear();LastAttempt=null;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void Install(){if(Instance==null)new GameObject("GridShiftAudio13").AddComponent<GridShiftAudio>();}
 void Awake(){if(Instance!=null){Destroy(gameObject);return;}Instance=this;DontDestroyOnLoad(gameObject);Library=Resources.Load<GridShiftAudioLibrary>("GridShiftAudioLibrary");
  if(Library==null){Debug.LogError("GridShift: audio library missing. Run GridShift/Audio 13/Install and validate.");enabled=false;return;}
  foreach(var e in Library.clips)if(e.clip!=null)clips[e.id]=e.clip;foreach(var g in Library.groups)if(g!=null)groups[g.name]=g;
  for(int i=0;i<Channels.Length;i++)volumes[i]=GetVolume(Channels[i]);
  for(int i=0;i<shots.Length;i++)shots[i]=Source("OneShot-"+i,"SFX",false);
  musicA=Source("Music-A","Music",true);musicB=Source("Music-B","Music",true);
  day=Loop("City_Ambience_Day","Ambience");night=Loop("City_Ambience_Night","Ambience");office=Loop("Office_Ventilation","Ambience");green=Loop("Green_Wind","Ambience");
  fallback=gameObject.AddComponent<AudioListener>();fallback.enabled=false;
  SceneManager.sceneLoaded+=Loaded;SceneManager.activeSceneChanged+=Changed;
 }
 void Start(){AudioListener.volume=1;Scan();ApplyVolumes(true);while(pending.Count>0){var r=pending.Dequeue();Emit(r.id,r.channel,r.gain,Vector3.zero,false,r.interval);}}
 void OnDestroy(){SceneManager.sceneLoaded-=Loaded;SceneManager.activeSceneChanged-=Changed;if(Instance==this){Instance=null;foreach(var v in attached)if(v!=null)Destroy(v);}}
 void OnApplicationPause(bool paused){if(paused)PlayerPrefs.Save();}void OnApplicationQuit(){PlayerPrefs.Save();}
 void Loaded(Scene s,LoadSceneMode m){Scan();}void Changed(Scene a,Scene b){Scan();havePlayer=false;pedestrianPosition.Clear();pedestrianStride.Clear();}
 public AudioSource Source(string name,string group,bool loop,Transform parent=null){var go=new GameObject(name);go.transform.SetParent(parent!=null?parent:transform,false);var s=go.AddComponent<AudioSource>();s.playOnAwake=false;s.loop=loop;s.volume=0;s.dopplerLevel=0;s.reverbZoneMix=0;s.outputAudioMixerGroup=Group(group);s.priority=group=="UI"?32:group=="Music"?64:160;return s;}
 public AudioMixerGroup Group(string group)=>groups.TryGetValue(group,out var g)?g:null;
 public AudioClip Clip(string id)=>clips.TryGetValue(id,out var c)?c:null;
 AudioSource Loop(string id,string group){var s=Source(id,group,true);s.clip=Clip(id);if(s.clip!=null)s.Play();return s;}
 public static bool Play(string id,string channel="SFX",float gain=.5f,float interval=.12f){LastAttempt=id+" instance="+(Instance!=null);if(Instance==null){if(pending.Count<16)pending.Enqueue(new Request{id=id,channel=channel,gain=gain,interval=interval});return false;}return Instance.Emit(id,channel,gain,Vector3.zero,false,interval);}
 public static bool At(string id,Vector3 position,string channel,float gain=.4f,float interval=.08f)=>Instance!=null&&Instance.Emit(id,channel,gain,position,true,interval);
 bool Emit(string id,string channel,float gain,Vector3 position,bool spatial,float interval){if(!enabled||Library==null){LastRejected=id+": not initialized";return false;}var c=Clip(id);if(c==null){LastRejected=id+": missing clip";return false;}
  if(cooldown.TryGetValue(id,out var at)&&Time.unscaledTime<at){LastRejected=id+": cooldown";return false;}
  // Four reserved interface voices; world voices can never evict confirmations.
  bool ui=channel=="UI";int start=ui?0:4,end=ui?4:shots.Length;AudioSource s=null;int priority=ui?32:id=="Act_Unlocked"||id=="Mission_Completed"?50:id=="Delivery_Completed"?70:channel=="Footsteps"?100:channel=="Ambience"?170:90;
  for(int i=start;i<end;i++)if(shots[i].clip==null||!shots[i].isPlaying){s=shots[i];break;}
  if(s==null)for(int i=start;i<end;i++)if(shots[i].priority>priority&&(s==null||shots[i].priority>s.priority))s=shots[i];if(s==null){LastRejected=id+": pool full";return false;}
  cooldown[id]=Time.unscaledTime+interval;s.Stop();s.clip=c;s.outputAudioMixerGroup=Group(channel);s.spatialBlend=spatial?1:0;s.transform.position=spatial?position:transform.position;s.rolloffMode=AudioRolloffMode.Linear;s.minDistance=1.5f;s.maxDistance=24;s.priority=priority;s.pitch=1;s.volume=Mathf.Clamp01(gain);s.Play();PlayedEvents++;LastEvent=id;played[id]=Count(id)+1;return true;
 }
 void ApplyVolumes(bool instant){for(int i=0;i<Channels.Length;i++){volumes[i]=instant?GetVolume(Channels[i]):Mathf.MoveTowards(volumes[i],GetVolume(Channels[i]),Time.unscaledDeltaTime*2);Library.mixer.SetFloat(Channels[i]+"Volume",ToDecibels(volumes[i]));}}
 void Scan(){scanAt=Time.unscaledTime+1;var active=SceneManager.GetActiveScene();PlayerMovement next=null;foreach(var p in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))if(p.gameObject.scene==active&&p.isActiveAndEnabled){next=p;break;}
  if(next!=player){player=next;havePlayer=false;stride=0;if(player!=null){lastPlayer=player.transform.position;grounded=player.IsGrounded;jumps=player.JumpsExecuted;}}
  Camera view=Camera.main;if(view==null&&player!=null&&player.CameraTransform!=null)view=player.CameraTransform.GetComponent<Camera>();
  AudioListener chosen=null;if(view!=null&&view.isActiveAndEnabled){chosen=view.GetComponent<AudioListener>();if(chosen==null)chosen=view.gameObject.AddComponent<AudioListener>();}
  if(chosen==null)chosen=fallback;
  foreach(var l in FindObjectsByType<AudioListener>(FindObjectsInactive.Include,FindObjectsSortMode.None))l.enabled=l==chosen;
  chosen.enabled=true;listener=chosen;
  foreach(var b in FindObjectsByType<Button>(FindObjectsSortMode.None))if(b.GetComponent<GridShiftAudioButton>()==null)b.gameObject.AddComponent<GridShiftAudioButton>();
  foreach(var car in TrafficCar.Active)if(car!=null&&car.GetComponent<GridShiftVehicleAudio>()==null){var v=car.gameObject.AddComponent<GridShiftVehicleAudio>();attached.Add(v);}
  pedestrians=FindObjectsByType<PedestrianAgent>(FindObjectsSortMode.None);
 }
 void Update(){using(marker.Auto())Tick();}
 void Tick(){if(Library==null)return;if(Time.unscaledTime>=scanAt)Scan();ApplyVolumes(false);
  bool menu=SceneManager.GetActiveScene().name=="MainMenu",inside=SceneManager.GetActiveScene().name=="Apartment"||SceneManager.GetActiveScene().name=="Office"||SceneManager.GetActiveScene().name=="FactoryInterior";
  float dt=Time.unscaledDeltaTime;indoor=Mathf.MoveTowards(indoor,inside?1:0,dt*.8f);
  float hour=WorldSession.Instance!=null?WorldSession.Instance.ClockHour:12;
  float daylight=Mathf.SmoothStep(0,1,Mathf.InverseLerp(5,7,hour))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(17,19,hour)));
  dayMix=Mathf.MoveTowards(dayMix,daylight,dt*.3f);float outdoors=menu?0:1-indoor;
  day.volume=outdoors*dayMix*.6f;night.volume=outdoors*(1-dayMix)*.45f;office.volume=menu?0:indoor*.28f;
  float park=0;if(listener!=null)foreach(var z in GridShiftAudioZone.Active)if(z!=null)park=Mathf.Max(park,1-Mathf.Clamp01(Vector3.Distance(listener.transform.position,z.transform.position)/Mathf.Max(1,z.radius)));
  green.volume=outdoors*park*.35f;
  bool important=false;var m=DeliveryManager.Instance;if(m!=null&&m.Company!=null&&m.Company.Ready){var mission=m.Company.Missions.Selected;if(mission!=null&&mission.type=="Especial"&&mission.status==CompanyMissionStatus.Active&&!mission.rewardClaimed)foreach(var order in m.orders)if(order.id==mission.orderId&&order.status!=DeliveryStatus.Completed){important=true;break;}}
  string desired=menu?"Menu_CityInMotion":inside?"Office_AfterHours":important?"Mission_ForwardMotion":null;
  if(WorldSession.Instance!=null&&WorldSession.Instance.Busy)desired=null;Music(desired,dt);
  if(Time.unscaledTime>=worldAt){worldAt=Time.unscaledTime+.25f;SelectVehicles(menu||inside||Time.timeScale<=0);Deadlines(m);}
  if(!menu&&!inside&&Time.timeScale>0)People(park,daylight);
 }
 public void RefreshListener(){Scan();}
 void LateUpdate(){if(listener==null||!listener.gameObject.activeInHierarchy||(listener.TryGetComponent<Camera>(out var listenerCamera)&&listenerCamera!=null&&!listenerCamera.isActiveAndEnabled))Scan();Footsteps();}
 void Deadlines(DeliveryManager m){if(m==null){observedManager=null;lateOrders.Clear();return;}bool baseline=observedManager!=m||m.Restoring;if(baseline){observedManager=m;lateOrders.Clear();}
  foreach(var o in m.orders)if(o.status!=DeliveryStatus.Completed&&o.deadlineSeconds>0&&o.elapsedSeconds>o.deadlineSeconds&&lateOrders.Add(o.id)&&!baseline)Play("Delivery_Late","SFX",.25f,2);
 }
 void Music(string id,float dt){if(id!=musicId){var old=musicA;musicA=musicB;musicB=old;musicId=id;musicA.Stop();musicA.clip=id==null?null:Clip(id);musicA.volume=0;if(musicA.clip!=null)musicA.Play();musicMix=0;}
  musicMix=Mathf.MoveTowards(musicMix,1,dt*.45f);musicA.volume=Mathf.MoveTowards(musicA.volume,musicId==null?0:.48f,dt*.24f);musicB.volume=Mathf.MoveTowards(musicB.volume,0,dt*.24f);if(musicB.volume<=0&&musicB.isPlaying)musicB.Stop();
 }
 void SelectVehicles(bool silent){for(int i=0;i<VehicleBudget;i++){nearest[i]=null;distances[i]=float.MaxValue;}
  if(!silent&&listener!=null)foreach(var car in TrafficCar.Active){if(car==null||!car.isActiveAndEnabled)continue;float d=(car.transform.position-listener.transform.position).sqrMagnitude;if(d>65*65)continue;for(int i=0;i<VehicleBudget;i++)if(d<distances[i]){for(int j=VehicleBudget-1;j>i;j--){distances[j]=distances[j-1];nearest[j]=nearest[j-1];}distances[i]=d;nearest[i]=car;break;}}
  foreach(var car in TrafficCar.Active){if(car==null)continue;var v=car.GetComponent<GridShiftVehicleAudio>();if(v==null)continue;bool selected=false;for(int i=0;i<VehicleBudget;i++)if(nearest[i]==car){selected=true;break;}v.Audible=selected;}
 }
 void Footsteps(){if(player==null)return;Vector3 now=player.transform.position,delta=now-lastPlayer;lastPlayer=now;
  if(!havePlayer){havePlayer=true;grounded=player.IsGrounded;jumps=player.JumpsExecuted;return;}
  if(!player.enabled||player.ControlsBlocked||Time.timeScale<=0||(WorldSession.Instance!=null&&WorldSession.Instance.Busy)){grounded=player.IsGrounded;jumps=player.JumpsExecuted;stride=0;return;}
  bool on=player.IsGrounded;if(player.JumpsExecuted!=jumps){jumps=player.JumpsExecuted;Play("Jump","Footsteps",.35f);}
  if(!grounded&&on&&Time.timeScale>0)Play("Land","Footsteps",.5f,.3f);grounded=on;
  if(!player.enabled||player.ControlsBlocked||Time.timeScale<=0||(WorldSession.Instance!=null&&WorldSession.Instance.Busy)||!on||delta.sqrMagnitude>4){stride=0;return;}
  delta.y=0;if(delta.sqrMagnitude<.000001f)return;stride+=delta.magnitude;float length=player.IsRunning?1.65f:1.95f;
  if(stride>=length){stride%=length;string surface=SceneManager.GetActiveScene().name!="SampleScene"?"Office":"Concrete";
   if(Physics.Raycast(now+Vector3.up*.4f,Vector3.down,out var hit,2,~0,QueryTriggerInteraction.Ignore)){var tag=hit.collider.GetComponentInParent<GridShiftFootstepSurface>();if(tag!=null)surface=tag.surface.ToString();else if(hit.collider is TerrainCollider)surface="Grass";}
   Play("Footstep_"+surface+"_"+(stepVariant++%3+1),"Footsteps",player.IsRunning?.65f:.42f,.03f);
  }
 }
 readonly Dictionary<int,Vector3> pedestrianPosition=new Dictionary<int,Vector3>();
 readonly Dictionary<int,float> pedestrianStride=new Dictionary<int,float>();
 void People(float park,float daylight){if(listener==null)return;
  // Distance accumulation uses real NPC displacement, no footsteps while waiting.
  int close=0;foreach(var p in pedestrians){if(p==null||!p.isActiveAndEnabled)continue;int id=p.GetInstanceID();Vector3 at=p.transform.position;bool known=pedestrianPosition.TryGetValue(id,out var old);pedestrianPosition[id]=at;
   if(!known||(at-listener.transform.position).sqrMagnitude>14*14)continue;Vector3 delta=at-old;delta.y=0;if(delta.sqrMagnitude>1||delta.sqrMagnitude<.0000001f)continue;
   float accumulated=(pedestrianStride.TryGetValue(id,out var s)?s:0)+delta.magnitude;if(accumulated>1.5f){pedestrianStride[id]=0;if(close++<2)At("Footstep_Concrete_"+(stepVariant++%3+1),at,"Footsteps",.22f,.04f);}else pedestrianStride[id]=accumulated;
   if(Time.time>=nextMurmur&&daylight>.5f){At("Pedestrian_Murmur",at,"Ambience",.12f,10);nextMurmur=Time.time+18+(id&15);}
  }
  if(park>.3f&&Time.time>=nextChirp){At(daylight>.5f?"Bird_Chirp":"Night_Insects",listener.transform.position+listener.transform.right*5,"Ambience",.15f,5);nextChirp=Time.time+8+(stepVariant%7);}
 }
}
