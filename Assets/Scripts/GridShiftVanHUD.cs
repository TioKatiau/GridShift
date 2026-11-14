using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class GridShiftVanHUD : MonoBehaviour {
 public GridShiftPlayerVan van;TMP_Text speed,details,delivery;CanvasGroup group;GameObject personalPanel;Image condition,fuel;
 void Start(){var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=20;var scale=gameObject.AddComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1920,1080);scale.matchWidthOrHeight=.5f;group=gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;
  var card=GridShiftUI.Rect("Vehicle instruments",transform,new Vector2(300,210));card.anchorMin=card.anchorMax=new Vector2(1,0);card.pivot=new Vector2(1,0);card.anchoredPosition=new Vector2(-530,28);GridShiftContextHUD.Surface(card.gameObject.AddComponent<Image>());
  speed=Text(card,44,new Vector2(0,56),new Vector2(270,65));details=Text(card,18,new Vector2(0,-8),new Vector2(270,70));
  fuel=Bar(card,-60,new Color(1,.8f,.15f));condition=Bar(card,-83,new Color(.4f,.85f,.65f));
  var mission=GridShiftUI.Rect("Personal delivery",transform,new Vector2(410,100));mission.anchorMin=mission.anchorMax=new Vector2(0,1);mission.pivot=new Vector2(0,1);mission.anchoredPosition=new Vector2(24,-110);GridShiftContextHUD.Surface(mission.gameObject.AddComponent<Image>());personalPanel=mission.gameObject;delivery=GridShiftUI.Text(mission,"",19,90);GridShiftUI.Stretch(delivery.rectTransform,new Vector2(14,10),new Vector2(-14,-10));
 }
 static TMP_Text Text(Transform p,int size,Vector2 at,Vector2 bounds){var t=GridShiftUI.Text(p,"",size,bounds.y);t.rectTransform.sizeDelta=bounds;t.rectTransform.anchoredPosition=at;t.alignment=TextAlignmentOptions.Center;return t;}
 static Image Bar(Transform p,float y,Color color){var r=GridShiftUI.Rect("Gauge",p,new Vector2(260,10));r.anchoredPosition=new Vector2(0,y);r.gameObject.AddComponent<Image>().color=new Color(.2f,.22f,.25f);var fill=GridShiftUI.Rect("Fill",r,Vector2.zero);GridShiftUI.Stretch(fill,Vector2.zero,Vector2.zero);var im=fill.gameObject.AddComponent<Image>();im.color=color;im.raycastTarget=false;return im;}
 void Update(){if(van==null)return;personalPanel.SetActive(!GridShiftTutorial.Active);group.alpha=van.Driving&&!GridShiftOperationsUI.ModalOpen&&!GridShiftCampaignUI.ModalOpen?1:0;if(!van.Driving)return;var h=van.Health;speed.text=(van.Speed*3.6f).ToString("0")+" <size=22>km/h</size>";
  var forward=Vector3.Dot(van.GetComponent<Rigidbody>().linearVelocity,van.transform.forward);details.text=(forward<-.2f?"R":van.Speed<.2f?"N":"D")+"  ·  "+(van.ParkingBrake?"FREIO":"EM MOVIMENTO")+"\nComb. "+h.fuel.ToString("0")+"% · Cond. "+h.condition.ToString("0")+"%\n"+(h.condition<30?"<color=#FFB04D>MANUTENÇÃO NECESSÁRIA</color>":string.IsNullOrEmpty(van.CargoOrderId)?"Carga vazia · "+van.capacityKg+" kg":"Carga: 1 encomenda · "+van.UsedCapacityKg.ToString("0.#")+" kg");
  var road=van.GetComponent<GridShiftRoadRules>();if(road!=null)details.text+="\nL: "+(road.LightMode==0?"Apagado":road.LightMode==1?"Farol baixo":"Farol alto")+" · Z/X: setas · V: alerta";fuel.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(h.fuel/100),1);condition.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(h.condition/100),1);
  var o=van.Manager.PersonalOrder;delivery.text=o==null?"GRIDSHIFT · ENTREGAS PESSOAIS\nNenhuma entrega ativa":o.goods+"\n"+van.Manager.Point(o.destinationId)?.displayName+"\n"+(o.status==DeliveryStatus.ToPickup?"Preparar e coletar":string.IsNullOrEmpty(van.CargoOrderId)?"Levar à porta do destinatário":"Estacionar e retirar a encomenda")+" · "+Mathf.Max(0,o.deadlineSeconds-o.elapsedSeconds).ToString("0")+" s";
 }
}
