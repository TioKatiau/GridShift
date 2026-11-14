using TMPro;
using UnityEngine;

public static class GridShiftFactoryEntrance {
 public static Transform Entrance{get;private set;}
 public static void Install(GridShiftActiveOperations operations,GridShiftActiveConfig authorConfig=null){var cfg=authorConfig!=null?authorConfig:operations.Config;var manager=operations.GetComponent<DeliveryManager>();var pickup=Application.isPlaying?manager.Point(cfg.depotPointId):null;var source=Application.isPlaying?manager.Point(cfg.factoryPointId):null;if(pickup!=null&&source!=null){pickup.transform.position=cfg.depotPosition;pickup.pedestrianAccess=pickup.transform;pickup.route=source.route;pickup.segment=source.segment;pickup.vehicleStop=source.vehicleStop;pickup.displayName="Factory · Centro de distribuição GridShift";}var saved=operations.transform.Find("GridShift Factory Access 18");bool authored=saved!=null;var root=authored?saved.gameObject:new GameObject("GridShift Factory Access 18");if(!authored){root.transform.SetParent(operations.transform,false);root.transform.SetPositionAndRotation(cfg.depotPosition,cfg.depotRotation);}Entrance=root.transform;if(pickup!=null)pickup.transform.position=Entrance.position;float depth=1;if(Physics.Raycast(cfg.depotPosition+Vector3.up*1.2f,cfg.depotRotation*Vector3.forward,out var wall,5,~((1<<6)|(1<<7)|(1<<8)|(1<<9)),QueryTriggerInteraction.Ignore)&&Mathf.Abs(wall.normal.y)<.3f)depth=Mathf.Max(.05f,wall.distance-.045f);
  if(!authored){Part(root.transform,"Pedestrian door",new Vector3(0,1.15f,depth),new Vector3(2.1f,2.3f,.07f),cfg.structureMaterial);
  foreach(float x in new[]{-1.12f,1.12f})Part(root.transform,"Door jamb",new Vector3(x,1.2f,depth-.015f),new Vector3(.12f,2.4f,.12f),cfg.floorMaterial);
  Part(root.transform,"Door header",new Vector3(0,2.45f,depth-.015f),new Vector3(2.36f,.12f,.12f),cfg.floorMaterial);Part(root.transform,"Door handle",new Vector3(.8f,1.1f,depth-.07f),new Vector3(.06f,.25f,.07f),cfg.accentMaterial);
  var sign=new GameObject("GridShift distribution signage",typeof(TextMeshPro));sign.transform.SetParent(root.transform,false);sign.transform.localPosition=new Vector3(0,3,depth-.065f);var text=sign.GetComponent<TextMeshPro>();text.text="<color=#FFD23F>GS · GRIDSHIFT</color>\nCENTRO DE DISTRIBUIÇÃO";text.fontSize=3.5f;text.alignment=TextAlignmentOptions.Center;text.rectTransform.sizeDelta=new Vector2(5,1);
  }var enter=GridShiftWorldAction.Create(root.transform,"Entrar no centro de distribuição",Vector3.zero,p=>{if(operations.Van!=null&&operations.Van.Driving)return false;WorldSession.Instance.RequestTransition("FactoryInterior",p.transform);return true;});if(!enter.sceneAuthored){enter.focusOffset=new Vector3(0,1.2f,depth-.4f);enter.focusSize=new Vector3(2.4f,2.4f,.65f);}
  if(!authored)Part(root.transform,"Vehicle services terminal",new Vector3(2,.65f,0),new Vector3(.45f,1.3f,.25f),cfg.structureMaterial);
  GridShiftWorldAction.Create(root.transform,"Abastecer van",new Vector3(2,0,-.4f),p=>DeliveryManager.Instance.Company.Service(GridShiftPlayerVan.VehicleId,false,out _));
  GridShiftWorldAction.Create(root.transform,"Manutenção da van",new Vector3(3,0,-.4f),p=>DeliveryManager.Instance.Company.Service(GridShiftPlayerVan.VehicleId,true,out _));
  GridShiftWorldAction.Create(root.transform,"Solicitar guincho · R$ 15",new Vector3(2,0,.5f),p=>operations.Van!=null&&operations.Van.RecoverToDepot(p));
 }
 static void Part(Transform parent,string name,Vector3 at,Vector3 size,Material material){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=at;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;go.GetComponent<Collider>().enabled=false;}
}

