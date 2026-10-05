using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Keeps track of which location the player is currently at and gives
// other scripts (hotspots, navigation arrows, UI) its data from
// campus_data.json.
//
// A location becomes "current" in one of two ways:
//
//  1. Site-based (Tour360): SiteArrivalHandler calls
//     SceneDataLoader.SetCurrentSite("AlphaSite") every time it shows a
//     site, including when the player moves between sites with the
//     navigation arrows WITHOUT changing scene. Matches LocationData.id.
//     Nothing needs to be placed in the scene for this to work.
//
//  2. Scene-based (fallback): if a scene contains a SceneDataLoader
//     component and no site was set, it matches LocationData.scene_name
//     to the active scene's name (the original behaviour).
//
// IMPORTANT for other scripts: the current location can change while the
// scene stays the same, so don't just read CurrentLocation once in Start.
// Subscribe to LocationChanged in OnEnable and unsubscribe in OnDisable:
//
//     void OnEnable()  { SceneDataLoader.LocationChanged += Show; if (SceneDataLoader.CurrentLocation != null) Show(SceneDataLoader.CurrentLocation); }
//     void OnDisable() { SceneDataLoader.LocationChanged -= Show; }
//     void Show(LocationData location) { /* build hotspots / arrows */ }

public class SceneDataLoader : MonoBehaviour
{
    public static LocationData CurrentLocation { get; private set; }

    // Fired whenever the current location changes. Can be null if a site
    // id or scene had no matching entry in campus_data.json.
    public static event Action<LocationData> LocationChanged;

    // Site id waiting to be applied once campus_data.json has loaded.
    private static string pendingSiteId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        CurrentLocation = null;
        pendingSiteId = null;
        LocationChanged = null;

        JsonLoader.DataLoaded -= OnDataLoaded;
        JsonLoader.DataLoaded += OnDataLoaded;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Forget the old location when a new scene replaces the current one,
    // so e.g. BuildingMap doesn't still think you're standing in Alpha.
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
        {
            CurrentLocation = null;
            pendingSiteId = null;
        }
    }

    /// <summary>
    /// Makes the location with this id the current one. Safe to call before
    /// campus_data.json has finished loading - it will be applied as soon
    /// as the data is ready.
    /// </summary>
    public static void SetCurrentSite(string siteId)
    {
        if (string.IsNullOrEmpty(siteId)) return;

        pendingSiteId = siteId;

        if (JsonLoader.LoadedData != null)
        {
            ApplyPendingSite();
        }
    }

    /// <summary>
    /// Looks up any location by id (e.g. to get the label of the site a
    /// navigation arrow points to). Returns null if not found or if the
    /// data hasn't loaded yet.
    /// </summary>
    public static LocationData FindLocationById(string id)
    {
        return FindLocation(location => location.id == id);
    }

    private static void OnDataLoaded()
    {
        if (!string.IsNullOrEmpty(pendingSiteId))
        {
            ApplyPendingSite();
        }
    }

    private static void ApplyPendingSite()
    {
        string siteId = pendingSiteId;
        pendingSiteId = null;

        LocationData location = FindLocationById(siteId);

        if (location == null)
        {
            Debug.LogWarning($"SceneDataLoader: no location with id '{siteId}' in campus_data.json.");
        }

        SetCurrent(location);
    }

    private static LocationData FindLocation(Predicate<LocationData> match)
    {
        if (JsonLoader.LoadedData == null || JsonLoader.LoadedData.locations == null) return null;

        foreach (LocationData location in JsonLoader.LoadedData.locations)
        {
            if (location != null && match(location)) return location;
        }

        return null;
    }

    private static void SetCurrent(LocationData location)
    {
        CurrentLocation = location;

        if (location != null)
        {
            int hotspotCount = location.hotspots != null ? location.hotspots.Count : 0;
            int linkCount = location.links != null ? location.links.Count : 0;
            Debug.Log($"Location data found for: {location.label} ({hotspotCount} hotspots, {linkCount} links)");
        }

        LocationChanged?.Invoke(location);
    }

    // ---- Scene-based fallback (only runs if this component is in a scene) ----

    private void Start()
    {
        StartCoroutine(MatchActiveSceneWhenReady());
    }

    private IEnumerator MatchActiveSceneWhenReady()
    {
        while (JsonLoader.LoadedData == null)
        {
            yield return null;
        }

        // A site has already been chosen (e.g. by SiteArrivalHandler), so
        // don't overwrite it with a scene-name match.
        if (CurrentLocation != null || !string.IsNullOrEmpty(pendingSiteId))
        {
            yield break;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        LocationData location = FindLocation(l => l.scene_name == currentSceneName);

        if (location == null)
        {
            Debug.LogWarning("SceneDataLoader: no location found in JSON for scene: " + currentSceneName);
        }

        SetCurrent(location);
    }
}
