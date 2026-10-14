using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

/// <summary>
/// Attach this to the ROOT "XR Origin (VR)" object in your FIRST scene only
/// (MainMenu). It makes the whole rig (camera, controllers, everything
/// under it) survive scene loads, so BuildingMap, Tour360, etc. do NOT
/// need their own XR Origin - the same one carries over automatically.
///
/// Going BACK to MainMenu loads MainMenu's own copy of the rig again. That
/// copy is removed straight away, but it must be switched off first:
///   - its InputActionManager turns the controller actions OFF when it is
///     destroyed - and those actions are shared with the real rig, so the
///     controllers would freeze;
///   - its camera is also tagged MainCamera, so canvases could attach to the
///     wrong (about-to-be-deleted) camera.
/// Runs before other scripts (DefaultExecutionOrder) so the copy is switched
/// off as early as possible, and the real rig's input is turned back on.
/// </summary>
[DefaultExecutionOrder(-10000)]
public class PersistentXROrigin : MonoBehaviour
{
    public static PersistentXROrigin Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Duplicate from a reloaded MainMenu: switch it off immediately,
            // delete it, and make sure the real rig's controls stay on.
            gameObject.SetActive(false);
            Destroy(gameObject);
            Instance.ReEnableInput();
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ReEnableInput()
    {
        StartCoroutine(ReEnableInputNextFrame());
    }

    private IEnumerator ReEnableInputNextFrame()
    {
        // Wait until the duplicate has actually been destroyed (its
        // OnDisable may have switched the shared input actions off).
        yield return null;

        foreach (InputActionManager manager in GetComponentsInChildren<InputActionManager>(true))
        {
            manager.EnableInput();
        }
    }
}
