using UnityEngine;

// Optional explicit green-space zones. No broad name-based world classification.
public sealed class GridShiftAudioZone : MonoBehaviour {
 public float radius=18;
 public static readonly System.Collections.Generic.List<GridShiftAudioZone> Active=new System.Collections.Generic.List<GridShiftAudioZone>();
 void OnEnable(){Active.Add(this);}void OnDisable(){Active.Remove(this);}
 void OnDrawGizmosSelected(){Gizmos.color=Color.green;Gizmos.DrawWireSphere(transform.position,radius);}
}
