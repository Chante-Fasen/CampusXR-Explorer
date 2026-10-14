using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
///     0 = Unity's forward (+Z, the shared direction all photos are turned to face), 90 = right,
///     180 = behind, 270 = left - measured in the WORLD (Unity's +Z is 0),
///     after the photo spheres have been rotated to face the same way.
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

    [Header("Guided tour")]
    public bool tourGuide = false;
    private List<string> TourRoute => JsonLoader.LoadedData != null ? JsonLoader.LoadedData.tour_route : null;
    public Color guideColor = new Color(1f, 0.15f, 0.15f, 1f);
    public Color guideHoverColor = new Color(1f, 0.5f, 0.5f, 1f);
    private int tourIndex = 0;

    // Hotspot scripts read this to know which bubble to highlight.
    public static string GuidedHotspotId { get; private set; }
    public static event System.Action<string> GuideChanged;
    
    public static event System.Action TourStarted;
    public static event System.Action TourStopped;
    public static event System.Action TourFinished;
    private bool tourFinishedFired;

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
        GuidedHotspotId = null;
    }

    // ---------------------------------------------------------------------
    // Building arrows
    // ---------------------------------------------------------------------

    private void BuildArrows(LocationData location)
    {
        ClearArrows();

        // Work this out first so hotspots still get highlighted at sites with no arrows.
        Hop guidedHop = (tourGuide && location != null) ? GetNextGuidedHop(location) : null;
        string guidedTarget = (guidedHop != null && guidedHop.hotspotId == null) ? guidedHop.targetId : null;
        GuidedHotspotId = guidedHop != null ? guidedHop.hotspotId : null;
        GuideChanged?.Invoke(GuidedHotspotId);

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
        // With TourCentreFollower the site sphere sits at the player's eyes,
        // so measure from the sphere (arrows then move with the photo).
        // Without it, fall back to the camera's height.
        bool sphereFollowsPlayer = siteTransform != null && FindAnyObjectByType<TourCentreFollower>() != null;
        Vector3 centre = siteTransform != null ? siteTransform.position : cam.transform.position;
        centre.y = (sphereFollowsPlayer ? siteTransform.position.y : cam.transform.position.y) - heightBelowEyes;

        // yaw is measured in WORLD directions (the spheres are rotated so all
        // photos face the same real-world way), so the sphere's own rotation
        // is NOT added here.

        foreach (LinkData link in location.links)
        {
            if (link == null || string.IsNullOrEmpty(link.target_id)) continue;

            Vector3 direction = Quaternion.Euler(0f, link.yaw, 0f) * Vector3.forward;
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

            if (guidedTarget != null && link.target_id == guidedTarget)
                ApplyArrowColors(arrowObject, guideColor, guideHoverColor);

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
        SiteNavigator.GoTo(link.target_id);
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

    // ---------------------------------------------------------------------
    // Guided tour
    // ---------------------------------------------------------------------

    public void StartTour()
    {
        tourGuide = true;
        tourIndex = 0;
        tourFinishedFired = false;
        TourStarted?.Invoke();
        if (SceneDataLoader.CurrentLocation != null) BuildArrows(SceneDataLoader.CurrentLocation);
    }

    public void StopTour()
    {
        tourGuide = false;
        TourStopped?.Invoke();
        if (SceneDataLoader.CurrentLocation != null) BuildArrows(SceneDataLoader.CurrentLocation);
    }

    private class Hop
    {
        public string targetId;
        public string hotspotId;   // null = take an arrow, otherwise use this hotspot
    }

    private Hop GetNextGuidedHop(LocationData current)
    {
        List<string> route = TourRoute;
        if (route == null || route.Count == 0) return null;

        while (tourIndex < route.Count && route[tourIndex] == current.id)
            tourIndex++;

        if (tourIndex >= route.Count)
        {
            if (!tourFinishedFired)
            {
                tourFinishedFired = true;
                Debug.Log("Guided tour finished.");
                TourFinished?.Invoke();
            }
            return null;
        }

        Hop hop = FindFirstHop(current.id, route[tourIndex]);
        if (hop == null)
            Debug.LogWarning($"Guided tour: no path from '{current.id}' to '{route[tourIndex]}'. Check the id in tour_route.");
        return hop;
    }

    private Hop FindFirstHop(string startId, string goalId)
    {
        LocationData start = GetLocation(startId);
        if (start == null) return null;

        var visited = new HashSet<string> { startId };
        var firstHop = new Dictionary<string, Hop>();
        var queue = new Queue<string>();

        void Expand(LocationData from, Hop inherited)
        {
            // Arrows first, so they win if a hotspot leads to the same place.
            if (from.links != null)
            {
                foreach (LinkData l in from.links)
                {
                    if (l == null || string.IsNullOrEmpty(l.target_id) || !visited.Add(l.target_id)) continue;
                    firstHop[l.target_id] = inherited ?? new Hop { targetId = l.target_id };
                    queue.Enqueue(l.target_id);
                }
            }
            if (from.hotspots != null)
            {
                foreach (HotspotData h in from.hotspots)
                {
                    // Info-only bubbles have no target_id, so they're skipped.
                    if (h == null || string.IsNullOrEmpty(h.target_id) || !visited.Add(h.target_id)) continue;
                    firstHop[h.target_id] = inherited ?? new Hop { targetId = h.target_id, hotspotId = h.id };
                    queue.Enqueue(h.target_id);
                }
            }
        }

        Expand(start, null);

        while (queue.Count > 0)
        {
            string id = queue.Dequeue();
            if (id == goalId) return firstHop[id];

            LocationData loc = GetLocation(id);
            if (loc != null) Expand(loc, firstHop[id]);
        }
        return null;
    }

    private LocationData GetLocation(string id)
    {
        return SceneDataLoader.FindLocationById(id);
    }

    private void ApplyArrowColors(GameObject arrowObject, Color normal, Color hover)
    {
        Button button = arrowObject.GetComponentInChildren<Button>();
        if (button == null) return;

        ColorBlock colors = button.colors;
        colors.normalColor = normal;
        colors.selectedColor = normal;
        colors.highlightedColor = hover;
        colors.pressedColor = hover * 0.8f;
        button.colors = colors;
    }

    public void ToggleTour()
    {
        if (tourGuide) StopTour();
        else StartTour();
    }
}
