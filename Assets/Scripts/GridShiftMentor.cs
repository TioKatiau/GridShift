using UnityEngine;
using UnityEngine.SceneManagement;

// Carlos owns the explanations; the existing tutorial still owns progress and saves.
public static class GridShiftMentor {
 public static Transform Carlos {get;set;}
 public static void Talk(){
  if(!GridShiftTutorial.Active){GridShiftCampaignUI.Instance.MerchantContract();return;}
  if(GridShiftTutorial.Step==0)GridShiftTutorial.Begin();
  if(GridShiftTutorial.Step==2){GridShiftTutorial.Record("interact");GridShiftTutorial.Next();}
  var ui=GridShiftCampaignUI.Instance;
  ui.Conversation("Carlos · Seu mentor", "Vamos construir sua transportadora juntos.\n\n"+GridShiftTutorial.Titles[GridShiftTutorial.Step]+"\n"+GridShiftTutorial.Instructions()[GridShiftTutorial.Step], "Entendi, vou fazer",ui.Close,
   "Rever orientação",()=>ui.SetConversationText(GridShiftTutorial.Instructions()[GridShiftTutorial.Step]+"\n\nSiga o marcador dourado. Volte a conversar comigo quando precisar de ajuda."));
 }
}
