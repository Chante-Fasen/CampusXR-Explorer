using UnityEngine;

/// <summary>
/// Attach this to the button (or any object with a UI Button / VR-interactable
/// component) that should send the player to another scene AND straight to
/// a specific site/hotspot in it.
///
/// Setup:
/// 1. Add this script to your button GameObject.
/// 2. Fill in "Target Scene Name" (must match the scene name exactly,
///    and the scene must be added in File > Build Settings > Scenes In Build).
/// 3. Fill in "Target Site Id" — whatever id your hotspot system uses to
///    identify that site (e.g. "Library", "Cafeteria_01").
/// 4. On the Button component's OnClick() list, drag this GameObject in,
///    then pick SiteRedirectButton > GoToTargetSite().
///    (If you're using XR Interaction Toolkit, wire this method to the
///    Interactable's "Select Entered" event instead.)
/// </summary>
public class SiteRedirectButton : MonoBehaviour
{
    [Tooltip("Exact name of the scene to load.")]
    public string targetSceneName;

    [Tooltip("Id of the site/hotspot to focus once the new scene loads. Leave blank if not needed.")]
    public string targetSiteId;

    // Hook this up to the Button's OnClick event in the Inspector.
    public void GoToTargetSite()
    {
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("SceneTransitionManager not found. Make sure it exists in your first/bootstrap scene.");
            return;
        }

        SceneTransitionManager.Instance.GoToSite(targetSceneName, targetSiteId);
    }
}
