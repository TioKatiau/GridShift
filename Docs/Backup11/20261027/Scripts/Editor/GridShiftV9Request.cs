using System.IO;
using UnityEditor;
[InitializeOnLoad]public static class GridShiftV9Request {
 static GridShiftV9Request(){EditorApplication.update+=Poll;}
 static void Poll(){if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;const string path="Temp/gridshift-v9-request.txt";if(!File.Exists(path))return;string cmd=File.ReadAllText(path).Trim();File.Delete(path);if(cmd=="import")GridShiftV9Setup.Import();if(cmd=="visual"){SessionState.SetBool("GS.V9.Visual",true);GridShiftV9Checks.Run();}if(cmd=="test")GridShiftV9Checks.Run();if(cmd=="core")GridShiftV9Regression.Batch();if(cmd=="native")GridShiftV9NativeChecks.Batch();}
}


