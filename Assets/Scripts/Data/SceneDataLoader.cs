using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Finds the LocationData that matches the currently active scene,
// so hotspot scripts and other components can pull the right data
// without needing to know how JSON parsing works.
public class SceneDataLoader : MonoBehaviour
{
    // The location data matching this scene - other scripts
    // (like hotspot placement) will read from this
    public static LocationData CurrentLocation;

    void Start()
    {
        StartCoroutine(ResolveLocation());
    }

    IEnumerator ResolveLocation()
    {
        // JsonLoader now loads asynchronously (required for Android),
        // so wait for it to finish rather than assuming it already has.
        yield return new WaitUntil(() => JsonLoader.IsLoaded);

        // Get the name of the scene currently open
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Search through every location to find the one whose
        // scene_name matches the scene we're currently in
        CurrentLocation = null;

        foreach (LocationData location in JsonLoader.LoadedData.locations)
        {
            if (location.scene_name == currentSceneName)
            {
                CurrentLocation = location;
                break;
            }
        }

        if (CurrentLocation != null)
        {
            Debug.Log("Scene data found for: " + CurrentLocation.label +
                      " (" + CurrentLocation.hotspots.Count + " hotspots)");
        }
        else
        {
            Debug.LogWarning("No matching location found in JSON for scene: " + currentSceneName);
        }
    }
}