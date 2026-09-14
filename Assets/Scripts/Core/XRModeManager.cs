using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

// Starts and stops the XR subsystems on demand so the same build runs
// as a flat Android app or as a VR app. XR is NOT initialised on startup;
// StartVR is called when the user chooses VR mode.
public class XRModeManager : MonoBehaviour
{
    public static XRModeManager Instance;
    public static bool IsVRActive { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Call this from the "Enter VR Mode" button.
    public IEnumerator StartVR()
    {
        if (IsVRActive) yield break;

        if (XRGeneralSettings.Instance == null || XRGeneralSettings.Instance.Manager == null)
        {
            Debug.LogError("XR Management is not configured for this platform.");
            yield break;
        }

        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            // No OpenXR runtime and no Cardboard - this device cannot do VR.
            Debug.LogWarning("No XR loader available on this device.");
            yield break;
        }

        XRGeneralSettings.Instance.Manager.StartSubsystems();
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        IsVRActive = true;

        Debug.Log("VR started with loader: " +
                  XRGeneralSettings.Instance.Manager.activeLoader.name);
    }

    // Call this from the "Exit VR" control.
    public void StopVR()
    {
        if (!IsVRActive) return;

        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Screen.orientation = ScreenOrientation.Portrait;
        IsVRActive = false;

        Debug.Log("VR stopped, returned to phone mode.");
    }

    // Use this to grey out the VR Mode button on phones with no gyroscope.
    public static bool DeviceSupportsVR()
    {
        return SystemInfo.supportsGyroscope;
    }
}