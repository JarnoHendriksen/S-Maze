using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIHandler : MonoBehaviour
{
    public static void StartGame()
    {
        SceneManager.LoadScene("S-Maze");
    }

    public static void OpenSettings()
    {
        // MainMenuUIHandler.instance.OpenSettings();
    }

    public static void ExitGame()
    {
        Application.Quit();
    }
}
