using System;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName="GridShift/Audio Library")]
public sealed class GridShiftAudioLibrary : ScriptableObject {
 [Serializable] public struct Entry { public string id; public AudioClip clip; }
 public AudioMixer mixer;
 public AudioMixerGroup[] groups;
 public Entry[] clips;
 public AudioClip Find(string id){if(clips!=null)foreach(var e in clips)if(e.id==id)return e.clip;return null;}
 public AudioMixerGroup Group(string id){if(groups!=null)foreach(var g in groups)if(g!=null&&g.name==id)return g;return null;}
}
