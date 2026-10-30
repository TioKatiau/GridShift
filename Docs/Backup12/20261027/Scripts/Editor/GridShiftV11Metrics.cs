using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
public static class GridShiftV11Metrics {
 public static IEnumerator Sample(){yield return new WaitForSecondsRealtime(10);double cpu=Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;float start=Time.realtimeSinceStartup,frames=0;double elapsed=0;while(Time.realtimeSinceStartup-start<8){frames++;elapsed+=Time.unscaledDeltaTime;yield return null;}File.WriteAllText("Docs/Validacao11/native-performance.txt","Condição: editor batch DX11, cidade+Office, tablet Unity UI nativo; amostra 8s após aquecimento 10s.\nUnity allocated bytes="+UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()+"\nUnity reserved bytes="+UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong()+"\nEditor CPU ms/second="+((Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds-cpu)/8).ToString("F2")+"\nMean frame ms="+(elapsed/Math.Max(1,frames)*1000).ToString("F2")+"\nWorking set do sistema operacional indisponível nesta API do editor. Replay não idêntico; sem conclusão de ganho global de FPS.\n");}
}
