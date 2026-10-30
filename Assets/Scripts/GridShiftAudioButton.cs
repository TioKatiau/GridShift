using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Attached once to real uGUI buttons; OnPointerEnter runs only on entry.
[DisallowMultipleComponent] public sealed class GridShiftAudioButton : MonoBehaviour,IPointerEnterHandler {
 Button button;
 void Awake(){button=GetComponent<Button>();button.onClick.AddListener(Click);}
 void OnDestroy(){if(button!=null)button.onClick.RemoveListener(Click);}
 void Click(){GridShiftAudio.Play("UI_Click","UI",.3f);}
 public void OnPointerEnter(PointerEventData e){if(button!=null&&button.IsInteractable())GridShiftAudio.Play("UI_Hover","UI",.13f);}
}
