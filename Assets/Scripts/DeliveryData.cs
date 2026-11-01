using System;
using System.Collections.Generic;
using UnityEngine;
public enum DeliveryMode{Fleet,Personal}
public enum DeliveryStatus{WaitingVehicle,ToPickup,Loading,Transporting,Unloading,Completed,WaitingRoute}
public enum FleetStatus{Available,ToPickup,Loading,Transporting,Unloading,Completed,WaitingRoute,Unavailable}
public enum FleetKind{Car,Truck}
public enum DeliveryKind{Standard,Heavy,Urgent,Priority,Long,Local}
[Serializable]public class DeliveryOffer{public string id,goods,pickupId,destinationId;public float weightKg;public DeliveryKind kind;}
[Serializable]public class DeliveryOrder {
 public string id,requestId,goods,pickupId,destinationId,vehicleId,createdAt,completedAt;
 public float weightKg,estimatedMeters,travelledMeters,reward;
 public DeliveryStatus status;
 public DeliveryMode mode;public double completedDay;public int courierPhase,courierNode;public Vector3 courierPosition;public bool courierInitialized;
 public bool rewardPaid;public DeliveryKind kind;public float elapsedSeconds,deadlineSeconds;
}
[Serializable]public class DeliveryRequest{public DeliveryMode mode;public string requestId,goods,pickupId,destinationId;public float weightKg;public DeliveryKind kind;}
[Serializable]public class FleetSnapshot{public string id,kind,state,deliveryId;public float capacityKg,speed,maximumSpeed,x,z;}
[Serializable]public class DeliveryPointSnapshot{public string id,name,type;public bool accessible,unlocked;public int requiredExpansion;public float x,z;}
[Serializable]public class DeliverySnapshot {
 public string mode="URBAN FLOW — CENTRAL DE OPERAÇÕES",selectedId;
 public CompanyCampaignState company;public double gameDay;public float money;public int concurrentLimit;public List<UpgradeView> upgrades=new List<UpgradeView>();public List<DeliveryOffer> offers=new List<DeliveryOffer>();
 public List<DeliveryOrder> orders=new List<DeliveryOrder>();public List<FleetSnapshot> fleet=new List<FleetSnapshot>();public List<DeliveryPointSnapshot> points=new List<DeliveryPointSnapshot>();
}


