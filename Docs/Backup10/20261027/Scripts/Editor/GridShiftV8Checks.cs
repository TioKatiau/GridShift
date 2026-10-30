using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class GridShiftV8Checks
{
    public static void Audit()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        var report=new StringBuilder();
        foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Road ")))
            report.AppendLine(r.name+" | "+r.bounds+" | pos="+r.transform.position+" rot="+r.transform.eulerAngles+" scale="+r.transform.lossyScale+" | prefab="+PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(r.gameObject));
        foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())report.AppendLine("ROOT "+root.name+" children="+root.transform.childCount);
        File.WriteAllText("v8-audit.txt",report.ToString());EditorApplication.Exit(0);
    }
    public static void Traffic()
    {
        var report=new StringBuilder();
        try{EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);TrafficBehaviourChecks.Run();report.AppendLine("PASS existing traffic physics regression suite");ReverseSafety(report);report.AppendLine("RESULT SUCCESS");}
        catch(Exception e){report.AppendLine("FAIL "+e);}
        File.WriteAllText("v8-traffic.txt",report.ToString());EditorApplication.Exit(report.ToString().Contains("FAIL")?1:0);
    }
    static void ReverseSafety(StringBuilder report)
    {
        var root=new GameObject("ReverseSafety8");var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;var oldMode=Physics.simulationMode;
        try{
            Physics.simulationMode=SimulationMode.Script;
            var road=GameObject.CreatePrimitive(PrimitiveType.Cube);road.name="Road Test";road.transform.SetParent(root.transform);road.transform.position=new Vector3(0,-.25f,50);road.transform.localScale=new Vector3(12,.5f,160);
            var route=new GameObject("TestRoute").AddComponent<TrafficRoute>();route.transform.SetParent(root.transform);route.pontos=new Transform[2];for(int i=0;i<2;i++){var p=new GameObject("Point").transform;p.SetParent(root.transform);p.position=Vector3.forward*i*100;route.pontos[i]=p;}
            var go=new GameObject("TestCar");go.transform.SetParent(root.transform);var box=go.AddComponent<BoxCollider>();box.center=Vector3.up*1.1f;box.size=new Vector3(2,2,6);var car=go.AddComponent<TrafficCar>();car.ConfigureProfile(box,false,5,2,4);car.ResetRoute(route);
            var update=typeof(TrafficCar).GetMethod("FixedUpdate",flags);var late=typeof(TrafficCar).GetMethod("LateUpdate",flags);
            for(int i=0;i<300;i++){update.Invoke(car,null);late.Invoke(car,null);Physics.Simulate(Time.fixedDeltaTime);}
            var junction=new GameObject("ReservedJunction").AddComponent<TrafficJunction>();junction.transform.SetParent(root.transform);junction.TryEnter(car);typeof(TrafficCar).GetField("reserva",flags).SetValue(car,junction);typeof(TrafficCar).GetField("autorizado",flags).SetValue(car,true);
            float before=car.transform.position.z;
            typeof(TrafficCar).GetField("velocidadeAtual",flags).SetValue(car,0f);typeof(TrafficCar).GetField("correctionNeeded",flags).SetValue(car,true);
            for(int i=0;i<90;i++){update.Invoke(car,null);Physics.Simulate(Time.fixedDeltaTime);}
            if(car.transform.position.z>=before-.25f||car.transform.position.z<before-2.05f)throw new Exception("Ré segura não respeitou distância: "+(before-car.transform.position.z));
            report.AppendLine("PASS ré curta com reserva exclusiva percorre histórico real, em asfalto, sem teleporte");
            // Start a fresh request and place a pedestrian-size obstacle directly behind the body.
            typeof(TrafficCar).GetField("reversing",flags).SetValue(car,false);typeof(TrafficCar).GetField("maneuverCooldown",flags).SetValue(car,0f);typeof(TrafficCar).GetField("maneuverAttempts",flags).SetValue(car,0);typeof(TrafficCar).GetField("correctionNeeded",flags).SetValue(car,true);
            var obstacle=GameObject.CreatePrimitive(PrimitiveType.Capsule);obstacle.name="RearPedestrian";obstacle.transform.SetParent(root.transform);obstacle.transform.position=car.transform.position+Vector3.back*3.4f+Vector3.up;Physics.SyncTransforms();float stopped=car.transform.position.z;
            for(int i=0;i<90;i++){update.Invoke(car,null);Physics.Simulate(Time.fixedDeltaTime);}
            if(Mathf.Abs(car.transform.position.z-stopped)>.03f)throw new Exception("Ré atravessou obstáculo traseiro");report.AppendLine("PASS obstáculo traseiro impede ré e mantém área ocupada protegida");
            Object.DestroyImmediate(obstacle);Object.DestroyImmediate(go);
            go=new GameObject("ParkingTestCar");go.transform.SetParent(root.transform);box=go.AddComponent<BoxCollider>();box.center=Vector3.up*1.1f;box.size=new Vector3(2,2,6);car=go.AddComponent<TrafficCar>();car.ConfigureProfile(box,false,5,2,4);car.ResetRoute(route);
            var bay=new GameObject("TestBay").AddComponent<TrafficParkingBay>();bay.transform.SetParent(root.transform);bay.width=2.5f;bay.length=7;bay.approach=go.transform;
            var poses=new System.Collections.Generic.List<Transform>();var forward=new GameObject("Forward").transform;forward.SetParent(root.transform);forward.position=Vector3.forward*10;poses.Add(forward);
            var entry=new System.Collections.Generic.List<TrafficParkingBay.Step>{new TrafficParkingBay.Step{point=forward}};Vector3 position=forward.position;float angle=Mathf.Acos(1-2.4f/9)*Mathf.Rad2Deg;
            for(int half=0;half<2;half++)for(int i=1;i<=30;i++){float prior=half==0?-angle*(i-1)/30:-angle+angle*(i-1)/30;float yaw=half==0?-angle*i/30:-angle+angle*i/30;position-=Quaternion.Euler(0,(prior+yaw)*.5f,0)*Vector3.forward*(4.5f*angle*Mathf.Deg2Rad/30);var point=new GameObject("Arc").transform;point.SetParent(root.transform);point.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));poses.Add(point);entry.Add(new TrafficParkingBay.Step{point=point,reverse=true});}
            bay.entry=entry.ToArray();bay.exit=poses.AsEnumerable().Reverse().Select(p=>new TrafficParkingBay.Step{point=p}).ToArray();Physics.SyncTransforms();
            if(!car.RequestParking(bay))throw new Exception("Vaga válida recusada");
            for(int i=0;i<14000&&car.Diagnostic!=TrafficCar.DiagnosticState.PARKED;i++){update.Invoke(car,null);Physics.Simulate(Time.fixedDeltaTime);}
            if(car.Diagnostic!=TrafficCar.DiagnosticState.PARKED||Vector3.Distance(car.transform.position,position)>.35f)throw new Exception("Baliza não concluiu: "+car.transform.position+" alvo="+position+" estado="+car.Diagnostic);
            report.AppendLine("PASS baliza com avanço, ré em dois arcos e alinhamento final");
            if(!car.LeaveParking())throw new Exception("Saída da vaga recusada");
            for(int i=0;i<14000&&bay.Occupant!=null;i++){update.Invoke(car,null);Physics.Simulate(Time.fixedDeltaTime);}
            if(bay.Occupant!=null||Mathf.Abs(car.transform.position.x)>.3f)throw new Exception("Vaga não foi liberada após saída");report.AppendLine("PASS saída de baliza devolve veículo à faixa e libera vaga");
        }finally{Object.DestroyImmediate(root);Physics.simulationMode=oldMode;Physics.SyncTransforms();}
    }
    public static void Structure()=>ValidateStructure(true);
    static bool ValidateStructure(bool exit)
    {
        var report=new StringBuilder();
        try{
            foreach(var name in new[]{"MainMenu","Office","SampleScene"}){
                EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");
                foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))
                    if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)throw new Exception("Script ausente: "+name+"/"+t.name);
                report.AppendLine("PASS "+name+" sem scripts ausentes");
            }
            Physics.SyncTransforms();var routes=Object.FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None).Where(r=>r.Valida).ToArray();var manager=Object.FindFirstObjectByType<DeliveryManager>();
            if(manager.points.Select(p=>p.id).Distinct().Count()!=manager.points.Length)throw new Exception("IDs de destino duplicados");
            File.WriteAllLines("v8-network.txt",routes.OrderBy(r=>r.name).Select(r=>r.name+" | "+r.pontos[0].position+" -> "+r.pontos.Last().position+" | "+string.Join(",",r.Connections().Select(n=>n.name))));
            var added=routes.Where(r=>r.name.StartsWith("Route8_")).ToArray();if(added.Length==0)throw new Exception("Nenhuma rota nova");
            foreach(var r in added){if(!r.Connections().Any())throw new Exception("Rota sem saída: "+r.name);if(DeliveryRouting.Find(manager.points[0].route,r)==null||DeliveryRouting.Find(r,manager.points[0].route)==null)throw new Exception("Rota desconectada: "+r.name);
                for(int i=1;i<r.pontos.Length;i++)for(int j=0;j<=10;j++){var at=Vector3.Lerp(r.pontos[i-1].position,r.pontos[i].position,j/10f);if(!Physics.Raycast(at+Vector3.up*.3f,Vector3.down,out var hit,1,~0,QueryTriggerInteraction.Ignore)||!hit.collider.name.StartsWith("Road ")||Mathf.Abs(hit.point.y-at.y)>.12f)throw new Exception("Rota fora do asfalto: "+r.name+" / "+at+" hit="+(hit.collider!=null?hit.collider.name+" "+hit.point.ToString():"none")+" all="+string.Join(";",Physics.RaycastAll(at+Vector3.up*.3f,Vector3.down,1).Select(h=>h.collider.name+"="+h.point.y)));}}
            report.AppendLine("PASS "+added.Length+" rotas novas com asfalto, saída e conexão de ida e volta");
            foreach(var p in manager.points){if(!p.Valid)throw new Exception("Parada inválida: "+p.id);if(DeliveryRouting.Find(manager.points[0].route,p.route)==null)throw new Exception("Destino sem rota: "+p.id);}
            report.AppendLine("PASS "+manager.points.Length+" destinos com IDs únicos e rotas acessíveis");
            var graph=Object.FindFirstObjectByType<PedestrianGraph>();int east=Enumerable.Range(0,graph.nodes.Length).Count(i=>graph.nodes[i].position.x>380&&graph.nodes[i].neighbours.Length>0);
            if(east<20)throw new Exception("Calçadas novas sem grafo: "+east);var population=Object.FindFirstObjectByType<PedestrianManager>();var path=new System.Collections.Generic.Queue<int>();
            int from=Array.FindIndex(graph.nodes,n=>n.position.x<360&&n.neighbours.Length>0);bool connected=false;
            for(int i=0;i<graph.nodes.Length;i++)if(graph.nodes[i].position.x>420&&population.Path(from,i,path)){connected=true;break;}
            if(!connected)throw new Exception("Pedestres sem ligação entre bairro antigo e novo");report.AppendLine("PASS "+east+" nós de pedestres no bairro novo com ligação à cidade antiga");
            if(Object.FindFirstObjectByType<GridShiftCityBoundary>()==null)throw new Exception("Limites ausentes");report.AppendLine("PASS terreno de continuidade e limites físicos");
            report.AppendLine("RESULT SUCCESS");
        }catch(Exception e){report.AppendLine("FAIL "+e);}
        File.WriteAllText("v8-structure.txt",report.ToString());bool success=!report.ToString().Contains("FAIL");if(exit)EditorApplication.Exit(success?0:1);return success;
    }
    public static void All(){
        try{EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);TrafficBehaviourChecks.Run();var report=new StringBuilder("PASS 10 testes existentes de trânsito\n");ReverseSafety(report);report.AppendLine("RESULT SUCCESS");File.WriteAllText("v8-traffic.txt",report.ToString());GridShiftV8Setup.Configure();if(!ValidateStructure(false))throw new Exception("Validação estrutural falhou");GridShiftV8PlayChecks.Batch();}
        catch(Exception e){File.WriteAllText("v8-all-failure.txt",e.ToString());EditorApplication.Exit(1);}
    }
    public static void FinalizeNative(){
        try{
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);TrafficBehaviourChecks.Run();var report=new StringBuilder("PASS 10 testes existentes de trânsito\n");ReverseSafety(report);report.AppendLine("RESULT SUCCESS");File.WriteAllText("v8-traffic.txt",report.ToString());
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");foreach(var bay in Object.FindObjectsByType<TrafficParkingBay>(FindObjectsSortMode.None)){bay.width=2.2f;bay.length=5.2f;}EditorSceneManager.SaveScene(scene);
            if(!ValidateStructure(false))throw new Exception("Validação estrutural falhou");
            string backup="Docs/Validacao8/PreviousNative-"+DateTime.Now.ToString("yyyyMMdd-HHmmss");Directory.CreateDirectory(backup);foreach(var name in new[]{"v7-native-save.json","v7-native-save.json.bak"})if(File.Exists(name))File.Move(name,backup+"/"+name);
            GridShiftV7NativeChecks.Batch();
        }catch(Exception e){File.WriteAllText("v8-final-native-failure.txt",e.ToString());EditorApplication.Exit(1);}
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void SafePedestrianSave(){if(SessionState.GetBool("V8PedSafe",false)){DeliverySaveStore.OverridePath=Path.GetFullPath("v8-pedestrian-"+Guid.NewGuid().ToString("N")+".json");GridShiftTutorial.Prefix="GS.V8.PedestrianTests.";GridShiftTutorial.Skip();}}
    public static void Pedestrians(){SessionState.SetBool("V8PedSafe",true);GridShiftV4PedChecks.Batch();}
    public static void FinalCore(){
        try{
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);TrafficBehaviourChecks.Run();var report=new StringBuilder("PASS 10 testes existentes de trânsito\n");ReverseSafety(report);report.AppendLine("RESULT SUCCESS");File.WriteAllText("v8-traffic.txt",report.ToString());
            if(!ValidateStructure(false))throw new Exception("Validação estrutural falhou");
            string backup="Docs/Validacao8/PreviousCore-"+DateTime.Now.ToString("yyyyMMdd-HHmmss");Directory.CreateDirectory(backup);foreach(var name in new[]{"v7-core-save.json","v7-core-save.json.bak"})if(File.Exists(name))File.Move(name,backup+"/"+name);
            SessionState.SetBool("V8CacheProbe",true);GridShiftV7Checks.Batch();
        }catch(Exception e){File.WriteAllText("v8-final-core-failure.txt",e.ToString());EditorApplication.Exit(1);}
    }
    public static void OfferAudit(){
        var report=new StringBuilder();try{
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");var manager=Object.FindFirstObjectByType<DeliveryManager>();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;typeof(DeliveryManager).GetMethod("Awake",flags).Invoke(manager,null);typeof(CompanyProgression).GetMethod("Awake",flags).Invoke(manager.Progression,null);
            var available=manager.points.Where(p=>p.Valid&&manager.Progression.Unlocked(p)).OrderBy(p=>p.id).ToArray();int checks=0;
            foreach(var a in available.Take(6)){var visited=new System.Collections.Generic.HashSet<TrafficRoute>{a.route};var pending=new System.Collections.Generic.Queue<TrafficRoute>();pending.Enqueue(a.route);while(pending.Count>0)foreach(var next in pending.Dequeue().Connections())if(visited.Add(next))pending.Enqueue(next);foreach(var b in available){if(visited.Contains(b.route)!=(DeliveryRouting.Find(a.route,b.route)!=null))throw new Exception("Conectividade divergente");checks++;}}
            report.AppendLine("PASS "+checks+" consultas de alcance equivalentes ao Dijkstra existente");var method=typeof(DeliveryManager).GetMethod("BuildOffers",flags);var watch=new System.Diagnostics.Stopwatch();watch.Start();for(int i=0;i<10;i++)method.Invoke(manager,null);watch.Stop();report.AppendLine("build_mean_ms="+(watch.Elapsed.TotalMilliseconds/10).ToString("F4"));
            var initial=manager.Offers();manager.DemoLogin("Audit");manager.Select("missing");var after=manager.Offers();if(!initial.Select(o=>o.id).SequenceEqual(after.Select(o=>o.id)))throw new Exception("Login alterou catálogo");
            watch.Restart();for(int i=0;i<10;i++)manager.Offers();watch.Stop();report.AppendLine("cached_mean_ms="+(watch.Elapsed.TotalMilliseconds/10).ToString("F4"));
            foreach(var offer in initial){if(!manager.Accept(offer.id,out var error))throw new Exception(error);if(!manager.Accept(offer.id,out error))throw new Exception("Idempotência perdida");}
            if(manager.orders.Count!=6||manager.Offers().Count!=6||manager.Offers().Any(o=>!o.id.StartsWith("OF-1-")))throw new Exception("Renovação de ofertas incorreta");report.AppendLine("PASS aceitação idempotente e renovação de seis contratos preservadas");report.AppendLine("RESULT SUCCESS");
        }catch(Exception e){report.AppendLine("FAIL "+e);}File.WriteAllText("v8-offer-audit.txt",report.ToString());EditorApplication.Exit(report.ToString().Contains("FAIL")?1:0);
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void ProbeCache(){if(SessionState.GetBool("V8CacheProbe",false)){SessionState.SetBool("V8CacheProbe",false);new GameObject("V8OfferCacheProbe").AddComponent<GridShiftV8OfferCacheProbe>();}}
}
public sealed class GridShiftV8OfferCacheProbe:MonoBehaviour {
    System.Collections.IEnumerator Start(){yield return new WaitForSecondsRealtime(1);var manager=DeliveryManager.Instance;var method=typeof(DeliveryManager).GetMethod("BuildOffers",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);var watch=new System.Diagnostics.Stopwatch();const int count=10;
        watch.Start();for(int i=0;i<count;i++)method.Invoke(manager,null);watch.Stop();double rebuilt=watch.Elapsed.TotalMilliseconds/count;manager.Offers();watch.Restart();for(int i=0;i<count;i++)manager.Offers();watch.Stop();double cached=watch.Elapsed.TotalMilliseconds/count;
        var expected=(System.Collections.Generic.List<DeliveryOffer>)method.Invoke(manager,null);bool same=manager.Offers().Select(o=>o.id).SequenceEqual(expected.Select(o=>o.id));File.WriteAllText("v8-offer-cache.txt","rebuild_mean_ms="+rebuilt.ToString("F4")+"\ncached_mean_ms="+cached.ToString("F4")+"\nPASS same offer identities="+same+"\nRESULT "+(same?"SUCCESS":"FAILURE"));
    }
}
