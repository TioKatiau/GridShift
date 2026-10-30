using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

// Editor-only asset setup. Runtime audio uses public Unity APIs exclusively.
[InitializeOnLoad] public static class GridShiftAudioSetup {
 const string MixerPath="Assets/Audio/Mixers/GridShift.mixer",LibraryPath="Assets/Resources/GridShiftAudioLibrary.asset";
 static GridShiftAudioSetup(){EditorApplication.delayCall+=AutoInstall;}
 static void AutoInstall(){if(!EditorApplication.isPlayingOrWillChangePlaymode&&AssetDatabase.LoadAssetAtPath<GridShiftAudioLibrary>(LibraryPath)==null)Install();}
 [MenuItem("GridShift/Audio 13/Install and validate")]
 public static void Install(){
  Directory.CreateDirectory("Assets/Audio/Mixers");Directory.CreateDirectory("Assets/Resources");AssetDatabase.Refresh();
  var mixer=AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
  if(mixer==null){
   // Unity exposes no public API for authoring mixer groups. Use the editor's
   // own authoring methods once, then persist a normal editable .mixer asset.
   var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.Audio.AudioMixerController")).First(t=>t!=null);
   var controller=type.GetMethod("CreateMixerControllerAtPath",BindingFlags.Public|BindingFlags.Static).Invoke(null,new object[]{MixerPath});
   var master=type.GetProperty("masterGroup").GetValue(controller);
   var gs=new object[7];gs[0]=master;
   for(int i=1;i<gs.Length;i++){gs[i]=type.GetMethod("CreateNewGroup").Invoke(controller,new object[]{GridShiftAudio.Channels[i],false});type.GetMethod("AddChildToParent").Invoke(controller,new[]{gs[i],master});}
   var exposedProperty=type.GetProperty("exposedParameters");var element=exposedProperty.PropertyType.GetElementType();var exposed=Array.CreateInstance(element,7);
   for(int i=0;i<gs.Length;i++){var value=Activator.CreateInstance(element);element.GetField("name").SetValue(value,GridShiftAudio.Channels[i]+"Volume");element.GetField("guid").SetValue(value,gs[i].GetType().GetMethod("GetGUIDForVolume").Invoke(gs[i],null));exposed.SetValue(value,i);EditorUtility.SetDirty((UnityEngine.Object)gs[i]);}
   exposedProperty.SetValue(controller,exposed);mixer=(AudioMixer)controller;EditorUtility.SetDirty(mixer);AssetDatabase.SaveAssets();
  }
  foreach(var guid in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Audio"})){
   string path=AssetDatabase.GUIDToAssetPath(guid);var importer=(AudioImporter)AssetImporter.GetAtPath(path);var sample=importer.defaultSampleSettings;
   bool music=path.Contains("/Music/"),ambient=path.Contains("/Ambient/")||path.Contains("Ventilation"),loop=music||ambient||path.Contains("_Loop");
   // PCM for seamlessly looping engines and low-latency short effects.
   sample.loadType=music?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
   sample.compressionFormat=music?AudioCompressionFormat.Vorbis:AudioCompressionFormat.PCM;sample.quality=.75f;sample.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
   sample.preloadAudioData=!music;importer.defaultSampleSettings=sample;importer.forceToMono=!music&&!ambient;importer.loadInBackground=music;
   importer.SaveAndReimport();
  }
  var library=AssetDatabase.LoadAssetAtPath<GridShiftAudioLibrary>(LibraryPath);if(library==null){library=ScriptableObject.CreateInstance<GridShiftAudioLibrary>();AssetDatabase.CreateAsset(library,LibraryPath);}
  library.mixer=mixer;library.groups=GridShiftAudio.Channels.Select(n=>mixer.FindMatchingGroups(n).First(g=>g.name==n)).ToArray();
  library.clips=AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Audio"}).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p=>p).Select(p=>new GridShiftAudioLibrary.Entry{id=Path.GetFileNameWithoutExtension(p),clip=AssetDatabase.LoadAssetAtPath<AudioClip>(p)}).ToArray();
  EditorUtility.SetDirty(library);AssetDatabase.SaveAssets();ConfigurePrefabs();Debug.Log("GridShift Audio 13: "+library.clips.Length+" clips, "+library.groups.Length+" mixer groups ready.");
 }
 static void ConfigurePrefabs(){
  var paths=AssetDatabase.GetDependencies(new[]{"Assets/Scenes/SampleScene.unity","Assets/Scenes/Office.unity"},true).Where(p=>p.EndsWith(".prefab")).ToArray();
  var report=new System.Text.StringBuilder();
  foreach(string path in paths){bool vehicle=path.Contains("/Vehicles/")&&Path.GetFileName(path).StartsWith("Vehicle_");bool tree=Path.GetFileName(path).StartsWith("Natures_Big Tree");if(!vehicle&&!tree)continue;
   var root=PrefabUtility.LoadPrefabContents(path);bool changed=false;try{if(vehicle&&root.GetComponent<GridShiftVehicleAudio>()==null){root.AddComponent<GridShiftVehicleAudio>();changed=true;}if(tree&&root.GetComponent<GridShiftAudioZone>()==null){root.AddComponent<GridShiftAudioZone>().radius=12;changed=true;}if(changed)PrefabUtility.SaveAsPrefabAsset(root,path);report.AppendLine(path+" | "+(vehicle?"GridShiftVehicleAudio":"GridShiftAudioZone radius 12m"));}finally{PrefabUtility.UnloadPrefabContents(root);}
  }
  Directory.CreateDirectory("Docs/Audio13");File.WriteAllText("Docs/Audio13/prefabs.txt",report.ToString());
 }
}
