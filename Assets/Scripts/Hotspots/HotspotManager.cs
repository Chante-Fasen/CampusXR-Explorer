using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Spawns the hotspot markers for whichever site the player is currently at,
/// using the "hotspots" list for that location in campus_data.json, and
/// opens the popup when one is clicked.
///
/// SETUP (Tour360):
///   Add this component to the "Managers" object (same one that has
///   SiteArrivalHandler and NavigationArrowManager). Everything else fills
///   itself in - a popup is created automatically if none is assigned.
///
/// WHERE HOTSPOTS APPEAR (world_position in the JSON):
///   Metres measured from the centre of the site's photo sphere. With
///   TourCentreFollower on Managers, that centre is the player's eyes:
///     x = right(+) / left(-)
///     y = up(+) / down(-) from eye level  (0 = eye level, about -1.6 = floor)
///     z = forward(+) / behind(-)   (forward = Unity's +Z, the shared direction all photos are turned to face)
///   Keep markers within about 4 m of the centre - the photo sphere has a
///   5 m radius, so anything further out would be hidden behind the photo.
///   Easiest way to get a position: in Tour360, turn on the site, put a
///   temporary camera at (0,0,0), place a small sphere on the thing in the
///   photo (check it in the Game view), copy the sphere's Position.
///
/// WHAT THE POPUP SHOWS (fields of each hotspot in the JSON):
///   "title"        - heading at the top, e.g. "Alpha Classroom"
///   "image"        - picture under the title: file name WITHOUT extension
///                    from Assets/Images/Hotspot Popup Images, e.g. "alpha-classroom"
///                    (after adding new images there, click the ⋮ menu on this
///                    component > "Refresh Hotspot Images" and save the scene)
///   "description"  - text under the picture
///   "target_id"    - optional: site the "Enter" button takes you to, e.g. "AlphaSite"
///   "button_label" - optional: text on that button (default "Enter")
///
///   Older fields still work: overlay_type "text"/"image" with content, and
///   overlay_type "audio" with content = AudioClip path in a Resources folder.
///
/// EXAMPLE - an "i" in front of the Alpha classroom door:
///   { "id": "alpha_door", "world_position": { "x": 2.5, "y": 0, "z": 1.0 },
///     "title": "Alpha Classroom", "image": "alpha-classroom",
///     "description": "Main lecture venue for first-years...",
///     "target_id": "AlphaSite", "button_label": "Enter Alpha" }
/// </summary>
public class HotspotManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Leave empty to find it automatically in this scene.")]
    public SiteArrivalHandler siteArrivalHandler;

    [Tooltip("Leave empty to create a popup automatically.")]
    public HotspotPopup popup;

    [Tooltip("Optional custom marker prefab. Leave empty to use the built-in 'i' marker.")]
    public GameObject markerPrefab;

    [Header("Popup images")]
    [Tooltip("Folder the hotspot popup images live in.")]
    public string imageFolder = "Assets/Images/Hotspot Popup Images";

    [Tooltip("Filled automatically from the folder above. After adding new images, use the ⋮ menu > 'Refresh Hotspot Images', then save the scene.")]
    public List<Texture2D> hotspotImages = new List<Texture2D>();

    [Header("Placement")]
    [Tooltip("Markers further than this from the site centre are pulled in so they don't end up behind the photo.")]
    public float maxDistanceFromCentre = 4.5f;

    [Tooltip("Turn the markers to always face the player.")]
    public bool faceCamera = true;

    [Header("Built-in marker look")]
    [Tooltip("Diameter of the built-in marker in metres.")]
    public float markerSize = 0.25f;
    public Color markerColor = new Color(0.12f, 0.37f, 0.88f, 0.95f);
    public Color markerHoverColor = new Color(0.35f, 0.65f, 1f, 1f);

    private readonly List<GameObject> spawnedMarkers = new List<GameObject>();
    private LocationData currentLocation;
    private AnalyticsLogger analyticsLogger;
    private bool warnedNoAnalytics;
    private static Sprite circleSprite;

    private void Awake()
    {
        if (siteArrivalHandler == null)
        {
            siteArrivalHandler = FindAnyObjectByType<SiteArrivalHandler>();
        }

        if (popup == null)
        {
            popup = FindAnyObjectByType<HotspotPopup>();
        }

        if (popup == null)
        {
            GameObject popupObject = new GameObject("HotspotPopup");
            popupObject.transform.SetParent(transform, false);
            popup = popupObject.AddComponent<HotspotPopup>();
        }

        popup.SetImageLibrary(hotspotImages);
    }

