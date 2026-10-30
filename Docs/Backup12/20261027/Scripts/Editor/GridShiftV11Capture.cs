using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class GridShiftV11Capture {
 public static void Save(string name,Camera view){Directory.CreateDirectory("Docs/Validacao11");var go=new GameObject("CampaignCapture10");var camera=go.AddComponent<Camera>();camera.CopyFrom(view);camera.enabled=false;camera.transform.SetPositionAndRotation(view.transform.position,view.transform.rotation);var canvases=Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=.2f;}var target=new RenderTexture(1920,1080,24);camera.targetTexture=target;Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(1920,1080,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();File.WriteAllBytes("Docs/Validacao11/"+name+".png",image.EncodeToPNG());RenderTexture.active=old;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=null;canvases[i].planeDistance=distances[i];}target.Release();Object.Destroy(target);Object.Destroy(image);Object.Destroy(go);Canvas.ForceUpdateCanvases();}
}

