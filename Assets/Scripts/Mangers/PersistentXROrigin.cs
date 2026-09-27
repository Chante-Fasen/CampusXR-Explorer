using UnityEngine;

/// <summary>
/// Attach this to the ROOT "XR Origin (VR)" object in your FIRST scene only
/// (MainMenu). It makes the whole rig (camera, controllers, everything
/// under it) survive scene loads, so BuildingMap, Tour360, etc. do NOT
/// need their own XR Origin - the same one carries over automatically.
/// </summary>
public class PersistentXROrigin : MonoBehaviour
{
    public static PersistentXROrigin Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // A second copy tried to load in some other scene - destroy it,
            // since we already have the original one carrying over.
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
