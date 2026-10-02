using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to the Start button in MainMenu (or to a manager object in
/// that scene). Wire the button's OnClick to StartButtonPressed().
/// </summary>
public class MainMenuStartButton : MonoBehaviour
{
    [Tooltip("Exact name of the splash screen scene to load.")]
    public string splashSceneName = "SplashScreen";

    public void StartButtonPressed()
    {
        SceneManager.LoadScene(splashSceneName);
    }
}
