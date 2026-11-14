using UnityEditor;
using UnityEditor.SceneManagement;
public static class GridShiftV24Checks {public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");SessionState.SetBool("GS24Checks",true);EditorApplication.isPlaying=true;}}
