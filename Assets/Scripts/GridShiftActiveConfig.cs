using UnityEngine;

[CreateAssetMenu(menuName="GridShift/Active operations 16")]
public sealed class GridShiftActiveConfig:ScriptableObject {
 public GameObject vanPrefab;
 public Material floorMaterial,structureMaterial,accentMaterial;
 public Vector3 depotPosition,vanPosition;
 public Quaternion depotRotation=Quaternion.identity,vanRotation=Quaternion.identity;
 public string factoryPointId;public string depotPointId="GS-DEPOT16";
}

[System.Serializable] public sealed class GridShiftVanState {
 public bool cargoOpen;public bool placed,hasSafePosition;public Vector3 position,safeExit,safePosition;public Quaternion rotation=Quaternion.identity,safeRotation=Quaternion.identity;public string cargoOrderId;
}
