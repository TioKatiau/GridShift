using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public sealed class GridShiftMerchant:MonoBehaviour {
 public float range=3;PlayerMovement player;TMP_Text prompt,marker;
 void OnEnable(){InteractionFocus.Register(this,range);}void OnDisable(){InteractionFocus.Remove(this);if(prompt!=null)prompt.gameObject.SetActive(false);}
 void Start(){player=Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).FirstOrDefault(p=>p.gameObject.scene==gameObject.scene);var label=new GameObject("MerchantPrompt",typeof(TextMeshPro));label.transform.SetParent(transform,false);label.transform.localPosition=new Vector3(0,2.2f,0);prompt=label.GetComponent<TextMeshPro>();prompt.rectTransform.sizeDelta=new Vector2(4,1);prompt.fontSize=3;prompt.alignment=TextAlignmentOptions.Center;prompt.color=new Color(1,.83f,.2f);prompt.text="CARLOS\n["+GridShiftControls.KeyName(GSAction.Interact)+"] Conversar";prompt.gameObject.SetActive(false);var sign=new GameObject("MissionMarker",typeof(TextMeshPro));sign.layer=11;sign.transform.SetParent(transform,false);sign.transform.localPosition=new Vector3(0,2.8f,0);marker=sign.GetComponent<TextMeshPro>();marker.rectTransform.sizeDelta=new Vector2(3,1);marker.fontSize=4;marker.alignment=TextAlignmentOptions.Center;marker.color=new Color(1,.82f,.12f);marker.text="! CARLOS";}
 void Update(){bool near=player!=null&&!player.ControlsBlocked&&SceneManager.GetActiveScene()==gameObject.scene&&Time.timeScale>0&&(WorldSession.Instance==null||!WorldSession.Instance.Busy)&&InteractionFocus.Selected(this,player.transform);if(marker!=null&&player!=null){marker.gameObject.SetActive(DeliveryManager.Instance.Company.Missions.First.status!=CompanyMissionStatus.Completed&&Vector3.Distance(player.transform.position,transform.position)<100);marker.transform.rotation=player.CameraTransform.rotation;}if(prompt!=null){prompt.gameObject.SetActive(near);if(near)prompt.transform.rotation=player.CameraTransform.rotation;}if(near&&GridShiftControls.Down(GSAction.Interact)&&InteractionFocus.Consume()){GridShiftControls.SuppressFrame();GridShiftCampaignUI.Instance.Merchant();}}
}



