using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class GridShiftV19Build {
 public static void Batch(){Directory.CreateDirectory("Docs/Validacao19");if(!GridShiftV19Audit.Run()){EditorApplication.Exit(2);return;}PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.resizableWindow=true;var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();var build=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName="Builds/GridShift_Feira_SENAI_2026/GridShift.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});File.WriteAllText("Docs/Validacao19/build.txt",build.summary.result+"\nErrors="+build.summary.totalErrors+"\nBytes="+build.summary.totalSize+"\nResolution=1920x1080 FullScreenWindow\nScenes="+string.Join(",",scenes)+"\n");EditorApplication.Exit(build.summary.result==BuildResult.Succeeded?0:1);}
}
