using TMPro;
using UnityEngine;
using UnityEngine.UI;
public sealed class GridShiftBrowserLoading:MonoBehaviour {
 GridShiftUwbTerminal owner;RectTransform root;
 public void Initialize(GridShiftUwbTerminal terminal){owner=terminal;owner.browser.browserClient.OnLoadFinish+=Loaded;}
 void Loaded(string url){if(root!=null)root.gameObject.SetActive(false);}
 public void Show(CanvasGroup surface){if(surface==null)return;if(root==null){root=new GameObject("UrbanFlowPreparing",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image)).GetComponent<RectTransform>();root.sizeDelta=new Vector2(220,110);root.GetComponent<Image>().color=new Color(.025f,.04f,.07f,.96f);root.GetComponent<Image>().raycastTarget=false;var brand=GridShiftBranding.Current;if(brand!=null&&brand.urbanFlow!=null){var image=new GameObject("OriginalUrbanFlow",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));image.transform.SetParent(root,false);image.GetComponent<RectTransform>().sizeDelta=new Vector2(180,62);image.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,12);image.GetComponent<Image>().sprite=brand.urbanFlow;image.GetComponent<Image>().preserveAspect=true;image.GetComponent<Image>().raycastTarget=false;}else{var text=Label("Urban Flow",21);text.rectTransform.anchoredPosition=new Vector2(0,13);}Label("Preparando o navegador…",12).rectTransform.anchoredPosition=new Vector2(0,-32);}
  root.SetParent(surface.transform,false);root.anchoredPosition=Vector2.zero;root.localScale=Vector3.one;root.localRotation=Quaternion.identity;root.SetAsLastSibling();root.gameObject.SetActive(true);
 }
 TMP_Text Label(string value,float size){var go=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(root,false);var text=go.GetComponent<TextMeshProUGUI>();text.rectTransform.sizeDelta=new Vector2(210,42);text.text=value;text.fontSize=size;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;return text;}
 public void Hide(){if(root!=null)root.gameObject.SetActive(false);}
 void OnDestroy(){if(owner!=null&&owner.browser!=null)owner.browser.browserClient.OnLoadFinish-=Loaded;if(root!=null)Destroy(root.gameObject);}
}
