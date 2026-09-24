using UnityEngine;

/// <summary>
/// Attach this to CanvasBuildingMap (or any object in the BuildingMap scene).
/// Drag PanelGroundFloor and PanelTopFloor into the fields, then wire
/// ButtonGroundFloor's OnClick to ShowGroundFloor() and ButtonTopFloor's
/// OnClick to ShowTopFloor().
/// </summary>
public class BuildingMapPanelSwitcher : MonoBehaviour
{
    [Tooltip("Drag PanelGroundFloor here.")]
    public GameObject panelGroundFloor;

    [Tooltip("Drag PanelTopFloor here.")]
    public GameObject panelTopFloor;

    // Hook to ButtonGroundFloor's OnClick.
    public void ShowGroundFloor()
    {
        panelGroundFloor.SetActive(true);
        panelTopFloor.SetActive(false);
    }

    // Hook to ButtonTopFloor's OnClick.
    public void ShowTopFloor()
    {
        panelGroundFloor.SetActive(false);
        panelTopFloor.SetActive(true);
    }
}
