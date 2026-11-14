using UnityEditor;
using UnityEditor.SceneManagement;
public static class GridShiftV23Checks {public static void Photo(){SessionState.SetBool("GS23Photo",true);Batch();}public static void Batch(){EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");SessionState.SetBool("GS23Checks",true);EditorApplication.isPlaying=true;}}
