using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class GridShiftUITheme : ScriptableObject {
 public Sprite[] sprites;
 static GridShiftUITheme current;
 Dictionary<string,Sprite> lookup;
 public static Sprite Get(string name){if(current==null)current=Resources.Load<GridShiftUITheme>("GridShiftUITheme");if(current==null)return null;if(current.lookup==null){current.lookup=new Dictionary<string,Sprite>();foreach(var s in current.sprites)if(s!=null)current.lookup[s.name]=s;}return current.lookup.TryGetValue(name,out var found)?found:null;}
 public static void Surface(Image image,string name,bool sliced=true){if(image==null)return;var sprite=Get(name);if(sprite==null)return;image.sprite=sprite;image.color=Color.white;image.canvasRenderer.SetColor(Color.white);image.type=sliced?Image.Type.Sliced:Image.Type.Simple;image.pixelsPerUnitMultiplier=2;image.raycastTarget=false;}
 public static void Button(Button button,bool primary=false){var image=button.GetComponent<Image>();Surface(image,button.name.Contains("Sair")?"Botao_Alerta":primary?"Botao_Amarelo_Normal":"Botao_Escuro_Normal");image.raycastTarget=true;button.targetGraphic=image;button.transition=Selectable.Transition.SpriteSwap;image.CrossFadeColor(Color.white,0,true,true);button.spriteState=new SpriteState{highlightedSprite=Get("Botao_Amarelo_Hover"),selectedSprite=Get("Botao_Amarelo_Hover"),pressedSprite=Get("Botao_Amarelo_Pressionado"),disabledSprite=Get("Botao_Desativado")};var feedback=button.gameObject.AddComponent<GridShiftUIButtonFeedback>();feedback.primary=primary;string icon=button.name.Contains("Controles")?"Icone_Controles":button.name.Contains("Configura")?"Icone_Configuracoes":button.name.Contains("Sair")?"Icone_Sair":button.name.Contains("Voltar")?"Icone_Voltar":button.name.Contains("Continuar")||button.name.Contains("Novo Jogo")?"Icone_Play":button.name.Contains("Comprar")?"Icone_Caminhao":button.name.Contains("cidade")||button.name.Contains("jogador")?"Icone_Mapa":button.name=="+"?"Icone_Zoom_Mais":button.name=="−"?"Icone_Zoom_Menos":null;if(icon!=null){var art=Art(button.transform,icon,new Vector2(26,26),Vector2.zero);art.rectTransform.anchorMin=art.rectTransform.anchorMax=new Vector2(0,.5f);art.rectTransform.anchoredPosition=new Vector2(27,0);}var text=button.GetComponentInChildren<TMP_Text>();if(text!=null)text.color=primary?new Color(.035f,.045f,.06f):Color.white;}
 public static Image Art(Transform parent,string sprite,Vector2 size,Vector2 position){var go=new GameObject(sprite,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);var image=go.GetComponent<Image>();image.rectTransform.sizeDelta=size;image.rectTransform.anchoredPosition=position;Surface(image,sprite,false);image.preserveAspect=true;return image;}
}
public sealed class GridShiftUIButtonFeedback:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler,ISelectHandler,IDeselectHandler {
 public bool primary;bool hover,pressed;Vector3 original;Button button;TMP_Text label;
 void Awake(){original=transform.localScale;button=GetComponent<Button>();label=GetComponentInChildren<TMP_Text>();enabled=false;}
 void OnEnable(){if(button==null)button=GetComponent<Button>();if(label==null)label=GetComponentInChildren<TMP_Text>();}
 void OnDisable(){transform.localScale=original;}
 void Wake(){enabled=true;}
 public void OnPointerEnter(PointerEventData e){hover=true;Wake();}public void OnPointerExit(PointerEventData e){hover=pressed=false;Wake();}public void OnPointerDown(PointerEventData e){pressed=true;Wake();}public void OnPointerUp(PointerEventData e){pressed=false;Wake();}public void OnSelect(BaseEventData e){hover=true;Wake();}public void OnDeselect(BaseEventData e){hover=pressed=false;Wake();}
 void Update(){bool active=button!=null&&button.IsInteractable();var target=original*(active?(pressed?.98f:hover?1.015f:1):1);transform.localScale=Vector3.Lerp(transform.localScale,target,1-Mathf.Exp(-22*Time.unscaledDeltaTime));if(label!=null)label.color=active&&(primary||hover||pressed)?new Color(.035f,.045f,.06f):Color.white;if((transform.localScale-target).sqrMagnitude<.000001f){transform.localScale=target;enabled=false;}}
}


