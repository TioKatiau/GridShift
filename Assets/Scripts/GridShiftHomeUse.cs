using UnityEngine;
public sealed class GridShiftHomeUse:MonoBehaviour {
 public string kind,title;public Vector3 localPoint;
 void Start(){GetComponentInParent<GridShiftApartment>().AddUse(transform,kind,title,localPoint);}
}
