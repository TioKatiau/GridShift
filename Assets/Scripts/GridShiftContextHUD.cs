using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One fixed-size presentation surface for physical interactions; objects own no canvases.
public sealed class GridShiftContextHUD : MonoBehaviour {
 static Sprite background;
 public static void Surface(Image image){if(background==null){var texture=new Texture2D(32,32,TextureFormat.RGBA32,false);texture.name="GridShift rounded panel";for(int y=0;y<32;y++)for(int x=0;x<32;x++){float dx=Mathf.Max(7-x,Mathf.Max(0,x-24)),dy=Mathf.Max(7-y,Mathf.Max(0,y-24));texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(7.5f-Mathf.Sqrt(dx*dx+dy*dy))));}texture.Apply();background=Sprite.Create(texture,new Rect(0,0,32,32),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(8,8,8,8));}image.sprite=background;image.type=Image.Type.Sliced;image.color=new Color(.035f,.045f,.06f,.92f);image.raycastTarget=false;}
 static GridShiftContextHUD instance; CanvasGroup group; TMP_Text label; int offered=-1;
 public static void Offer(string text){
  if(instance==null){var root=new GameObject("GridShift Context HUD");instance=root.AddComponent<GridShiftContextHUD>();}
  instance.GetComponent<Canvas>().enabled=true;instance.offered=Time.frameCount;instance.label.text="<color=#FFD23F>["+GridShiftControls.KeyName(GSAction.Interact)+"]</color> "+text;
 }
 public static bool Visible=>instance!=null&&instance.group!=null&&instance.group.alpha>.5f&&instance.GetComponent<Canvas>().isActiveAndEnabled;
 void Awake(){
  DontDestroyOnLoad(gameObject);var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=35;
  var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
  var panel=GridShiftUI.Rect("Context",transform,new Vector2(520,68));panel.anchorMin=panel.anchorMax=new Vector2(.5f,.23f);panel.anchoredPosition=Vector2.zero;
  Surface(panel.gameObject.AddComponent<Image>());
  group=panel.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;group.alpha=0;
  label=GridShiftUI.Text(panel,"",22,60);GridShiftUI.Stretch(label.rectTransform,new Vector2(16,6),new Vector2(-16,-6));label.alignment=TextAlignmentOptions.Center;
 }
 void LateUpdate(){bool show=!GridShiftDepotAttendant.ModalOpen&&offered==Time.frameCount&&Time.timeScale>0&&!GridShiftOperationsUI.ModalOpen&&!GridShiftCampaignUI.ModalOpen;group.alpha=GridShiftDepotAttendant.ModalOpen||GridShiftOperationsUI.ModalOpen||GridShiftCampaignUI.ModalOpen||Time.timeScale<=0?0:Mathf.MoveTowards(group.alpha,show?1:0,Time.unscaledDeltaTime*12);}
 void OnDestroy(){if(instance==this)instance=null;}
}
