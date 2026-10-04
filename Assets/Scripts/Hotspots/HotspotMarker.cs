using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One clickable hotspot marker (the floating "i" icon). Created at runtime
/// by HotspotManager - you don't place these by hand.
///
/// If you make your own marker prefab instead of using the built-in one, put
/// this script on the prefab's root and make sure the prefab contains a
/// world-space Canvas with a UI Button. The manager hooks the Button up
/// automatically.
/// </summary>
public class HotspotMarker : MonoBehaviour
{
    public HotspotData Hotspot { get; private set; }

    private HotspotManager manager;

    public void Setup(HotspotManager owner, HotspotData hotspot)
    {
        manager = owner;
        Hotspot = hotspot;

        Button button = GetComponentInChildren<Button>(true);
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }
        else
        {
            Debug.LogWarning($"HotspotMarker '{name}' has no UI Button, so it can't be clicked.");
        }

        foreach (Canvas canvas in GetComponentsInChildren<Canvas>(true))
        {
            if (canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }
        }
    }

    // Hooked to the Button's OnClick.
    public void OnClicked()
    {
        if (manager != null && Hotspot != null)
        {
            manager.OpenHotspot(Hotspot);
        }
    }
}
