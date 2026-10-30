using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting;
using TMPro;

// Commands go to Chromium. Availability is read from its actual Navigation API,
// including pushState/popstate; no parallel URL stack is maintained by Unity.
public sealed class GridShiftBrowserNavigation:MonoBehaviour {
 GridShiftUwbTerminal owner;RectTransform bar,viewport;Button back,forward;float poll,zoomUntil;bool visible;GridShiftDeliveryBridge deliveries;
 public bool CanGoBack{get;private set;}public bool CanGoForward{get;private set;}
 public void Initialize(GridShiftUwbTerminal terminal){owner=terminal;var client=owner.browser.browserClient;client.jsMethodManager.jsMethodsEnable=true;client.RegisterJsMethod<bool,bool>("GridShiftHistory",History);client.RegisterJsMethod<bool>("GridShiftTyping",Typing);client.RegisterJsMethod<string>("GridShiftScale",ScaleReported);client.OnLoadFinish+=Loaded;client.OnUrlChanged+=Loading;client.OnLoadStart+=Loading;deliveries=gameObject.AddComponent<GridShiftDeliveryBridge>();deliveries.Initialize(owner);}
 public void ZoomIn(){owner.pageZoom=Mathf.Min(250,owner.pageZoom+25);owner.ApplyZoom();}
 public void ZoomOut(){owner.pageZoom=Mathf.Max(100,owner.pageZoom-25);owner.ApplyZoom();}
 [Preserve] void Typing(bool editing){if(owner.inputRouter!=null)owner.inputRouter.TextEditing=editing;}
 [Preserve] void ScaleReported(string text){if(double.TryParse(text,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out var value)&&System.Math.Abs(value-owner.pageZoom/100d)<.02)zoomUntil=0;}
 void Loading(string url){zoomUntil=Time.unscaledTime+8;}
 [Preserve] void History(bool previous,bool next){CanGoBack=previous;CanGoForward=next;if(back!=null)back.interactable=previous;if(forward!=null)forward.interactable=next;}
 void Loaded(string url){if(owner.inputRouter.Ready){owner.ApplyZoom();ReadHistory();}}
 public void Back(){if(owner.inputRouter.Ready&&CanGoBack)owner.browser.browserClient.ExecuteJs("history.back()");}
 public void Forward(){if(owner.inputRouter.Ready&&CanGoForward)owner.browser.browserClient.ExecuteJs("history.forward()");}
 public void Reload(){if(owner.inputRouter.Ready)owner.browser.browserClient.ExecuteJs("location.reload()");}
 public void Home(){if(owner.inputRouter.Ready)owner.browser.browserClient.LoadUrl(owner.homeUrl);}
 public void ReadHistory(){if(!owner.inputRouter.Ready)return;owner.browser.browserClient.ExecuteJs("if(window.uwb&&window.uwb.ExecuteJsMethod){uwb.ExecuteJsMethod('GridShiftHistory',!!(window.navigation&&navigation.canGoBack),!!(window.navigation&&navigation.canGoForward));uwb.ExecuteJsMethod('GridShiftScale',String(devicePixelRatio));uwb.ExecuteJsMethod('GridShiftTyping',!!(document.activeElement&&(document.activeElement.isContentEditable||/^(INPUT|TEXTAREA|SELECT)$/.test(document.activeElement.tagName))));}");}
 void Update(){if(visible&&owner.inputRouter.Ready&&Time.unscaledTime>=poll){poll=Time.unscaledTime+.4f;ReadHistory();if(Time.unscaledTime<zoomUntil)owner.ApplyZoom();}}
 public void Show(CanvasGroup surface){if(surface==null)return;visible=true;zoomUntil=Time.unscaledTime+8;if(bar==null)Build();bar.SetParent(surface.transform,false);bar.anchorMin=new Vector2(0,1);bar.anchorMax=Vector2.one;bar.pivot=new Vector2(.5f,1);bar.sizeDelta=new Vector2(0,32);bar.anchoredPosition=Vector2.zero;bar.localScale=Vector3.one;bar.localRotation=Quaternion.identity;bar.gameObject.SetActive(true);
  viewport=owner.browser.transform.parent as RectTransform;viewport.anchorMin=Vector2.zero;viewport.anchorMax=Vector2.one;viewport.offsetMin=Vector2.zero;viewport.offsetMax=new Vector2(0,-32);History(CanGoBack,CanGoForward);
 }
 public void Hide(){visible=false;if(bar!=null)bar.gameObject.SetActive(false);}
 void Build(){bar=new GameObject("BrowserNavigation",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image)).GetComponent<RectTransform>();bar.GetComponent<Image>().color=new Color(.035f,.055f,.085f,.98f);var row=bar.gameObject.AddComponent<HorizontalLayoutGroup>();row.padding=new RectOffset(5,5,3,3);row.spacing=4;row.childControlWidth=false;row.childControlHeight=true;row.childForceExpandWidth=false;back=Button("<",Back);forward=Button(">",Forward);Button("↻",Reload);Button("Home",Home);Button("−",ZoomOut);Button("+",ZoomIn);var demo=Button("DEMO",()=>deliveries.OpenDemo());demo.GetComponent<RectTransform>().sizeDelta=new Vector2(56,26);var text=new GameObject("Brand",typeof(RectTransform),typeof(TextMeshProUGUI));text.transform.SetParent(bar,false);var label=text.GetComponent<TextMeshProUGUI>();label.text="UrbanLink Flow";label.fontSize=8;label.color=new Color(.65f,.75f,.9f);label.alignment=TextAlignmentOptions.MidlineRight;label.raycastTarget=false;text.AddComponent<LayoutElement>().flexibleWidth=1;}
 Button Button(string label,UnityEngine.Events.UnityAction action){var go=new GameObject(label,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(bar,false);go.GetComponent<RectTransform>().sizeDelta=new Vector2(label=="Home"?47:26,26);go.GetComponent<Image>().color=new Color(.14f,.19f,.26f);var button=go.GetComponent<Button>();var colors=button.colors;colors.highlightedColor=new Color(.45f,.65f,1);colors.disabledColor=new Color(.22f,.25f,.3f,.45f);button.colors=colors;button.onClick.AddListener(()=>{action();UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(owner.browser.gameObject);});var t=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI));t.transform.SetParent(go.transform,false);var rect=t.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;var text=t.GetComponent<TextMeshProUGUI>();text.text=label=="↻"?"":label;if(label=="↻"){var icon=new GameObject("RefreshIcon",typeof(RectTransform),typeof(CanvasRenderer),typeof(GridShiftRefreshIcon));icon.transform.SetParent(go.transform,false);icon.GetComponent<RectTransform>().sizeDelta=new Vector2(16,16);icon.GetComponent<GridShiftRefreshIcon>().raycastTarget=false;}text.fontSize=15;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;return button;}
 void OnDestroy(){if(owner!=null&&owner.browser!=null){owner.browser.browserClient.OnLoadFinish-=Loaded;owner.browser.browserClient.OnUrlChanged-=Loading;owner.browser.browserClient.OnLoadStart-=Loading;}if(bar!=null)Destroy(bar.gameObject);}
}




