using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
        // This line closes the application when run as a build.
        Application.Quit();

        // In the Unity Editor, Application.Quit() does not close the editor.
        // You can add a Debug.Log to confirm the function is being called.
        Debug.Log("Quitting game...");

        // Optional: For testing in the editor, you can stop play mode.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}