using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent singleton that remembers which "site" the player wants to land
/// on after a scene change, and performs the load asynchronously behind a
/// fade. Put this on an empty GameObject in the FIRST scene only (e.g. a
/// "Bootstrap" or "Managers" object). It survives scene loads via
/// DontDestroyOnLoad, so every other scene can read TargetSiteId from it
/// once loading finishes.
///
/// The fade matters in VR: a hard cut between two 360 environments is a
/// common nausea trigger. XR subsystems are deliberately left running across
/// the load so a user in VR does not drop out of stereo mid-transition.
///
/// The fade canvas is optional. With none assigned the behaviour is the
/// same minus the fade.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Optional fade overlay")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 0.4f;

    // The id of the site/hotspot to focus on once the new scene loads.
    // Empty when just loading a scene with no specific target.
    public string TargetSiteId { get; private set; }

    // True while a transition is in flight. Guards against double-clicks.
    public static bool IsTransitioning { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
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
        StartCoroutine(TransitionRoutine(sceneName));
    }

    /// <summary>
    /// Call once TargetSiteId has been used in the new scene, so a later
    /// plain scene load does not reuse a stale target.
    /// </summary>
    public void ClearTarget()
    {
        TargetSiteId = "";
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        IsTransitioning = true;

        yield return Fade(0f, 1f);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
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