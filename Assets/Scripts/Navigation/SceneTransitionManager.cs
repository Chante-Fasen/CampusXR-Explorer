using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Async scene loading with a fade. Fades matter in VR: a hard cut between
// two 360 environments is a common nausea trigger.
// Attach to a persistent object that also holds a full-screen black Image
// on a Canvas set to Screen Space - Overlay.
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 0.4f;

    public static bool IsTransitioning { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvas != null) fadeCanvas.alpha = 0f;
    }

    public void GoToScene(string sceneName)
    {
        if (IsTransitioning)
        {
            Debug.LogWarning("Transition already running, ignoring request for: " + sceneName);
            return;
        }
        StartCoroutine(TransitionRoutine(sceneName));
    }

    IEnumerator TransitionRoutine(string sceneName)
    {
        IsTransitioning = true;

        yield return Fade(0f, 1f);

        // XR subsystems are deliberately left running across the load so a
        // user in VR does not drop out of stereo mid-transition.
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // Unity reports 0.9 when the scene is loaded and waiting to activate.
        while (op.progress < 0.9f)
        {
            yield return null;
        }

        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        // One frame for the new scene's Start() methods to run before revealing.
        yield return null;

        yield return Fade(1f, 0f);

        IsTransitioning = false;
    }

    IEnumerator Fade(float from, float to)
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
