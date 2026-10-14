using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to a map button that should send the player to a specific
/// site (e.g. ButtonAlpha -> Tour360 / AlphaEntrance).
///
/// Setup:
/// 1. Add this script to your button GameObject.
/// 2. "Target Scene Name": the scene the site lives in (e.g. Tour360). Must be
///    in File > Build Settings > Scenes In Build.
/// 3. "Target Site Id": the site id from campus_data.json (e.g. AlphaEntrance).
/// 4. On the Button's OnClick(), pick SiteRedirectButton > GoToTargetSite().
///
/// Behaviour:
///   - Map panel INSIDE Tour360: switches the photo instantly (no loading)
///     and closes the map panel.
///   - From another scene: loads the target scene (with the loading screen)
///     and lands on the site.
/// </summary>
public class SiteRedirectButton : MonoBehaviour
{
    [Tooltip("Exact name of the scene to load.")]
    public string targetSceneName;

    [Tooltip("Id of the site/hotspot to focus once the new scene loads. Leave blank if not needed.")]
    public string targetSiteId;

    // Hook this up to the Button's OnClick event in the Inspector.
    public void GoToTargetSite()
    {
        bool sameScene = string.IsNullOrEmpty(targetSceneName)
                         || targetSceneName == SceneManager.GetActiveScene().name;

        // Already in the right scene: switch the photo directly.
        if (sameScene && !string.IsNullOrEmpty(targetSiteId))
        {
            SiteArrivalHandler handler = FindAnyObjectByType<SiteArrivalHandler>();
            if (handler != null && handler.HasSite(targetSiteId))
            {
                handler.ActivateSite(targetSiteId);

                TourPanel panel = GetComponentInParent<TourPanel>(true);
                if (panel != null) panel.Hide();
                return;
            }
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"SiteRedirectButton on '{name}': no Target Scene Name and site '{targetSiteId}' isn't in this scene.");
            return;
        }

        SceneTransitionManager.GetOrCreate().GoToSite(targetSceneName, targetSiteId);
    }
}
