using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(CanvasRenderer))]public sealed class GridShiftMapRouteGraphic:MaskableGraphic {
 public Camera mapCamera;public DeliveryManager manager;public RectTransform mapRect;
 readonly List<Vector3> points=new List<Vector3>();float next;Color lineColor;
 protected override void Awake(){base.Awake();raycastTarget=false;}
 void LateUpdate(){if(Time.unscaledTime<next)return;next=Time.unscaledTime+.15f;SetVerticesDirty();}
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(manager==null||mapCamera==null||mapRect==null||!GridShiftTrackedRoute.BuildForMap(manager,points))return;var settings=manager.GetComponent<GridShiftRouteView>();lineColor=settings!=null?settings.RouteColor:new Color(.3f,.85f,1,.75f);float width=settings!=null?settings.mapWidth:2.6f;for(int i=1;i<points.Count;i++)Line(vh,points[i-1],points[i],width);}
 void Line(VertexHelper vh,Vector3 a,Vector3 b,float width){var p=mapCamera.WorldToViewportPoint(a);var q=mapCamera.WorldToViewportPoint(b);if(p.z<=0||q.z<=0)return;Vector2 from=p,to=q;float enter=0,leave=1;var delta=to-from;if(!Clip(-delta.x,from.x,ref enter,ref leave)||!Clip(delta.x,1-from.x,ref enter,ref leave)||!Clip(-delta.y,from.y,ref enter,ref leave)||!Clip(delta.y,1-from.y,ref enter,ref leave))return;to=from+delta*leave;from+=delta*enter;var r=mapRect.rect;from=new Vector2((from.x-.5f)*r.width,(from.y-.5f)*r.height);to=new Vector2((to.x-.5f)*r.width,(to.y-.5f)*r.height);var d=to-from;if(d.sqrMagnitude<.1f)return;var side=new Vector2(-d.y,d.x).normalized*width*.5f;int n=vh.currentVertCount;vh.AddVert(from-side,lineColor,Vector2.zero);vh.AddVert(from+side,lineColor,Vector2.zero);vh.AddVert(to+side,lineColor,Vector2.zero);vh.AddVert(to-side,lineColor,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
 static bool Clip(float p,float q,ref float enter,ref float leave){if(Mathf.Abs(p)<.00001f)return q>=0;float t=q/p;if(p<0){if(t>leave)return false;enter=Mathf.Max(enter,t);}else{if(t<enter)return false;leave=Mathf.Min(leave,t);}return true;}
}
