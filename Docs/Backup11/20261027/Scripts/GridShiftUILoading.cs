using UnityEngine;
using UnityEngine.UI;
// A shared presentation layer; progress comes only from the scene operation.
public sealed class GridShiftUILoading:MonoBehaviour {
 Image bar;public void Initialize(){var logo=GridShiftUITheme.Art(transform,"GRIDSHIFT_Logo_Horizontal",new Vector2(550,150),new Vector2(0,210));var track=GridShiftUITheme.Art(transform,"Slider_Trilho_Vazio",new Vector2(600,20),new Vector2(0,-140));track.preserveAspect=false;bar=GridShiftUITheme.Art(track.transform,"Slider_Preenchimento_Amarelo",new Vector2(600,20),Vector2.zero);bar.preserveAspect=false;bar.type=Image.Type.Filled;bar.fillMethod=Image.FillMethod.Horizontal;bar.fillAmount=0;}
 public void Progress(float value){if(bar==null)Initialize();bar.gameObject.SetActive(value>=0);if(value>=0)bar.fillAmount=Mathf.Clamp01(value);}
}
