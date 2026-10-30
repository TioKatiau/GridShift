using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class GridShiftV8Setup
{
    const string Models="Assets/SimplePoly City - Low Poly Assets/Prefab/";
    static readonly List<string> report=new List<string>();
    static Transform root;
    static Vector3 V(float x,float z)=>new Vector3(x,-4.6f,z);
    static void Ref(Object o,string field,Object value){var s=new SerializedObject(o);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
    static Transform Point(Transform parent,string name,Vector3 at,Vector3 direction){var t=new GameObject(name).transform;t.SetParent(parent);t.position=at;t.rotation=Quaternion.LookRotation(direction);return t;}
    static GameObject Model(string path,Vector3 at,float yaw=0){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Models+path+".prefab");if(prefab==null)throw new Exception("Prefab ausente: "+path);var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.transform.SetParent(root);go.transform.SetPositionAndRotation(at,Quaternion.Euler(0,yaw,0));
        if(path.StartsWith("Roads/")&&!go.GetComponentsInChildren<Collider>().Any()){foreach(var mesh in go.GetComponentsInChildren<MeshFilter>()){var c=mesh.gameObject.AddComponent<MeshCollider>();c.sharedMesh=mesh.sharedMesh;}}
        return go;}
    [MenuItem("GridShift/Atualização 8.0/Expandir e validar cidade")]
    public static void Configure()
    {
        if(EditorApplication.isPlaying)throw new Exception("Saia do Play Mode para configurar a cidade.");
        if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        Directory.CreateDirectory("Docs/Validacao8");report.Clear();
        string backup=Path.Combine("Docs/Validacao8","Backup-"+DateTime.Now.ToString("yyyyMMdd-HHmmss"));Directory.CreateDirectory(backup);
        foreach(string name in new[]{"SampleScene","Office"})File.Copy("Assets/Scenes/"+name+".unity",backup+"/"+name+".unity");
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        if(GameObject.Find("CityExpansion8")!=null)throw new Exception("A expansão já existe. Nenhuma duplicação foi aplicada.");
        root=new GameObject("CityExpansion8").transform;
        var oldRoutes=Object.FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None).Where(r=>r.Valida).ToArray();
        RepairJunctions(oldRoutes);
        var renderers=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        // Two eastbound links and a return avenue form a connected block, reusing 20 m road tiles.
        foreach(float z in new[]{201.4f,261.4f}){
            var old=renderers.First(r=>r.name.StartsWith("Road T_Intersection")&&Vector3.Distance(r.transform.position,V(364.1f,z))<.2f);
            var cross=AssetDatabase.LoadAssetAtPath<GameObject>(Models+"Roads/Road Intersection_01.prefab");
            // Preserve the object, all references and its transform; replace only its mesh and materials.
            old.GetComponent<MeshFilter>().sharedMesh=cross.GetComponent<MeshFilter>().sharedMesh;old.sharedMaterials=cross.GetComponent<MeshRenderer>().sharedMaterials;
            var collider=old.GetComponent<MeshCollider>();if(collider!=null)collider.sharedMesh=cross.GetComponent<MeshFilter>().sharedMesh;
            old.transform.rotation=Quaternion.identity;
            foreach(float x in new[]{384.1f,404.1f})Model("Roads/Road Lane_01",V(x,z),90);
        }
        Model("Roads/Road Corner_01",V(424.1f,201.4f));Model("Roads/Road Corner_01",V(424.1f,261.4f),270);
        foreach(float z in new[]{221.4f,241.4f})Model("Roads/Road Lane_01",V(424.1f,z));
        // Keep the scenery clear of the new carriageways; relocate only greenery in the expansion corridor.
        foreach(var r in renderers.Where(r=>r.name.StartsWith("Natures_")&&r.transform.position.x>374&&r.transform.position.x<435&&r.transform.position.z>191&&r.transform.position.z<272))r.transform.position+=Vector3.right*75;
        foreach(var r in renderers.Where(r=>r.name.StartsWith("Props_")&&(r.name.Contains("BillBoard")||r.name.Contains("Wind"))&&r.bounds.max.x>370&&r.bounds.min.x<435&&r.bounds.max.z>191&&r.bounds.min.z<272)){
            var previous=r.transform.position;r.transform.position+=Vector3.right*100;report.Add("Elemento de paisagem removido do corredor viário: "+r.name+" de "+previous+" para "+r.transform.position);
        }
        var copied=new List<MeshRenderer>();
        foreach(var b in renderers.Where(r=>r.name.StartsWith("Building_")&&r.transform.position.x>314&&r.transform.position.x<354&&r.transform.position.z>211&&r.transform.position.z<251&&r.bounds.size.x<14&&r.bounds.size.z<14)){
            var go=Object.Instantiate(b.gameObject,root);go.name=b.name+"_Bairro8";go.transform.position=b.transform.position+Vector3.right*60;copied.Add(go.GetComponent<MeshRenderer>());
        }
        // Two residences use the same prefabs and frontage scale as the original neighbourhood.
        var shops=renderers.Where(r=>r.name.Contains("Shop")&&r.name.StartsWith("Building_")&&r.bounds.size.x<14&&r.bounds.size.z<14).OrderBy(r=>r.name).Take(3).ToArray();
        for(int i=0;i<shops.Length;i++){var go=Object.Instantiate(shops[i].gameObject,root);go.name=shops[i].name+"_Bairro8_"+i;go.transform.SetPositionAndRotation(V(i==1?408:389,i==2?244:219),Quaternion.Euler(0,i==2?0:180,0));copied.Add(go.GetComponent<MeshRenderer>());}
        var houses=renderers.Where(r=>r.name.StartsWith("Building_House")).GroupBy(r=>r.name).OrderBy(g=>g.Key).Select(g=>g.First()).Take(2).ToArray();
        for(int i=0;i<houses.Length;i++){var go=Object.Instantiate(houses[i].gameObject,root);go.name=houses[i].name+"_Bairro8_"+i;go.transform.SetPositionAndRotation(V(446.5f,215+i*28),Quaternion.Euler(0,270,0));copied.Add(go.GetComponent<MeshRenderer>());}
        var grassSource=renderers.Where(r=>r.bounds.size.y<1&&r.bounds.size.x>100&&r.bounds.size.z>100).OrderByDescending(r=>r.bounds.size.x*r.bounds.size.z).FirstOrDefault();
        var terrain=GameObject.CreatePrimitive(PrimitiveType.Cube);terrain.name="CityLandscape8";terrain.transform.SetParent(root);terrain.transform.position=V(280,250)+Vector3.down*.54f;terrain.transform.localScale=new Vector3(620,1,520);
        if(grassSource!=null)terrain.GetComponent<Renderer>().sharedMaterial=grassSource.sharedMaterial;
        else {var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.color=new Color(.36f,.53f,.13f);AssetDatabase.CreateAsset(mat,"Assets/Generated/CityLandscape8.mat");terrain.GetComponent<Renderer>().sharedMaterial=mat;}
        var boundary=root.gameObject.AddComponent<GridShiftCityBoundary>();boundary.playable=new Bounds(V(280,250),new Vector3(390,60,300));
        var min=boundary.playable.min;var max=boundary.playable.max;
        for(int i=0;i<4;i++){var go=new GameObject("InvisibleCityLimit"+i);go.transform.SetParent(root);var box=go.AddComponent<BoxCollider>();go.transform.position=i<2?new Vector3(i==0?min.x:max.x,2,250):new Vector3(280,2,i==2?min.z:max.z);box.size=i<2?new Vector3(1,16,300):new Vector3(390,16,1);}
        var lamp=renderers.FirstOrDefault(r=>r.name.StartsWith("Props_Street Light"));var tree=renderers.FirstOrDefault(r=>r.name.StartsWith("Natures_Big Tree"));
        for(int i=0;i<6;i++){if(lamp!=null){var go=Object.Instantiate(lamp.gameObject,root);go.transform.position=V(i<3?381+i*15:434, i<3?210:215+(i-3)*20);}
            if(tree!=null){var go=Object.Instantiate(tree.gameObject,root);go.transform.position=V(470,195+i*15);}}
        RegisterExpansionLamps();Routes(oldRoutes);Parking();
        typeof(GridShiftV4Setup).GetMethod("Signals",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,null);
        RepairStops();
        Physics.SyncTransforms();
        var manager=Object.FindFirstObjectByType<PedestrianManager>();
        typeof(GridShiftV3Setup).GetMethod("BuildGraph",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{manager.graph});manager.daytimeCount=Mathf.Min(manager.maximum,Mathf.Max(manager.daytimeCount,40));
        Destinations(copied);Markers(manager);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        report.Add("Expansão: 1 quarteirão, 3 ligações viárias bidirecionais, 8 peças de rua, 2 cruzamentos novos; prédios="+copied.Count);
        report.Add("Grafo: "+manager.graph.nodes.Length+" nós, "+manager.graph.crossings.Length+" travessias; entradas="+manager.graph.nodes.Count(n=>n.entrance));
        File.WriteAllLines("Docs/Validacao8/configuracao.txt",report);
    }
    static void RepairJunctions(TrafficRoute[] routes)
    {
        var junctions=Object.FindObjectsByType<TrafficJunction>(FindObjectsSortMode.None).ToList();int repaired=0;
        var roadCenters=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Road Intersection")||r.name.StartsWith("Road T_Intersection")||r.name.StartsWith("Road Corner")).ToArray();
        foreach(var route in routes){var end=route.pontos.Last().position;var outgoing=(end-route.pontos[route.pontos.Length-2].position).normalized;var incoming=(route.pontos[1].position-route.pontos[0].position).normalized;
            var center=end-outgoing*20-Vector3.Cross(Vector3.up,outgoing)*3.2f;
            if(!roadCenters.Any(r=>Vector3.Distance(r.transform.position,center)<.2f))throw new Exception("Centro de cruzamento sem peça física: "+route.name+" / "+center);
            var junction=junctions.FirstOrDefault(j=>Vector3.Distance(j.transform.position,center)<.2f);
            if(junction==null){junction=new GameObject("Junction8_Repaired").AddComponent<TrafficJunction>();junction.transform.SetParent(root);junction.transform.position=center;junctions.Add(junction);}
            if(route.Cruzamento!=junction){report.Add("Reserva corrigida: "+route.name+" / "+route.Cruzamento?.transform.position+" -> "+center);repaired++;Ref(route,"cruzamento",junction);}
            var so=new SerializedObject(route);var line=so.FindProperty("linhaParada").objectReferenceValue as Transform;var exit=so.FindProperty("saidaCruzamento").objectReferenceValue as Transform;
            if(line==null){line=Point(route.transform,"StopLine8",center, incoming);Ref(route,"linhaParada",line);}line.SetPositionAndRotation(center-incoming*12+Vector3.Cross(Vector3.up,incoming)*3.2f,Quaternion.LookRotation(incoming));
            if(exit==null){exit=Point(route.transform,"Exit8",end,outgoing);Ref(route,"saidaCruzamento",exit);}exit.SetPositionAndRotation(end,Quaternion.LookRotation(outgoing));
        }
        report.Add("Associações antigas de cruzamento corrigidas pela geometria: "+repaired);
    }
    static void RepairStops(){
        foreach(var p in Object.FindFirstObjectByType<DeliveryManager>().points){if(!p.Valid)continue;var a=p.route.pontos[p.segment].position;var b=p.route.pontos[p.segment+1].position;var direction=(b-a).normalized;float maximum=Vector3.Distance(a,b)-6;
            if(p.route.GetParada(out var line,out _))maximum=Mathf.Min(maximum,Vector3.Dot(line-a,direction)-12);
            if(maximum<8){report.Add("AJUSTE MANUAL DE PARADA: "+p.id);continue;}
            float distance=Vector3.Dot(p.Stop-a,direction);var corrected=a+direction*Mathf.Clamp(distance,8,maximum);if(Vector3.Distance(p.Stop,corrected)>.1f){report.Add("Parada afastada do cruzamento: "+p.id+" / "+p.Stop+" -> "+corrected);p.vehicleStop.position=corrected;}
        }
    }
    static void Routes(TrafficRoute[] existing)
    {
        var nodes=existing.Select(r=>r.Cruzamento).Where(j=>j!=null).Distinct().ToList();
        foreach(var at in new[]{V(424.1f,201.4f),V(424.1f,261.4f)}){var j=new GameObject("Junction8").AddComponent<TrafficJunction>();j.transform.SetParent(root);j.transform.position=at;nodes.Add(j);}
        var edges=new HashSet<string>();string Key(Vector3 a,Vector3 b)=>a.ToString("F1")+"/"+b.ToString("F1");
        foreach(var r in existing){var center=r.Cruzamento.transform.position;var incoming=(r.pontos[1].position-r.pontos[0].position).normalized;var previous=nodes.Where(j=>j!=r.Cruzamento&&Vector3.Dot((center-j.transform.position).normalized,incoming)>.99f).OrderBy(j=>(center-j.transform.position).sqrMagnitude).FirstOrDefault();if(previous!=null){edges.Add(Key(center,previous.transform.position));edges.Add(Key(previous.transform.position,center));}}
        var joins=new[]{(V(364.1f,201.4f),V(424.1f,201.4f)),(V(364.1f,261.4f),V(424.1f,261.4f)),(V(424.1f,201.4f),V(424.1f,261.4f))};
        var fresh=new HashSet<string>();foreach(var e in joins){edges.Add(Key(e.Item1,e.Item2));edges.Add(Key(e.Item2,e.Item1));fresh.Add(Key(e.Item1,e.Item2));fresh.Add(Key(e.Item2,e.Item1));}
        int added=0;var signals=new Dictionary<string,TrafficLight>();var signalTemplate=Object.FindObjectsByType<TrafficLight>(FindObjectsSortMode.None).First(l=>l.name.StartsWith("Props_Traffic Signal"));
        var intersection=Object.FindFirstObjectByType<TrafficIntersection>();var iso=new SerializedObject(intersection);
        var horizontal=(TrafficLightGroup)iso.FindProperty("grupoHorizontal").objectReferenceValue;var vertical=(TrafficLightGroup)iso.FindProperty("grupoVertical").objectReferenceValue;
        foreach(var node in nodes){var center=node.transform.position;var neighbours=nodes.Where(n=>edges.Contains(Key(n.transform.position,center))).ToArray();
            foreach(var previous in neighbours)foreach(var after in neighbours){if(previous==after||(!fresh.Contains(Key(previous.transform.position,center))&&!fresh.Contains(Key(center,after.transform.position))))continue;
                var incoming=(center-previous.transform.position).normalized;var outgoing=(after.transform.position-center).normalized;
                var rightIn=Vector3.Cross(Vector3.up,incoming)*3.2f;var rightOut=Vector3.Cross(Vector3.up,outgoing)*3.2f;
                var route=new GameObject("Route8_"+added++).AddComponent<TrafficRoute>();route.transform.SetParent(root);var pts=new List<Vector3>{previous.transform.position+incoming*20+rightIn};
                if(Vector3.Dot(incoming,outgoing)<.99f){float radius=Vector3.Cross(incoming,outgoing).y>0?7.3f:13.7f;
                    var a=center-incoming*10.5f+rightIn;var b=center+outgoing*10.5f+rightOut;pts.Add(a);
                    var c=a+incoming*radius*.55228475f;var d=b-outgoing*radius*.55228475f;
                    int count=Mathf.CeilToInt(radius*Mathf.PI);for(int i=1;i<=count;i++){float t=i/(float)count,u=1-t;pts.Add(a*u*u*u+3*c*u*u*t+3*d*u*t*t+b*t*t*t);}}
                pts.Add(center+outgoing*20+rightOut);
                route.pontos=pts.Select((p,i)=>Point(route.transform,"Waypoint"+i,p,incoming)).ToArray();Ref(route,"cruzamento",node);
                var line=Point(route.transform,"StopLine",center-incoming*12+rightIn,incoming);Ref(route,"linhaParada",line);Ref(route,"saidaCruzamento",route.pontos.Last());
                string signalKey=center.ToString("F1")+incoming.ToString("F1");
                if(!signals.TryGetValue(signalKey,out var light)){
                    light=Object.Instantiate(signalTemplate,root);light.name="Props_Traffic Signal_8";light.transform.SetPositionAndRotation(line.position+Vector3.Cross(Vector3.up,incoming)*5.6f+Vector3.up*.2f,Quaternion.LookRotation(-incoming));signals[signalKey]=light;
                    var group=Mathf.Abs(incoming.x)>.5f?horizontal:vertical;var so=new SerializedObject(group);var array=so.FindProperty("semaforos");int n=array.arraySize;array.InsertArrayElementAtIndex(n);array.GetArrayElementAtIndex(n).objectReferenceValue=light;so.ApplyModifiedPropertiesWithoutUndo();
                }
                Ref(route,"semaforo",light);
            }
        }
        var all=Object.FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None).Where(r=>r.Valida).ToArray();
        foreach(var r in all){var end=r.pontos.Last().position;var direction=(end-r.pontos[r.pontos.Length-2].position).normalized;var links=all.Where(n=>n!=r&&Vector3.Distance(end,n.pontos[0].position)<.1f&&Vector3.Dot(direction,(n.pontos[1].position-n.pontos[0].position).normalized)>-.3f).ToArray();var so=new SerializedObject(r);var a=so.FindProperty("proximasRotas");a.arraySize=links.Length;for(int i=0;i<links.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=links[i];so.ApplyModifiedPropertiesWithoutUndo();}
        var anchor=Object.FindFirstObjectByType<DeliveryManager>().points[0].route;
        foreach(var r in all.Where(r=>r.name.StartsWith("Route8_")))if(DeliveryRouting.Find(anchor,r)==null||DeliveryRouting.Find(r,anchor)==null){report.Add("Aproximação inexistente na rede antiga descartada: "+r.name);Object.DestroyImmediate(r.gameObject);added--;}
        report.Add("Rotas dirigidas novas="+added);
    }
    static void Parking()
    {
        var road=Object.FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None).First(r=>r.name.StartsWith("Route8_")&&Vector3.Distance(r.pontos[0].position,V(384.1f,198.2f))<.1f);
        var bay=new GameObject("ParkingBay8").AddComponent<TrafficParkingBay>();bay.transform.SetParent(root);bay.transform.SetPositionAndRotation(V(393,195.8f),Quaternion.Euler(0,90,0));bay.accessRoute=road;bay.width=2.2f;bay.length=5.2f;bay.timeout=70;
        bay.approach=Point(bay.transform,"Approach",V(392,198.2f),Vector3.right);
        var entry=new List<TrafficParkingBay.Step>();var arc=new List<Transform>();
        Vector3 position=V(399,198.2f);entry.Add(new TrafficParkingBay.Step{point=Point(bay.transform,"PullAhead",position,Vector3.right)});
        arc.Add(entry[0].point);float angle=Mathf.Acos(1-2.4f/9f)*Mathf.Rad2Deg;float radius=4.5f;int count=30;
        for(int half=0;half<2;half++)for(int i=1;i<=count;i++){
            float previous=half==0?90-angle*(i-1)/count:90-angle+angle*(i-1)/count;
            float yaw=half==0?90-angle*i/count:90-angle+angle*i/count;
            position-=Quaternion.Euler(0,(previous+yaw)*.5f,0)*Vector3.forward*(radius*angle*Mathf.Deg2Rad/count);
            var point=Point(bay.transform,"ReverseArc_"+half+"_"+i,position,Quaternion.Euler(0,yaw,0)*Vector3.forward);arc.Add(point);entry.Add(new TrafficParkingBay.Step{point=point,reverse=true});
        }
        bay.entry=entry.ToArray();bay.exit=arc.AsEnumerable().Reverse().Select(p=>new TrafficParkingBay.Step{point=p}).Concat(new[]{new TrafficParkingBay.Step{point=Point(bay.transform,"RejoinLane",V(401,198.2f),Vector3.right)}}).ToArray();
        report.Add("1 vaga de baliza: objetivo opcional para tráfego ambiente; veículos em entrega são excluídos");
    }
    static void Destinations(List<MeshRenderer> buildings)
    {
        var manager=Object.FindFirstObjectByType<DeliveryManager>();var points=manager.points.ToList();var routes=Object.FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Route8_")&&r.Valida).ToArray();
        foreach(var b in buildings){var options=new List<(TrafficRoute route,Vector3 stop,float distance)>();foreach(var r in routes){var a=r.pontos[0].position;var dir=(r.pontos[1].position-a).normalized;float length=Vector3.Distance(a,r.pontos[1].position)-7;if(r.GetParada(out var line,out _))length=Mathf.Min(length,Vector3.Dot(line-a,dir)-12);if(length<8)continue;var stop=a+dir*Mathf.Clamp(Vector3.Dot(b.bounds.center-a,dir),8,length);float distance=Vector3.ProjectOnPlane(b.bounds.center-stop,Vector3.up).sqrMagnitude;if(distance>32*32||points.Any(p=>(p.Stop-stop).sqrMagnitude<36)||DeliveryRouting.Find(points[0].route,r)==null||DeliveryRouting.Find(r,points[0].route)==null)continue;
                if(Physics.OverlapBox(stop+Vector3.up*1.6f,new Vector3(1.8f,1.1f,4.8f),Quaternion.LookRotation(dir),~0,QueryTriggerInteraction.Ignore).Any(c=>!c.name.StartsWith("Road ")))continue;options.Add((r,stop,distance));}
            if(options.Count==0){report.Add("Sem parada segura para "+b.name);continue;}var best=options.OrderBy(o=>o.distance).First();var pnt=new GameObject("Delivery8_"+b.name).AddComponent<DeliveryPoint>();pnt.transform.SetParent(GameObject.Find("DeliveryPoints").transform);pnt.transform.position=b.bounds.ClosestPoint(best.stop);pnt.vehicleStop=Point(pnt.transform,"VehicleStop",best.stop,(best.route.pontos[1].position-best.route.pontos[0].position).normalized);pnt.establishment=b.transform;pnt.route=best.route;pnt.segment=0;pnt.accessible=true;pnt.id="P8-"+Hash128.Compute(b.name).ToString().Substring(0,12);pnt.displayName="Bairro Leste · "+b.name;pnt.establishmentType=b.name.Contains("House")?"Residência":"Comércio";points.Add(pnt);
        }manager.points=points.ToArray();report.Add("Destinos novos="+(points.Count-manager.points.Count(p=>!p.id.StartsWith("P8-"))));
    }
    static void Markers(PedestrianManager population)
    {
        var buildings=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Building_")).ToArray();
        foreach(var p in Object.FindFirstObjectByType<DeliveryManager>().points){var stop=p.Stop;string expected=p.name.Replace("Delivery8_","").Replace("Delivery_","").Replace("Partner_","");
            var matching=buildings.Where(r=>r.name==expected).ToArray();
            var b=p.establishment!=null?p.establishment.GetComponent<MeshRenderer>():matching.OrderBy(r=>(r.bounds.ClosestPoint(stop)-stop).sqrMagnitude).FirstOrDefault();
            if(b==null){p.accessNeedsReview=true;report.Add("AJUSTE MANUAL: "+p.id+" sem vínculo inequívoco com a construção");continue;}p.establishment=b.transform;
            // Use verified connected sidewalk nodes on the building's side of the road.
            var candidates=population.graph.nodes.Where(n=>n.neighbours.Length>0&&Mathf.Abs(n.position.y-stop.y)<1&&Vector3.Distance(n.position,stop)<22&&Vector3.ProjectOnPlane(n.position-b.bounds.ClosestPoint(n.position),Vector3.up).magnitude<3).OrderBy(n=>n.entrance?0:1).ThenBy(n=>Vector3.ProjectOnPlane(n.position-b.bounds.ClosestPoint(stop),Vector3.up).sqrMagnitude).ToArray();
            var candidate=candidates.FirstOrDefault(n=>population.Supported(n.position,n.position.y,n.entrancePath)&&population.Clear(null,n.position,.65f));
            if(candidate==null){p.accessNeedsReview=true;report.Add("AJUSTE MANUAL: "+p.id+" / "+b.name+" sem acesso de calçada validado");continue;}
            p.accessNeedsReview=!candidate.entrance;p.transform.position=candidate.position+Vector3.up*.015f;p.vehicleStop.position=stop;
            report.Add((candidate.entrance?"PORTA VALIDADA: ":"CALÇADA VALIDADA; conferir porta: ")+p.id+" / "+b.name+" / "+p.transform.position);
        }
    }
    public static void Batch(){try{Configure();EditorApplication.Exit(0);}catch(Exception e){Directory.CreateDirectory("Docs/Validacao8");File.WriteAllText("Docs/Validacao8/falha-configuracao.txt",e.ToString());EditorApplication.Exit(1);}}
    static void RegisterExpansionLamps(){var environment=Object.FindFirstObjectByType<CityEnvironment>();var expansion=GameObject.Find("CityExpansion8");if(environment==null||expansion==null)return;var lamps=environment.lamps.ToList();foreach(var light in expansion.GetComponentsInChildren<Light>(true).Where(l=>l.name=="GridShiftLamp")){if(lamps.Any(l=>l.light==light))continue;light.shadows=LightShadows.None;light.enabled=false;light.intensity=0;lamps.Add(new StreetLamp{light=light,bulb=light.transform.Find("Bulb")?.GetComponent<Renderer>(),offset=Mathf.Repeat(light.transform.position.x*.127f+light.transform.position.z*.319f,1)*.25f});}environment.lamps=lamps.ToArray();}
    public static void FinalizeLighting(){var scene=EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");RegisterExpansionLamps();EditorSceneManager.SaveScene(scene);var environment=Object.FindFirstObjectByType<CityEnvironment>();var expansion=GameObject.Find("CityExpansion8");var lights=expansion.GetComponentsInChildren<Light>(true).Where(l=>l.name=="GridShiftLamp").ToArray();bool valid=lights.Length==6&&lights.All(l=>environment.lamps.Any(s=>s.light==l))&&environment.lightBudget==24;File.WriteAllText("v8-lighting.txt","PASS postes novos registrados no ciclo e orçamento: "+lights.Length+"\nRESULT "+(valid?"SUCCESS":"FAILURE"));EditorApplication.Exit(valid?0:1);}
}
