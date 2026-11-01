using UnityEngine;
public sealed class DeliveryPoint:MonoBehaviour {
 public string id,displayName,establishmentType="Comércio";
 public Transform pedestrianAccess;
 public bool personalOnly;
 public Transform vehicleStop;
 public Transform establishment;
 public TrafficRoute route;
 public int segment=0;
 [Tooltip("A calçada foi validada, mas a porta ainda precisa de confirmação manual.")] public bool accessNeedsReview;
 public bool accessible;[Range(0,3)]public int requiredExpansion;
 public Vector3 Stop=>vehicleStop!=null?vehicleStop.position:transform.position;
 public bool Valid=>accessible&&route!=null&&route.Valida&&segment>=0&&segment<route.pontos.Length-1&&vehicleStop!=null;
}
