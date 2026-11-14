#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

// Production footage only: the camera observes the live scene and never alters orders or traffic.
public sealed class GridShiftV19Film:MonoBehaviour {
 public static bool Enabled{get=>SessionState.GetBool("GS19Film",false);set=>SessionState.SetBool("GS19Film",value);}static GridShiftV19Film instance;Process encoder;Camera camera,source;RenderTexture target;Texture2D pixels;byte[] bytes;int frame,total;bool includeUI;Action<float> director;string shot;float priorCapture;
 public const string Output="Docs/FeiraSENAI2026";
 static GridShiftV19Film Get(){if(instance==null){var go=new GameObject("Real gameplay film capture 19");Object.DontDestroyOnLoad(go);instance=go.AddComponent<GridShiftV19Film>();}return instance;}
 public static IEnumerator Record(string name,Camera view,float seconds,bool ui=true,Action<Camera,float> position=null){if(!Enabled)yield break;string existing=Output+"/clipes/"+name+".mp4";if(File.Exists(existing)&&new FileInfo(existing).Length>1024){using(var stream=File.OpenRead(existing)){int n=(int)Math.Min(65536,stream.Length);var tail=new byte[n];stream.Seek(-n,SeekOrigin.End);stream.Read(tail,0,n);if(System.Text.Encoding.ASCII.GetString(tail).Contains("moov")){File.AppendAllText(Output+"/capturas.txt","REUSE finalized real capture "+name+"\n");yield break;}}}var f=Get();f.Begin(name,view,seconds,ui,position);while(f.frame<f.total)yield return null;f.End();}
 public static void BeginShot(string name,Camera view,bool ui=true){if(Enabled)Get().Begin(name,view,600,ui,null);}
 public static void EndShot(){if(Enabled&&instance!=null&&instance.encoder!=null)instance.End();}
 void Begin(string name,Camera view,float seconds,bool ui,Action<Camera,float> position){if(encoder!=null)End();Directory.CreateDirectory(Output+"/clipes");Directory.CreateDirectory(Output+"/previas");shot=name;source=view!=null?view:Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault(c=>c.isActiveAndEnabled);if(source==null)throw new Exception("No live camera for shot "+name);includeUI=ui;frame=0;total=Mathf.RoundToInt(seconds*30);if(camera==null){camera=new GameObject("Film view 19").AddComponent<Camera>();camera.transform.SetParent(transform);camera.enabled=false;target=new RenderTexture(1920,1080,24);pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);bytes=new byte[1920*1080*3];}priorCapture=Time.captureDeltaTime;Time.captureDeltaTime=1f/30;director=position==null?(Action<float>)null:t=>position(camera,t);var encoderPath=Environment.GetEnvironmentVariable("GS19_FFMPEG")??"C:/Users/Gb191/OneDrive/Desktop/GridShift/Tools/Media19/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v9.0.2.exe";var args="-hide_banner -loglevel error -y -f rawvideo -pixel_format rgb24 -video_size 1920x1080 -framerate 30 -i pipe:0 -vf vflip -an -c:v libx264 -preset ultrafast -crf 19 -pix_fmt yuv420p \""+Path.GetFullPath(Output+"/clipes/"+name+".mp4")+"\"";encoder=Process.Start(new ProcessStartInfo(encoderPath,args){UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardError=true});File.AppendAllText(Output+"/capturas.txt","BEGIN "+shot+" scene="+SceneManager.GetActiveScene().name+" real live state; no injected reward\n");}
 void LateUpdate(){if(encoder==null||frame>=total||source==null)return;camera.CopyFrom(source);camera.enabled=false;camera.targetTexture=target;camera.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);camera.GetUniversalAdditionalCameraData().renderPostProcessing=source.GetUniversalAdditionalCameraData().renderPostProcessing;director?.Invoke(frame/30f);
  var playerRenderers=includeUI?Array.Empty<Renderer>():Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None).SelectMany(p=>p.GetComponentsInChildren<Renderer>(true)).ToArray();var playerStates=playerRenderers.Select(r=>r.enabled).ToArray();foreach(var r in playerRenderers)r.enabled=false;var canvases=Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);var states=canvases.Select(c=>(c.enabled,c.renderMode,c.worldCamera,c.planeDistance)).ToArray();foreach(var canvas in canvases){if(!includeUI){canvas.enabled=false;continue;}if(canvas.renderMode==RenderMode.ScreenSpaceOverlay){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.8f;}}
  Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1920,1080),0,0,false);pixels.Apply(false,false);RenderTexture.active=old;if(frame==15)File.WriteAllBytes(Output+"/previas/"+shot+".png",pixels.EncodeToPNG());pixels.GetRawTextureData<byte>().CopyTo(bytes);encoder.StandardInput.BaseStream.Write(bytes,0,bytes.Length);
  for(int i=0;i<canvases.Length;i++)if(canvases[i]!=null){canvases[i].enabled=states[i].Item1;canvases[i].renderMode=states[i].Item2;canvases[i].worldCamera=states[i].Item3;canvases[i].planeDistance=states[i].Item4;}for(int i=0;i<playerRenderers.Length;i++)if(playerRenderers[i]!=null)playerRenderers[i].enabled=playerStates[i];frame++;
 }
 void End(){encoder.StandardInput.Close();string error=encoder.StandardError.ReadToEnd();encoder.WaitForExit();int code=encoder.ExitCode;encoder.Dispose();encoder=null;Time.captureDeltaTime=priorCapture;File.AppendAllText(Output+"/capturas.txt","END "+shot+" frames="+frame+" seconds="+(frame/30f)+" exit="+code+"\n");if(code!=0)throw new Exception(error);}
 void OnDestroy(){if(encoder!=null)End();}
}

#endif
