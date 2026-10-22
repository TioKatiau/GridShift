using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu:MonoBehaviour {
 public static PauseMenu Instance{get;private set;}
 public bool IsOpen{get;private set;}
 public bool SettingsOpen{get;private set;}
 CanvasGroup group;RectTransform panel;GameObject main,settings;PlayerMovement player;
 bool oldBlocked,oldCursorVisible;CursorLockMode oldCursor;float oldScale=1;Coroutine animation;
 ComputerInteraction[] terminals=new ComputerInteraction[0];
 readonly Color yellow=new Color(.95f,.8f,.17f),dark=new Color(.075f,.085f,.11f,.98f);
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Install(){if(Instance==null)new GameObject("GridShiftPause").AddComponent<PauseMenu>();}
 void Awake(){if(Instance!=null){Destroy(gameObject);return;}Instance=this;DontDestroyOnLoad(gameObject);ApplySaved();Build();SceneManager.sceneLoaded+=Loaded;Cache();}
 void OnDestroy(){SceneManager.sceneLoaded-=Loaded;if(Instance==this){if(IsOpen)Time.timeScale=oldScale;Instance=null;}}
 void Loaded(Scene scene,LoadSceneMode mode){Cache();}
 void Cache(){terminals=Object.FindObjectsByType<ComputerInteraction>(FindObjectsSortMode.None);}
 void LateUpdate(){if(Keyboard.current==null||!Keyboard.current.escapeKey.wasPressedThisFrame||EscapeInput.Consumed)return;
  if(WorldSession.Instance!=null&&WorldSession.Instance.Busy)return;
  foreach(var t in terminals)if(t!=null&&t.isActiveAndEnabled&&(t.UsingComputer||t.MovingCamera))return;
  EscapeInput.Consume();if(!IsOpen)Open();else if(SettingsOpen)ShowSettings(false);else Close();
 }
 public void Open(){if(IsOpen||(WorldSession.Instance!=null&&WorldSession.Instance.Busy))return;
  player=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).FirstOrDefault(p=>p.gameObject.scene==SceneManager.GetActiveScene());
  oldBlocked=player!=null&&player.ControlsBlocked;if(player!=null)player.BlockControls(true);
  oldScale=Time.timeScale;oldCursor=Cursor.lockState;oldCursorVisible=Cursor.visible;Time.timeScale=0;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;IsOpen=true;group.gameObject.SetActive(true);group.interactable=true;group.blocksRaycasts=true;ShowSettings(false);Animate(true);
 }
 public void Close(){if(!IsOpen)return;IsOpen=false;SettingsOpen=false;Time.timeScale=oldScale;if(player!=null)player.BlockControls(oldBlocked);Cursor.lockState=oldCursor;Cursor.visible=oldCursorVisible;group.interactable=false;group.blocksRaycasts=false;GamePreferences.Save();Animate(false);}
 public void ShowSettings(bool show){SettingsOpen=show;main.SetActive(!show);settings.SetActive(show);panel.sizeDelta=new Vector2(680,show?900:440);}
 void Animate(bool opening){if(animation!=null)StopCoroutine(animation);animation=StartCoroutine(Fade(opening));}
 IEnumerator Fade(bool opening){float start=group.alpha;for(float t=0;t<.2f;t+=Time.unscaledDeltaTime){float k=Mathf.SmoothStep(0,1,t/.2f);group.alpha=Mathf.Lerp(start,opening?1:0,k);panel.localScale=Vector3.one*Mathf.Lerp(.97f,1,group.alpha);yield return null;}group.alpha=opening?1:0;if(!opening)group.gameObject.SetActive(false);animation=null;}
 RectTransform Rect(string name,Transform parent,Vector2 size){var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.sizeDelta=size;return r;}
 TMP_Text Text(Transform parent,string text,int size=24){var r=Rect("Text",parent,new Vector2(580,36));var label=r.gameObject.AddComponent<TextMeshProUGUI>();label.text=text;label.fontSize=size;label.color=Color.white;label.alignment=TextAlignmentOptions.MidlineLeft;label.raycastTarget=false;return label;}
 void Stack(RectTransform r,float spacing){var layout=r.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=spacing;layout.childControlHeight=false;layout.childControlWidth=true;layout.childForceExpandHeight=false;layout.padding=new RectOffset(30,30,24,24);}
 Button Button(Transform parent,string label,UnityEngine.Events.UnityAction click){var r=Rect(label,parent,new Vector2(580,48));var img=r.gameObject.AddComponent<Image>();img.color=new Color(.14f,.16f,.2f);var b=r.gameObject.AddComponent<Button>();var colors=b.colors;colors.highlightedColor=yellow;colors.pressedColor=new Color(.7f,.57f,.12f);b.colors=colors;b.onClick.AddListener(click);var t=Text(r,label,23);t.alignment=TextAlignmentOptions.Center;t.rectTransform.anchorMin=Vector2.zero;t.rectTransform.anchorMax=Vector2.one;t.rectTransform.offsetMin=t.rectTransform.offsetMax=Vector2.zero;return b;}
 void Slider(Transform parent,string name,float min,float max,float value,UnityEngine.Events.UnityAction<float> changed){var title=Text(parent,name,20);var r=Rect(name,parent,new Vector2(580,22));var background=r.gameObject.AddComponent<Image>();background.color=new Color(.18f,.2f,.25f);var slider=r.gameObject.AddComponent<Slider>();var fill=Rect("Fill",r,Vector2.zero);fill.anchorMin=Vector2.zero;fill.anchorMax=Vector2.one;fill.offsetMin=fill.offsetMax=Vector2.zero;fill.gameObject.AddComponent<Image>().color=yellow;slider.fillRect=fill;var handle=Rect("Handle",r,new Vector2(16,26));handle.gameObject.AddComponent<Image>().color=Color.white;slider.handleRect=handle;slider.targetGraphic=handle.GetComponent<Image>();slider.minValue=min;slider.maxValue=max;slider.SetValueWithoutNotify(value);void UpdateLabel(float v){title.text=name+"  "+v.ToString("0.00");}UpdateLabel(value);slider.onValueChanged.AddListener(v=>{UpdateLabel(v);changed(v);});}
 void Toggle(Transform parent,string label,bool value,UnityEngine.Events.UnityAction<bool> changed){bool state=value;Button b=null;b=Button(parent,label+": "+(state?"Ligado":"Desligado"),()=>{state=!state;b.GetComponentInChildren<TMP_Text>().text=label+": "+(state?"Ligado":"Desligado");changed(state);});}
 void Build(){var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=800;var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;gameObject.AddComponent<GraphicRaycaster>();
  var overlay=Rect("PauseOverlay",transform,Vector2.zero);overlay.anchorMin=Vector2.zero;overlay.anchorMax=Vector2.one;overlay.offsetMin=overlay.offsetMax=Vector2.zero;overlay.gameObject.AddComponent<Image>().color=new Color(0,0,0,.65f);group=overlay.gameObject.AddComponent<CanvasGroup>();
  panel=Rect("Panel",overlay,new Vector2(680,900));panel.gameObject.AddComponent<Image>().color=dark;
  var mr=Rect("Main",panel,new Vector2(680,420));Stack(mr,18);main=mr.gameObject;Text(mr,"<color=#F0CE38>GRIDSHIFT</color>",42);Text(mr,"Jogo pausado",24);Button(mr,"Continuar",Close);Button(mr,"Configurações",()=>ShowSettings(true));Button(mr,"Sair do jogo",()=>{
#if UNITY_EDITOR
   UnityEditor.EditorApplication.isPlaying=false;
#else
   Application.Quit();
#endif
  });
  var sr=Rect("Settings",panel,new Vector2(680,900));Stack(sr,10);settings=sr.gameObject;Text(sr,"CONFIGURAÇÕES",30);Button(sr,"Voltar",()=>ShowSettings(false));
  var tabs=Rect("Tabs",sr,new Vector2(620,48));var horizontal=tabs.gameObject.AddComponent<HorizontalLayoutGroup>();horizontal.spacing=8;horizontal.childForceExpandWidth=true;horizontal.childControlWidth=true;
  var pages=new GameObject[3];for(int i=0;i<3;i++){var p=Rect("Page"+i,sr,new Vector2(620,560));Stack(p,9);pages[i]=p.gameObject;pages[i].SetActive(i==0);}
  string[] names={"Vídeo","Áudio","Jogabilidade"};for(int i=0;i<3;i++){int index=i;Button(tabs,names[i],()=>{for(int j=0;j<3;j++)pages[j].SetActive(j==index);});}
  Video(pages[0].transform);var audio=pages[1].transform;Text(audio,"ÁUDIO",24);Slider(audio,"Volume geral",0,1,AudioListener.volume,v=>{AudioListener.volume=v;PlayerPrefs.SetFloat("GS.Volume",v);});Text(audio,"A cena não possui canais separados de\nmúsica, ambiente e efeitos configurados.",19);
  var play=pages[2].transform;Text(play,"JOGABILIDADE",24);Slider(play,"Sensibilidade",.2f,3,GamePreferences.MouseSensitivity,v=>GamePreferences.MouseSensitivity=v);Slider(play,"Efeitos de câmera",0,1,GamePreferences.CameraIntensity,v=>GamePreferences.CameraIntensity=v);Toggle(play,"Câmera ao correr",GamePreferences.RunBob,v=>GamePreferences.RunBob=v);Slider(play,"Head bob",0,1,GamePreferences.HeadBob,v=>GamePreferences.HeadBob=v);
  group.alpha=0;group.gameObject.SetActive(false);
 }
 void Video(Transform parent){Text(parent,"VÍDEO",24);var resolutions=Screen.resolutions.Select(r=>new Vector2Int(r.width,r.height)).Distinct().ToList();if(resolutions.Count==0)resolutions.Add(new Vector2Int(Screen.width,Screen.height));int index=resolutions.FindIndex(r=>r.x==Screen.width&&r.y==Screen.height);if(index<0)index=0;Button rb=null;rb=Button(parent,"Resolução: "+resolutions[index].x+" × "+resolutions[index].y,()=>{index=(index+1)%resolutions.Count;var size=resolutions[index];Screen.SetResolution(size.x,size.y,Screen.fullScreen);PlayerPrefs.SetInt("GS.Width",size.x);PlayerPrefs.SetInt("GS.Height",size.y);rb.GetComponentInChildren<TMP_Text>().text="Resolução: "+size.x+" × "+size.y;});
 Toggle(parent,"Tela cheia",Screen.fullScreen,v=>{Screen.fullScreen=v;PlayerPrefs.SetInt("GS.Fullscreen",v?1:0);});
 Button qb=null;qb=Button(parent,"Qualidade: "+QualitySettings.names[QualitySettings.GetQualityLevel()],()=>{int q=(QualitySettings.GetQualityLevel()+1)%QualitySettings.names.Length;QualitySettings.SetQualityLevel(q);PlayerPrefs.SetInt("GS.Quality",q);qb.GetComponentInChildren<TMP_Text>().text="Qualidade: "+QualitySettings.names[q];});
 Toggle(parent,"VSync",QualitySettings.vSyncCount>0,v=>{QualitySettings.vSyncCount=v?1:0;PlayerPrefs.SetInt("GS.VSync",v?1:0);});Slider(parent,"Limite de FPS¹",30,240,Mathf.Clamp(Application.targetFrameRate,30,240),v=>{Application.targetFrameRate=Mathf.RoundToInt(v);PlayerPrefs.SetInt("GS.FPS",Application.targetFrameRate);});Text(parent,"¹ O VSync tem prioridade sobre o limite de FPS.",17);
 }
 void ApplySaved(){AudioListener.volume=PlayerPrefs.GetFloat("GS.Volume",1);if(PlayerPrefs.HasKey("GS.Quality"))QualitySettings.SetQualityLevel(Mathf.Clamp(PlayerPrefs.GetInt("GS.Quality"),0,QualitySettings.names.Length-1));if(PlayerPrefs.HasKey("GS.VSync"))QualitySettings.vSyncCount=PlayerPrefs.GetInt("GS.VSync");Application.targetFrameRate=PlayerPrefs.GetInt("GS.FPS",60);if(PlayerPrefs.HasKey("GS.Width"))Screen.SetResolution(PlayerPrefs.GetInt("GS.Width"),PlayerPrefs.GetInt("GS.Height"),PlayerPrefs.GetInt("GS.Fullscreen",1)!=0);else if(PlayerPrefs.HasKey("GS.Fullscreen"))Screen.fullScreen=PlayerPrefs.GetInt("GS.Fullscreen")!=0;}
}
