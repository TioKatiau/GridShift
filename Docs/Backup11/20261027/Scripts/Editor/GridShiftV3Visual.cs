using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
[InitializeOnLoad] public static class GridShiftV3Visual {
 static GridShiftV3Visual(){EditorApplication.playModeStateChanged+=Changed;}
 public static void Batch(){GridShiftV3Setup.Configure();EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");foreach(var c in Object.FindObjectsByType<ClimaReal>(FindObjectsSortMode.None))c.enabled=false;SessionState.SetBool("V3Visual",true);EditorApplication.isPlaying=true;}
 static void Changed(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("V3Visual",false)){SessionState.SetBool("V3Visual",false);new GameObject("VisualChecks").AddComponent<GridShiftV3Capture>();}}
}
public class GridShiftV3Capture:MonoBehaviour {
 void Hour(float h){typeof(WorldSession).GetField("clockTime",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(WorldSession.Instance,h/24*1920);}
 void Save(Camera camera,string name){var old=camera.targetTexture;var rt=new RenderTexture(1280,720,24);camera.targetTexture=rt;camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();Directory.CreateDirectory("v3-images");File.WriteAllBytes("v3-images/"+name+".png",image.EncodeToPNG());RenderTexture.active=previous;camera.targetTexture=old;Destroy(image);rt.Release();Destroy(rt);}
 IEnumerator Start(){
  Hour(12);yield return new WaitForSeconds(8);var p=Object.FindFirstObjectByType<PlayerMovement>();p.BlockControls(true);var camera=p.CameraTransform.GetComponent<Camera>();
  Save(camera,"city-day");var pop=Object.FindFirstObjectByType<PedestrianManager>();if(pop.Agents.Count>0){var agent=pop.Agents[0];camera.transform.position=agent.transform.position+new Vector3(4,2.2f,-4);camera.transform.LookAt(agent.transform.position+Vector3.up);yield return new WaitForSeconds(1);Save(camera,"pedestrian-day");}
  camera.transform.position=new Vector3(360,1.5f,175);camera.transform.rotation=Quaternion.Euler(5,190,0);Hour(1);yield return new WaitForSeconds(2);Save(camera,"city-night");
  var house=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).FirstOrDefault(r=>r.name.StartsWith("Building_House_01"));if(house!=null){camera.transform.position=house.bounds.center+house.transform.forward*14+Vector3.up*2;camera.transform.LookAt(house.bounds.center);Hour(12);yield return new WaitForSeconds(1);Save(camera,"house-front");}
  WorldSession.Instance.RequestTransition("Office",p.transform);while(WorldSession.Instance.Busy)yield return null;var computer=Object.FindObjectsByType<ComputerInteraction>(FindObjectsSortMode.None).First(c=>c.gameObject.scene.name=="Office");
  computer.EnterComputer();while(computer.MovingCamera)yield return null;yield return new WaitForSeconds(1);
  File.WriteAllText("v3-terminal.txt","using="+computer.UsingComputer+" camera="+computer.playerCamera.isActiveAndEnabled+" screen="+computer.screenContent.gameObject.activeInHierarchy+" alpha="+computer.screenContent.alpha+" text="+computer.statusText.text+" textActive="+computer.statusText.isActiveAndEnabled+" characters="+computer.statusText.textInfo.characterCount+" textBounds="+computer.statusText.bounds+" canvas="+computer.screenContent.GetComponent<Canvas>().enabled+" textPos="+computer.statusText.transform.position+" cameraPos="+computer.playerCamera.transform.position);
  Save(computer.playerCamera,"terminal-active");computer.ExitComputer();while(computer.MovingCamera)yield return null;Save(computer.playerCamera,"office-return");
  File.WriteAllText("v3-visual.txt","Screenshots rendered in URP from isolated project; WebView unavailable.");EditorApplication.Exit(0);
 }
}
