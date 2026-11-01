using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GridShiftV15Setup {
 public static void Configure(){
  Directory.CreateDirectory("Assets/Resources");Directory.CreateDirectory("Assets/Generated/Deliveries15");
  const string materialPath="Assets/Generated/Deliveries15/PackageCardboard.mat";
  var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
  if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=new Color(.56f,.34f,.15f)};AssetDatabase.CreateAsset(material,materialPath);}
  var box=GameObject.CreatePrimitive(PrimitiveType.Cube);box.name="DeliveryPackage";box.transform.localScale=new Vector3(.28f,.18f,.22f);box.GetComponent<Renderer>().sharedMaterial=material;box.GetComponent<Collider>().enabled=false;
  const string tapePath="Assets/Generated/Deliveries15/PackageTape.mat";var tapeMaterial=AssetDatabase.LoadAssetAtPath<Material>(tapePath);if(tapeMaterial==null){tapeMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=new Color(.95f,.72f,.16f)};AssetDatabase.CreateAsset(tapeMaterial,tapePath);}
  var tape=GameObject.CreatePrimitive(PrimitiveType.Cube);tape.name="GridShift parcel seal";tape.transform.SetParent(box.transform,false);tape.transform.localScale=new Vector3(.16f,1.012f,1.012f);tape.GetComponent<Renderer>().sharedMaterial=tapeMaterial;tape.GetComponent<Collider>().enabled=false;
  PrefabUtility.SaveAsPrefabAsset(box,"Assets/Resources/DeliveryPackage.prefab");Object.DestroyImmediate(box);
  EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");Physics.SyncTransforms();int accesses=0;
  foreach(var point in Object.FindFirstObjectByType<DeliveryManager>().points.Where(p=>p!=null&&p.Valid)){
   // Legacy points were placed on verified sidewalk nodes. These are receiving
   // areas, not a claim that every legacy building has a modelled usable door.
   var old=point.pedestrianAccess;point.pedestrianAccess=point.transform;
   if(GridShiftDeliveryAccess.TryGet(point,out _)){EditorUtility.SetDirty(point);accesses++;}else point.pedestrianAccess=old;
  }
  EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
  Directory.CreateDirectory("Docs/Validacao15");File.WriteAllText("Docs/Validacao15/setup.txt","Receiving areas with ground validated="+accesses+"\nPrefab=Assets/Resources/DeliveryPackage.prefab\n");
 }
 public static void Batch(){Configure();EditorApplication.Exit(0);}
}
