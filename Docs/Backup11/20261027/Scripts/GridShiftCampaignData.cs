using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public sealed class CompanyTransaction { public string id,category,description; public double day; public float amount; }
[Serializable] public sealed class CompanyBill { public string id,description,category;public int week;public double dueDay;public float amount,lateFee;public bool paid,penalized,deferred; }
[Serializable] public sealed class CompanyWeek {public int week;public float revenue,expenses,profit,balance,invoiced,deliveries,contracts,otherIncome,salaries,fuel,rent,maintenance,taxes,services,upgrades;}
[Serializable] public sealed class CompanyEmployee {public string id,name,role,benefit;public float salary,payrollAccrued;public int experience=1;public bool hired;}
[Serializable] public sealed class CompanyVehicleHealth {public string id;public float fuel=100,condition=100,meters;public double maintenanceUntil;}
public enum CompanyMissionStatus {Locked,Available,Active,Completed,Failed}
[Serializable] public sealed class CompanyMission {public string id,name,description,type,location,requirement,consequence,orderId;public string[] objectives;public int step;public float reward;public bool rewardClaimed;public CompanyMissionStatus status;}
[Serializable] public sealed class CompanyCampaignState {
 public int version=1,reputation=25,closedWeeks,act=1;public bool introSeen,complete;public string merchantPoint,deliveryPoint,selectedMission="first-contract";public double lastFinanceDay,nextRecoveryDay,nextEventDay=1;public int lastEventDay=-1;
 public List<CompanyTransaction> ledger=new List<CompanyTransaction>();public List<CompanyBill> bills=new List<CompanyBill>();public List<CompanyWeek> weeks=new List<CompanyWeek>();public List<CompanyEmployee> employees=new List<CompanyEmployee>();public List<CompanyVehicleHealth> vehicles=new List<CompanyVehicleHealth>();public List<CompanyMission> missions=new List<CompanyMission>();
 public DeliveryOffer eventOffer;public double eventExpiry;public string news="A VetorLog domina os contratos da cidade. Uma oportunidade pode mudar o início da GridShift.";
}



