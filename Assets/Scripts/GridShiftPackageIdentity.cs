using UnityEngine;
using System.Linq;
using TMPro;
public sealed class GridShiftPackageIdentity:MonoBehaviour {
 public DeliveryOrder Order=>DeliveryManager.Instance?.orders.FirstOrDefault(o=>o.id==orderId);
 public float WeightKg=>Order?.weightKg??0;public string Goods=>Order?.goods;public string Pickup=>Order?.pickupId;public string SizeName=>WeightKg<=5?"Pequena":"Média";public string State=>Order?.status.ToString();
 public string packageId,orderId;[SerializeField]TMP_Text label;string shown;
 public void AuthorScene(){ShowLabel("Destino da encomenda");}
 public void ShowLabel(string destination){if(shown==destination)return;shown=destination;
  if(label==null){var root=new GameObject("ParcelLabel",typeof(TextMeshPro));root.transform.SetParent(transform,false);root.transform.localPosition=new Vector3(0,.512f,0);root.transform.localRotation=Quaternion.Euler(90,0,0);label=root.GetComponent<TextMeshPro>();label.rectTransform.sizeDelta=new Vector2(.95f,.8f);label.fontSize=.85f;label.alignment=TextAlignmentOptions.Center;label.color=Color.white;}
  label.text="GS #"+(orderId!=null&&orderId.Length>=8?orderId.Substring(0,8):"---")+"\n|||| ||| || ||||\n"+(destination.Length>18?destination.Substring(0,18):destination);
 }
}
