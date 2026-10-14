using UnityEngine;

/// <summary>
/// In-tour Building Map panel (Tour360 "CanvasMap") - the building map pops
/// up in front of the player over the 360 photo, like the Settings panel.
///
/// SETUP (Tour360):
///   1. Copy CanvasBuidlingMap from the BuildingMap scene into Tour360,
///      rename it "CanvasMap" and add this component (canvas setup: see
///      TourPanel.cs - World Space, raycasters, Canvas Camera Assigner,
///      top checkbox unticked).
///   2. The site buttons keep their SiteRedirectButton (Target Scene Tour360
///      + Target Site Id). Inside Tour360 they now switch the photo
///      instantly and close the map.
///   3. The tour's "Map" button: SettingsButton -> Open Map.
///      Add a close button on the map: SettingsButton -> Close Panel.
///
/// The ground floor is shown each time the map opens, if the canvas has a
/// BuildingMapPanelSwitcher.
/// </summary>
public class MapPanel : TourPanel
{
    [Tooltip("Show the ground floor every time the map opens.")]
    public bool openOnGroundFloor = true;

    protected override void OnShown()
    {
        if (!openOnGroundFloor) return;

        BuildingMapPanelSwitcher switcher = GetComponentInChildren<BuildingMapPanelSwitcher>(true);
        if (switcher != null && switcher.panelGroundFloor != null && switcher.panelTopFloor != null)
        {
            switcher.ShowGroundFloor();
        }
    }
}
