using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class GridShiftRoadSurface22 {
 public static int Corrected{get;private set;}
 public static void Install(Scene scene){Corrected=0;if(scene.name!="SampleScene")return;Physics.SyncTransforms();foreach(var c in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Collider>(true))){if(!c.enabled||c.isTrigger||c.attachedRigidbody!=null)continue;string name=c.name;bool paint=name.StartsWith("Road Split Line",StringComparison.Ordinal)||name.StartsWith("Crosswalk Stripe",StringComparison.Ordinal)||name.StartsWith("Road Crosswalk Line",StringComparison.Ordinal);if(!paint||c.bounds.size.y>.25f)continue;var b=c.bounds;var hits=Physics.RaycastAll(new Vector3(b.center.x,b.max.y+.1f,b.center.z),Vector3.down,1,~0,QueryTriggerInteraction.Ignore);if(!hits.Any(h=>h.collider!=c&&h.collider.name.StartsWith("Road",StringComparison.Ordinal)&&h.normal.y>.9f&&Mathf.Abs(h.point.y-b.min.y)<.3f))continue;c.enabled=false;Corrected++;}Debug.Log("Road22: "+Corrected+" decorative marking colliders removed; supporting roads retained.");}
}
