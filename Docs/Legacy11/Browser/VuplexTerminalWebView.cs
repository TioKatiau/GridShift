#if GRIDSHIFT_VUPLEX
using System;
using UnityEngine;
using Vuplex.WebView;
// Defina GRIDSHIFT_VUPLEX somente depois de importar 3D WebView Windows + Mac.
public class VuplexTerminalWebView:TerminalWebView {
 public CanvasWebViewPrefab browser;
 private int request;
 public override async void Open(string url,Action<string> status){
  int current=++request;
  try{
   browser.gameObject.SetActive(true);await browser.WaitUntilInitialized();if(current!=request)return;
   browser.WebView.LoadUrl(url);var loading=browser.WebView.WaitForNextPageLoadToFinish();
   if(await System.Threading.Tasks.Task.WhenAny(loading,System.Threading.Tasks.Task.Delay(30000))!=loading)throw new TimeoutException("Tempo de conexão esgotado.");
   await loading;if(current==request)status("");
  }catch(Exception e){if(current==request)status("Não foi possível carregar a página.\n"+e.Message+"\nESC para sair.");}
 }
 public override void Hide(){request++;if(browser!=null){if(browser.WebView!=null&&browser.WebView.IsInitialized){browser.WebView.StopLoad();browser.WebView.LoadUrl("about:blank");}browser.gameObject.SetActive(false);}}
}
#endif