#if UNITY_EDITOR
    // Fill the image list automatically when the component is first added...
    private void Reset()
    {
        RefreshHotspotImages();
    }

    // ...and whenever it's empty (e.g. after pulling from Git).
    private void OnValidate()
    {
        if (hotspotImages == null || hotspotImages.Count == 0)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && (hotspotImages == null || hotspotImages.Count == 0)) RefreshHotspotImages();
            };
        }
    }

    [ContextMenu("Refresh Hotspot Images")]
    private void RefreshHotspotImages()
    {
        if (!UnityEditor.AssetDatabase.IsValidFolder(imageFolder))
        {
            Debug.LogWarning($"HotspotManager: image folder '{imageFolder}' not found.");
            return;
        }

        List<Texture2D> found = new List<Texture2D>();
        foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:Texture2D", new[] { imageFolder }))
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            Texture2D texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture != null) found.Add(texture);
        }
        found.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));

        hotspotImages = found;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"HotspotManager: found {found.Count} hotspot images in '{imageFolder}'. Save the scene to keep them.");
    }
#endif

    private void OnEnable()
    {
        SceneDataLoader.LocationChanged += BuildMarkers;

        if (SceneDataLoader.CurrentLocation != null)
        {
            BuildMarkers(SceneDataLoader.CurrentLocation);
        }
    }

    private void OnDisable()
    {
        SceneDataLoader.LocationChanged -= BuildMarkers;
        ClearMarkers();
    }

    private void LateUpdate()
    {
        if (!faceCamera || spawnedMarkers.Count == 0) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        foreach (GameObject marker in spawnedMarkers)
        {
            if (marker == null) continue;

            // Face away from the camera (that's how world-space UI is read),
            // turning only around the vertical axis so markers stay upright.
            Vector3 away = marker.transform.position - cam.transform.position;
            away.y = 0f;
            if (away.sqrMagnitude > 0.0001f)
            {
                marker.transform.rotation = Quaternion.LookRotation(away, Vector3.up);
            }
        }
    }

    // ---------------------------------------------------------------------
    // Building markers
    // ---------------------------------------------------------------------

    private void BuildMarkers(LocationData location)
    {
        ClearMarkers();
        currentLocation = location;

        if (popup != null) popup.Hide();

        if (location == null || location.hotspots == null || location.hotspots.Count == 0) return;

        Transform site = GetSiteTransform(location.id);
        Vector3 centre = site != null ? site.position : Vector3.zero;
        // world_position is measured in WORLD directions (the spheres are
        // rotated so all photos face the same real-world way), so the
        // sphere's own rotation is NOT applied here.

        foreach (HotspotData hotspot in location.hotspots)
        {
            if (hotspot == null) continue;

            Vector3 local = hotspot.world_position != null
                ? new Vector3(hotspot.world_position.x, hotspot.world_position.y, hotspot.world_position.z)
                : new Vector3(0f, 1.5f, 2f);

            // Keep it inside the photo sphere.
            Vector3 flat = new Vector3(local.x, 0f, local.z);
            if (flat.magnitude > maxDistanceFromCentre)
            {
                Debug.LogWarning($"Hotspot '{hotspot.id}' is {flat.magnitude:0.0} m from the centre - pulled in to {maxDistanceFromCentre} m so it isn't hidden behind the photo.");
                flat = flat.normalized * maxDistanceFromCentre;
                local = new Vector3(flat.x, local.y, flat.z);
            }

            Vector3 position = centre + local;

            GameObject markerObject = markerPrefab != null
                ? Instantiate(markerPrefab, position, Quaternion.identity, transform)
                : CreateBuiltInMarker(position);

            markerObject.name = "Hotspot · " + hotspot.id;

            HotspotMarker marker = markerObject.GetComponent<HotspotMarker>();
            if (marker == null) marker = markerObject.AddComponent<HotspotMarker>();
            marker.Setup(this, hotspot);

            spawnedMarkers.Add(markerObject);
        }
    }

    private void ClearMarkers()
    {
        foreach (GameObject marker in spawnedMarkers)
        {
            if (marker != null) Destroy(marker);
        }
        spawnedMarkers.Clear();
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
    // Clicking a hotspot
    // ---------------------------------------------------------------------

    public void OpenHotspot(HotspotData hotspot)
    {
        if (popup != null)
        {
            popup.Show(currentLocation, hotspot);
        }

        LogToAnalytics(hotspot);
    }

    private void LogToAnalytics(HotspotData hotspot)
    {
        if (analyticsLogger == null)
        {
            analyticsLogger = FindAnyObjectByType<AnalyticsLogger>();
        }

        if (analyticsLogger == null)
        {
            if (!warnedNoAnalytics)
            {
                Debug.Log("HotspotManager: no AnalyticsLogger in the scene, so hotspot clicks aren't being logged to Firebase.");
                warnedNoAnalytics = true;
            }
            return;
        }

        string siteId = currentLocation != null ? currentLocation.id : "";
        analyticsLogger.LogHotspotTrigger(siteId, hotspot.id);
    }

    // ---------------------------------------------------------------------
    // Built-in marker (used when no prefab is assigned)
    // ---------------------------------------------------------------------

    private GameObject CreateBuiltInMarker(Vector3 position)
    {
        const float canvasSize = 100f; // UI units

        GameObject root = new GameObject("HotspotMarker", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        root.transform.position = position;
        root.transform.localScale = Vector3.one * (markerSize / canvasSize);

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        root.AddComponent<GraphicRaycaster>();              // mouse / touch / Editor
        root.AddComponent<TrackedDeviceGraphicRaycaster>(); // VR controller rays
        ((RectTransform)root.transform).sizeDelta = new Vector2(canvasSize, canvasSize);

        GameObject circleObject = new GameObject("Circle", typeof(RectTransform));
        circleObject.transform.SetParent(root.transform, false);
        RectTransform circleRect = (RectTransform)circleObject.transform;
        circleRect.anchorMin = Vector2.zero;
        circleRect.anchorMax = Vector2.one;
        circleRect.offsetMin = Vector2.zero;
        circleRect.offsetMax = Vector2.zero;

        Image circle = circleObject.AddComponent<Image>();
        circle.sprite = GetCircleSprite();
        circle.color = Color.white;

        Button button = circleObject.AddComponent<Button>();
        button.targetGraphic = circle;
        ColorBlock colors = button.colors;
        colors.normalColor = markerColor;
        colors.highlightedColor = markerHoverColor;
        colors.selectedColor = markerColor;
        colors.pressedColor = markerHoverColor * 0.8f;
        colors.colorMultiplier = 1f;
        button.colors = colors;

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform));
        iconObject.transform.SetParent(circleObject.transform, false);
        RectTransform iconRect = (RectTransform)iconObject.transform;
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        TextMeshProUGUI icon = iconObject.AddComponent<TextMeshProUGUI>();
        icon.text = "i";
        icon.fontSize = 64f;
        icon.fontStyle = FontStyles.Bold;
        icon.alignment = TextAlignmentOptions.Center;
        icon.color = Color.white;
        icon.raycastTarget = false;

        return root;
    }

    // White disc with a slightly brighter rim, drawn once and reused.
    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null) return circleSprite;

        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        float radius = size * 0.5f - 1f;
        Vector2 centre = new Vector2(size * 0.5f, size * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), centre);
                float alpha = Mathf.Clamp01(radius - distance + 0.5f); // soft edge
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        circleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return circleSprite;
    }
}
