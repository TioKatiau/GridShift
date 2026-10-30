using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoltstroStudios.UnityWebBrowser;
using VoltstroStudios.UnityWebBrowser.Shared;
using VoltstroStudios.UnityWebBrowser.Shared.Events;
public sealed class GridShiftUwbInputRouter:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IDragHandler {
 public WebBrowserUIBasic browser;public GridShiftUwbInput input;public Camera eventCamera;[Min(1)]public float scrollPixels=110;
 public bool Focused{get;private set;}public bool TextEditing{get;set;}
 public static bool BrowserHasFocus{get;private set;}
 static GridShiftUwbInputRouter owner;
 readonly List<RaycastResult> scrollHits=new List<RaycastResult>(16);
 PointerEventData scrollPointer;EventSystem scrollEvents;float scrollRemainder;
 readonly HashSet<WindowsKey> heldKeys=new HashSet<WindowsKey>();readonly HashSet<MouseClickType> heldButtons=new HashSet<MouseClickType>();Vector2 lastPosition=new Vector2(-1,-1);
 public bool Ready=>browser!=null&&browser.browserClient!=null&&!browser.browserClient.HasDisposed&&browser.browserClient.IsConnected&&browser.browserClient.ReadySignalReceived;
 public void SetFocus(bool focused){if(focused&&owner!=null&&owner!=this)owner.SetFocus(false);if(!focused){Release();scrollRemainder=0;if(owner==this)owner=null;}else owner=this;Focused=focused;BrowserHasFocus=owner!=null;if(input!=null)input.SetActive(focused);var image=GetComponent<RawImage>();if(image!=null)image.raycastTarget=focused;}
 bool ScrollTarget(Vector2 screen){
  var events=EventSystem.current;if(events==null)return false;
  if(scrollEvents!=events){scrollEvents=events;scrollPointer=new PointerEventData(events);}
  scrollPointer.Reset();scrollPointer.position=screen;scrollHits.Clear();events.RaycastAll(scrollPointer,scrollHits);
  return scrollHits.Count>0&&scrollHits[0].gameObject==gameObject;
 }
 void SendScroll(Vector2 screen){
  float ticks=input.GetScroll();if(ticks==0)return;
  // Drag capture must never turn an off-surface wheel event into a browser event.
  if(!Map(screen,out var pixel)||!ScrollTarget(screen)){scrollRemainder=0;return;}
  scrollRemainder+=ticks*scrollPixels;int delta=(int)scrollRemainder;
  if(delta!=0){browser.browserClient.SendMouseMove(pixel);browser.browserClient.SendMouseScroll(pixel,delta);scrollRemainder-=delta;}
 }
 public bool Map(Vector2 screen,out Vector2 pixel,bool clamp=false){pixel=Vector2.zero;var rect=GetComponent<RectTransform>();var texture=browser!=null?browser.browserClient?.BrowserTexture:null;if(rect==null||texture==null)return false;var camera=eventCamera!=null?eventCamera:GetComponentInParent<Canvas>()?.worldCamera;
  if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,screen,camera,out var local))return false;var bounds=rect.rect;bool inside=bounds.Contains(local);if(!inside&&!clamp)return false;
  pixel=new Vector2(Mathf.Clamp((local.x-bounds.xMin)/bounds.width*texture.width,0,texture.width-1),Mathf.Clamp((bounds.yMax-local.y)/bounds.height*texture.height,0,texture.height-1));return inside||clamp;
 }
 void LateUpdate(){if(!Focused||!Ready||input==null)return;input.EnsureKeyboard();var screen=input.GetCursorPos();if(Map(screen,out var pixel,heldButtons.Count>0)){if(pixel!=lastPosition){browser.browserClient.SendMouseMove(pixel);lastPosition=pixel;}}SendScroll(screen);
  var down=input.GetDownKeys();var up=input.GetUpKeys();string text=input.GetFrameInputBuffer();foreach(var key in down)heldKeys.Add(key);foreach(var key in up)heldKeys.Remove(key);if(down.Length>0||up.Length>0||text.Length>0)browser.browserClient.SendKeyboardControls(down,up,text.ToCharArray());
 }
 static MouseClickType Button(PointerEventData data)=>data.button==PointerEventData.InputButton.Right?MouseClickType.Right:data.button==PointerEventData.InputButton.Middle?MouseClickType.Middle:MouseClickType.Left;
 public void OnPointerDown(PointerEventData data){if(!Focused||!Ready||!Map(data.position,out var pixel))return;TextEditing=true;var button=Button(data);heldButtons.Add(button);lastPosition=pixel;browser.browserClient.SendMouseClick(pixel,Mathf.Max(1,data.clickCount),button,MouseEventType.Down);EventSystem.current?.SetSelectedGameObject(gameObject);}
 public void OnPointerUp(PointerEventData data){var button=Button(data);if(!Focused||!Ready||!heldButtons.Remove(button)||!Map(data.position,out var pixel,true))return;lastPosition=pixel;browser.browserClient.SendMouseClick(pixel,Mathf.Max(1,data.clickCount),button,MouseEventType.Up);}
 public void OnDrag(PointerEventData data){if(Focused&&Ready&&heldButtons.Count>0&&Map(data.position,out var pixel,true)){lastPosition=pixel;browser.browserClient.SendMouseMove(pixel);}}
 void Release(){if(Ready){if(heldKeys.Count>0)browser.browserClient.SendKeyboardControls(Array.Empty<WindowsKey>(),heldKeys.ToArray(),Array.Empty<char>());foreach(var button in heldButtons)browser.browserClient.SendMouseClick(lastPosition,1,button,MouseEventType.Up);}heldKeys.Clear();heldButtons.Clear();input?.DisableIme();}
 void OnDisable(){SetFocus(false);}
 void OnApplicationFocus(bool focus){if(!focus)Release();}
}
