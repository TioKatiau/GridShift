using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class GridShiftSession {
 public static IEnumerator ReturnToMenu(){var manager=DeliveryManager.Instance;if(manager!=null)manager.GetComponent<DeliveryPersistence>().Save();if(PauseMenu.Instance!=null)PauseMenu.Instance.Close();GridShiftOperationsUI.Instance?.Close();GridShiftCampaignUI.Instance?.Close();Time.timeScale=1;if(FadeManager.Instance!=null)yield return FadeManager.Instance.FadeOut();yield return SceneManager.LoadSceneAsync("MainMenu",LoadSceneMode.Single);Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
}
