using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-2100)]
public sealed class GridShiftFactoryInterior:MonoBehaviour {
 public static GridShiftFactoryInterior Instance{get;private set;}
 public Transform exitPoint,attendantPoint;
 Material dark,steel,wood,yellow,white;
 void Awake(){Instance=this;if(GetComponent<GridShiftFactoryDetail>()==null)gameObject.AddComponent<GridShiftFactoryDetail>();foreach(var map in GetComponentsInChildren<MinimapCamera>(true))map.GetComponent<Camera>().cullingMask&=~(1<<12);var ops=DeliveryManager.Instance?.GetComponent<GridShiftActiveOperations>();if(ops!=null){ops.BindInterior(transform);foreach(var npc in GetComponentsInChildren<GridShiftDepotAttendant>())npc.operations=ops;}}
 void OnDestroy(){if(Instance==this)Instance=null;}
 public Transform Station(string command)=>GetComponentsInChildren<GridShiftFactoryStation>().FirstOrDefault(s=>s.command==command)?.transform;
 public void BuildGeometry(){
  var cfg=Resources.Load<GridShiftActiveConfig>("GridShiftActiveConfig");dark=cfg.structureMaterial;yellow=cfg.accentMaterial;
  steel=Material("Steel",new Color(.35f,.4f,.44f));wood=Material("Cardboard",new Color(.48f,.3f,.15f));white=Material("Walls",new Color(.72f,.75f,.74f));
  Block("Floor",new Vector3(0,-.12f,0),new Vector3(24,.24f,22),cfg.floorMaterial);
  Block("West wall",new Vector3(-12,2,0),new Vector3(.2f,4,22),white);Block("East wall",new Vector3(12,2,0),new Vector3(.2f,4,22),white);
  Block("North wall",new Vector3(0,2,11),new Vector3(24,4,.2f),white);Block("South wall",new Vector3(0,2,-11),new Vector3(24,4,.2f),white);
  Block("Ceiling",new Vector3(0,4.05f,0),new Vector3(24,.12f,22),white).layer=12;
  foreach(float x in new[]{-8f,0,8})foreach(float z in new[]{-7f,0,7}){Block("Ceiling fixture",new Vector3(x,3.85f,z),new Vector3(2,.08f,.4f),yellow,false);var go=new GameObject("Warehouse lighting");go.transform.SetParent(transform,false);go.transform.localPosition=new Vector3(x,3.4f,z);var light=go.AddComponent<Light>();light.type=LightType.Point;light.range=10;light.intensity=2.5f;light.shadows=LightShadows.None;}
  Block("Pedestrian exit",new Vector3(0,1.2f,-10.85f),new Vector3(1.5f,2.4f,.08f),dark);Block("Exit handle",new Vector3(.5f,1.1f,-10.77f),new Vector3(.12f,.12f,.1f),steel,false);
  Sign("SAÍDA · CIDADE",new Vector3(0,2.7f,-10.7f),.8f,180);exitPoint=Marker("exit",new Vector3(0,0,-9.7f));
  Table("Reception counter",new Vector3(-7,.45f,-6),new Vector3(5,.9f,.85f));
  attendantPoint=Marker("attendant",new Vector3(-7,0,-7.2f));attendantPoint.gameObject.AddComponent<GridShiftDepotAttendant>();
  Monitor(new Vector3(-8,1.2f,-6));Sign("GS · GRIDSHIFT\nRECEPÇÃO",new Vector3(-7,2.5f,-10.65f),1,180);
  Table("Conference",new Vector3(-6,.4f,-.1f),new Vector3(3,.8f,1));Block("Barcode scanner",new Vector3(-6,.87f,-.1f),new Vector3(.35f,.14f,.3f),yellow,false);Marker("scan",new Vector3(-6,0,-1.2f));Sign("02 · CONFERÊNCIA",new Vector3(-6,2.6f,.6f),.75f);
  Shelf(-10,4,"SETOR A · PEQUENAS");Shelf(-10,8,"SETOR B · MÉDIAS");Shelf(9,8,"SETOR C · CARGAS DA FROTA");
  Table("Pickup A",new Vector3(-8.5f,.4f,4),new Vector3(1.1f,.8f,1));Table("Pickup B",new Vector3(-8.5f,.4f,8),new Vector3(1.1f,.8f,1));
  Marker("sort-a",new Vector3(-7.4f,0,4));Marker("sort-b",new Vector3(-7.4f,0,8));
  Table("Packing bench",new Vector3(6,.4f,3),new Vector3(4,.8f,1.2f));Block("Packing mat",new Vector3(6,.82f,3),new Vector3(1,.04f,.8f),yellow,false);Block("Scale",new Vector3(7,.9f,3),new Vector3(.6f,.2f,.5f),steel,false);Marker("pack-small",new Vector3(5,0,1.7f));Marker("pack-medium",new Vector3(7,0,1.7f));Marker("label",new Vector3(8.8f,0,3));Sign("04 · EMBALAGEM",new Vector3(6,2.6f,3.7f),.75f);
  Table("Dispatch desk",new Vector3(6,.45f,-5.2f),new Vector3(4,.9f,1));Monitor(new Vector3(6,1.2f,-5.2f));Marker("accept",new Vector3(5,0,-6.4f));Marker("release",new Vector3(7,0,-6.4f));Marker("delegate",new Vector3(9,0,-5.2f));Sign("05 · PEDIDOS E EXPEDIÇÃO",new Vector3(6,2.6f,-4.5f),.75f);
  Block("Industrial loading door",new Vector3(11.85f,1.7f,-5),new Vector3(.1f,3.4f,4),steel);Sign("CARGA E DESCARGA",new Vector3(11.7f,3.5f,-5),.65f,90);
  Table("Administration desk",new Vector3(8,.4f,-8.7f),new Vector3(3,.8f,1));Monitor(new Vector3(8,1.15f,-8.7f));Marker("administration",new Vector3(8,0,-7.5f));Sign("ADMINISTRAÇÃO",new Vector3(8,2.5f,-10.7f),.65f,180);
  foreach(float z in new[]{-7f,-3,1,5,9})Block("Aisle guide",new Vector3(0,.006f,z),new Vector3(.08f,.012f,1.2f),yellow,false);
  foreach(var pos in new[]{new Vector3(4,.08f,7),new Vector3(6,.08f,7)}){Block("Pallet",pos,new Vector3(1.2f,.16f,1),wood);Block("Dispatch stock",pos+Vector3.up*.48f,new Vector3(.8f,.8f,.7f),wood);}
 }
 static Material Material(string name,Color color){return new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Factory "+name,color=color};}
 GameObject Block(string name,Vector3 at,Vector3 size,Material material,bool solid=true){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(transform,false);go.transform.localPosition=at;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;go.GetComponent<Collider>().enabled=solid;return go;}
 void Table(string name,Vector3 at,Vector3 size){Block(name+" top",at+Vector3.up*(size.y/2-.04f),new Vector3(size.x,.08f,size.z),dark);foreach(float x in new[]{-.4f,.4f})foreach(float z in new[]{-.35f,.35f})Block(name+" leg",at+new Vector3(x*size.x,-.03f,z*size.z),new Vector3(.12f,size.y-.08f,.12f),steel);}
 void Monitor(Vector3 at){Block("Monitor",at,new Vector3(.6f,.4f,.07f),dark,false);Block("Monitor screen",at+Vector3.back*.04f,new Vector3(.5f,.31f,.02f),yellow,false);}
 void Shelf(float x,float z,string title){foreach(float y in new[]{.7f,1.5f,2.3f}){Block("Stock shelf",new Vector3(x,y,z),new Vector3(1.4f,.09f,2.5f),steel);foreach(float dz in new[]{-.7f,.6f}){var box=Block("Decorative stock",new Vector3(x,y+.3f,z+dz),new Vector3(.8f,.5f,.65f),wood,false);Block("Packing tape",new Vector3(x,y+.56f,z+dz),new Vector3(.12f,.025f,.67f),yellow,false);}}foreach(float dx in new[]{-.7f,.7f})foreach(float dz in new[]{-1.25f,1.25f})Block("Shelf upright",new Vector3(x+dx,1.4f,z+dz),new Vector3(.08f,2.8f,.08f),dark);Sign(title,new Vector3(x,3.1f,z),.6f);}
 void Sign(string text,Vector3 at,float font,float yaw=0){var go=new GameObject(text,typeof(TextMeshPro));go.transform.SetParent(transform,false);go.transform.localPosition=at;go.transform.localRotation=Quaternion.Euler(0,yaw,0);var label=go.GetComponent<TextMeshPro>();label.text=text;label.fontSize=font*3;label.alignment=TextAlignmentOptions.Center;label.rectTransform.sizeDelta=new Vector2(4,.7f);label.textWrappingMode=TextWrappingModes.NoWrap;var backing=GameObject.CreatePrimitive(PrimitiveType.Cube);backing.name="Fixed station sign";backing.transform.SetParent(go.transform,false);backing.transform.localPosition=new Vector3(0,0,.04f);backing.transform.localScale=new Vector3(4,.7f,.04f);backing.GetComponent<Renderer>().sharedMaterial=dark;backing.GetComponent<Collider>().enabled=false;label.color=new Color(1,.84f,.2f);}
 Transform Marker(string command,Vector3 at){var go=new GameObject("Station-"+command);go.transform.SetParent(transform,false);go.transform.localPosition=at;go.AddComponent<GridShiftFactoryStation>().command=command;return go.transform;}
}

