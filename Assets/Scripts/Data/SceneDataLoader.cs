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
        // Safety check - make sure JsonLoader has already run
        // and successfully loaded the campus data
        if (JsonLoader.LoadedData == null)
        {
            Debug.LogError("SceneDataLoader: campus data not loaded yet. Check JsonLoader ran first.");
            return;
        }

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
