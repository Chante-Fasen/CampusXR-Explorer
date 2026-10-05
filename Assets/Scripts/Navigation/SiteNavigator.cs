using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One place for "take the player to site X", used by the navigation arrows
/// and by the hotspot popup's "Enter" button.
///
///   - If the site is in the current scene's SiteArrivalHandler list, it just
///     switches the photo (instant, no scene load).
///   - Otherwise, if campus_data.json says the site lives in another scene,
///     it loads that scene and lands on the site via SceneTransitionManager.
/// </summary>
public static class SiteNavigator
{
    public static bool GoTo(string siteId)
    {
        if (string.IsNullOrEmpty(siteId)) return false;

        // Same scene - just switch the photo.
        SiteArrivalHandler handler = Object.FindAnyObjectByType<SiteArrivalHandler>();
        if (handler != null && handler.HasSite(siteId))
        {
            handler.ActivateSite(siteId);
            return true;
        }

        // Another scene - load it and land on the site.
        LocationData target = SceneDataLoader.FindLocationById(siteId);
        bool otherScene = target != null
                          && !string.IsNullOrEmpty(target.scene_name)
                          && target.scene_name != SceneManager.GetActiveScene().name;

        if (otherScene && SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.GoToSite(target.scene_name, siteId);
            return true;
        }

        Debug.LogWarning($"SiteNavigator: can't go to '{siteId}' - it isn't in this scene's SiteArrivalHandler list or in campus_data.json with a scene_name.");
        return false;
    }
}
