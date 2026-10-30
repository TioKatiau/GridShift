using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;
[Serializable]public class DeliveryBridgeCommand{public string op,id,requestId,goods,pickupId,destinationId,orderId,offerId,upgradeId,user;public float weightKg;public DeliveryKind kind;}
public sealed class GridShiftDeliveryBridge:MonoBehaviour {
 GridShiftUwbTerminal owner;string currentUrl;float updateAt;string lastSession,lastKeys;
 public void Initialize(GridShiftUwbTerminal terminal){owner=terminal;owner.browser.browserClient.RegisterJsMethod<string,string>("GridShiftDelivery",Command);owner.browser.browserClient.OnUrlChanged+=UrlChanged;owner.browser.browserClient.OnLoadFinish+=UrlChanged;}
 void UrlChanged(string url){currentUrl=url;updateAt=0;lastSession=lastKeys=null;}
 public void OpenDemo(){var manager=DeliveryManager.Instance;if(manager==null||!owner.inputRouter.Ready)return;string url=manager.DemoUrl;if(url!=null)owner.browser.browserClient.LoadUrl(url);}
 bool Allowed(string token=null){var manager=DeliveryManager.Instance;return manager!=null&&manager.LocalServer!=null&&currentUrl==manager.LocalServer.Url&&(token==null||token==manager.LocalServer.Token);}
 [Preserve]void Command(string token,string json){if(!Allowed(token)||json==null||json.Length>4096)return;try{var command=JsonUtility.FromJson<DeliveryBridgeCommand>(json);if(command==null)return;var manager=DeliveryManager.Instance;string error=null;if(command.op=="login"){if(!manager.DemoLogin(command.user))error="Informe um nome de até 80 caracteres.";}else if(command.op=="logout"){manager.DemoLogout();}else if(!manager.DemoAuthorized){error="Entre no modo local antes de gerenciar pedidos.";}else if(command.op=="tutorial"){GridShiftOperationsUI.Instance?.RequestTutorial();}else if(command.op=="buy"){manager.Progression.Buy(command.upgradeId,out error);}else if(command.op=="accept"){manager.Accept(command.offerId,out error);}else if(command.op=="create"){manager.TryCreate(new DeliveryRequest{requestId=command.requestId,goods=command.goods,weightKg=command.weightKg,kind=command.kind,pickupId=command.pickupId,destinationId=command.destinationId},out _,out error);}else if(command.op=="view-tab"){if(command.id=="operations")manager.Company.Missions.VisitOperations();}else if(command.op=="save"){if(!manager.GetComponent<DeliveryPersistence>().Save())error="Não foi possível salvar a sessão.";}else if(new[]{"pay-bill","defer-bill","hire","dismiss","refuel","repair","read-mission","start-side","start-mission","confirm-mission","select-mission","recovery"}.Contains(command.op)){manager.Company.Command(command.op,command.id,out error);}else if(command.op=="select")manager.Select(command.orderId);else return;string result=JsonUtility.ToJson(new BridgeReply{ok=error==null,message=error??"Pedido recebido pela simulação local."});owner.browser.browserClient.ExecuteJs("window.gridshiftAck&&window.gridshiftAck("+result+")");updateAt=0;}catch(Exception){owner.browser.browserClient.ExecuteJs("window.gridshiftAck&&window.gridshiftAck({ok:false,message:'Pedido inválido.'})");}}
 [Serializable]class KeysInfo{public string summary;public string[] steps;}
 [Serializable]class DemoIdentity{public string user;}
 [Serializable]class BridgeReply{public bool ok;public string message;}
 void Update(){if(owner==null||!owner.inputRouter.Ready||!owner.inputRouter.Focused||!Allowed()||Time.unscaledTime<updateAt)return;updateAt=Time.unscaledTime+1;var manager=DeliveryManager.Instance;string session=JsonUtility.ToJson(new DemoIdentity{user=manager.DemoUser});
 if(session!=lastSession){owner.browser.browserClient.ExecuteJs("window.gridshiftSession&&window.gridshiftSession("+session+")");lastSession=session;}
 string keys=JsonUtility.ToJson(new KeysInfo{summary=string.Join(" · ",new[]{GSAction.Tablet,GSAction.Map,GSAction.Controls,GSAction.Tutorial,GSAction.Upgrades}.Select(a=>GridShiftControls.KeyName(a)+": "+GridShiftControls.Label(a))),steps=GridShiftTutorial.Instructions()});
 if(keys!=lastKeys){owner.browser.browserClient.ExecuteJs("window.gridshiftKeys&&window.gridshiftKeys("+keys+")");lastKeys=keys;}
 if(!manager.DemoAuthorized)return;var snapshot=JsonUtility.ToJson(manager.Snapshot());owner.browser.browserClient.ExecuteJs("window.gridshiftState&&window.gridshiftState("+snapshot+")");}
 void OnDestroy(){if(owner!=null&&owner.browser!=null){owner.browser.browserClient.OnUrlChanged-=UrlChanged;owner.browser.browserClient.OnLoadFinish-=UrlChanged;}}
}


