using UnityEngine;
public static class GamePreferences {
 public static float MouseSensitivity {get=>PlayerPrefs.GetFloat("GS.MouseSensitivity",1);set{PlayerPrefs.SetFloat("GS.MouseSensitivity",Mathf.Clamp(value,.2f,3));}}
 public static float CameraIntensity {get=>PlayerPrefs.GetFloat("GS.CameraIntensity",1);set{PlayerPrefs.SetFloat("GS.CameraIntensity",Mathf.Clamp01(value));}}
 public static bool RunBob {get=>PlayerPrefs.GetInt("GS.RunBob",1)!=0;set{PlayerPrefs.SetInt("GS.RunBob",value?1:0);}}
 public static float HeadBob {get=>PlayerPrefs.GetFloat("GS.HeadBob",.6f);set{PlayerPrefs.SetFloat("GS.HeadBob",Mathf.Clamp01(value));}}
 public static void Save(){PlayerPrefs.Save();}
}
