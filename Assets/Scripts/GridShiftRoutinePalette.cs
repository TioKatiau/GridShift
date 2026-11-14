using System.Linq;using UnityEngine;
public sealed class GridShiftRoutinePalette:MonoBehaviour {
 public Renderer[] skin=System.Array.Empty<Renderer>(),outfit=System.Array.Empty<Renderer>();
 public void AuthorScene(){var all=GetComponentsInChildren<Renderer>(true);outfit=all.Where(r=>r.name.IndexOf("sleeve",System.StringComparison.OrdinalIgnoreCase)>=0).ToArray();skin=all.Where(r=>!outfit.Contains(r)).ToArray();}
 public void Apply(GridShiftCharacterData character){var block=new MaterialPropertyBlock();void Paint(Renderer[] renderers,int choice,Color[] colors){if(choice<0)return;foreach(var renderer in renderers){if(renderer==null)continue;block.Clear();block.SetColor("_BaseColor",colors[Mathf.Clamp(choice,0,colors.Length-1)]);renderer.SetPropertyBlock(block);}}Paint(skin,character.skin,GridShiftCharacter.SkinColors);Paint(outfit,character.outfit,GridShiftCharacter.OutfitColors);}
}
