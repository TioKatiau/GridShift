using UnityEngine;
using UnityEngine.UI;

public static class GridShiftDialogueStyle {
 public static void Surface(Image image){image.sprite=null;image.color=new Color(.025f,.045f,.07f,.97f);image.raycastTarget=true;}
 public static void Brand(Transform panel){if(panel.Find("Dialogue brand")!=null)return;var r=GridShiftUI.Rect("Dialogue brand",panel,new Vector2(62,62));r.anchorMin=r.anchorMax=new Vector2(0,1);r.anchoredPosition=new Vector2(34,-34);GridShiftUITheme.Art(r,"GS_Simbolo_Transparente",new Vector2(50,50),Vector2.zero);}
 public static void BrandRow(Transform panel){var row=GridShiftUI.Rect("Dialogue brand",panel,new Vector2(550,64));var le=row.gameObject.AddComponent<LayoutElement>();le.preferredHeight=64;GridShiftUITheme.Art(row,"GS_Simbolo_Transparente",new Vector2(58,58),new Vector2(-235,0));var label=GridShiftUI.Text(row,"GRIDSHIFT  <color=#FFD23F>· PESSOAS E CONEXÕES</color>",18,50);label.rectTransform.sizeDelta=new Vector2(450,45);label.rectTransform.anchoredPosition=new Vector2(38,0);}
}
