using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
public enum GSAction{Forward,Back,Left,Right,Run,Jump,Tablet,Interact,Map,Pause,Controls,Tutorial,Upgrades,Deliveries}
public static class GridShiftControls {
 public static readonly GSAction[] Actions=(GSAction[])Enum.GetValues(typeof(GSAction));
 static readonly Key[] defaults={Key.W,Key.S,Key.A,Key.D,Key.LeftShift,Key.Space,Key.G,Key.E,Key.M,Key.Escape,Key.F2,Key.F1,Key.U,Key.J};
 static readonly string[] labels={"Avançar","Recuar","Esquerda","Direita","Correr","Pular","Tablet / guardar","Interagir / computador","Ampliar mapa","Pausa / voltar","Tela de controles","Tutorial","Upgrades da empresa","Painel de entregas"};
 static Dictionary<GSAction,Key> bindings;static int suppressed=-1;
 public static bool Capturing{get;set;}
 public static string Prefix="GS.Keys.";
 static void Load(){if(bindings!=null)return;bindings=new Dictionary<GSAction,Key>();foreach(var a in Actions){var key=(Key)PlayerPrefs.GetInt(Prefix+a,(int)defaults[(int)a]);if(key==Key.None||!Enum.IsDefined(typeof(Key),key)||bindings.Values.Contains(key))key=defaults[(int)a];bindings[a]=key;}if(bindings.Values.Distinct().Count()!=Actions.Length)Reset();}
 public static Key Binding(GSAction action){Load();return bindings[action];}
 public static string Label(GSAction action)=>labels[(int)action];
 public static string KeyName(GSAction action)=>Binding(action).ToString().Replace("LeftShift","Shift esquerdo").Replace("RightShift","Shift direito").Replace("Space","Espaço").Replace("Escape","ESC");
 public static bool Down(GSAction action)=>!Capturing&&suppressed!=Time.frameCount&&Keyboard.current!=null&&Keyboard.current[Binding(action)].wasPressedThisFrame;
 public static bool Held(GSAction action)=>!Capturing&&Keyboard.current!=null&&Keyboard.current[Binding(action)].isPressed;
 public static bool Back=>!Capturing&&suppressed!=Time.frameCount&&Keyboard.current!=null&&(Keyboard.current.escapeKey.wasPressedThisFrame||Keyboard.current[Binding(GSAction.Pause)].wasPressedThisFrame);
 public static bool InterfaceBack(bool editing)=>!Capturing&&suppressed!=Time.frameCount&&Keyboard.current!=null&&(Keyboard.current.escapeKey.wasPressedThisFrame||(!editing&&Down(GSAction.Pause)));
 public static void SuppressFrame(){suppressed=Time.frameCount;EscapeInput.Consume();}
 public static bool Rebind(GSAction action,Key key,out string error){Load();error=null;if(key==Key.None||key==Key.LeftWindows||key==Key.RightWindows||key==Key.PrintScreen||(key==Key.Escape&&action!=GSAction.Pause)){error="Tecla reservada pelo sistema. ESC sempre permite voltar.";return false;}foreach(var pair in bindings)if(pair.Key!=action&&pair.Value==key){error="Conflito com: "+Label(pair.Key)+". Escolha outra tecla.";return false;}bindings[action]=key;PlayerPrefs.SetInt(Prefix+action,(int)key);PlayerPrefs.Save();SuppressFrame();return true;}
 public static void Reset(){bindings=new Dictionary<GSAction,Key>();foreach(var a in Actions){bindings[a]=defaults[(int)a];PlayerPrefs.DeleteKey(Prefix+a);}PlayerPrefs.Save();SuppressFrame();}
 public static void Reload(){bindings=null;Capturing=false;}
}

