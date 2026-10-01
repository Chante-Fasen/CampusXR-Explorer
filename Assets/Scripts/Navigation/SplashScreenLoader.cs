using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Attach this to an empty GameObject in your SplashScreen scene. It waits
/// for waitSeconds (default 3) then automatically loads the next scene.
/// No button needed - it runs on its own as soon as the scene starts.
/// </summary>
public class SplashScreenLoader : MonoBehaviour
{
    [Tooltip("Exact name of the scene to load after the splash screen.")]
    public string nextSceneName = "BuildingMap";

    [Tooltip("How many seconds to show the splash screen before switching.")]
    public float waitSeconds = 3f;

    private void Start()
    {
        StartCoroutine(LoadAfterDelay());
    }

    private IEnumerator LoadAfterDelay()
    {
        yield return new WaitForSeconds(waitSeconds);
        SceneManager.LoadScene(nextSceneName);
    }
}
