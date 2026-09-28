using UnityEngine;

// Resolves a location id from campus_data.json to its Unity scene and
// hands it to the transition system. Map buttons call
// GoToLocation("computer_lab") with the id from the JSON.
public class LocationNavigator : MonoBehaviour
{
    public static LocationNavigator Instance;

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

    public void GoToLocation(string locationId)
    {
        if (!JsonLoader.IsLoaded)
        {
            Debug.LogError("Campus data not loaded yet, cannot navigate.");
            return;
        }

        LocationData target = FindLocation(locationId);

        if (target == null)
        {
            Debug.LogError("No location in campus_data.json with id: " + locationId);
            return;
        }

        if (string.IsNullOrEmpty(target.scene_name))
        {
            Debug.LogError("Location '" + locationId + "' has no scene_name set.");
            return;
        }

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("SceneTransitionManager not present in scene.");
            return;
        }

        Debug.Log("Navigating to " + target.label + " -> scene " + target.scene_name);
        SceneTransitionManager.Instance.GoToScene(target.scene_name);
    }

    public LocationData FindLocation(string locationId)
    {
        foreach (LocationData location in JsonLoader.LoadedData.locations)
        {
            if (location.id == locationId) return location;
        }
        return null;
    }
}
