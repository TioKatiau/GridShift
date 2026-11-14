using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Observes the same directed lanes/stop lines used by traffic. Bills use company saves.
public sealed class GridShiftRoadRules:MonoBehaviour {
 GridShiftPlayerVan van;TrafficRoute[] routes;Vector3 previous;float sampleAt,wrongSeconds;readonly Dictionary<string,float> cooldown=new Dictionary<string,float>();
 public int LightMode {get;private set;}=1;public int Indicator {get;private set;}
 GridShiftVehicleLamps lamps;
 void Start(){van=GetComponent<GridShiftPlayerVan>();routes=FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None);previous=transform.position;lamps=gameObject.GetComponent<GridShiftVehicleLamps>();if(lamps==null)lamps=gameObject.AddComponent<GridShiftVehicleLamps>();}
 public void SetLights(int mode){LightMode=Mathf.Clamp(mode,0,2);if(lamps!=null)lamps.mode=LightMode;}
 public void SetIndicator(int side){Indicator=Indicator==side?0:Mathf.Clamp(side,-1,2);if(lamps!=null)lamps.indicator=Indicator;}
 void Update(){if(van==null||van.Manager==null||!van.Manager.Company.Ready)return;
  bool controls=van.Driving&&Time.timeScale>0&&!GridShiftOperationsUI.ModalOpen&&!GridShiftCampaignUI.ModalOpen&&PauseMenu.Instance?.IsOpen!=true;
  if(controls&&Keyboard.current!=null){var k=Keyboard.current;if(GridShiftControls.Down(GSAction.VehicleLights))SetLights((LightMode+1)%3);if(GridShiftControls.Down(GSAction.LeftIndicator))SetIndicator(-1);if(GridShiftControls.Down(GSAction.RightIndicator))SetIndicator(1);if(GridShiftControls.Down(GSAction.Hazard))SetIndicator(2);}
  if(Time.time<sampleAt)return;float dt=.15f;sampleAt=Time.time+dt;var now=transform.position;
  if(controls&&van.Speed>1&&Vector3.Distance(previous,now)<8){
   foreach(var route in routes)if(route!=null&&route.gameObject.scene==gameObject.scene&&route.GetSemaforo()!=null&&route.GetParada(out var stop,out var dir))if(CrossedRed(previous,now,stop,dir,route.GetSemaforo().EstaVermelho()))Issue("red-light","Avanço de sinal vermelho",35,12);
   if(WrongWay(now,Vector3.ProjectOnPlane(van.GetComponent<Rigidbody>().linearVelocity,Vector3.up).normalized,routes)){wrongSeconds+=dt;if(wrongSeconds>=2){Issue("wrong","Circulação na contramão",30,30);wrongSeconds=0;}}else wrongSeconds=0;
  }else wrongSeconds=0;previous=now;
 }
 public static bool CrossedRed(Vector3 from,Vector3 to,Vector3 stop,Vector3 direction,bool red){if(!red||Mathf.Abs(to.y-stop.y)>2)return false;float a=Vector3.Dot(from-stop,direction),b=Vector3.Dot(to-stop,direction);if(a>=0||b<0||b-a<.05f)return false;var crossing=Vector3.Lerp(from,to,-a/(b-a));return Vector3.ProjectOnPlane(crossing-stop,direction).magnitude<2;}
 public static bool WrongWay(Vector3 at,Vector3 forward,TrafficRoute[] lanes){float best=1.8f*1.8f;Vector3 direction=Vector3.zero;bool ambiguous=false;foreach(var lane in lanes){if(lane==null||!lane.Valida||lane.Cruzamento!=null&&Vector3.ProjectOnPlane(at-lane.Cruzamento.transform.position,Vector3.up).sqrMagnitude<144)continue;for(int i=1;i<lane.pontos.Length;i++){var a=lane.pontos[i-1].position;var ab=Vector3.ProjectOnPlane(lane.pontos[i].position-a,Vector3.up);if(ab.sqrMagnitude<.01f||Mathf.Abs(at.y-a.y)>1.5f)continue;float t=Vector3.Dot(at-a,ab)/ab.sqrMagnitude;if(t<.1f||t>.9f)continue;float d=Vector3.ProjectOnPlane(at-(a+ab*t),Vector3.up).sqrMagnitude;if(d<best-.04f){best=d;direction=ab.normalized;ambiguous=false;}else if(Mathf.Abs(d-best)<.04f&&Vector3.Dot(direction,ab.normalized)<0)ambiguous=true;}}return !ambiguous&&direction!=Vector3.zero&&Vector3.Dot(forward,direction)<-.65f;}
 public bool Issue(string key,string description,float amount,float seconds=10){if(van==null||van.Manager==null||!van.Manager.Company.Ready||cooldown.TryGetValue(key,out var until)&&Time.time<until)return false;cooldown[key]=Time.time+seconds;var c=van.Manager.Company;var id="traffic:"+Guid.NewGuid().ToString("N");c.Finance.Bill(id,description+" · infração fictícia de gameplay","Multas",amount,c.Day+3,(int)(c.Day/7)+1);c.Save();van.Manager.Notify("MULTA · "+description+" · R$ "+amount.ToString("0")+" · Consulte Financeiro");return true;}
 void OnCollisionEnter(Collision hit){if(van==null||!van.Driving||hit.relativeVelocity.magnitude<2)return;if(hit.collider.GetComponentInParent<TrafficCar>()!=null)Issue("collision:"+hit.collider.GetInstanceID(),"Colisão com veículo",45,15);var person=hit.collider.GetComponentInParent<GridShiftCitizen>();if(person!=null&&person.Strike(hit.relativeVelocity))Issue("pedestrian:"+person.GetInstanceID(),"Atropelamento de pedestre",90,30);}
}



