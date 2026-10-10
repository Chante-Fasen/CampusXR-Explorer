using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Attach this to an empty GameObject in your SplashScreen scene.
///
/// Two jobs:
///  1. Normal splash (MainMenu > Start): loads nextSceneName (BuildingMap)
///     in the background and shows the splash for at least waitSeconds.
///  2. Loading screen: when SceneTransitionManager sends the player to a
///     site (e.g. map > Alpha), it sets PendingScene and opens this scene
///     first. The real scene (Tour360) then loads in the background while
///     the slider shows the actual loading progress.
///
/// In both cases the slider follows the real progress, so the old
/// SliderController animation is switched off automatically.
/// </summary>
public class SplashScreenLoader : MonoBehaviour
{
    // Set by SceneTransitionManager.GoToSite before opening this scene.
    public static string PendingScene;

    [Tooltip("Exact name of the scene to load after the splash screen (when not used as a loading screen).")]
    public string nextSceneName = "BuildingMap";

    [Tooltip("Minimum seconds to show the splash screen at app start.")]
    public float waitSeconds = 3f;

    [Tooltip("Minimum seconds to show the loading screen between scenes (avoids a quick flash).")]
    public float minimumLoadingSeconds = 0.75f;

    [Tooltip("Slider to fill with the loading progress. Leave empty to find it automatically.")]
    public Slider progressSlider;

    private string sceneToLoad;
    private float minimumSeconds;

    private void Awake()
    {
        if (!string.IsNullOrEmpty(PendingScene))
        {
            sceneToLoad = PendingScene;
            minimumSeconds = minimumLoadingSeconds;
            PendingScene = null;
        }
        else
        {
            sceneToLoad = nextSceneName;
            minimumSeconds = waitSeconds;
        }

        if (progressSlider == null)
        {
            progressSlider = FindAnyObjectByType<Slider>();
        }

        // We drive the slider ourselves - stop the old timed animation.
        // (Disabling in Awake means its Start never runs.)
        foreach (SliderController controller in FindObjectsByType<SliderController>(FindObjectsSortMode.None))
        {
            controller.enabled = false;
        }

        SetProgress(0f);
    }

    private void Start()
    {
        StartCoroutine(LoadInBackground());
    }

    private IEnumerator LoadInBackground()
    {
        // Let the splash render at least one frame before the heavy work starts.
        yield return null;

        float startTime = Time.unscaledTime;

        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneToLoad);
        if (loading == null)
        {
            Debug.LogError($"SplashScreenLoader: couldn't load scene '{sceneToLoad}'. Is it in File > Build Settings?");
            yield break;
        }

        // Load fully in the background, but don't switch until we say so.
        loading.allowSceneActivation = false;

        // Unity reports 0 -> 0.9 while loading; 0.9 means "ready to switch".
        while (loading.progress < 0.9f)
        {
            float loaded = Mathf.Clamp01(loading.progress / 0.9f);
            float timeShare = minimumSeconds > 0f ? Mathf.Clamp01((Time.unscaledTime - startTime) / minimumSeconds) : 1f;

            // Never run ahead of the real progress or the minimum display time.
            SetProgress(Mathf.Min(loaded, timeShare));
            yield return null;
        }

        // Fill the bar over the rest of the minimum display time.
        while (Time.unscaledTime - startTime < minimumSeconds)
        {
            float timeShare = Mathf.Clamp01((Time.unscaledTime - startTime) / minimumSeconds);
            SetProgress(timeShare);
            yield return null;
        }

        SetProgress(1f);
        yield return null;

        loading.allowSceneActivation = true;
    }

    private void SetProgress(float progress)
    {
        if (progressSlider == null) return;
        progressSlider.value = Mathf.Lerp(progressSlider.minValue, progressSlider.maxValue, progress);
    }
}
