using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public sealed class GridShiftCampaignWorld:MonoBehaviour {
 GameObject merchant;IEnumerator Start(){var company=GetComponent<GridShiftCompany>();while(!company.Ready)yield return null;var config=company.Balance;if(config.merchantPrefab==null)yield break;merchant=Instantiate(config.merchantPrefab,config.merchantPosition,config.merchantRotation);merchant.name="Carlos — Comerciante GridShift";SceneManager.MoveGameObjectToScene(merchant,gameObject.scene);}
 void OnDestroy(){if(merchant!=null)Destroy(merchant);}
}
