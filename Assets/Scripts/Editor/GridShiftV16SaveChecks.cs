using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
public static class GridShiftV16SaveChecks {
 public static void Batch(){var report=new StringBuilder();bool ok=true;void Check(bool condition,string label){report.AppendLine((condition?"PASS ":"FAIL ")+label);ok&=condition;}
  string root=Path.GetFullPath("Temp/V16Compatibility-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);string id=Guid.NewGuid().ToString("N");string legacy=Path.Combine(root,"v4.json");
  File.WriteAllText(legacy,"{\"version\":4,\"slot\":1,\"money\":42,\"clock\":720,\"calendarDays\":0.5,\"currentScene\":\"SampleScene\",\"activePlayerRotation\":{\"w\":1},\"playerRotation\":{\"w\":1},\"vehicles\":[],\"orders\":[{\"id\":\""+id+"\",\"goods\":\"Encomenda antiga\",\"weightKg\":8,\"reward\":25,\"status\":3,\"mode\":1,\"pickupId\":\"old-pickup\",\"destinationId\":\"old-destination\"}]}");
  Check(DeliverySaveStore.ReadAt(legacy,out var data,out _)&&data.orders[0].mode==DeliveryMode.Personal&&data.orders[0].status==DeliveryStatus.Transporting&&data.personalVan==null,"save v4 sem campos novos mantém encomenda pessoal carregada");
  if(data!=null){data.version=5;data.personalVan=new GridShiftVanState{placed=true,hasSafePosition=true,position=new Vector3(2,0,2),safePosition=new Vector3(2,0,2),safeExit=new Vector3(4,0,2),cargoOrderId=id};string valid=Path.Combine(root,"valid-v5.json");Check(DeliverySaveStore.WriteAt(valid,data)&&DeliverySaveStore.ReadAt(valid,out _,out _),"save v5 aceita carga vinculada a pedido ativo");data.personalVan.cargoOrderId="unknown-order";string bad=Path.Combine(root,"unknown-cargo.json");DeliverySaveStore.WriteAt(bad,data);Check(!DeliverySaveStore.ReadAt(bad,out _,out _),"save rejeita carga da van sem pedido correspondente");data.personalVan=null;data.orders[0].depotRequired=true;data.orders[0].depotLabelled=true;string stages=Path.Combine(root,"invalid-stages.json");DeliverySaveStore.WriteAt(stages,data);Check(!DeliverySaveStore.ReadAt(stages,out _,out _),"save rejeita etiqueta pronta sem conferência e embalagem");data.orders[0].depotLabelled=false;data.version=6;string future=Path.Combine(root,"future.json");DeliverySaveStore.WriteAt(future,data);Check(!DeliverySaveStore.ReadAt(future,out _,out _),"versão futura v6 não é interpretada como save atual");}
  report.AppendLine("RESULT "+(ok?"SUCCESS":"FAILURE"));Directory.CreateDirectory("Docs/Validacao16");File.WriteAllText("Docs/Validacao16/storage.txt",report.ToString());EditorApplication.Exit(ok?0:1);
 }
}
