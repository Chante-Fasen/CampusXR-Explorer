using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base for pop-up panels inside Tour360 (Settings, Map, ...): a world-space
/// canvas shown over the 360 photo instead of loading another scene.
///
/// SETUP for any panel canvas in Tour360:
///   - Canvas: Render Mode = World Space, scale 0.001, plus a
///     Tracked Device Graphic Raycaster (controller rays) and a
///     Canvas Camera Assigner:
///       Head Lock ticked   -> follows your head at "Head Locked Distance"
///       Head Lock unticked -> placed once in front of you at "Distance"
///   - Untick the GameObject (top checkbox) so it starts hidden - NOT the
///     Canvas component.
///   - Only one panel is open at a time: opening one closes the others.
///   - While a panel is open, the tour's own buttons (CanvasPersistentUI)
///     are hidden so they don't overlap it.
/// </summary>
public class TourPanel : MonoBehaviour
{
    private static readonly List<TourPanel> AllPanels = new List<TourPanel>();

    [Header("Placement")]
    [Tooltip("How far in front of the player the panel appears when NOT head-locked, in metres.")]
    public float distance = 1.2f;

    [Tooltip("Raise (+) or lower (-) the panel relative to eye level, in metres.")]
    public float heightOffset = 0f;

    [Tooltip("Distance used when Head Lock is ticked on the Canvas Camera Assigner. Keep it below the tour buttons' distance (1 m) so the panel sits in front of them.")]
    public float headLockedDistance = 0.9f;

    [Header("Tour buttons")]
    [Tooltip("Hide the tour's Map/Settings buttons while the panel is open.")]
    public bool hideTourButtonsWhileOpen = true;

    [Tooltip("Name of the canvas holding the tour's buttons.")]
    public string tourButtonsName = "CanvasPersistentUI";

    [Tooltip("Hide the panel when the scene starts, even if it was left switched on in the Editor.")]
    public bool startHidden = true;

    private static GameObject tourButtons;
    private bool initialised;

    /// <summary>Open = switched on AND its Canvas is drawing.</summary>
    public bool IsOpen
    {
        get
        {
            if (!gameObject.activeSelf) return false;
            Canvas canvas = GetComponent<Canvas>();
            return canvas == null || canvas.enabled;
        }
    }

    // Only runs if the panel was left switched on in the scene. Make sure it
    // starts closed, otherwise the first button press would "close" a panel
    // you can't see, and you'd need a second press to open it.
    protected virtual void Awake()
    {
        if (!AllPanels.Contains(this)) AllPanels.Add(this);

        // (Awake also runs the first time Show() switches the panel on -
        // don't hide it straight away in that case.)
        if (startHidden && !showing) gameObject.SetActive(false);
    }

    private bool showing;

    // Panels normally start inactive, so Awake may not run until first shown -
    // they are also registered in Show, and removed here.
    protected virtual void OnDestroy()
    {
        AllPanels.Remove(this);
    }

    public void Show()
    {
        if (!AllPanels.Contains(this)) AllPanels.Add(this);

        // Only one panel at a time.
        foreach (TourPanel other in AllPanels.ToArray())
        {
            if (other != null && other != this && other.IsOpen) other.Hide();
        }

        if (!initialised)
        {
            initialised = true;
            OnFirstShow();
        }

        CanvasCameraAssigner assigner = GetComponent<CanvasCameraAssigner>();
        bool headLocked = assigner != null && assigner.headLock;

        if (headLocked)
        {
            assigner.headLockDistance = headLockedDistance;

            // Already attached to the camera from an earlier open: reset its
            // spot. (The first time, the assigner attaches it in its Start.)
            Camera cam = Camera.main;
            if (cam != null && transform.parent == cam.transform)
            {
                transform.localPosition = new Vector3(0f, heightOffset, headLockedDistance);
                transform.localRotation = Quaternion.identity;
            }
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            // If the Canvas COMPONENT was unticked (instead of the GameObject),
            // nothing draws even though the panel is "open" - turn it back on.
            canvas.enabled = true;
            if (canvas.worldCamera == null) canvas.worldCamera = Camera.main;
            canvas.sortingOrder = 10;
        }

        showing = true;
        gameObject.SetActive(true);
        showing = false;
        if (!headLocked) PlaceInFrontOfPlayer();

        SetTourButtonsVisible(false);
        OnShown();
        Debug.Log(GetType().Name + ": opened" + (headLocked ? " (head-locked)." : "."));
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        SetTourButtonsVisible(true);
        Debug.Log(GetType().Name + ": closed.");
    }

    public void Toggle()
    {
        if (IsOpen) Hide();
        else Show();
    }

    /// <summary>Find a panel of this type in the scene, even while hidden.</summary>
    public static T Find<T>() where T : TourPanel
    {
        return Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
    }

    /// <summary>Close whichever panel is open (if any).</summary>
    public static void CloseAll()
    {
        foreach (TourPanel panel in Object.FindObjectsByType<TourPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (panel.IsOpen) panel.Hide();
        }
    }

    /// <summary>Runs once, the first time the panel opens.</summary>
    protected virtual void OnFirstShow() { }

    /// <summary>Runs every time the panel opens.</summary>
    protected virtual void OnShown() { }

    private void SetTourButtonsVisible(bool visible)
    {
        if (!hideTourButtonsWhileOpen) return;

        if (tourButtons == null && !string.IsNullOrEmpty(tourButtonsName))
        {
            tourButtons = GameObject.Find(tourButtonsName); // finds it even when attached to the camera
        }
        if (tourButtons != null && tourButtons != gameObject) tourButtons.SetActive(visible);
    }

    private void PlaceInFrontOfPlayer()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning(GetType().Name + ": no Main Camera found - can't place the panel. Start from MainMenu so the XR rig carries over.");
            return;
        }

        // Keep it level: use only the horizontal part of where the player looks.
        Vector3 forward = cam.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) forward = cam.transform.up; // looking straight up/down
        forward.y = 0f;
        forward.Normalize();

        Vector3 position = cam.transform.position + forward * distance + Vector3.up * heightOffset;

        // Not parented to the camera, so it stays put while the player looks around.
        if (transform.parent == cam.transform) transform.SetParent(null, true);
        transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, Vector3.up));
    }
}
