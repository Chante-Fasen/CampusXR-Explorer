using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to a "Back to Map" button that should stay on screen no
/// matter which site (Alpha/Gamma/Reception) is currently active. Wire the
/// button's OnClick to BackToBuildingMap().
/// </summary>
public class BackToBuildingMapButton : MonoBehaviour
{
    [Tooltip("Exact name of the building map scene to return to.")]
    public string buildingMapSceneName = "BuildingMap";

    public void BackToBuildingMap()
    {
        SceneManager.LoadScene(buildingMapSceneName);
    }
}
