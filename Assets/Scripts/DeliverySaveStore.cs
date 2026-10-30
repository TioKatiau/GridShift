using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
[Serializable]public class DeliverySaveData{public int version=3;public bool newCompany;public TutorialSaveData tutorial;public string profileId,companyName="GridShift Logistics",savedUtc,currentScene="SampleScene",trackedVehicleId;public int slot;public double playSeconds;public Vector3 activePlayerPosition;public Quaternion activePlayerRotation=Quaternion.identity;public List<FleetSaveData> vehicles=new List<FleetSaveData>();public SimulationSaveData simulation=new SimulationSaveData();public CompanyCampaignState company;public double calendarDays;public List<UpgradeLevel> upgrades=new List<UpgradeLevel>();public float money,clock;public string selected;public Vector3 playerPosition;public Quaternion playerRotation=Quaternion.identity;public List<DeliveryOrder> orders=new List<DeliveryOrder>();}
public static class DeliverySaveStore {
 public static string OverridePath;
 public static string PathName=>string.IsNullOrEmpty(OverridePath)?(GridShiftProfiles.LoggedIn&&GridShiftProfiles.ActiveSlot>0?GridShiftProfiles.SlotPath(GridShiftProfiles.ActiveSlot):Path.Combine(Application.persistentDataPath,"gridshift-session.json")):OverridePath;
 public static DeliverySaveData Pending;
 public static bool StartFresh;
 public static bool Read(out DeliverySaveData data)=>ReadAt(PathName,out data,out _);
 public static bool ReadAt(string path,out DeliverySaveData data,out bool recovered){recovered=false;if(Parse(path,out data))return true;if(Parse(path+".bak",out data)){recovered=true;return true;}return false;}
 static bool Parse(string path,out DeliverySaveData data){data=null;try{if(!File.Exists(path)||new FileInfo(path).Length>16*1024*1024)return false;data=JsonUtility.FromJson<DeliverySaveData>(File.ReadAllText(path));if(data==null||(data.version<1||data.version>3)||data.orders==null||data.orders.Count>500||!Finite(data.clock)||!Finite(data.playerPosition.x)||!Finite(data.playerPosition.y)||!Finite(data.playerPosition.z)||!Finite(data.playerRotation.x)||!Finite(data.playerRotation.y)||!Finite(data.playerRotation.z)||!Finite(data.playerRotation.w)||float.IsNaN(data.money)||float.IsInfinity(data.money)||data.money<0)return false;var ids=new HashSet<string>();foreach(var order in data.orders)if(order==null||!Enum.IsDefined(typeof(DeliveryStatus),order.status)||!Enum.IsDefined(typeof(DeliveryKind),order.kind)||!Finite(order.elapsedSeconds)||order.elapsedSeconds<0||!Guid.TryParseExact(order.id,"N",out _)||!ids.Add(order.id)||order.weightKg<=0||float.IsNaN(order.weightKg)||float.IsInfinity(order.weightKg)||order.reward<0||float.IsNaN(order.reward)||float.IsInfinity(order.reward))return false;if(data.version>=2&&(!GridShiftCampaignValidation.Valid(data.company)||double.IsNaN(data.calendarDays)||double.IsInfinity(data.calendarDays)||data.calendarDays<0))return false;if(data.version>=3&&(!GridShiftSaveValidation.Valid(data)))return false;if(data.version<3){data.currentScene="SampleScene";data.companyName="GridShift Logistics";data.vehicles=new List<FleetSaveData>();data.simulation=new SimulationSaveData();data.activePlayerPosition=data.playerPosition;data.activePlayerRotation=data.playerRotation;}return true;}catch{return false;}}
 static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
 public static bool Write(DeliverySaveData data)=>WriteAt(PathName,data);
 public static bool WriteAt(string path,DeliverySaveData data){try{Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(data,true));if(File.Exists(path))File.Replace(temp,path,Parse(path,out _)?path+".bak":null);else File.Move(temp,path);return true;}catch(Exception e){Debug.LogWarning("Não foi possível salvar a sessão: "+e.Message);return false;}}
}





