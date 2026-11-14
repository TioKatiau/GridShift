using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
public static class GridShiftV19Audit {
 public static void Batch(){EditorApplication.Exit(Run()?0:1);}
 public static bool Run(){Directory.CreateDirectory("Docs/Validacao19");var report=new StringBuilder();report.AppendLine("STATIC AUDIT: enabled scenes and project script references");int missing=0;foreach(string path in AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/")&&(p.EndsWith(".unity")||p.EndsWith(".prefab")))){string contents=File.ReadAllText(path);foreach(System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(contents,@"m_Script: \{fileID: [^,]+, guid: ([a-f0-9]{32})")){if(string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(match.Groups[1].Value))){report.AppendLine("P0 MISSING SCRIPT "+path+" "+match.Groups[1].Value);missing++;}}}report.AppendLine("Missing script GUID references="+missing);foreach(var entry in EditorBuildSettings.scenes.Where(s=>s.enabled)){var scene=EditorSceneManager.OpenScene(entry.path);int lost=0;foreach(var root in scene.GetRootGameObjects())foreach(var transform in root.GetComponentsInChildren<Transform>(true))lost+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);report.AppendLine(entry.path+" missingComponents="+lost+" cameras="+Object.FindObjectsByType<Camera>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length+" listeners="+Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length);missing+=lost;}report.AppendLine("RESULT "+(missing==0?"SUCCESS":"FAILURE"));File.WriteAllText("Docs/Validacao19/auditoria-estatica.txt",report.ToString());return missing==0;}
}
