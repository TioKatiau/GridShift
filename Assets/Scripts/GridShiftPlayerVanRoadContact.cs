using UnityEngine;

public sealed partial class GridShiftPlayerVan {
 // Four rounded wheel contacts, with chassis clearance, replace the long low runners.
 // The old 12 cm effective radius could wedge on the triangulated crossing seams.
 Vector3[] roadSupports;float roadBottom;
 void BuildRoadContact(){roadBottom=shape.center.y-shape.size.y*.5f;var center=shape.center;var size=shape.size;shape.size=new Vector3(size.x,size.y-.55f,size.z);shape.center=center+Vector3.up*.275f;roadSupports=new Vector3[4];int i=0;foreach(float side in new[]{-1f,1f})foreach(float end in new[]{-1f,1f}){var go=new GameObject("Rounded wheel road contact");go.layer=gameObject.layer;go.transform.SetParent(transform,false);var contact=go.AddComponent<SphereCollider>();contact.radius=.32f;contact.center=new Vector3(center.x+side*size.x*.32f,roadBottom+.32f,center.z+end*size.z*.35f);contact.material=friction;roadSupports[i++]=contact.center-Vector3.up*.32f;}}
 // Suppress solver bounce only while all four wheels remain above the same flat road.
 // Real curbs, ramps and airborne motion are not projected or teleported onto the ground.
 void StabilizeRoadContact(){if(roadSupports==null||body.linearVelocity.y<=.05f)return;float low=float.PositiveInfinity,high=float.NegativeInfinity;foreach(var local in roadSupports){var at=transform.TransformPoint(local);if(!Physics.Raycast(at+Vector3.up*.28f,Vector3.down,out var hit,.45f,~((1<<6)|(1<<7)|(1<<8)|(1<<9)),QueryTriggerInteraction.Ignore)||hit.normal.y<.97f||hit.collider.attachedRigidbody!=null)return;low=Mathf.Min(low,hit.point.y);high=Mathf.Max(high,hit.point.y);}if(high-low>.045f)return;var v=body.linearVelocity;v.y=Mathf.Min(v.y,.05f);body.linearVelocity=v;}
}
