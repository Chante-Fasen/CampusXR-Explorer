using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Put this on an empty GameObject in EVERY destination scene (the scenes
/// you redirect INTO). On Start, it checks SceneTransitionManager for a
/// requested site id, then ACTIVATES that site's GameObject (e.g. a 360
/// panorama sphere) and DEACTIVATES all the others in the list. This suits
/// tours where each site is a toggled GameObject rather than a position the
/// camera walks to.
/// </summary>
public class SiteArrivalHandler : MonoBehaviour
{
    [System.Serializable]
    public class SiteEntry
    {
        public string siteId;
        public GameObject siteObject; // e.g. ReceptionSite, GammaSite, AlphaSite
    }

    [Tooltip("List every site in this scene the player could be sent to.")]
    public List<SiteEntry> sites = new List<SiteEntry>();

    [Tooltip("If true, the first entry in the list is shown by default when no site was requested.")]
    public bool defaultToFirstSite = true;

    private void Start()
    {
        string requestedId = "";

        if (SceneTransitionManager.Instance != null)
        {
            requestedId = SceneTransitionManager.Instance.TargetSiteId;
        }

        if (string.IsNullOrEmpty(requestedId) && defaultToFirstSite && sites.Count > 0)
        {
            requestedId = sites[0].siteId;
        }

        ActivateSite(requestedId);

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.ClearTarget();
        }
    }

    /// <summary>
    /// Activates the site whose siteId matches, deactivates every other
    /// site in the list. Also callable directly (e.g. from in-scene
    /// navigation buttons that don't need a scene change).
    /// </summary>
    public void ActivateSite(string siteId)
    {
        bool found = false;

        foreach (SiteEntry entry in sites)
        {
            if (entry.siteObject == null) continue;

            bool isMatch = entry.siteId == siteId;
            entry.siteObject.SetActive(isMatch);

            if (isMatch) found = true;
        }

        if (!found && !string.IsNullOrEmpty(siteId))
        {
            Debug.LogWarning($"Site id '{siteId}' not found in this scene's site list.");
        }

        if (found)
        {
            // Tell the data side which location we're now at, so hotspots and
            // navigation arrows for this site (from campus_data.json) can show.
            SceneDataLoader.SetCurrentSite(siteId);
        }
    }
}