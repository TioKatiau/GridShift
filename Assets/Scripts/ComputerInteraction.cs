using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
public class ComputerInteraction : MonoBehaviour {
 [Header("Player")] public Transform player; public PlayerMovement playerMovement;
 [Header("Câmera")] public Camera playerCamera; public Transform computerViewPoint;
 [Header("Interface")] public GameObject interactionText,minimap,compass,Aim,hudRoot,hudBottom;
 [Header("Configurações")] public float interactionDistance=2f,zoomDuration=.8f;
 [Range(20,90)] public float terminalFieldOfView=45;
 [Min(.1f)] public float fadeDuration=.5f;
 public TerminalWebView webView;
 public CanvasGroup screenContent;
 public TMP_Text statusText;
 public string url="https://urbanlink-flow.onrender.com";
 public bool UsingComputer=>usingComputer;
 public bool MovingCamera=>movingCamera;
 bool usingComputer,movingCamera,oldMovement,oldBlocked,oldCursorVisible;
 CursorLockMode oldCursor; float oldFov; Vector3 oldPosition;Quaternion oldRotation;
 bool[] oldHud;GameObject[] hud;
 void OnEnable(){InteractionFocus.Register(this,interactionDistance);}
 void OnDisable(){InteractionFocus.Remove(this);if(usingComputer)Restore();}
 void Start(){if(interactionText!=null)interactionText.SetActive(false);SetScreenAlpha(0);}
 void SetScreenAlpha(float alpha){if(screenContent!=null)screenContent.alpha=alpha;if(statusText!=null)statusText.alpha=alpha;}
 void Update(){
  bool active=Time.timeScale>0&&player!=null&&playerCamera!=null&&UnityEngine.SceneManagement.SceneManager.GetActiveScene()==gameObject.scene&&(WorldSession.Instance==null||!WorldSession.Instance.Busy);
  if(!active){if(interactionText!=null)interactionText.SetActive(false);return;}
  bool near=!usingComputer&&InteractionFocus.Selected(this,player);
  if(interactionText!=null){interactionText.SetActive(near);if(near)interactionText.transform.rotation=playerCamera.transform.rotation;}
  if(Keyboard.current==null||movingCamera)return;
  if(usingComputer){if(Keyboard.current.escapeKey.wasPressedThisFrame){EscapeInput.Consume();ExitComputer();}}
  else if(near&&Keyboard.current.eKey.wasPressedThisFrame&&InteractionFocus.Consume())EnterComputer();
 }
 public void EnterComputer(){
  if(usingComputer||movingCamera||playerCamera==null||computerViewPoint==null||playerMovement==null)return;
  oldPosition=playerCamera.transform.localPosition;oldRotation=playerCamera.transform.localRotation;oldFov=playerCamera.fieldOfView;
  oldMovement=playerMovement.enabled;oldBlocked=playerMovement.ControlsBlocked;oldCursor=Cursor.lockState;oldCursorVisible=Cursor.visible;
  hud=new[]{minimap,compass,Aim,hudRoot,hudBottom};oldHud=new bool[hud.Length];for(int i=0;i<hud.Length;i++)if(hud[i]!=null){oldHud[i]=hud[i].activeSelf;hud[i].SetActive(false);}
  usingComputer=true;playerMovement.BlockControls(true);if(interactionText!=null)interactionText.SetActive(false);
  StartCoroutine(Animate(true));
 }
 public void ExitComputer(){if(!usingComputer||movingCamera)return;if(webView!=null)webView.Hide();StartCoroutine(Animate(false));}
 IEnumerator Animate(bool entering){
  movingCamera=true;var camera=playerCamera.transform;Vector3 from=camera.position;Quaternion rotation=camera.rotation;float fov=playerCamera.fieldOfView;
  Vector3 target=entering?computerViewPoint.position:camera.parent.TransformPoint(oldPosition);
  Quaternion targetRot=entering?computerViewPoint.rotation:camera.parent.rotation*oldRotation;
  float startAlpha=screenContent!=null?screenContent.alpha:0;
  for(float t=0;t<Mathf.Max(.01f,zoomDuration);t+=Time.unscaledDeltaTime){float k=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/Mathf.Max(.01f,zoomDuration)));
   camera.SetPositionAndRotation(Vector3.Lerp(from,target,k),Quaternion.Slerp(rotation,targetRot,k));playerCamera.fieldOfView=Mathf.Lerp(fov,entering?terminalFieldOfView:oldFov,k);
   SetScreenAlpha(Mathf.Lerp(startAlpha,entering?1:0,Mathf.Clamp01(t/Mathf.Max(.01f,fadeDuration))));yield return null;
  }
  camera.SetPositionAndRotation(target,targetRot);playerCamera.fieldOfView=entering?terminalFieldOfView:oldFov;
  SetScreenAlpha(entering?1:0);
  movingCamera=false;
  if(entering){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
   if(webView!=null){if(statusText!=null)statusText.text="Conectando…";webView.Open(url,ReportStatus);}
   else ReportStatus("Navegador indisponível.\nESC para sair.");
  }else Restore();
 }
 void ReportStatus(string text){if(statusText!=null)statusText.text=text;}
 void Restore(){if(webView!=null)webView.Hide();SetScreenAlpha(0);
  if(playerCamera!=null){playerCamera.transform.localPosition=oldPosition;playerCamera.transform.localRotation=oldRotation;playerCamera.fieldOfView=oldFov;}
  if(playerMovement!=null){playerMovement.enabled=oldMovement;playerMovement.BlockControls(oldBlocked);}
  if(hud!=null)for(int i=0;i<hud.Length;i++)if(hud[i]!=null)hud[i].SetActive(oldHud[i]);
  Cursor.lockState=oldCursor;Cursor.visible=oldCursorVisible;usingComputer=movingCamera=false;
 }
}
