using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-200)]
public sealed class PortableTablet:MonoBehaviour {
 public PlayerMovement player;public Camera view;public GameObject modelPrefab;public GridShiftUwbTerminal browserPrefab;
 public Vector3 position=new Vector3(0,-.035f,.76f),rotation=new Vector3(2,0,0);public float scale=1,animationDuration=.42f;
 [Tooltip("Permite caminhar com setas, sem interpretar letras digitadas na página como movimento.")]
 public bool allowWalking=false;
 [Range(100,250)]public float pageZoom=200;public Vector2Int resolution=new Vector2Int(1600,1000);[Min(1)]public float scrollPixels=110;
 public string url="https://urbanlink-flow.onrender.com";
 public bool IsOpen{get;private set;}public bool Animating{get;private set;}
 public GridShiftUwbTerminal Browser=>host;
 GameObject model;CanvasGroup screen;GridShiftUwbTerminal host,owned;
 Transform oldParent;CanvasGroup oldSurface;Vector3 oldLocal,oldScale;Quaternion oldRotation;bool blocked,cursorVisible;CursorLockMode cursor;float oldZoom;Vector2Int oldResolution;bool oldAspect;
 CanvasGroup[] hud;float[] hudAlpha;Coroutine transition;
 void Awake(){if(player==null)player=GetComponent<PlayerMovement>();if(view==null&&player!=null)view=player.CameraTransform.GetComponent<Camera>();}
 void Update(){if(Keyboard.current==null)return;if(IsOpen){if(GridShiftControls.BrowserBack(host!=null&&host.inputRouter.TextEditing)){EscapeInput.Consume();Close();}else if(GridShiftControls.Down(GSAction.Tablet)&&(host==null||!host.inputRouter.TextEditing))Close();if(allowWalking&&!Animating&&Keyboard.current!=null){var k=Keyboard.current;var step=new Vector3((k.rightArrowKey.isPressed?1:0)-(k.leftArrowKey.isPressed?1:0),0,(k.upArrowKey.isPressed?1:0)-(k.downArrowKey.isPressed?1:0));var cc=player.GetComponent<CharacterController>();if(cc.enabled)cc.Move((player.transform.TransformDirection(Vector3.ClampMagnitude(step,1))*2+Vector3.down*2)*Time.deltaTime);}return;}
  if(GridShiftControls.Down(GSAction.Tablet))Open();
 }
 public bool Open(){if(IsOpen||Animating||player==null||view==null||player.ControlsBlocked||Time.timeScale<=0||SceneManager.GetActiveScene()!=gameObject.scene||(PauseMenu.Instance!=null&&PauseMenu.Instance.IsOpen)||(WorldSession.Instance!=null&&WorldSession.Instance.Busy))return false;
  if(model==null){model=Instantiate(modelPrefab,view.transform,false);model.name="PortableTabletViewModel";screen=model.GetComponentInChildren<CanvasGroup>(true);screen.GetComponent<Canvas>().worldCamera=view;}
  host=null;foreach(var t in Object.FindObjectsByType<ComputerInteraction>(FindObjectsSortMode.None))if(t.gameObject.scene==gameObject.scene){if(t.UsingComputer||t.MovingCamera)return false;host=t.webView as GridShiftUwbTerminal;if(host!=null)break;}
  if(host==null){if(owned==null){owned=Instantiate(browserPrefab,transform);owned.name="PortableChromium";}host=owned;}
  if(host==null)return false;oldParent=host.browser.transform.parent.parent;var viewport=host.browser.transform.parent;oldLocal=viewport.localPosition;oldRotation=viewport.localRotation;oldScale=viewport.localScale;oldSurface=host.surface;oldZoom=host.pageZoom;oldResolution=host.resolution;oldAspect=host.matchMonitorAspect;
  viewport.SetParent(screen.transform,false);viewport.localPosition=Vector3.zero;viewport.localRotation=Quaternion.identity;viewport.localScale=Vector3.one;host.surface=screen;host.pageZoom=pageZoom;host.resolution=resolution;host.matchMonitorAspect=true;host.inputRouter.eventCamera=view;host.inputRouter.scrollPixels=scrollPixels;
  blocked=player.ControlsBlocked;cursor=Cursor.lockState;cursorVisible=Cursor.visible;player.BlockControls(true);Cursor.lockState=CursorLockMode.None;Cursor.visible=true;IsOpen=true;DeliveryManager.Instance?.Company?.Missions.TabletOpened();model.SetActive(true);screen.alpha=0;
  // Reduce only the HUD canvases belonging to this avatar; never fade the web canvas.
  var groups=new System.Collections.Generic.List<CanvasGroup>();foreach(var c in player.GetComponentsInChildren<Canvas>(true))if(!c.transform.IsChildOf(model.transform)){var g=c.GetComponent<CanvasGroup>();if(g==null)g=c.gameObject.AddComponent<CanvasGroup>();groups.Add(g);}hud=groups.ToArray();hudAlpha=new float[hud.Length];for(int i=0;i<hud.Length;i++){hudAlpha[i]=hud[i].alpha;hud[i].alpha=0;}
  transition=StartCoroutine(Animate(true));return true;
 }
 public void Close(){if(!IsOpen||Animating)return;host.Hide();transition=StartCoroutine(Animate(false));}
 IEnumerator Animate(bool entering){Animating=true;Vector3 below=position+new Vector3(0,-.85f,.08f);for(float t=0;t<Mathf.Max(.05f,animationDuration);t+=Time.unscaledDeltaTime){float k=Mathf.SmoothStep(0,1,t/Mathf.Max(.05f,animationDuration));model.transform.localPosition=Vector3.Lerp(entering?below:position,entering?position:below,k);model.transform.localRotation=Quaternion.Euler(rotation+new Vector3(Mathf.Lerp(entering?14:0,entering?0:14,k),0,0));model.transform.localScale=Vector3.one*scale;screen.alpha=entering?Mathf.InverseLerp(.55f,1,k):1-k;yield return null;}
  Animating=false;transition=null;if(entering){model.transform.localPosition=position;model.transform.localRotation=Quaternion.Euler(rotation);screen.alpha=1;host.Open(url,s=>{});}else Restore();
 }
 void Restore(){if(host!=null){host.Hide();var viewport=host.browser.transform.parent;viewport.SetParent(oldParent,false);viewport.localPosition=oldLocal;viewport.localRotation=oldRotation;viewport.localScale=oldScale;host.surface=oldSurface;host.pageZoom=oldZoom;host.resolution=oldResolution;host.matchMonitorAspect=oldAspect;}if(screen!=null)screen.alpha=0;if(model!=null)model.SetActive(false);if(hud!=null)for(int i=0;i<hud.Length;i++)if(hud[i]!=null)hud[i].alpha=hudAlpha[i];if(player!=null)player.BlockControls(blocked);Cursor.lockState=cursor;Cursor.visible=cursorVisible;IsOpen=Animating=false;host=null;}
 void OnDisable(){if(transition!=null)StopCoroutine(transition);if(IsOpen)Restore();if(owned!=null){Destroy(owned.gameObject);owned=null;}}
 void OnDestroy(){if(model!=null)Destroy(model);}
}


