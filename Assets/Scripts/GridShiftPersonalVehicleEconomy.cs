using UnityEngine;
public sealed partial class GridShiftCompany {
 public void PersonalVehicleDistance(float meters){if(!Ready||meters<=0||meters>30)return;var health=Health(GridShiftPlayerVan.VehicleId);health.meters+=meters;health.fuel=Mathf.Max(0,health.fuel-meters/1000*Balance.carFuelPerKm);health.condition=Mathf.Max(0,health.condition-meters/1000*Balance.wearPerKm);}
}
