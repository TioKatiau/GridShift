using System.Collections;
using System.Linq;
using UnityEngine;
public sealed class DeliveryPersistence:MonoBehaviour {
 DeliveryManager manager;float saveAt;bool initialized;
 IEnumerator Start(){yield return null;manager=GetComponent<DeliveryManager>();if(manager==null)yield break;if(DeliverySaveStore.StartFresh){DeliverySaveStore.StartFresh=false;if(WorldSession.Instance!=null)WorldSession.Instance.RestoreClock(720);}var data=DeliverySaveStore.Pending;DeliverySaveStore.Pending=null;if(data!=null){foreach(var order in data.orders)if(order.status!=DeliveryStatus.Completed){order.status=DeliveryStatus.WaitingVehicle;order.vehicleId=null;}manager.RestoreEconomy(data.money,data.orders,data.selected);manager.Progression.Restore(data.upgrades);var player=Object.FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(p=>p.gameObject.scene.name=="SampleScene");if(player!=null&&data.playerPosition.sqrMagnitude>1){var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.SetPositionAndRotation(data.playerPosition,data.playerRotation);cc.enabled=true;}if(WorldSession.Instance!=null)WorldSession.Instance.RestoreClock(data.clock);manager.Notify("Sessão restaurada · pedidos pendentes retornaram à fila.");}initialized=true;saveAt=Time.unscaledTime+10;}
 void Update(){if(initialized&&Time.unscaledTime>=saveAt){saveAt=Time.unscaledTime+10;Save();}}
 public bool Save(){if(manager==null)return false;var player=Object.FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(p=>p.gameObject.scene.name=="SampleScene");var data=new DeliverySaveData{money=manager.Money,upgrades=manager.Progression.levels,clock=WorldSession.Instance!=null?WorldSession.Instance.ClockTime:0,selected=manager.SelectedId,orders=manager.orders,playerPosition=player!=null?player.transform.position:Vector3.zero,playerRotation=player!=null?player.transform.rotation:Quaternion.identity};return DeliverySaveStore.Write(data);}
 void OnApplicationQuit(){if(initialized)Save();}
}

