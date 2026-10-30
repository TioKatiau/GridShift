using System.IO;
using UnityEditor;
[InitializeOnLoad]public static class GridShiftV10Request {
 static GridShiftV10Request(){EditorApplication.update+=Poll;}
 static void Poll(){if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;const string p="Temp/gridshift-v10-request.txt";if(!File.Exists(p))return;string command=File.ReadAllText(p).Trim();File.Delete(p);if(command=="setup")GridShiftV10Setup.Build();if(command=="native")GridShiftV10NativeChecks.Batch();if(command=="test")GridShiftV10Checks.Run();}
}

