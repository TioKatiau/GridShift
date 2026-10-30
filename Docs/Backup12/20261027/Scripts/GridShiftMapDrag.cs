using UnityEngine;
using UnityEngine.EventSystems;

// Pointer capture belongs to the map surface, so buttons and other panels never pan it.
public sealed class GridShiftMapDrag : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public GridShiftOperationsUI owner;
    public void OnBeginDrag(PointerEventData e) { }
    public void OnDrag(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left || owner == null) return;
        var rect = (RectTransform)transform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out var current) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position - e.delta, e.pressEventCamera, out var previous)) return;
        owner.PanMap(current - previous, rect.rect.size);
    }
}
