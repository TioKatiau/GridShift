using System.Linq;
using UnityEngine;
public sealed class GridShiftHomeTV:MonoBehaviour {
 Material screenMaterial;Texture priorTexture;Color priorColor;
 public void AuthorScene(){var screen=GetComponentsInChildren<Renderer>().FirstOrDefault(r=>r.name=="Screen");if(screen==null)return;var a=GridShiftWorldAction.Create(transform,"Ligar / desligar TV",transform.InverseTransformPoint(screen.bounds.center),null);GridShiftApartment.NormalizeFocus(a.transform);a.sightRoot=transform;a.focusOwner=screen.transform;a.focusSize=new Vector3(.7f,.45f,.2f);a.focusOffset=Vector3.zero;a.range=2.7f;a.sceneAuthored=true;}
 void Start(){var screen=GetComponentsInChildren<Renderer>().FirstOrDefault(r=>r.name=="Screen");if(screen==null)return;screenMaterial=screen.material;priorTexture=screenMaterial.mainTexture;priorColor=screenMaterial.color;screenMaterial.EnableKeyword("_EMISSION");var action=GridShiftWorldAction.Create(transform,"Ligar / desligar TV",transform.InverseTransformPoint(screen.bounds.center),p=>{SetOn(!(GridShiftLife.Instance?.State.tvOn??false));return true;});GridShiftApartment.NormalizeFocus(action.transform);action.sightRoot=transform;action.focusOwner=screen.transform;action.focusSize=new Vector3(.7f,.45f,.2f);action.focusOffset=Vector3.zero;action.range=2.7f;Refresh();}
 public void SetOn(bool on){if(GridShiftLife.Instance==null)return;GridShiftLife.Instance.State.tvOn=on;foreach(var tv in Object.FindObjectsByType<GridShiftHomeTV>(FindObjectsSortMode.None))tv.Refresh();GridShiftLife.Instance.Changed();}
 public void Refresh(){if(screenMaterial==null)return;bool on=GridShiftLife.Instance?.State.tvOn==true;var logo=GridShiftUITheme.Get("GS_Simbolo_Transparente");screenMaterial.mainTexture=on&&logo!=null?logo.texture:priorTexture;screenMaterial.color=on?Color.white:priorColor;screenMaterial.SetColor("_EmissionColor",on?new Color(.25f,.5f,.7f):Color.black);}
 void OnDestroy(){if(screenMaterial!=null)Destroy(screenMaterial);}
}
