using UnityEditor;using UnityEditor.SceneManagement;
public static class GridShiftV25Checks {public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");SessionState.SetBool("GS25Checks",true);EditorApplication.isPlaying=true;}}
