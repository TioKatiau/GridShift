using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using VoltstroStudios.UnityWebBrowser;
using VoltstroStudios.UnityWebBrowser.Communication;
using Resolution=VoltstroStudios.UnityWebBrowser.Shared.Resolution;
[DefaultExecutionOrder(-900)]
public class GridShiftUwbTerminal:TerminalWebView {
 public WebBrowserUIBasic browser;
 public GridShiftUwbInputRouter inputRouter;
 public GridShiftUwbInput inputTemplate;
 public ComputerInteraction terminal;
 public CanvasGroup surface;
 [Range(100,250)] public float pageZoom=200;
 public string homeUrl="https://urbanlink-flow.onrender.com";
 public GridShiftBrowserNavigation navigation;
 public CanvasGroup Surface=>surface!=null?surface:terminal!=null?terminal.screenContent:null;
 [Header("Resolução real do Chromium")]
 public Vector2Int resolution=new Vector2Int(1600,900);
 [Tooltip("Adapta a altura à tela física do CRT, sem esticar ou cortar o site.")]
 public bool matchMonitorAspect=true;
 [SerializeField] Vector2Int actualResolution;
 Vector2Int requestedResolution;
 public Vector2Int ActualResolution=>actualResolution;
 [Range(15,60)] public int textureRefreshRate=30; float textureAccumulator;
 GridShiftBrowserLoading brandLoading;Coroutine loading;GridShiftUwbInput runtimeInput;CommunicationLayer runtimeCommunication;bool opened,loaded;
 public Vector2Int TargetResolution(){float aspect=16f/9;if(matchMonitorAspect&&Surface!=null){var r=Surface.GetComponent<RectTransform>().rect;if(r.height>0)aspect=r.width/Mathf.Max(1,r.height-32);}int width=Mathf.Clamp(resolution.x,800,2560),height=matchMonitorAspect?Mathf.RoundToInt(width/Mathf.Max(.5f,aspect)):Mathf.Clamp(resolution.y,600,2160);return new Vector2Int(width,Mathf.Clamp(height,600,2160));}
 void Awake(){if(terminal==null)terminal=GetComponent<ComputerInteraction>();if(browser==null)return;
  runtimeInput=inputTemplate!=null?Instantiate(inputTemplate):ScriptableObject.CreateInstance<GridShiftUwbInput>();browser.inputHandler=runtimeInput;browser.disableMouseInputs=true;browser.disableKeyboardInputs=true;
  if(inputRouter==null)inputRouter=browser.GetComponent<GridShiftUwbInputRouter>();if(inputRouter==null)inputRouter=browser.gameObject.AddComponent<GridShiftUwbInputRouter>();inputRouter.browser=browser;inputRouter.input=runtimeInput;if(terminal!=null)inputRouter.eventCamera=terminal.playerCamera;
  navigation=GetComponent<GridShiftBrowserNavigation>();if(navigation==null)navigation=gameObject.AddComponent<GridShiftBrowserNavigation>();navigation.Initialize(this);brandLoading=gameObject.AddComponent<GridShiftBrowserLoading>();brandLoading.Initialize(this);
  if(browser.browserClient.communicationLayer!=null){runtimeCommunication=Instantiate(browser.browserClient.communicationLayer);browser.browserClient.communicationLayer=runtimeCommunication;if(runtimeCommunication is TCPCommunicationLayer tcp){tcp.inPort=FreePort();do{tcp.outPort=FreePort();}while(tcp.inPort==tcp.outPort);}}
  Hide();
 }
 static int FreePort(){var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();int port=((IPEndPoint)listener.LocalEndpoint).Port;listener.Stop();return port;}
 public override void Open(string url,Action<string> status){if(browser==null){status?.Invoke("Navegador não configurado.");return;}if(loading!=null)StopCoroutine(loading);homeUrl=url;opened=true;navigation.Show(Surface);if(!loaded)brandLoading.Show(Surface);browser.gameObject.SetActive(true);browser.enabled=true;browser.GetComponent<RawImage>().enabled=false;if(Surface!=null){Surface.interactable=true;Surface.blocksRaycasts=true;}loading=StartCoroutine(LoadWebsite(url,status));}
 IEnumerator LoadWebsite(string url,Action<string> status){float started=Time.realtimeSinceStartup;status?.Invoke("Conectando…");while(!inputRouter.Ready&&Time.realtimeSinceStartup-started<30)yield return null;if(!opened)yield break;if(!inputRouter.Ready){status?.Invoke("Chromium não iniciou. Verifique o log do UWB.");loading=null;yield break;}
  try{ApplyResolution();ApplyZoom();browser.GetComponent<RawImage>().enabled=true;browser.browserClient.AudioMute(false);browser.browserClient.ExecuteJs("document.querySelectorAll('video[data-gridshift-resume],audio[data-gridshift-resume]').forEach(e=>{delete e.dataset.gridshiftResume;e.play().catch(()=>{});})");if(!loaded){if(browser.browserClient.initialUrl!=url)browser.browserClient.LoadUrl(url);loaded=true;}browser.enabled=false;textureAccumulator=1;inputRouter.SetFocus(true);status?.Invoke("");}catch(Exception e){status?.Invoke("Erro ao acessar o site.\n"+e.Message);}loading=null;
 }
 public void ApplyResolution(){if(browser==null||!inputRouter.Ready)return;var target=TargetResolution();requestedResolution=target;var current=browser.browserClient.Resolution;if(current.Width!=target.x||current.Height!=target.y)browser.browserClient.Resolution=new Resolution((uint)target.x,(uint)target.y);actualResolution=new Vector2Int(browser.browserClient.BrowserTexture.width,browser.browserClient.BrowserTexture.height);var image=browser.GetComponent<RawImage>();image.texture=browser.browserClient.BrowserTexture;image.texture.filterMode=FilterMode.Bilinear;image.texture.anisoLevel=1;var fit=browser.GetComponent<AspectRatioFitter>();if(fit==null)fit=browser.gameObject.AddComponent<AspectRatioFitter>();fit.aspectRatio=actualResolution.x/(float)actualResolution.y;fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;}
 float appliedZoom;
 double ZoomLevel=>Math.Log(Mathf.Clamp(pageZoom,100,250)/100d)/Math.Log(1.2d);
 public void ApplyZoom(){if(inputRouter==null||!inputRouter.Ready)return;browser.browserClient.SetZoomLevel(ZoomLevel);appliedZoom=pageZoom;}
 void LateUpdate(){if(opened&&inputRouter!=null&&inputRouter.Ready){float interval=1f/Mathf.Clamp(textureRefreshRate,15,60);textureAccumulator+=Time.unscaledDeltaTime;if(textureAccumulator>=interval){browser.browserClient.LoadTextureData();browser.browserClient.UpdateFps();textureAccumulator%=interval;}if(TargetResolution()!=requestedResolution)ApplyResolution();if(appliedZoom!=pageZoom)ApplyZoom();}}
 public override void Hide(){opened=false;if(brandLoading!=null)brandLoading.Hide();if(loading!=null){StopCoroutine(loading);loading=null;}if(navigation!=null)navigation.Hide();if(inputRouter!=null)inputRouter.SetFocus(false);if(browser!=null){if(inputRouter!=null&&inputRouter.Ready){browser.browserClient.AudioMute(true);browser.browserClient.ExecuteJs("document.querySelectorAll('video,audio').forEach(e=>{if(!e.paused){e.dataset.gridshiftResume='1';e.pause();}})");}browser.StopAllCoroutines();var image=browser.GetComponent<RawImage>();if(image!=null){image.raycastTarget=false;image.enabled=false;}browser.enabled=false;}if(Surface!=null){Surface.interactable=false;Surface.blocksRaycasts=false;}}
 void OnDisable(){Hide();}
 void OnDestroy(){if(runtimeInput!=null){runtimeInput.SetActive(false);Destroy(runtimeInput);}if(runtimeCommunication!=null)Destroy(runtimeCommunication);}
}

