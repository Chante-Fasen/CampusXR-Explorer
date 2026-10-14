using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent singleton that remembers which "site" the player wants to land
/// on after a scene change, and performs the load behind a fade.
/// It survives scene loads via DontDestroyOnLoad, so every other scene can
/// read TargetSiteId from it once loading finishes.
///
/// Loading screen: when useLoadingScreen is on, GoToSite fades out, shows the
/// SplashScreen scene (small, opens instantly), fades back in, and the splash
/// then loads the real scene in the background with a real progress bar
/// (see SplashScreenLoader). With it off, the scene loads asynchronously
/// behind the fade instead.
///
/// The fade matters in VR: a hard cut between two 360 environments is a
/// common nausea trigger. XR subsystems are deliberately left running across
/// the load so a user in VR does not drop out of stereo mid-transition.
/// The fade canvas is optional. With none assigned the behaviour is the
/// same minus the fade.
///
/// Duplicates: if a scene that also contains this component is opened again
/// (e.g. SplashScreen used as the loading screen), only the extra COMPONENT
/// is removed - not its whole GameObject - so things that share the object
/// (like SplashScreenLoader on "Managers") keep working.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Loading screen")]
    [Tooltip("Show the splash screen with a progress bar while the next scene loads.")]
    public bool useLoadingScreen = true;

    [Tooltip("Exact name of the scene used as the loading screen.")]
    public string loadingSceneName = "SplashScreen";

    [Header("Optional fade overlay")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 0.4f;

    // The id of the site/hotspot to focus on once the new scene loads.
    // Empty when just loading a scene with no specific target.
    public string TargetSiteId { get; private set; }

    // True while a transition is in flight. Guards against double-clicks.
    public static bool IsTransitioning { get; private set; }

    /// <summary>
    /// The manager, created on the spot if no scene has made one yet (e.g.
    /// Free Roam from MainMenu, before SplashScreen has ever loaded).
    /// </summary>
    public static SceneTransitionManager GetOrCreate()
    {
        if (Instance == null)
        {
            new GameObject("SceneTransitionManager").AddComponent<SceneTransitionManager>();
        }
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Hide this copy's fade overlay so it can't cover the screen,
            // then remove only this component (see "Duplicates" above).
            if (fadeCanvas != null) fadeCanvas.gameObject.SetActive(false);
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f;
            fadeCanvas.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Load a scene and, once there, jump straight to a specific site.
    /// </summary>
    /// <param name="sceneName">Exact scene name, must be in Build Settings.</param>
    /// <param name="siteId">Site to focus in that scene. Pass "" if not needed.</param>
    public void GoToSite(string sceneName, string siteId)
    {
        if (IsTransitioning)
        {
            Debug.LogWarning("Transition already running, ignoring request for: " + sceneName);
            return;
        }

        TargetSiteId = siteId;

        if (useLoadingScreen && !string.IsNullOrEmpty(loadingSceneName))
        {
            StartCoroutine(LoadingScreenRoutine(sceneName));
        }
        else
        {
            StartCoroutine(TransitionRoutine(sceneName));
        }
    }

    /// <summary>
    /// Call once TargetSiteId has been used in the new scene, so a later
    /// plain scene load does not reuse a stale target.
    /// </summary>
    public void ClearTarget()
    {
        TargetSiteId = "";
    }

    // Fade out -> show the splash/loading scene -> fade in. The splash scene
    // then loads sceneName itself and fills its slider with the progress.
    private IEnumerator LoadingScreenRoutine(string sceneName)
    {
        IsTransitioning = true;

        yield return Fade(0f, 1f);

        SplashScreenLoader.PendingScene = sceneName;
        SceneManager.LoadScene(loadingSceneName);

        // One frame so the splash scene is up before revealing it.
        yield return null;

        yield return Fade(1f, 0f);

        IsTransitioning = false;
    }

    // Fade out -> load sceneName in the background -> fade in.
    private IEnumerator TransitionRoutine(string sceneName)
    {
        IsTransitioning = true;

        yield return Fade(0f, 1f);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null)
        {
            Debug.LogError("SceneTransitionManager: couldn't load scene '" + sceneName + "'. Is it in Build Settings?");
            yield return Fade(1f, 0f);
            IsTransitioning = false;
            yield break;
        }
        op.allowSceneActivation = false;

        // Unity reports 0.9 when the scene is loaded and waiting to activate.
        while (op.progress < 0.9f)
        {
            yield return null;
        }

        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        // One frame so the new scene's Start() methods run before revealing.
        yield return null;

        yield return Fade(1f, 0f);

        IsTransitioning = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeCanvas == null) yield break;

        fadeCanvas.blocksRaycasts = true;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvas.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = to;
        fadeCanvas.blocksRaycasts = (to > 0.5f);
    }
}
