using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
[InitializeOnLoad]public static class GridShiftV19Supplement {
 static GridShiftV19Supplement(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("GS19Supplement",false)){var go=new GameObject("Supplementary real warehouse view");Object.DontDestroyOnLoad(go);go.AddComponent<GridShiftV19SupplementRunner>();}};}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Prepare(){if(!SessionState.GetBool("GS19Supplement",false))return;DeliverySaveStore.OverridePath=Path.GetFullPath("Temp/Film19Supplement-"+Guid.NewGuid().ToString("N")+"/slot.json");DeliverySaveStore.Pending=null;DeliverySaveStore.StartFresh=true;GridShiftCompany.SuppressIntro=true;}
 public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");SessionState.SetBool("GS19Supplement",true);GridShiftV19Film.Enabled=true;EditorApplication.isPlaying=true;}
}
public sealed class GridShiftV19SupplementRunner:MonoBehaviour {
 IEnumerator Start(){while(DeliveryManager.Instance?.GetComponent<GridShiftActiveOperations>()?.Van==null)yield return null;GridShiftTutorial.Skip();GridShiftOperationsUI.Instance.Close();var m=DeliveryManager.Instance;var player=InteractionFocus.PlayerFor(m);yield return WorldSession.Instance.RequestTransition("FactoryInterior",player.transform);yield return new WaitForSeconds(.5f);yield return GridShiftV19Film.Record("03-factory-interior-clean",Camera.main,8,false,(c,t)=>{var root=GridShiftFactoryInterior.Instance.transform;c.fieldOfView=75;c.transform.position=root.TransformPoint(new Vector3(Mathf.Sin(t*.1f)*.8f,1.85f,-9.6f));c.transform.LookAt(root.TransformPoint(new Vector3(0,1.6f,3)));});SessionState.SetBool("GS19Supplement",false);EditorApplication.Exit(0);}
}
