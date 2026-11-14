using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]public static class GridShiftV23Regression {
 static string report;static double deadline;
 static GridShiftV23Regression(){if(!SessionState.GetBool("GS23Regression",false))return;report=SessionState.GetString("GS23RegressionPath","");deadline=EditorApplication.timeSinceStartup+600;UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;EditorApplication.update+=Poll;}
 public static void Batch(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-gridshift-report");report=Path.Combine(i>=0&&i+1<args.Length?args[i+1]:"Docs/Validacao23/regressao-editor","runtime.txt");if(File.Exists(report))File.Delete(report);SessionState.SetString("GS23RegressionPath",report);SessionState.SetBool("GS23Regression",true);deadline=EditorApplication.timeSinceStartup+600;EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");EditorApplication.update+=Poll;EditorApplication.isPlaying=true;}
 static void Poll(){if(File.Exists(report)){string text=File.ReadAllText(report);if(text.Contains("RESULT SUCCESS")){SessionState.SetBool("GS23Regression",false);EditorApplication.Exit(0);return;}if(text.Contains("RESULT FAILURE")){SessionState.SetBool("GS23Regression",false);EditorApplication.Exit(1);return;}}if(EditorApplication.timeSinceStartup>deadline){SessionState.SetBool("GS23Regression",false);EditorApplication.Exit(2);}}
}
