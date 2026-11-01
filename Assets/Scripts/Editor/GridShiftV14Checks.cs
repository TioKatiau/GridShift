using System;
using System.IO;
using System.Text;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GridShiftV14Checks {
 public static void Batch(){
  var report=new StringBuilder();bool ok=true;
  void Check(bool condition,string label){report.AppendLine((condition?"PASS ":"FAIL ")+label);ok&=condition;}
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var root=new GameObject("V14 physics regression");
  try {
   var manager=root.AddComponent<PedestrianManager>();
   Vector3 start=new Vector3(0,.025f,0),end=new Vector3(0,.025f,2);
   Check(manager.CanStep(null,start,end),"corredor livre permite deslocamento");
   var wall=new GameObject("Thin wall");wall.transform.SetParent(root.transform);
   wall.transform.position=new Vector3(0,1,1);wall.AddComponent<BoxCollider>().size=new Vector3(3,2,.04f);
   Physics.SyncTransforms();
   Check(manager.Clear(null,end,.62f),"destino livre isoladamente não detecta parede intermediária");
   Check(!manager.CanStep(null,start,end),"varredura impede atravessar parede fina entre posições");
   wall.transform.position=new Vector3(5,1,1);Physics.SyncTransforms();
   Check(manager.CanStep(null,start,end),"retirar obstáculo libera trajeto sem ignorar colisões");
   wall.transform.position=new Vector3(0,1,2);Physics.SyncTransforms();
   Check(!manager.CanStep(null,start,end),"destino ocupado permanece bloqueado");
   wall.GetComponent<Collider>().isTrigger=true;Physics.SyncTransforms();
   Check(manager.CanStep(null,start,end),"gatilho de interação não bloqueia passagem");
   wall.SetActive(false);
   var floor=new GameObject("Road V14 sidewalk");floor.transform.SetParent(root.transform);
   floor.transform.position=new Vector3(0,-.1f,0);floor.AddComponent<BoxCollider>().size=new Vector3(6,.2f,20);
   manager.graph=root.AddComponent<PedestrianGraph>();
   manager.graph.nodes=new[]{new PedestrianNode{position=start,neighbours=new[]{1}},new PedestrianNode{position=new Vector3(0,.025f,8),neighbours=new[]{0}}};
   var person=new GameObject("Measured pedestrian");person.transform.SetParent(root.transform);
   var agent=person.AddComponent<PedestrianAgent>();agent.Initialize(manager,0,1.25f);
   Check(agent.WalkTo(1),"destino real acessível pelo grafo");
   Physics.SyncTransforms();var previous=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
   try {
    var tick=typeof(PedestrianAgent).GetMethod("FixedUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
    for(int i=0;i<40;i++){tick.Invoke(agent,null);Physics.Simulate(Time.fixedDeltaTime);}
    Check(agent.DistanceWalked>.1f&&agent.CurrentSpeed>.1f,"velocidade e distância refletem caminhada física");
    wall.SetActive(true);wall.GetComponent<Collider>().isTrigger=false;
    wall.transform.position=agent.transform.position+new Vector3(0,1,.32f);Physics.SyncTransforms();
    for(int i=0;i<15;i++){tick.Invoke(agent,null);Physics.Simulate(Time.fixedDeltaTime);}
    Check(agent.CurrentSpeed<.01f,"pedestre bloqueado não mantém animação de caminhada");
   }finally{Physics.simulationMode=previous;}
   // Exercise the real JSON store with disposable files, never personal slots.
   string saveDirectory=Path.GetFullPath("Temp/V14Saves-"+Guid.NewGuid().ToString("N"));
   string savePath=Path.Combine(saveDirectory,"slot-01.json");
   var save=new DeliverySaveData{version=3,slot=1,profileId="v14-isolated",money=42,company=new CompanyCampaignState()};
   Check(DeliverySaveStore.WriteAt(savePath,save),"gravação real de save v3 em destino temporário");
   bool readable=DeliverySaveStore.ReadAt(savePath,out var loaded,out bool recovered);
   Check(readable&&!recovered&&loaded.money==42&&loaded.slot==1,"leitura v3 preserva saldo e slot");
   save.money=84;Check(DeliverySaveStore.WriteAt(savePath,save),"segunda gravação atômica produz backup");
   File.WriteAllText(savePath,"{ invalid JSON");
   readable=DeliverySaveStore.ReadAt(savePath,out loaded,out recovered);
   Check(readable&&recovered&&loaded.money==42,"arquivo corrompido recupera backup anterior válido");
   string future=Path.Combine(saveDirectory,"future.json");
   save.version=99;DeliverySaveStore.WriteAt(future,save);
   Check(!DeliverySaveStore.ReadAt(future,out _,out _),"versão futura desconhecida é recusada");
   string legacy=Path.Combine(saveDirectory,"legacy.json");
   File.WriteAllText(legacy,"{\"version\":1,\"money\":25,\"clock\":720,\"orders\":[]}");
   Check(DeliverySaveStore.ReadAt(legacy,out loaded,out _)&&loaded.money==25&&loaded.vehicles!=null,"partida v1 continua legível sem apagar o original");
  }catch(Exception e){ok=false;report.AppendLine(e.ToString());}
  finally{UnityEngine.Object.DestroyImmediate(root);Physics.SyncTransforms();}
  report.AppendLine("RESULT "+(ok?"SUCCESS":"FAILURE"));
  Directory.CreateDirectory("Docs/Validacao14");File.WriteAllText("Docs/Validacao14/physics.txt",report.ToString());
  EditorApplication.Exit(ok?0:1);
 }
}

