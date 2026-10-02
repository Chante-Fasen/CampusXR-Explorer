using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Finds the LocationData that matches the currently active scene,
// so hotspot scripts and other components can pull the right data
// without needing to know how JSON parsing works.
//
// Uses a coroutine to wait for JsonLoader to finish, since JsonLoader
// now loads asynchronously (required for Android/Quest compatibility).

public class SceneDataLoader : MonoBehaviour
{
    public static LocationData CurrentLocation;

    void Start()
    {
        StartCoroutine(WaitForDataThenFindLocation());
    }

    IEnumerator WaitForDataThenFindLocation()
    {
        // Wait until JsonLoader has finished loading data.
        // This loops once per frame until LoadedData is no longer null.
        while (JsonLoader.LoadedData == null)
        {
            yield return null;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
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