using UnityEngine;
using TMPro;

// Decoration stays on walls/ceiling or inside existing workstation footprints.
public sealed class GridShiftFactoryDetail:MonoBehaviour {
 [SerializeField]bool sceneAuthored;Material blue,metal,paper,red,card;int pieces;public void AuthorScene(){if(sceneAuthored)return;Start();sceneAuthored=true;}
 void Start(){if(sceneAuthored)return;blue=Mat(new Color(.08f,.18f,.23f));metal=Mat(new Color(.42f,.48f,.51f));paper=Mat(new Color(.86f,.85f,.75f));red=Mat(new Color(.72f,.06f,.035f));card=Mat(new Color(.48f,.3f,.15f));
  foreach(float x in new[]{-11.8f,11.8f}){Block("Wall protection rail",new Vector3(x,.65f,0),new Vector3(.08f,.17f,21),blue);Block("Cable tray",new Vector3(x*.97f,3.55f,0),new Vector3(.16f,.12f,21),metal);for(int z=-9;z<=9;z+=3)Block("Wall panel joint",new Vector3(x,2,z),new Vector3(.012f,3.6f,.025f),metal);}
  foreach(float x in new[]{-8f,0,8}){Block("Ceiling beam",new Vector3(x,3.93f,0),new Vector3(.15f,.13f,21.6f),metal);for(int z=-9;z<=9;z+=3)Block("Ceiling ventilation",new Vector3(x,3.94f,z),new Vector3(.9f,.025f,.5f),blue);}
  foreach(float x in new[]{-1.1f,1.1f})Block("Pedestrian aisle",new Vector3(x,.014f,0),new Vector3(.06f,.008f,19),paper);
  foreach(float x in new[]{-10.5f,10.5f})for(int z=-9;z<=9;z+=6){Block("Safety wall notice",new Vector3(x,2.15f,10.87f),new Vector3(1.3f,.9f,.025f),blue);}
  foreach(float x in new[]{-3f,3f}){Block("Fire extinguisher bracket",new Vector3(x,1.1f,-10.83f),new Vector3(.4f,1,.06f),red);Block("Fire extinguisher",new Vector3(x,1.05f,-10.65f),new Vector3(.2f,.6f,.18f),metal);}
  foreach(var at in new[]{new Vector3(-8.7f,.96f,-6),new Vector3(5.6f,1,-5.2f),new Vector3(8.5f,.88f,-8.7f)}){for(int i=0;i<4;i++)Block("Forms and manifests",at+Vector3.up*.009f*i,new Vector3(.34f,.008f,.24f),paper);Block("Keyboard",at+Vector3.right*.5f,new Vector3(.43f,.035f,.15f),blue);Block("Desk cup",at+Vector3.left*.35f+Vector3.up*.05f,new Vector3(.09f,.12f,.09f),metal);}
  for(int i=0;i<8;i++){var at=new Vector3(4.6f+i*.38f,.86f,3.4f);Block("Packaging supplies",at,new Vector3(.28f,.08f,.25f),i%2==0?card:paper);}
  for(int i=0;i<6;i++){Block("Locker",new Vector3(-10.7f+i*.55f,1,-10.7f),new Vector3(.5f,2,.38f),blue);Block("Locker handle",new Vector3(-10.55f+i*.55f,1.1f,-10.48f),new Vector3(.035f,.15f,.03f),metal);}
  var sign=new GameObject("Safety instructions",typeof(TextMeshPro));sign.transform.SetParent(transform,false);sign.transform.localPosition=new Vector3(0,2.6f,10.84f);var text=sign.GetComponent<TextMeshPro>();text.text="GRIDSHIFT · SEGURANÇA EM PRIMEIRO LUGAR\nMantenha os corredores livres · Confira a identificação da carga";text.fontSize=1.4f;text.alignment=TextAlignmentOptions.Center;text.rectTransform.sizeDelta=new Vector2(9,1);sign.transform.localRotation=Quaternion.identity;
 }
 Material Mat(Color c)=>new Material(Shader.Find("Universal Render Pipeline/Lit")){color=c};
 void Block(string name,Vector3 at,Vector3 size,Material material){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(transform,false);go.transform.localPosition=at;go.transform.localScale=size;go.GetComponent<Collider>().enabled=false;go.GetComponent<Renderer>().sharedMaterial=material;pieces++;}
 void OnDestroy(){if(!Application.isPlaying)return;foreach(var m in new[]{blue,metal,paper,red,card})if(m!=null)Destroy(m);}
}
