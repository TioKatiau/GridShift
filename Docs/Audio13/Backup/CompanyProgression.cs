using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[Serializable]public class UpgradeLevel{public string id;public int level;}
[Serializable]public class UpgradeView {public string id,name,description,category,requirement;public int level,maxLevel;public float cost;public bool canBuy;}
public sealed class CompanyProgression:MonoBehaviour {
 public List<UpgradeLevel> levels=new List<UpgradeLevel>();DeliveryManager manager;
 public int Level(string id)=>Mathf.Clamp(levels.FirstOrDefault(l=>l.id==id)?.level??0,0,3);
 void Awake(){manager=GetComponent<DeliveryManager>();}
 public int FleetCars=>2+Level("fleet");public int Concurrent=>3+Level("dispatch")*2;
 public float Capacity=>1+.25f*Level("capacity");public float Handling=>1/(1+.3f*Level("handling"));public float Profit=>1+.15f*Level("profit");
 public bool Unlocked(DeliveryPoint point)=>point!=null&&point.requiredExpansion<=Level("district");
 public List<UpgradeView> Catalog(){string[] ids={"fleet","capacity","handling","dispatch","profit","district"},names={"Ampliar frota","Capacidade reforçada","Doca eficiente","Central de despacho","Contratos melhores","Rede de parceiros"},cats={"Frota","Frota","Logística","Operação","Operação","Expansão"},descs={"Adota mais um carro existente do trânsito por nível, sem gerar veículos extras.","+25% de capacidade de carga por nível nos veículos da empresa.","Reduz os tempos de carregamento e descarga da frota.","Mais duas entregas simultâneas por nível.","+15% de recompensa por nível nos novos pedidos.","Libera novos pontos físicos já conectados às ruas atuais."};float[] costs={150,120,100,140,180,130};var result=new List<UpgradeView>();for(int i=0;i<ids.Length;i++){int level=Level(ids[i]);int maximum=ids[i]=="district"?Mathf.Clamp(manager.points.Where(p=>p!=null).Select(p=>p.requiredExpansion).DefaultIfEmpty(0).Max(),0,3):3;string requirement=ids[i]=="profit"&&Level("handling")==0?"Requer Doca eficiente nível 1":"";float cost=costs[i]*(1+level*1.5f);result.Add(new UpgradeView{id=ids[i],name=names[i],category=cats[i],description=descs[i],requirement=requirement,level=level,maxLevel=maximum,cost=cost,canBuy=level<maximum&&string.IsNullOrEmpty(requirement)&&manager.Money>=cost});}return result;}
 public bool Buy(string id,out string error){var item=Catalog().FirstOrDefault(u=>u.id==id);error=null;if(item==null){error="Upgrade inexistente.";return false;}if(item.level>=item.maxLevel){error="Nível máximo atingido.";return false;}if(!string.IsNullOrEmpty(item.requirement)){error=item.requirement;return false;}if(!manager.TrySpend(item.cost)){error="Saldo insuficiente. Conclua entregas para investir.";return false;}var saved=levels.FirstOrDefault(l=>l.id==id);if(saved==null){saved=new UpgradeLevel{id=id};levels.Add(saved);}saved.level++;Apply();manager.Notify("Upgrade comprado · "+item.name+" nível "+saved.level);manager.GetComponent<DeliveryPersistence>()?.RequestSave();return true;}
 public void Apply(){foreach(var v in manager.fleet)if(v!=null)v.capacityKg=(v.kind==FleetKind.Truck?manager.truckCapacityKg:manager.carCapacityKg)*Capacity;manager.Changed();}
 public void Restore(List<UpgradeLevel> saved){levels=new List<UpgradeLevel>();if(saved!=null)foreach(var s in saved.Where(s=>s!=null&&s.id!=null).GroupBy(s=>s.id).Select(g=>g.First()))levels.Add(new UpgradeLevel{id=s.id,level=Mathf.Clamp(s.level,0,3)});Apply();}
}

