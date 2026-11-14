using System;using System.Collections.Generic;using UnityEngine;using UnityEngine.SceneManagement;
// Scene-resident source models. Live instances still belong to the existing simulation and save systems.
[DefaultExecutionOrder(-32000)]public sealed class GridShiftSceneTemplates:MonoBehaviour {
 [Serializable]public sealed class Entry {public string key;public GameObject model;}
 public Entry[] models=Array.Empty<Entry>();static readonly Dictionary<int,GridShiftSceneTemplates> scenes=new Dictionary<int,GridShiftSceneTemplates>();
 void Awake(){if(!Application.isPlaying)return;scenes[gameObject.scene.handle]=this;gameObject.SetActive(false);}
 void OnDestroy(){if(scenes.TryGetValue(gameObject.scene.handle,out var root)&&root==this)scenes.Remove(gameObject.scene.handle);}
 public static GameObject Find(Scene scene,string key){if(!scenes.TryGetValue(scene.handle,out var root)||root==null)return null;foreach(var entry in root.models)if(entry.key==key)return entry.model;return null;}
}
