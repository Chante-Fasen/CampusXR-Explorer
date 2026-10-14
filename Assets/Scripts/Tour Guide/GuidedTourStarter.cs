using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Starts Enrique's guided tour automatically when the visitor picked
/// "Guided Tour" on the Selection screen (MainMenuFlow.SelectedMode).
///
/// No setup needed - it isn't a component, it runs by itself.
///
/// When a scene with a NavigationArrowManager loads in Guided Tour mode, it
/// waits for the first site to be shown (so the right photo is up and the
/// first voiceover matches it), then calls NavigationArrowManager.StartTour():
/// red arrows / hotspots show the way and TourVoiceover plays the clips.
/// In Free Roam nothing happens, so the tour stays off.
/// </summary>
public static class GuidedTourStarter
{
    private static NavigationArrowManager pendingManager;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        pendingManager = null;
        SceneDataLoader.LocationChanged -= OnLocationChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneDataLoader.LocationChanged -= OnLocationChanged;
        pendingManager = null;

        if (MainMenuFlow.SelectedMode != MainMenuFlow.TourMode.GuidedTour) return;

        NavigationArrowManager manager = Object.FindAnyObjectByType<NavigationArrowManager>();
        if (manager == null || manager.gameObject.scene != scene) return; // not a tour scene

        if (manager.tourGuide) return; // already running

        pendingManager = manager;
        SceneDataLoader.LocationChanged += OnLocationChanged;
    }

    // The first site of this scene has been shown - start the tour from here.
    private static void OnLocationChanged(LocationData location)
    {
        if (location == null) return;

        SceneDataLoader.LocationChanged -= OnLocationChanged;

        NavigationArrowManager manager = pendingManager;
        pendingManager = null;

        if (manager != null && !manager.tourGuide)
        {
            Debug.Log("GuidedTourStarter: guided tour started at " + location.id + ".");
            manager.StartTour();
        }
    }
}
