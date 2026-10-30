using UnityEngine;
using UnityEngine.EventSystems;
public sealed class GridShiftNativeMapDrag:MonoBehaviour,IDragHandler {
 public Camera cameraView;
 public void OnDrag(PointerEventData e){if(cameraView==null)return;var r=(RectTransform)transform;if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(r,e.position,e.pressEventCamera,out var to)||!RectTransformUtility.ScreenPointToLocalPointInRectangle(r,e.position-e.delta,e.pressEventCamera,out var from))return;var delta=(to-from)*(cameraView.orthographicSize*2/Mathf.Max(1,r.rect.height));cameraView.transform.position-=cameraView.transform.right*delta.x+cameraView.transform.up*delta.y;}
}
