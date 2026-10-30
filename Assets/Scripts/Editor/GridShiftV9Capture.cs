using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class GridShiftV9Capture {
 public static void Save(string name,int width,int height){var camera=new GameObject("UICapture9").AddComponent<Camera>();camera.enabled=false;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.015f,.025f,.04f);camera.cullingMask=-1;var canvases=Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var render=new RenderTexture(width,height,24);camera.targetTexture=render;render.Create();for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceCamera;canvases[i].worldCamera=camera;canvases[i].planeDistance=1+i*.01f;}Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=render;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes("Docs/Validacao9/"+name+"-"+width+"x"+height+".png",image.EncodeToPNG());RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=null;canvases[i].planeDistance=distances[i];}camera.targetTexture=null;render.Release();Object.Destroy(render);Object.Destroy(image);Object.Destroy(camera.gameObject);Canvas.ForceUpdateCanvases();}
}

