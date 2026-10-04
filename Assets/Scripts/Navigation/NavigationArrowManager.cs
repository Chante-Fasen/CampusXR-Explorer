using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Builds the Street View style navigation arrows for whichever site the
/// player is currently at, using the "links" list for that location in
/// campus_data.json.
///
/// SETUP (Tour360):
///   1. Add this component to the "Managers" object (same one that has
///      SiteArrivalHandler). The Site Arrival Handler field fills itself in.
///   2. That's it - leave "Arrow Prefab" empty to use the built-in arrow,
///      or drag in your own prefab (see NavigationArrow.cs).
///
/// HOW IT WORKS:
///   - Every time the current site changes (SceneDataLoader.LocationChanged)
///     the old arrows are removed and one arrow is created per link.
///   - Each arrow is placed around the player using the link's "yaw":
///     0 = straight ahead (the photo's forward direction), 90 = right,
///     180 = behind, 270 = left. If a site sphere is rotated in Unity, its
///     Y rotation is added so the arrows still line up with the photo.
///   - Clicking an arrow switches to the target site in this scene via
///     SiteArrivalHandler (no scene load). If the target site lives in a
///     different scene, it loads that scene via SceneTransitionManager.
/// </summary>
public class NavigationArrowManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Leave empty to find it automatically in this scene.")]
    public SiteArrivalHandler siteArrivalHandler;

    [Tooltip("Optional custom arrow prefab. Leave empty to use the built-in arrow.")]
    public GameObject arrowPrefab;

    [Header("Placement")]
    [Tooltip("How far from the player (horizontally) each arrow sits, in metres. Keep it smaller than the photo sphere's radius (5 m for a sphere scaled 10).")]
    public float distance = 2.0f;

    [Tooltip("How far below the player's eyes the arrows sit, in metres.")]
    public float heightBelowEyes = 1.0f;

    [Tooltip("How much the arrows lean back towards the floor. 90 = lying flat, 0 = standing upright.")]
    [Range(0f, 90f)]
    public float tilt = 60f;

    [Header("Built-in arrow look")]
    [Tooltip("Width of the built-in arrow in metres.")]
    public float arrowSize = 0.45f;
    public Color arrowColor = new Color(1f, 1f, 1f, 0.9f);
    public Color arrowHoverColor = new Color(0.3f, 0.6f, 1f, 1f);

    private readonly List<GameObject> spawnedArrows = new List<GameObject>();
    private static Sprite arrowSprite;

    private void Awake()
    {
        if (siteArrivalHandler == null)
        {
            siteArrivalHandler = FindAnyObjectByType<SiteArrivalHandler>();
        }
    }

    private void OnEnable()
    {
        SceneDataLoader.LocationChanged += BuildArrows;

        if (SceneDataLoader.CurrentLocation != null)
        {
            BuildArrows(SceneDataLoader.CurrentLocation);
        }
    }

    private void OnDisable()
    {
        SceneDataLoader.LocationChanged -= BuildArrows;
        ClearArrows();
    }

    // ---------------------------------------------------------------------
    // Building arrows
    // ---------------------------------------------------------------------

    private void BuildArrows(LocationData location)
    {
        ClearArrows();

        if (location == null || location.links == null || location.links.Count == 0) return;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("NavigationArrowManager: no Main Camera found, can't place arrows. Start from MainMenu so the XR rig carries over.");
            return;
        }

        // Centre the arrows on the active site's sphere (where the photo is
        // taken from) at a height just below the player's eyes.
        Transform siteTransform = GetSiteTransform(location.id);
        Vector3 centre = siteTransform != null ? siteTransform.position : cam.transform.position;
        centre.y = cam.transform.position.y - heightBelowEyes;

        float siteYaw = siteTransform != null ? siteTransform.eulerAngles.y : 0f;

        foreach (LinkData link in location.links)
        {
            if (link == null || string.IsNullOrEmpty(link.target_id)) continue;

            Vector3 direction = Quaternion.Euler(0f, siteYaw + link.yaw, 0f) * Vector3.forward;
            Vector3 position = centre + direction * distance;

            // Face away from the player, then lean back towards the floor so
            // the arrow is readable from where the player is standing.
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(tilt, 0f, 0f);

            GameObject arrowObject = arrowPrefab != null
                ? Instantiate(arrowPrefab, position, rotation, transform)
                : CreateBuiltInArrow(position, rotation);

            arrowObject.name = "NavArrow → " + link.target_id;

            NavigationArrow arrow = arrowObject.GetComponent<NavigationArrow>();
            if (arrow == null) arrow = arrowObject.AddComponent<NavigationArrow>();
            arrow.Setup(this, link);

            spawnedArrows.Add(arrowObject);
        }
    }

    private void ClearArrows()
    {
        foreach (GameObject arrow in spawnedArrows)
        {
            if (arrow != null) Destroy(arrow);
        }
        spawnedArrows.Clear();
    }

    private Transform GetSiteTransform(string siteId)
    {
        if (siteArrivalHandler == null) return null;

        foreach (SiteArrivalHandler.SiteEntry entry in siteArrivalHandler.sites)
        {
            if (entry.siteId == siteId && entry.siteObject != null)
            {
                return entry.siteObject.transform;
            }
        }
        return null;
    }

    // ---------------------------------------------------------------------
    // Navigating
    // ---------------------------------------------------------------------

    public void Navigate(LinkData link)
    {
        string targetId = link.target_id;

        // Target site is in this scene - just switch the photo (instant).
        if (GetSiteTransform(targetId) != null)
        {
            siteArrivalHandler.ActivateSite(targetId);
            return;
        }

        // Target site is in another scene - load that scene and land on it.
        LocationData target = SceneDataLoader.FindLocationById(targetId);
        bool otherScene = target != null
                          && !string.IsNullOrEmpty(target.scene_name)
                          && target.scene_name != SceneManager.GetActiveScene().name;

        if (otherScene && SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.GoToSite(target.scene_name, targetId);
            return;
        }

        Debug.LogWarning($"NavigationArrowManager: can't navigate to '{targetId}' - it isn't in this scene's SiteArrivalHandler list or in campus_data.json with a scene_name.");
    }

    // ---------------------------------------------------------------------
    // Built-in arrow (used when no prefab is assigned)
    // ---------------------------------------------------------------------

    private GameObject CreateBuiltInArrow(Vector3 position, Quaternion rotation)
    {
        const float canvasWidth = 200f;   // UI units
        const float arrowHeight = 200f;
        const float labelHeight = 60f;

        GameObject root = new GameObject("NavArrow", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        root.transform.SetPositionAndRotation(position, rotation);
        root.transform.localScale = Vector3.one * (arrowSize / canvasWidth);

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        root.AddComponent<GraphicRaycaster>();              // mouse / Editor
        root.AddComponent<TrackedDeviceGraphicRaycaster>(); // VR controller rays

        RectTransform rootRect = (RectTransform)root.transform;
        rootRect.sizeDelta = new Vector2(canvasWidth, arrowHeight + labelHeight);

        // Arrow image (the clickable part)
        GameObject arrowImageObject = new GameObject("Arrow", typeof(RectTransform));
        arrowImageObject.transform.SetParent(root.transform, false);
        RectTransform arrowRect = (RectTransform)arrowImageObject.transform;
        arrowRect.anchorMin = new Vector2(0f, 1f);
        arrowRect.anchorMax = new Vector2(1f, 1f);
        arrowRect.pivot = new Vector2(0.5f, 1f);
        arrowRect.sizeDelta = new Vector2(0f, arrowHeight);
        arrowRect.anchoredPosition = Vector2.zero;

        Image image = arrowImageObject.AddComponent<Image>();
        image.sprite = GetArrowSprite();
        image.color = Color.white;
        image.raycastTarget = true;

        Button button = arrowImageObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = arrowColor;
        colors.highlightedColor = arrowHoverColor;
        colors.selectedColor = arrowColor;
        colors.pressedColor = arrowHoverColor * 0.8f;
        colors.colorMultiplier = 1f;
        button.colors = colors;

        // Label underneath (nearer the player)
        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(root.transform, false);
        RectTransform labelRect = (RectTransform)labelObject.transform;
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.sizeDelta = new Vector2(80f, labelHeight); // a bit wider than the arrow
        labelRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = 10f;
        label.fontSizeMax = 40f;
        label.color = Color.white;
        label.raycastTarget = false;

        return root;
    }

    // Draws a simple white "up" arrow (triangle head + shaft) once and reuses it.
    private static Sprite GetArrowSprite()
    {
        if (arrowSprite != null) return arrowSprite;

        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color clear = new Color(1f, 1f, 1f, 0f);
        Color solid = Color.white;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size;  // 0..1 left to right
                float v = (y + 0.5f) / size;  // 0..1 bottom to top
                bool inside;

                if (v >= 0.5f)
                {
                    // Head: triangle from full width at v=0.5 to a point at v=0.98
                    float halfWidth = Mathf.Lerp(0.48f, 0f, (v - 0.5f) / 0.48f);
                    inside = v <= 0.98f && Mathf.Abs(u - 0.5f) <= halfWidth;
                }
                else
                {
                    // Shaft
                    inside = v >= 0.05f && Mathf.Abs(u - 0.5f) <= 0.15f;
                }

                texture.SetPixel(x, y, inside ? solid : clear);
            }
        }

        texture.Apply();
        arrowSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return arrowSprite;
    }
}
