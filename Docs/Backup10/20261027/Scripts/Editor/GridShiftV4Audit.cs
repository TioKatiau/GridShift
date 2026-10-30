using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class GridShiftV4Audit {
 public static void Batch(){var text=new StringBuilder();
 bool printed=false;foreach(var name in new[]{"SampleScene","Office"}){EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");Physics.SyncTransforms();text.AppendLine("SCENE "+name);
 foreach(var p in Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None)){text.AppendLine("PLAYER "+EditorJsonUtility.ToJson(p));foreach(var c in p.GetComponents<Component>())text.AppendLine("COMP "+c.GetType().Name+" "+EditorJsonUtility.ToJson(c));}
 foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Props_Traffic Signal"))){var light=r.GetComponent<TrafficLight>();text.AppendLine("SIGNAL "+r.name+" pos="+r.transform.position+" rot="+r.transform.eulerAngles+" TL="+(light==null?"MISSING":EditorJsonUtility.ToJson(light)));
  foreach(Transform t in r.transform)text.AppendLine("CHILD "+t.name+" local="+t.localPosition+" type="+string.Join(",",t.GetComponents<Component>().Select(c=>c.GetType().Name)));
  if(light!=null){var so=new SerializedObject(light);foreach(var field in new[]{"luzVermelha","luzAmarela","luzVerde"}){var go=so.FindProperty(field).objectReferenceValue as GameObject;text.AppendLine("REF "+field+" "+(go==null?"NULL":go.name+" active="+go.activeSelf+" render="+go.GetComponent<Renderer>()?.enabled));}}
  if(!printed&&r.name.Contains("small")){printed=true;var m=r.GetComponent<MeshFilter>().sharedMesh;text.AppendLine("SMALL BOUNDS "+m.bounds);var v=m.vertices;var tr=m.triangles;var unique=new System.Collections.Generic.HashSet<string>();for(int i=0;i<tr.Length;i+=3){var a=v[tr[i]];var b=v[tr[i+1]];var c=v[tr[i+2]];var normal=Vector3.Cross(b-a,c-a).normalized;var box=new Bounds(a,Vector3.zero);box.Encapsulate(b);box.Encapsulate(c);if(box.center.y>1&&normal.z>.8f&&box.size.y<1.5f){var row="FACE "+box;if(unique.Add(row))text.AppendLine(row);}}}
 }
 }
 File.WriteAllText("v4-audit.txt",text.ToString());EditorApplication.Exit(0);}
}
