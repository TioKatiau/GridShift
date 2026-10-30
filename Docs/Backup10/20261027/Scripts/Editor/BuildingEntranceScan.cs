using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
public static class BuildingEntranceScan {
 public static void Batch(){var text=new System.Text.StringBuilder();
 foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/SimplePoly City - Low Poly Assets/Prefab/Buildings"})){
  string path=AssetDatabase.GUIDToAssetPath(guid);if(!path.Contains("House")||!path.Contains("color01"))continue;
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);var mesh=prefab.GetComponent<MeshFilter>().sharedMesh;var vertices=mesh.vertices;var tri=mesh.triangles;var found=new System.Collections.Generic.HashSet<string>();
  text.AppendLine(prefab.name+" bounds="+mesh.bounds);
  for(int i=0;i<tri.Length;i+=3){var a=vertices[tri[i]];var b=vertices[tri[i+1]];var c=vertices[tri[i+2]];var box=new Bounds(a,Vector3.zero);box.Encapsulate(b);box.Encapsulate(c);var n=Vector3.Cross(b-a,c-a).normalized;
   float width=Mathf.Max(box.size.x,box.size.z);if(box.size.y>1.35f&&box.size.y<2.8f&&box.min.y<1.2f&&width>.65f&&width<2f&&Mathf.Abs(n.y)<.1f){string line=box+" normal="+n;if(found.Add(line))text.AppendLine(line);}
  }
 }
 File.WriteAllText("v3-doors.txt",text.ToString());EditorApplication.Exit(0);}
}
