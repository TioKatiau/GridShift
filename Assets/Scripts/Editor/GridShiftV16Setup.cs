using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class GridShiftV16Setup {
 const string Folder="Assets/Generated/Operations16";
 static Material Material(string name,Color color){string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=color};AssetDatabase.CreateAsset(m,path);}return m;}
 public static void Configure(){
  GridShiftV15Setup.Configure();Directory.CreateDirectory(Folder);
  var white=Material("VanWhite",new Color(.8f,.83f,.83f));var yellow=Material("GridShiftYellow",new Color(.95f,.75f,.12f));var dark=Material("DepotGraphite",new Color(.12f,.16f,.2f));var floor=Material("DepotConcrete",new Color(.32f,.34f,.34f));
  var config=AssetDatabase.LoadAssetAtPath<GridShiftActiveConfig>("Assets/Resources/GridShiftActiveConfig.asset");if(config==null){config=ScriptableObject.CreateInstance<GridShiftActiveConfig>();AssetDatabase.CreateAsset(config,"Assets/Resources/GridShiftActiveConfig.asset");}
  var original=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SimplePoly City - Low Poly Assets/Prefab/Vehicles/Vehicle with Static Wheels/Vehicle_Pick up Truck_color01.prefab");if(original==null)throw new Exception("Modelo existente da picape não encontrado");
  var root=new GameObject("GridShiftVan16");var visual=Object.Instantiate(original,root.transform);visual.transform.localPosition=Vector3.zero;visual.transform.localRotation=Quaternion.identity;
  foreach(var c in root.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);foreach(var r in root.GetComponentsInChildren<Rigidbody>())Object.DestroyImmediate(r);
  var renderers=root.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
  // Existing pickup plus a simple closed cargo body, until a dedicated van model is authored.
  var enclosure=GameObject.CreatePrimitive(PrimitiveType.Cube);enclosure.name="Closed cargo body";enclosure.transform.SetParent(root.transform,false);enclosure.transform.localPosition=new Vector3(bounds.center.x,bounds.max.y*.85f,bounds.center.z-bounds.size.z*.18f);enclosure.transform.localScale=new Vector3(bounds.size.x*.88f,bounds.size.y*.65f,bounds.size.z*.42f);enclosure.GetComponent<Renderer>().sharedMaterial=white;Object.DestroyImmediate(enclosure.GetComponent<Collider>());
  var mark=GameObject.CreatePrimitive(PrimitiveType.Cube);mark.name="GridShift cargo stripe";mark.transform.SetParent(enclosure.transform,false);mark.transform.localScale=new Vector3(1.01f,.13f,1.01f);mark.GetComponent<Renderer>().sharedMaterial=yellow;Object.DestroyImmediate(mark.GetComponent<Collider>());
  renderers=root.GetComponentsInChildren<Renderer>();bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
  var box=root.AddComponent<BoxCollider>();box.center=bounds.center;box.size=bounds.size;root.AddComponent<Rigidbody>();root.AddComponent<GridShiftPlayerVan>();foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=6;
  config.vanPrefab=PrefabUtility.SaveAsPrefabAsset(root,"Assets/Resources/GridShiftPlayerVan.prefab");Object.DestroyImmediate(root);
  config.floorMaterial=floor;config.structureMaterial=dark;config.accentMaterial=yellow;
  var manager=Object.FindFirstObjectByType<DeliveryManager>();var existing=manager.points.Where(p=>p!=null&&p.Valid&&p.id!=config.depotPointId).ToArray();var anchor=existing.OrderBy(p=>(p.name.IndexOf("Warehouse",StringComparison.OrdinalIgnoreCase)>=0||p.name.IndexOf("Factory",StringComparison.OrdinalIgnoreCase)>=0)?0:1).ThenBy(p=>p.id).First();
  Physics.SyncTransforms();var routes=Object.FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None);bool found=false;Vector3 site=Vector3.zero;
  // Repeat setup preserves the authored location when it remains clear.
  for(int radius=8;radius<=70&&!found;radius+=2)for(int k=0;k<32&&!found;k++){
   float angle=k*Mathf.PI/16;var candidate=anchor.transform.position+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*radius;
   if(!Physics.Raycast(candidate+Vector3.up*2,Vector3.down,out var hit,4,~((1<<6)|(1<<9)|(1<<7)|(1<<8)),QueryTriggerInteraction.Ignore)||hit.normal.y<.9f||Mathf.Abs(hit.point.y-anchor.Stop.y)>.5f)continue;candidate.y=hit.point.y+.025f;
   bool road=false;foreach(var route in routes)if(route.Valida)for(int i=1;i<route.pontos.Length;i++){var a=route.pontos[i-1].position;var delta=route.pontos[i].position-a;float t=Mathf.Clamp01(Vector3.Dot(candidate-a,delta)/delta.sqrMagnitude);if(Vector3.ProjectOnPlane(candidate-(a+delta*t),Vector3.up).magnitude<7){road=true;break;}}if(road)continue;
   if(Physics.CheckBox(candidate+Vector3.up*1.2f,new Vector3(4.8f,1,3.8f),Quaternion.identity,~((1<<6)|(1<<9)|(1<<7)|(1<<8)),QueryTriggerInteraction.Ignore))continue;
   bool supported=true;foreach(var corner in new[]{new Vector3(-4.8f,0,-3.8f),new Vector3(4.8f,0,-3.8f),new Vector3(-4.8f,0,3.8f),new Vector3(4.8f,0,3.8f)})if(!Physics.Raycast(candidate+corner+Vector3.up,Vector3.down,out var surface,2,~((1<<6)|(1<<9)),QueryTriggerInteraction.Ignore)||surface.normal.y<.9f||Mathf.Abs(surface.point.y+.025f-candidate.y)>.1f){supported=false;break;}if(!supported)continue;
   site=candidate;found=true;
  }
  if(!found)throw new Exception("Nenhum pátio compacto livre e apoiado foi encontrado; não sobrepor ruas ou prédios.");
  config.depotPosition=site;config.depotRotation=Quaternion.identity;config.vanPosition=site+new Vector3(2.7f,.1f,-.3f);config.vanRotation=Quaternion.identity;
  var point=manager.points.FirstOrDefault(p=>p!=null&&p.id==config.depotPointId);if(point==null){var go=new GameObject("GridShiftDepot16Receiving");point=go.AddComponent<DeliveryPoint>();point.transform.SetParent(manager.transform,false);manager.points=manager.points.Concat(new[]{point}).ToArray();}
  point.transform.position=site+new Vector3(-1,0,0);point.id=config.depotPointId;point.displayName="Depósito GridShift · Expedição";point.establishmentType="Depósito";point.accessible=true;point.personalOnly=true;point.pedestrianAccess=point.transform;point.vehicleStop=anchor.vehicleStop;point.route=anchor.route;point.segment=anchor.segment;point.requiredExpansion=0;
  EditorUtility.SetDirty(manager);EditorUtility.SetDirty(point);EditorUtility.SetDirty(config);EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
  Directory.CreateDirectory("Docs/Validacao16");File.WriteAllText("Docs/Validacao16/setup.txt","Depot="+site+"\nVan="+config.vanPosition+"\nAnchor="+anchor.id+" "+anchor.displayName+"\nClear footprint=9.6 x 7.6 m; road-centre separation >=7 m\nModel=existing pickup with closed low-poly cargo body\n");
 }
 public static void Batch(){try{Configure();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
}
