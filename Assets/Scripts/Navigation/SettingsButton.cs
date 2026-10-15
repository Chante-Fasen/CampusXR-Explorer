using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Settings navigation.
///
/// Preferred: a SettingsPanel (in-tour canvas) in the current scene - opening
/// and closing just shows/hides it, so the 360 photo stays visible and the
/// player stays on the same site.
///
/// Fallback (no SettingsPanel in the scene): load the SettingsHelp scene and
/// remember the scene/site so Return goes back to the same place.
/// </summary>
public static class SettingsNavigator
{
    public const string SettingsSceneName = "SettingsHelp";
    public const string DefaultReturnScene = "BuildingMap";

    public static string ReturnScene { get; private set; } = "";
    public static string ReturnSiteId { get; private set; } = "";

    private static SettingsPanel FindPanel()
    {
        return Object.FindAnyObjectByType<SettingsPanel>(FindObjectsInactive.Include);
    }

    public static void OpenSettings()
    {
        SettingsPanel panel = FindPanel();
        if (panel != null)
        {
            panel.Show();
            return;
        }

        string current = SceneManager.GetActiveScene().name;
        if (current == SettingsSceneName) return;

        ReturnScene = current;
        ReturnSiteId = SceneDataLoader.CurrentLocation != null ? SceneDataLoader.CurrentLocation.id : "";
        SceneManager.LoadScene(SettingsSceneName);
    }

    public static void ReturnFromSettings()
    {
        SettingsPanel panel = FindPanel();
        if (panel != null)
        {
            panel.Hide();
            return;
        }

        string scene = string.IsNullOrEmpty(ReturnScene) ? DefaultReturnScene : ReturnScene;
        string site = ReturnSiteId;
        ReturnScene = "";
        ReturnSiteId = "";

        if (!string.IsNullOrEmpty(site) && SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.GoToSite(scene, site);
        }
        else
        {
            SceneManager.LoadScene(scene);
        }
    }
}

/// <summary>
/// Put this on a Settings-related button. It connects the button itself
/// (no OnClick setup needed) and switches off whatever the button did before,
/// e.g. a copied "Back to Map" OnClick.
///
///   Open Settings        - the tour's Settings button (toggles the panel)
///   Return From Settings - the panel's "Return" button
///   Main Menu            - same visitor: back to the Selection screen
///                          (Guided Tour / Free Roam), consent is kept
///   New Visitor          - hand the headset to someone new: back to the
///                          Start screen, their consent is cleared (so the
///                          next person is asked) and the volume goes back
///                          to 100%
///   Open Map             - the tour's Map button (toggles the map panel;
///                          loads the BuildingMap scene if there's no
///                          MapPanel in this scene)
///   Close Panel          - closes the panel this button sits in (e.g. a
///                          close/back button on the map)
/// </summary>
[RequireComponent(typeof(Button))]
public class SettingsButton : MonoBehaviour
{
    // Keep this order - Unity saves the choice as a number.
    public enum Action { OpenSettings, ReturnFromSettings, MainMenu, NewVisitor, OpenMap, ClosePanel }

    public Action action = Action.OpenSettings;

    [Tooltip("Used by Main Menu and New Visitor.")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("Used by Open Map when there's no MapPanel in the scene.")]
    public string buildingMapSceneName = "BuildingMap";

    private void Awake()
    {
        Button button = GetComponent<Button>();

        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            button.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Run);
    }

    // One physical press can arrive twice (e.g. controller + simulator mouse),
    // which would open and instantly close the panel. Ignore repeats.
    private static float lastPressTime = -10f;
    private const float RepeatGuardSeconds = 0.3f;

    public void Run()
    {
        if (Time.unscaledTime - lastPressTime < RepeatGuardSeconds) return;
        lastPressTime = Time.unscaledTime;

        switch (action)
        {
            case Action.OpenSettings:
                // Pressing Settings again while it's open closes it.
                SettingsPanel panel = Object.FindAnyObjectByType<SettingsPanel>(FindObjectsInactive.Include);
                if (panel != null && panel.IsOpen) panel.Hide();
                else SettingsNavigator.OpenSettings();
                break;

            case Action.ReturnFromSettings:
                SettingsNavigator.ReturnFromSettings();
                break;

            case Action.MainMenu:
                MainMenuFlow.OpenAtSelection = true;
                SceneManager.LoadScene(mainMenuSceneName);
                break;

            case Action.NewVisitor:
                ConsentManager.ResetConsent();
                SettingsPanel.ResetVolume();   // next visitor starts at full volume
                MainMenuFlow.OpenAtSelection = false;
                SceneManager.LoadScene(mainMenuSceneName);
                break;

            case Action.OpenMap:
                MapPanel map = TourPanel.Find<MapPanel>();
                if (map == null)
                {
                    // Map canvas copied into this scene but the Map Panel
                    // component wasn't added - add it now.
                    BuildingMapPanelSwitcher mapCanvas = Object.FindAnyObjectByType<BuildingMapPanelSwitcher>(FindObjectsInactive.Include);
                    if (mapCanvas != null && mapCanvas.gameObject.scene == gameObject.scene)
                    {
                        map = mapCanvas.gameObject.AddComponent<MapPanel>();
                        Debug.Log("SettingsButton: added Map Panel to '" + mapCanvas.name + "' - add it in the Editor too so you can adjust its settings.");
                    }
                }
                if (map != null) map.Toggle();
                else SceneManager.LoadScene(buildingMapSceneName);
                break;

            case Action.ClosePanel:
                TourPanel parentPanel = GetComponentInParent<TourPanel>(true);
                if (parentPanel != null) parentPanel.Hide();
                else TourPanel.CloseAll();
                break;
        }
    }
}
