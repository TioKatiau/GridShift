using System;
using UnityEngine;

[Serializable] public sealed class GridShiftCharacterData {
 public string name="Alex",pronouns="",origin="",biography="";public int age=25,model=-1,skin=-1,outfit=-1;
}
public static class GridShiftCharacter {
 public static GridShiftCharacterData Current{get;private set;}=new GridShiftCharacterData();public static int Revision{get;private set;}
 public static string PlayerName=>Current.name;
 public static readonly Color[] SkinColors={new Color(.95f,.78f,.65f),new Color(.82f,.62f,.44f),new Color(.65f,.43f,.28f),new Color(.45f,.28f,.18f),new Color(.28f,.16f,.11f),new Color(.76f,.51f,.36f)};
 public static readonly Color[] OutfitColors={new Color(.08f,.2f,.38f),new Color(.85f,.65f,.09f),new Color(.17f,.39f,.26f),new Color(.55f,.12f,.13f),new Color(.13f,.14f,.17f),new Color(.78f,.8f,.82f),new Color(.42f,.19f,.5f),new Color(.1f,.5f,.55f)};
 public static GridShiftCharacterData Copy(GridShiftCharacterData value)=>JsonUtility.FromJson<GridShiftCharacterData>(JsonUtility.ToJson(value??new GridShiftCharacterData()));
 static string Clean(string s,int max){s=(s??"").Trim().Replace("<","").Replace(">","");return s.Substring(0,Math.Min(max,s.Length));}
 public static GridShiftCharacterData Normalize(GridShiftCharacterData d){var c=Copy(d);c.name=Clean(c.name,40);if(c.name.Length==0)c.name="Alex";c.age=Mathf.Clamp(c.age,1,120);c.pronouns=Clean(c.pronouns,32);c.origin=Clean(c.origin,64);c.biography=Clean(c.biography,600);c.model=Mathf.Clamp(c.model,-1,9);c.skin=Mathf.Clamp(c.skin,-1,SkinColors.Length-1);c.outfit=Mathf.Clamp(c.outfit,-1,OutfitColors.Length-1);return c;}
 public static void Restore(GridShiftCharacterData data){Current=Normalize(data);Revision++;}
 public static void Commit(GridShiftCharacterData data){Restore(data);foreach(var player in UnityEngine.Object.FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include,FindObjectsSortMode.None))player.GetComponent<GridShiftPlayerAppearance>()?.Refresh();DeliveryManager.Instance?.GetComponent<DeliveryPersistence>()?.RequestSave();}
}

