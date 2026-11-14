using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class GridShiftV24Setup {
 public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/Apartment.unity");var home=Object.FindFirstObjectByType<GridShiftApartment>();foreach(string name in new[]{"Hallway","Terrace doorway frame","Terrace exterior doorframe","Apartment city outlook and terrace"}){var t=home.transform.Find(name);if(t!=null)Object.DestroyImmediate(t.gameObject);}var view=home.GetComponent<GridShiftApartmentView>();if(view!=null)Object.DestroyImmediate(view);
 foreach(var r in home.GetComponentsInChildren<Renderer>(true))if(r.name=="Int_Apt_01_Wall_01 (19)"){r.enabled=true;foreach(var c in r.GetComponents<Collider>())c.enabled=true;}
 home.spawn=new Vector3(-2.5f,.16f,.4f);home.look=Vector3.left;var player=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).First(p=>p.gameObject.scene==home.gameObject.scene);player.transform.SetPositionAndRotation(home.transform.TransformPoint(home.spawn),Quaternion.LookRotation(home.look));
 var front=home.transform.Find("Apartment_01/Doors Frames/DoorFrame_Apt_03");var exit=home.GetComponentsInChildren<GridShiftWorldAction>(true).First(a=>a.title=="Sair de casa");exit.transform.position=new Vector3(-2.5f,1.5f,-1.26f);exit.sightRoot=front;exit.focusOwner=front;exit.focusOffset=Vector3.zero;exit.focusSize=new Vector3(1.6f,2.8f,.35f);exit.range=3;
 Physics.SyncTransforms();Directory.CreateDirectory("Docs/Validacao24");File.WriteAllText("Docs/Validacao24/setup.txt","Interior only; exterior roots removed.\nSpawn="+home.spawn+"\nFront door="+(front!=null)+"\nExit bound="+(exit.sightRoot==front));EditorSceneManager.SaveScene(home.gameObject.scene);AssetDatabase.SaveAssets();EditorApplication.Exit(front!=null?0:2);
 }
}
