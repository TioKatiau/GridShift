using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
public static class GridShiftProfiles {
 public static string RootOverride;public static string ProfileId{get;private set;}public static int ActiveSlot{get;private set;}public static string CompanyName{get;private set;}="GridShift Logistics";
 public static string Root=>string.IsNullOrEmpty(RootOverride)?Path.Combine(Application.persistentDataPath,"Profiles"):RootOverride;
 public static bool LoggedIn=>!string.IsNullOrEmpty(ProfileId);
 public static bool Login(string email,string password,out string error){error=null;if(!ValidEmail(email)){error="Digite um e-mail válido, como nome@exemplo.com.";return false;}if(email.Length>254||(password!=null&&password.Length>1024)){error="Campos muito longos.";return false;}using(var hash=SHA256.Create())ProfileId=string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant().Normalize())).Select(b=>b.ToString("x2")));ActiveSlot=0;CompanyName="GridShift Logistics";return true;}
 public static bool ValidEmail(string email){email=email?.Trim();if(string.IsNullOrWhiteSpace(email)||email.Length>254||email.Any(char.IsWhiteSpace))return false;try{var address=new System.Net.Mail.MailAddress(email.Trim());return address.Address==email.Trim()&&address.Host.Contains(".")&&!address.Host.StartsWith(".")&&!address.Host.EndsWith(".");}catch{return false;}}
 public static void Logout(){ProfileId=null;ActiveSlot=0;CompanyName="GridShift Logistics";}
 public static string SlotPath(int slot){if(!LoggedIn||slot<1||slot>5)throw new InvalidOperationException("Perfil e slot válidos são necessários.");return Path.Combine(Root,ProfileId,"slot-"+slot.ToString("00")+".json");}
 public static void Activate(int slot,string company){SlotPath(slot);ActiveSlot=slot;CompanyName=string.IsNullOrWhiteSpace(company)?"GridShift Logistics":company.Trim().Substring(0,Math.Min(64,company.Trim().Length));}
 public static DeliverySaveData[] List(){var result=new DeliverySaveData[5];if(!LoggedIn)return result;for(int i=1;i<=5;i++)if(Read(i,out var data,out _))result[i-1]=data;return result;}
 public static bool Read(int slot,out DeliverySaveData data,out bool recovered){data=null;recovered=false;if(!LoggedIn||slot<1||slot>5)return false;bool ok=DeliverySaveStore.ReadAt(SlotPath(slot),out data,out recovered);if(!ok)return false;if(data.version>=3&&(data.profileId!=ProfileId||data.slot!=slot)){data=null;return false;}return true;}
 public static bool Create(int slot,string name,bool overwriteConfirmed,out string error)=>CreateCharacter(slot,name,null,overwriteConfirmed,out error);
 public static bool CreateCharacter(int slot,string name,GridShiftCharacterData character,bool overwriteConfirmed,out string error){error=null;string path=SlotPath(slot);if((File.Exists(path)||File.Exists(path+".bak"))&&!overwriteConfirmed){error="Confirme a substituição desta partida.";return false;}Activate(slot,name);var initial=new DeliverySaveData{saveId=Guid.NewGuid().ToString("N"),character=GridShiftCharacter.Normalize(character),newCompany=true,profileId=ProfileId,slot=slot,companyName=CompanyName,currentScene="SampleScene",savedUtc=DateTime.UtcNow.ToString("O")};if(!DeliverySaveStore.WriteAt(path,initial)){error="Não foi possível criar a partida.";return false;}return true;}
 public static bool Delete(int slot,bool confirmed,out string error){error=null;if(!confirmed){error="Confirme a exclusão.";return false;}string path=SlotPath(slot);try{foreach(var suffix in new[]{"",".bak",".tmp",".png"})if(File.Exists(path+suffix))File.Delete(path+suffix);if(ActiveSlot==slot)ActiveSlot=0;return true;}catch(Exception e){error="Não foi possível excluir este slot: "+e.Message;return false;}}
 public static bool Latest(out int slot,out DeliverySaveData data){slot=0;data=null;var list=List();for(int i=0;i<list.Length;i++)if(list[i]!=null&&(data==null||string.CompareOrdinal(list[i].savedUtc,data.savedUtc)>0)){data=list[i];slot=i+1;}return data!=null;}
 public static bool ImportLegacy(int slot,out string error){error=null;if(File.Exists(SlotPath(slot))){error="Escolha um slot vazio.";return false;}string legacy=Path.Combine(Application.persistentDataPath,"gridshift-session.json");if(!DeliverySaveStore.ReadAt(legacy,out var data,out _)){error="Partida anterior ausente ou inválida.";return false;}Activate(slot,"GridShift Logistics");data.version=5;data.profileId=ProfileId;data.slot=slot;data.companyName=CompanyName;data.savedUtc=DateTime.UtcNow.ToString("O");return DeliverySaveStore.WriteAt(SlotPath(slot),data);}
}

