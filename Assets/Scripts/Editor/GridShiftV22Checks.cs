using UnityEditor;
using UnityEditor.SceneManagement;
public static class GridShiftV22Checks {public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");SessionState.SetBool("GS22Checks",true);EditorApplication.isPlaying=true;}}
