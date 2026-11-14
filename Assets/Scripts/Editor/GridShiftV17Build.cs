using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class GridShiftV17Build {
 public static void Batch(){Directory.CreateDirectory("Docs/Validacao17");if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows64)){File.WriteAllText("Docs/Validacao17/build.txt","BLOCKED: Windows Standalone module is not installed for this Unity editor.\n");EditorApplication.Exit(2);return;}
  var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName="Builds/GridShift17/GridShift.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});File.WriteAllText("Docs/Validacao17/build.txt",report.summary.result+"\nErrors="+report.summary.totalErrors+"\nSize="+report.summary.totalSize+"\n");EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
 }
}
