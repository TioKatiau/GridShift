using System;
using TMPro;
using UnityEngine;

// Every new physical station participates in the existing focus and E arbitration.
public sealed class GridShiftWorldAction:MonoBehaviour {
 public string title;public Func<PlayerMovement,bool> action;public Func<bool> available;
 public float range=2.6f;PlayerMovement player;TMP_Text label;
 void OnEnable(){InteractionFocus.Register(this,range);}
 void OnDisable(){InteractionFocus.Remove(this);}
 void Start(){var go=new GameObject("ActionPrompt",typeof(TextMeshPro));go.transform.SetParent(transform,false);go.transform.localPosition=Vector3.up;label=go.GetComponent<TextMeshPro>();label.fontSize=2.4f;label.rectTransform.sizeDelta=new Vector2(4.5f,1);label.alignment=TextAlignmentOptions.Center;label.color=new Color(1,.83f,.2f);}
 void Update(){
  player=InteractionFocus.PlayerFor(this);
  bool selected=player!=null&&(available==null||available())&&GridShiftDeliveryAccess.InReach(player,transform.position,range)&&InteractionFocus.Selected(this,player.transform);
  if(label!=null){label.gameObject.SetActive(selected);if(selected){label.text="["+GridShiftControls.KeyName(GSAction.Interact)+"] "+title;label.transform.rotation=player.CameraTransform.rotation;}}
  if(selected&&GridShiftControls.Down(GSAction.Interact)&&InteractionFocus.Consume()){action?.Invoke(player);GridShiftControls.SuppressFrame();}
 }
 public static GridShiftWorldAction Create(Transform parent,string title,Vector3 local,Func<PlayerMovement,bool> action){var go=new GameObject(title);go.transform.SetParent(parent,false);go.transform.localPosition=local;var component=go.AddComponent<GridShiftWorldAction>();component.title=title;component.action=action;return component;}
}
