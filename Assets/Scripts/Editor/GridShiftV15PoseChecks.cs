using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class GridShiftV15PoseChecks {
 public static void Batch(){
  EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
  var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("e7960ca7e82a51144ac57591496310c6")));model.transform.position=Vector3.zero;
  var animator=model.GetComponent<Animator>();animator.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Generated/CityLife/Pedestrian.controller");animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;animator.Rebind();animator.SetFloat("Speed",1);animator.Update(.2f);
  var box=GridShiftDeliveryAccess.Package(model.transform);box.transform.localPosition=new Vector3(0,1.05f,.38f);
  var pose=model.AddComponent<GridShiftCourierCarryPose>();pose.package=box.transform;pose.actor=model.transform;
  var bones=model.GetComponentsInChildren<SkinnedMeshRenderer>().SelectMany(r=>r.bones).Where(t=>t!=null).Distinct().ToArray();
  var hand=bones.First(t=>t.name=="LeftHand");var before=hand.position;pose.ApplyPose();var after=hand.position;
  var camera=Object.FindFirstObjectByType<Camera>();camera.transform.position=new Vector3(-2,1.7f,2.8f);camera.transform.LookAt(Vector3.up);GridShiftV11Capture.Save("v15-pose",camera);
  Directory.CreateDirectory("Docs/Validacao15");float gap=Vector3.Distance(after,box.transform.position-Vector3.right*.145f-Vector3.up*.015f);
  File.WriteAllText("Docs/Validacao15/pose.txt","Applied="+pose.Applied+" hand_before="+before+" hand_after="+after+" hand_gap="+gap+"\nRESULT "+(pose.Applied&&gap<.1f?"SUCCESS":"FAILURE"));EditorApplication.Exit(pose.Applied&&gap<.1f?0:1);
 }
}
