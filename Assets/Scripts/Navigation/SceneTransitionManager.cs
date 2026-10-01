using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent singleton that remembers which "site" (hotspot/location) the
/// player wants to land on after a scene change. Put this on an empty
/// GameObject in your FIRST scene only (e.g. a "Bootstrap" or "Managers"
/// object). It survives scene loads via DontDestroyOnLoad, so every other
/// scene can read TargetSiteId from it once loading finishes.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    // The id of the site/hotspot to focus on once the new scene loads.
    // Leave empty if you're just loading a scene with no specific target.
    public string TargetSiteId { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Call this from a button's OnClick to load a scene and, once there,
    /// jump straight to a specific site.
    /// </summary>
    /// <param name="sceneName">Exact name of the scene to load (must be in Build Settings).</param>
    /// <param name="siteId">Id of the site/hotspot to focus in that scene. Pass "" if not needed.</param>
    public void GoToSite(string sceneName, string siteId)
    {
        TargetSiteId = siteId;
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Call this once you've used TargetSiteId in the new scene, so a later
    /// plain scene load doesn't accidentally reuse a stale target.
    /// </summary>
    public void ClearTarget()
    {
        TargetSiteId = "";
    }
}
