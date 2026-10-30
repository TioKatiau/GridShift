using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;

public static class GridShiftPerformanceProfileAnalysis
{
    public static void Batch()
    {
        ProfilerDriver.LoadProfile(Path.GetFullPath("performance.raw"), false);
        EditorApplication.delayCall += Analyze;
    }
    static void Analyze()
    {
        var totals = new Dictionary<string, double>(); var calls = new Dictionary<string, int>(); int frames = 0;
        for (int frame = ProfilerDriver.firstFrameIndex; frame <= ProfilerDriver.lastFrameIndex; frame += 5)
        using (var data = ProfilerDriver.GetRawFrameDataView(frame, 0))
        {
            if (!data.valid) continue; frames++;
            for (int i = 0; i < data.sampleCount; i++)
            {
                string name = data.GetSampleName(i);
                if (!totals.ContainsKey(name)) { totals[name] = 0; calls[name] = 0; }
                totals[name] += data.GetSampleTimeMs(i); calls[name]++;
            }
        }
        var report = new StringBuilder("Unity CPU Profiler: inclusive sample time, sampled every fifth frame\nFrames: " + frames + "\n");
        foreach (var item in totals.OrderByDescending(x => x.Value).Take(100)) report.AppendLine(item.Key + " | avg_ms=" + (item.Value / Math.Max(1, frames)).ToString("F4") + " | calls=" + calls[item.Key]);
        report.AppendLine("\nSCRIPT SAMPLES");
        foreach (var item in totals.Where(x => x.Key.Contains("Traffic") || x.Key.Contains("Pedestrian") || x.Key.Contains("GridShift") || x.Key.Contains("RealTimeClock") || x.Key.Contains("GC.Alloc") || x.Key.Contains("Physics.")).OrderByDescending(x => x.Value)) report.AppendLine(item.Key + " | avg_ms=" + (item.Value / Math.Max(1, frames)).ToString("F4") + " | calls=" + calls[item.Key]);
        File.WriteAllText("profile-analysis.txt", report.ToString()); EditorApplication.Exit(0);
    }
}
