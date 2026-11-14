using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class GridShiftV22Build {
 public static void Batch(){Directory.CreateDirectory("Docs/Validacao22");int missing=0;var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();foreach(var path in scenes){var scene=EditorSceneManager.OpenScene(path);foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);}if(missing>0){File.WriteAllText("Docs/Validacao22/build.txt","FAILED missing components="+missing);EditorApplication.Exit(2);return;}PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.resizableWindow=true;var build=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName="Builds/GridShift22_Casa_Rotina/GridShift.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});File.WriteAllText("Docs/Validacao22/build.txt",build.summary.result+"\nErrors="+build.summary.totalErrors+"\nBytes="+build.summary.totalSize+"\nMissingComponents="+missing+"\nScenes="+string.Join(",",scenes)+"\n");EditorApplication.Exit(build.summary.result==BuildResult.Succeeded?0:1);}
}
