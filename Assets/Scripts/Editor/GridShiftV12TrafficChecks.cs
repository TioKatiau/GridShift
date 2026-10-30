using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
public static class GridShiftV12TrafficChecks {
 static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Set(object o,string key,object value)=>o.GetType().GetField(key,Flags).SetValue(o,value);
 public static void Baseline()=>Run(false);
 public static void Batch()=>Run(true);
 static void Run(bool final){Directory.CreateDirectory("Docs/Validacao12");var log=new StringBuilder();bool ok=true;void Check(bool value,string name){log.AppendLine((value?"PASS ":"FAIL ")+name);ok&=value;}EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var root=new GameObject("V12Queue");var prior=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
 try{UnityEngine.Random.InitState(120);Transform Point(string name,float z){var go=new GameObject(name);go.transform.SetParent(root.transform);go.transform.position=Vector3.forward*z;return go.transform;}
 var route=Point("Lane",0).gameObject.AddComponent<TrafficRoute>();route.pontos=new[]{Point("Start",-100),Point("End",180)};var junction=Point("Junction",8).gameObject.AddComponent<TrafficJunction>();var light=Point("Signal",0).gameObject.AddComponent<TrafficLight>();light.SetGroupControl();light.DefinirEstado(TrafficLight.Estado.Vermelho);Set(route,"cruzamento",junction);Set(route,"semaforo",light);Set(route,"linhaParada",Point("Stop",0));Set(route,"saidaCruzamento",Point("Exit",20));
 var cars=new TrafficCar[5];float z=-3.2f,previousHalf=2;for(int i=0;i<5;i++){float half=i==2?4:2;if(i>0)z-=previousHalf+half+2.7f;var go=Point("Queue-"+i,z).gameObject;go.layer=30;var box=go.AddComponent<BoxCollider>();box.size=new Vector3(1.8f,1.8f,half*2);box.center=Vector3.up;var car=go.AddComponent<TrafficCar>();car.ConfigureProfile(box,i==2,8,2,4);Set(car,"obstaculos",(LayerMask)(1<<30));car.ResetRoute(route);cars[i]=car;previousHalf=half;}
 Physics.SyncTransforms();var tick=typeof(TrafficCar).GetMethod("FixedUpdate",Flags);void Step(){foreach(var c in cars)tick.Invoke(c,null);Physics.Simulate(.02f);}float initial=cars[0].transform.position.z;for(int k=0;k<750;k++)Step();Check(cars[0].transform.position.z+cars[0].BodySize.z/2<=-.9f,"cinco veículos respeitam vermelho");
 var pedestrian=Point("Pedestrian",8).gameObject.AddComponent<PedestrianAgent>();Check(junction.TryCross(pedestrian),"pedestre reserva faixa antes do verde");light.DefinirEstado(TrafficLight.Estado.Verde);for(int k=0;k<75;k++)Step();Check(cars.All(c=>c.transform.position.z+c.BodySize.z/2<0),"verde não permite invadir faixa ocupada por pedestre");junction.LeaveCrossing(pedestrian);Object.DestroyImmediate(pedestrian.gameObject);
 float[] starts=Enumerable.Repeat(-1f,5).ToArray();bool collision=false;float minGap=float.PositiveInfinity;for(int k=0;k<800;k++){Step();for(int i=0;i<5;i++){if(starts[i]<0&&cars[i].VelocidadeAtual>.2f)starts[i]=k*.02f;if(i>0){float gap=cars[i-1].transform.position.z-cars[i].transform.position.z-(cars[i-1].BodySize.z+cars[i].BodySize.z)/2;minGap=Mathf.Min(minGap,gap);}for(int j=i+1;j<5;j++){var a=cars[i].GetComponent<BoxCollider>();var b=cars[j].GetComponent<BoxCollider>();collision|=Physics.ComputePenetration(a,cars[i].transform.position,cars[i].transform.rotation,b,cars[j].transform.position,cars[j].transform.rotation,out _,out float depth)&&depth>.01f;}}}
 int crossed=cars.Count(c=>c.transform.position.z-c.BodySize.z/2>0);log.AppendLine("Green16s crossed="+crossed+" starts="+string.Join("; ",starts.Select(x=>x.ToString("F2")))+" minimum bumper gap="+minGap.ToString("F3"));Check(!collision&&minGap>=1.9f,"fila mista mantém folga e não apresenta interpenetração");if(final)Check(crossed>=4&&starts.All(x=>x>=0)&&starts[1]<4,"vários veículos seguem no mesmo verde sem esperar liberação total");
 // A stationary body downstream must still prevent admission.
 foreach(var c in cars)c.gameObject.SetActive(false);var obstacle=Point("DownstreamObstacle",24).gameObject;obstacle.layer=30;obstacle.AddComponent<BoxCollider>().size=new Vector3(2,2,6);var waiting=cars[0];waiting.gameObject.SetActive(true);waiting.SynchronizeSpawnPose(Vector3.back*3.2f,Quaternion.identity);waiting.ResetRoute(route);Physics.SyncTransforms();for(int k=0;k<400;k++){tick.Invoke(waiting,null);Physics.Simulate(.02f);}Check(waiting.transform.position.z+waiting.BodySize.z/2<0,"saída bloqueada impede entrada mesmo com verde");
 }catch(Exception e){ok=false;log.AppendLine("FAIL "+e);}finally{Object.DestroyImmediate(root);Physics.simulationMode=prior;Physics.SyncTransforms();}log.AppendLine("RESULT "+(ok?"SUCCESS":"FAILURE"));File.WriteAllText("Docs/Validacao12/traffic-"+(final?"final":"baseline")+".txt",log.ToString());EditorApplication.Exit(ok?0:1);}
}
