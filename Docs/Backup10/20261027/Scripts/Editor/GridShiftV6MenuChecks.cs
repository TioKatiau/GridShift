using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
[InitializeOnLoad]public static class GridShiftV6MenuChecks {
 static GridShiftV6MenuChecks(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("V6Menu",false)){SessionState.SetBool("V6Menu",false);var go=new GameObject("Menu6Checks");Object.DontDestroyOnLoad(go);go.AddComponent<GridShiftV6MenuPlayChecks>();}};}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Prepare(){if(SessionState.GetBool("V6Menu",false))DeliverySaveStore.OverridePath=SessionState.GetString("V6MenuSave","");}
 public static void Batch(){SessionState.SetString("V6MenuSave",Path.GetFullPath("v6-newgame-"+Guid.NewGuid().ToString("N")+".json"));EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");SessionState.SetBool("V6Menu",true);EditorApplication.isPlaying=true;}
}
public sealed class GridShiftV6MenuPlayChecks:MonoBehaviour {
 readonly StringBuilder report=new StringBuilder();
 void Check(bool ok,string message){if(!ok)throw new Exception(message);report.AppendLine("PASS "+message);}
 IEnumerator Start(){Application.runInBackground=true;yield return new WaitForSecondsRealtime(4);bool success=true;try{RunMenu();}catch(Exception e){success=false;report.AppendLine("FAIL "+e);}if(success){Object.FindFirstObjectByType<GridShiftMainMenu>().StartGame(false);float end=Time.realtimeSinceStartup+35;while(DeliveryManager.Instance==null&&Time.realtimeSinceStartup<end)yield return null;yield return null;yield return null;try{Check(DeliveryManager.Instance!=null,"Novo Jogo carrega SampleScene");var manager=DeliveryManager.Instance;Check(manager.orders.Count==0&&manager.Money==0,"Novo Jogo começa sem pedidos ou dinheiro anteriores");Check(Mathf.Abs(WorldSession.Instance.ClockTime-720)<10,"Novo Jogo inicia relógio às 9h");var missing=new DeliveryOrder{id=Guid.NewGuid().ToString("N"),weightKg=5,pickupId="removido",destinationId="removido",status=DeliveryStatus.WaitingVehicle};manager.orders.Add(missing);manager.Dispatch();Check(missing.status==DeliveryStatus.WaitingRoute,"local removido de salvamento aguarda rota sem exceção");manager.orders.Clear();Check(manager.GetComponent<DeliveryPersistence>().Save()&&DeliverySaveStore.Read(out var saved)&&saved.orders.Count==0,"sessão nova pode ser salva e lida");File.WriteAllText(DeliverySaveStore.PathName,"{invalido");Check(!DeliverySaveStore.Read(out _),"salvamento corrompido recusado");}catch(Exception e){success=false;report.AppendLine("FAIL "+e);}}
  report.AppendLine("RESULT "+(success?"SUCCESS":"FAILURE"));File.WriteAllText("v6-menu-checks.txt",report.ToString());EditorApplication.Exit(success?0:1);
 }
 void RunMenu(){var menu=Object.FindFirstObjectByType<GridShiftMainMenu>();Check(menu!=null,"intro e menu disponíveis sem arquivo anterior");var buttons=menu.GetComponentsInChildren<Button>(true);Check(!buttons.First(b=>b.name=="Continuar").interactable,"Continuar desabilitado sem salvamento");buttons.First(b=>b.name=="Créditos").onClick.Invoke();var credits=menu.transform.Find("Menu/Credits");Check(credits!=null&&credits.gameObject.activeSelf,"Créditos abre informações reais");credits.GetComponentsInChildren<Button>().First().onClick.Invoke();Check(!credits.gameObject.activeSelf,"Voltar fecha créditos");}
}
