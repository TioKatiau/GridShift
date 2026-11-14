using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class GridShiftV21Build {
 public static void Batch(){Directory.CreateDirectory("Docs/Validacao21");var rain=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/GridShiftRain20.mat");if(rain==null){rain=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));rain.SetFloat("_Surface",1);rain.SetFloat("_SrcBlend",5);rain.SetFloat("_DstBlend",10);rain.SetFloat("_ZWrite",0);rain.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");rain.renderQueue=3000;AssetDatabase.CreateAsset(rain,"Assets/Resources/GridShiftRain20.mat");AssetDatabase.SaveAssets();}if(!GridShiftV19Audit.Run()){EditorApplication.Exit(2);return;}PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.resizableWindow=true;var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();var build=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName="Builds/GridShift21_Conversas_Personagem/GridShift.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});File.WriteAllText("Docs/Validacao21/build.txt",build.summary.result+"\nErrors="+build.summary.totalErrors+"\nBytes="+build.summary.totalSize+"\nResolution=1920x1080 FullScreenWindow\nScenes="+string.Join(",",scenes)+"\n");EditorApplication.Exit(build.summary.result==BuildResult.Succeeded?0:1);}
}

