using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
[InitializeOnLoad]public static class GridShiftV18DelegationChecks{
 static GridShiftV18DelegationChecks(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("V18Delegate",false))new GameObject("V18 delegation regression").AddComponent<GridShiftV18DelegationRunner>();};}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Prepare(){if(!SessionState.GetBool("V18Delegate",false))return;DeliverySaveStore.OverridePath=Path.GetFullPath("Temp/V18Delegate-"+Guid.NewGuid().ToString("N")+"/slot.json");DeliverySaveStore.StartFresh=true;GridShiftCompany.SuppressIntro=true;GridShiftTutorial.Prefix="GS.V18.Delegation.";}
 public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");SessionState.SetBool("V18Delegate",true);EditorApplication.isPlaying=true;}
}
public sealed class GridShiftV18DelegationRunner:MonoBehaviour{
 StringBuilder report=new StringBuilder();bool done;float deadline;
 void Check(bool ok,string text){report.AppendLine((ok?"PASS ":"FAIL ")+text);File.WriteAllText("Docs/Validacao18/delegation.txt",report.ToString());if(!ok){Finish(false);throw new Exception(text);}}
 void Finish(bool ok){if(done)return;done=true;report.AppendLine("RESULT "+(ok?"SUCCESS":"FAILURE"));File.WriteAllText("Docs/Validacao18/delegation.txt",report.ToString());SessionState.SetBool("V18Delegate",false);EditorApplication.Exit(ok?0:1);}
 void Update(){if(!done&&deadline>0&&Time.realtimeSinceStartup>deadline){Check(false,"tempo limite");}}
 static void Place(PlayerMovement p,Vector3 at){p.enabled=false;var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=at+Vector3.up*.12f;Physics.SyncTransforms();cc.enabled=true;p.BlockControls(false);}
 IEnumerator Start(){Directory.CreateDirectory("Docs/Validacao18");deadline=Time.realtimeSinceStartup+70;while(DeliveryManager.Instance==null||!DeliveryManager.Instance.GetComponent<DeliveryPersistence>().Ready||DeliveryManager.Instance.GetComponent<GridShiftActiveOperations>().Van==null)yield return null;GridShiftTutorial.Skip();GridShiftOperationsUI.Instance.Close();var m=DeliveryManager.Instance;var ops=m.GetComponent<GridShiftActiveOperations>();var p=InteractionFocus.PlayerFor(m);yield return WorldSession.Instance.RequestTransition("FactoryInterior",p.transform);yield return new WaitForSeconds(.2f);p=InteractionFocus.PlayerFor(m);Place(p,ops.StationPosition("accept"));Check(ops.Execute(p,"accept",null,out _),"pedido padrão real criado após pular guia");var order=m.PersonalOrder;Check(m.Company.State.missions.Any(x=>x.id=="factory-first"&&x.orderId==order.id),"primeira missão também pode ser feita sem tutorial");Place(p,ops.Origin(order)+Vector3.right*1.2f);Check(m.PersonalInteract(p,order.pickupId,out _),"caixa coletada antes de delegar");Check(m.Company.Employee("elisa",true,out _),"contratação real do operador usa financeiro existente");Place(p,ops.StationPosition("delegate"));float before=m.Money;Check(ops.Execute(p,"delegate",order.id,out _)&&Mathf.Abs(m.Money-(before-3))<.001f,"delegação cobra serviço uma vez");float paid=m.Money;Check(!ops.Execute(p,"delegate",order.id,out _)&&m.Money==paid,"pedido em preparação não cobra delegação repetida");yield return new WaitForSeconds(8.3f);Check(order.depotReleased&&order.depotPicked&&order.status==DeliveryStatus.Transporting,"operador libera caixa já coletada para transporte sem bloquear a coleta");Check(m.GetComponent<DeliveryPersistence>().Save()&&DeliverySaveStore.Read(out var save)&&save.orders.Any(x=>x.id==order.id&&x.depotReleased&&x.status==DeliveryStatus.Transporting),"save registra delegação e caixa pronta");Finish(true);}
}
