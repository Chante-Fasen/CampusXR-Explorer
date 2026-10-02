using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to each scene's Canvas (CanvasMainMenu, CanvasBuidlingMap,
/// your persistent site UI, etc). Since the XR Origin/camera now lives in
/// a DIFFERENT scene (it carried over via PersistentXROrigin), you can't
/// drag it into the Canvas's Event Camera field in the Editor - this script
/// finds it automatically at runtime instead.
///
/// headLock parents the Canvas onto the camera (parenting updates instantly
/// with no per-frame lag, avoiding the "vibrating/catching up" jitter that
/// happens if you try to copy position/rotation manually in Update or
/// LateUpdate - the XR camera's real pose is applied even later than that,
/// right before rendering).
///
/// Because the camera lives under a DontDestroyOnLoad object, parenting
/// onto it would normally make this Canvas leak into every later scene too.
/// To prevent that, this script remembers which scene it originally
/// belonged to, and destroys itself the moment that scene unloads.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class CanvasCameraAssigner : MonoBehaviour
{
    [Tooltip("If true, this Canvas also becomes head-locked (follows wherever the player looks), like the MainMenu canvas.")]
    public bool headLock = false;

    [Tooltip("Only used if headLock is true - how far in front of the camera to sit.")]
    public float headLockDistance = 2f;

    private string originalSceneName;

    private void Awake()
    {
        // Record this BEFORE any reparenting happens, since parenting onto
        // a DontDestroyOnLoad object changes gameObject.scene afterwards.
        originalSceneName = gameObject.scene.name;
    }

    private void Start()
    {
        Camera vrCamera = Camera.main;

        if (vrCamera == null)
        {
            Debug.LogWarning("CanvasCameraAssigner: no Main Camera found. Make sure PersistentXROrigin carried over from MainMenu.");
            return;
        }

        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = vrCamera;

        if (headLock)
        {
            transform.SetParent(vrCamera.transform, worldPositionStays: false);
            transform.localPosition = new Vector3(0f, 0f, headLockDistance);
            transform.localRotation = Quaternion.identity;

            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == originalSceneName)
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
}