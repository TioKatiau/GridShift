using System.Linq;
using UnityEngine;

// Restore precisely the presentation state that existed before this conversation.
public sealed class GridShiftDialogueVisibility {
 Renderer[] renderers;bool[] states;GameObject hud;bool active;
 public GridShiftDialogueVisibility(PlayerMovement player){if(player==null)return;renderers=player.GetComponentsInChildren<Renderer>(true);states=renderers.Select(r=>r.enabled).ToArray();foreach(var r in renderers)r.enabled=false;hud=player.CameraTransform.Find("Hud")?.gameObject;if(hud!=null){active=hud.activeSelf;hud.SetActive(false);}}
 public void Restore(){if(renderers!=null)for(int i=0;i<renderers.Length;i++)if(renderers[i]!=null)renderers[i].enabled=states[i];if(hud!=null)hud.SetActive(active);renderers=null;hud=null;}
}
