using UnityEngine;
[CreateAssetMenu(menuName="GridShift/Campanha e equilíbrio 10.0")]
public sealed class GridShiftCampaignBalance:ScriptableObject {
 [Min(0)]public float startingCash=180,officeRent=24,electricity=5,services=6,fictionalTaxRate=.02f,lateFeeRate=.03f,firstMissionBonus=45;
 [Min(1)]public int billGraceDays=3;[Min(0)]public float carFuelPerKm=1.5f,truckFuelPerKm=3,wearPerKm=.8f,carRefuel=12,truckRefuel=24,repairPerPoint=.3f;[Min(1)]public float maintenanceGameHours=4;
 public GameObject merchantPrefab;public string merchantPoint,deliveryPoint;public Vector3 merchantPosition;public Quaternion merchantRotation=Quaternion.identity;
}


