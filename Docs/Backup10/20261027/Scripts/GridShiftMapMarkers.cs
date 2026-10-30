using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public sealed class GridShiftMapGlyph:MaskableGraphic,IPointerClickHandler {
 public string orderId;
 public void OnPointerClick(PointerEventData e){if(e.button==PointerEventData.InputButton.Left&&!string.IsNullOrEmpty(orderId))DeliveryManager.Instance?.Select(orderId);}

 public int kind;protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;if(kind==0){Tri(vh,new Vector2(0,r.yMax),new Vector2(r.xMax,r.yMin),new Vector2(0,r.yMin*.55f));Tri(vh,new Vector2(0,r.yMax),new Vector2(0,r.yMin*.55f),new Vector2(r.xMin,r.yMin));}else if(kind==1){Quad(vh,r.xMin,r.yMin*.7f,r.xMax,r.yMax*.7f);Quad(vh,r.xMin*.7f,r.yMin,r.xMin*.25f,r.yMin*.65f);Quad(vh,r.xMax*.25f,r.yMin,r.xMax*.7f,r.yMin*.65f);Quad(vh,r.xMin*.7f,r.yMax*.65f,r.xMin*.25f,r.yMax);Quad(vh,r.xMax*.25f,r.yMax*.65f,r.xMax*.7f,r.yMax);}else{Tri(vh,new Vector2(0,r.yMax),new Vector2(r.xMax,0),new Vector2(0,r.yMin));Tri(vh,new Vector2(0,r.yMax),new Vector2(0,r.yMin),new Vector2(r.xMin,0));}}
 void Tri(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c){int n=vh.currentVertCount;vh.AddVert(a,color,Vector2.zero);vh.AddVert(b,color,Vector2.zero);vh.AddVert(c,color,Vector2.zero);vh.AddTriangle(n,n+1,n+2);}void Quad(VertexHelper vh,float x,float y,float xx,float yy){Tri(vh,new Vector2(x,y),new Vector2(xx,yy),new Vector2(xx,y));Tri(vh,new Vector2(x,y),new Vector2(x,yy),new Vector2(xx,yy));}
}
public sealed class GridShiftMapMarkers:MonoBehaviour {
 public RawImage image;public Camera mapCamera;public DeliveryManager manager;public Transform player;public bool expanded;
 readonly HashSet<GridShiftMapGlyph> placed=new HashSet<GridShiftMapGlyph>();
 readonly Dictionary<string,GridShiftMapGlyph> icons=new Dictionary<string,GridShiftMapGlyph>();
 GridShiftMapGlyph Icon(string id,int kind,Color color){if(icons.TryGetValue(id,out var g))return g;var go=new GameObject(id,typeof(RectTransform),typeof(CanvasRenderer),typeof(GridShiftMapGlyph));go.transform.SetParent(image.transform,false);g=go.GetComponent<GridShiftMapGlyph>();g.rectTransform.anchorMin=g.rectTransform.anchorMax=Vector2.zero;g.rectTransform.pivot=Vector2.one*.5f;g.kind=kind;g.color=color;g.raycastTarget=expanded;icons[id]=g;return g;}
 void Place(GridShiftMapGlyph g,Vector3 world,float relativeSize,float rotation=0){var v=mapCamera.WorldToViewportPoint(world);bool visible=v.z>0&&v.x>=.02f&&v.x<=.98f&&v.y>=.02f&&v.y<=.98f;if(!expanded)visible&=(new Vector2(v.x-.5f,v.y-.5f)).sqrMagnitude<.23f;if(g.gameObject.activeSelf!=visible)g.gameObject.SetActive(visible);if(!visible)return;placed.Add(g);var r=image.rectTransform.rect;g.rectTransform.anchorMin=g.rectTransform.anchorMax=Vector2.zero;g.rectTransform.pivot=Vector2.one*.5f;g.rectTransform.anchoredPosition=new Vector2(v.x*r.width,v.y*r.height);g.rectTransform.sizeDelta=Vector2.one*(expanded?Mathf.Clamp(Mathf.Min(r.width,r.height)*relativeSize,10,28):Mathf.Min(r.width,r.height)*relativeSize);g.rectTransform.localRotation=Quaternion.Euler(0,0,rotation);}
 float Heading(Vector3 position,Vector3 direction){var a=mapCamera.WorldToViewportPoint(position);var b=mapCamera.WorldToViewportPoint(position+direction);return -Mathf.Atan2(b.x-a.x,b.y-a.y)*Mathf.Rad2Deg;}
 void LateUpdate(){if(image==null||mapCamera==null)return;placed.Clear();if(player!=null)Place(Icon("PlayerMapGlyph",0,new Color(.95f,.3f,.25f)),player.position,.052f,Heading(player.position,player.forward));if(manager==null){HideUnused();return;}foreach(var v in manager.fleet)if(v!=null){var glyph=Icon("Vehicle_"+v.id,1,new Color(1,.85f,.25f));glyph.orderId=v.Order?.id;Place(glyph,v.transform.position,manager.Selected==v.Order ? .055f : .044f,0);}var selected=manager.Selected;if(selected!=null&&selected.status!=DeliveryStatus.Completed){var p=manager.Point(selected.destinationId);if(p!=null)Place(Icon("DestinationMapGlyph",2,new Color(.38f,.75f,.8f)),p.transform.position,.045f);}if(expanded)foreach(var p in manager.points)if(p!=null&&p.Valid&&manager.Progression.Unlocked(p))Place(Icon(p.id,2,new Color(.5f,.58f,.69f,.7f)),p.transform.position,.012f);
  if(expanded)foreach(var order in manager.orders)if(order.status!=DeliveryStatus.Completed&&order!=selected){var point=manager.Point(order.destinationId);if(point!=null){var glyph=Icon("Order_"+order.id,2,new Color(.38f,.75f,.8f));glyph.orderId=order.id;Place(glyph,point.transform.position,.027f);}}
  foreach(var id in new[]{"DestinationMapGlyph","PlayerMapGlyph"})if(icons.TryGetValue(id,out var top))top.transform.SetAsLastSibling();
  HideUnused();}
 void HideUnused(){foreach(var g in icons.Values)if(g.gameObject.activeSelf&&!placed.Contains(g))g.gameObject.SetActive(false);}
}
