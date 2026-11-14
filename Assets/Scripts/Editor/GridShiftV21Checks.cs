using UnityEditor;
using UnityEditor.SceneManagement;
public static class GridShiftV21Checks {public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");SessionState.SetBool("GS21Checks",true);EditorApplication.isPlaying=true;}}
