using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using VoltstroStudios.UnityWebBrowser.Input;
using VoltstroStudios.UnityWebBrowser.Shared;
[CreateAssetMenu(menuName="GridShift/UWB Input System")]
public sealed class GridShiftUwbInput:WebBrowserInputHandler {
 readonly StringBuilder text=new StringBuilder();readonly List<WindowsKey> keys=new List<WindowsKey>();Keyboard keyboard;bool active;
 public void SetActive(bool value){active=value;if(!value){Detach();text.Clear();}else EnsureKeyboard();}
 void Detach(){if(keyboard!=null)keyboard.onTextInput-=Text;keyboard=null;}
 public void EnsureKeyboard(){if(!active)return;if(keyboard==Keyboard.current)return;Detach();keyboard=Keyboard.current;if(keyboard!=null)keyboard.onTextInput+=Text;}
 void Text(char character){if(active&&(!char.IsControl(character)||character=='\r'||character=='\n'||character=='\t'))text.Append(character);}
 public override Vector2 GetCursorPos()=>Mouse.current!=null?Mouse.current.position.ReadValue():Vector2.zero;
 // Input System 1.20 defaults to one unit per notch, rather than Win32's 120.
 // Keep fractional trackpad deltas; the router accumulates them before sending integers.
 public override float GetScroll(){
  if(!active||Mouse.current==null)return 0;
  float delta=Mouse.current.scroll.ReadValue().y;
  bool nativeWindows=InputSystem.settings.scrollDeltaBehavior==InputSettings.ScrollDeltaBehavior.KeepPlatformSpecificInputRange&&
   (Application.platform==RuntimePlatform.WindowsEditor||Application.platform==RuntimePlatform.WindowsPlayer);
  return nativeWindows?delta/120f:delta;
 }
 WindowsKey[] Keys(bool down){EnsureKeyboard();keys.Clear();if(!active||keyboard==null)return Array.Empty<WindowsKey>();foreach(var key in keyboard.allKeys){if(key.keyCode==Key.Escape||!(down?key.wasPressedThisFrame:key.wasReleasedThisFrame))continue;try{var translated=key.keyCode.UnityKeyToWindowKey();if(translated!=WindowsKey.None)keys.Add(translated);}catch(ArgumentOutOfRangeException){}}return keys.Count==0?Array.Empty<WindowsKey>():keys.ToArray();}
 public override WindowsKey[] GetDownKeys()=>Keys(true);
 public override WindowsKey[] GetUpKeys()=>Keys(false);
 public override string GetFrameInputBuffer(){string result=text.ToString();text.Clear();return active?result:string.Empty;}
 // O foco pertence ao terminal: sair da imagem não interrompe a digitação.
 public override void OnStart(){}
 public override void OnStop(){}
 public override void EnableIme(Vector2 location){EnsureKeyboard();if(active&&keyboard!=null){keyboard.SetIMEEnabled(true);keyboard.SetIMECursorPosition(GetCursorPos());}}
 public override void DisableIme(){if(keyboard!=null)keyboard.SetIMEEnabled(false);}
 void OnDisable(){DisableIme();SetActive(false);}
}
