using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One clickable navigation arrow (Street View style). Created at runtime by
/// NavigationArrowManager - you don't place these by hand.
///
/// If you make your own arrow prefab instead of using the built-in one, put
/// this script on the prefab's root and make sure the prefab contains a
/// world-space Canvas with a UI Button (and optionally a TextMeshPro text
/// for the label). The manager hooks the Button up automatically.
/// </summary>
public class NavigationArrow : MonoBehaviour
{
    public LinkData Link { get; private set; }

    private NavigationArrowManager manager;

    public void Setup(NavigationArrowManager owner, LinkData link)
    {
        manager = owner;
        Link = link;

        Button button = GetComponentInChildren<Button>(true);
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }
        else
        {
            Debug.LogWarning($"NavigationArrow '{name}' has no UI Button, so it can't be clicked.");
        }

        TMP_Text labelText = GetComponentInChildren<TMP_Text>(true);
        if (labelText != null)
        {
            bool hasLabel = !string.IsNullOrEmpty(link.label);
            labelText.text = hasLabel ? link.label : "";
            labelText.gameObject.SetActive(hasLabel);
        }

        // World-space canvases need the camera to receive clicks.
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
        if (manager != null && Link != null)
        {
            manager.Navigate(Link);
        }
    }
}
