using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
[Serializable]public class DeliverySaveData{public int version=1;public List<UpgradeLevel> upgrades=new List<UpgradeLevel>();public float money,clock;public string selected;public Vector3 playerPosition;public Quaternion playerRotation=Quaternion.identity;public List<DeliveryOrder> orders=new List<DeliveryOrder>();}
public static class DeliverySaveStore {
 public static string OverridePath;
 public static string PathName=>string.IsNullOrEmpty(OverridePath)?Path.Combine(Application.persistentDataPath,"gridshift-session.json"):OverridePath;
 public static DeliverySaveData Pending;
 public static bool StartFresh;
 public static bool Read(out DeliverySaveData data){data=null;try{if(!File.Exists(PathName))return false;data=JsonUtility.FromJson<DeliverySaveData>(File.ReadAllText(PathName));if(data==null||data.version!=1||data.orders==null||data.orders.Count>500||!Finite(data.clock)||!Finite(data.playerPosition.x)||!Finite(data.playerPosition.y)||!Finite(data.playerPosition.z)||!Finite(data.playerRotation.x)||!Finite(data.playerRotation.y)||!Finite(data.playerRotation.z)||!Finite(data.playerRotation.w)||float.IsNaN(data.money)||float.IsInfinity(data.money)||data.money<0)return false;var ids=new HashSet<string>();foreach(var order in data.orders)if(order==null||!Enum.IsDefined(typeof(DeliveryStatus),order.status)||!Enum.IsDefined(typeof(DeliveryKind),order.kind)||!Finite(order.elapsedSeconds)||order.elapsedSeconds<0||!Guid.TryParseExact(order.id,"N",out _)||!ids.Add(order.id)||order.weightKg<=0||float.IsNaN(order.weightKg)||float.IsInfinity(order.weightKg)||order.reward<0||float.IsNaN(order.reward)||float.IsInfinity(order.reward))return false;return true;}catch{return false;}}
 static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
 public static bool Write(DeliverySaveData data){try{string path=PathName;Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(data,true));if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);return true;}catch(Exception e){Debug.LogWarning("Não foi possível salvar a sessão: "+e.Message);return false;}}
}
