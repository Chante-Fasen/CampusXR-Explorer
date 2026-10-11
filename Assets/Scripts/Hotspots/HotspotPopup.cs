using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// The info panel that opens when a hotspot is clicked. It appears in front
/// of the player and shows, from top to bottom:
///
///   TITLE          - hotspot "title" (falls back to the site's label)
///   IMAGE          - hotspot "image": file name without extension from
///                    Assets/Images/Hotspot Popup Images, e.g. "alpha-classroom"
///   DESCRIPTION    - hotspot "description"
///   [Enter] button - only if the hotspot has a "target_id"; takes the player
///                    to that site (e.g. into the classroom). Label comes from
///                    "button_label" (default "Enter").
///
/// Parts that aren't filled in are simply left out and the panel shrinks.
///
/// The older overlay_type/content fields still work:
///   "text"  -> content is used as the description
///   "image" -> content is used as the image name
///   "audio" -> content is an AudioClip path in a Resources folder
///              (e.g. "Hotspots/reception_narration"); Play/Pause + Stop appear
///   "video" -> placeholder for now (Enrique's task)
///
/// HotspotManager creates one of these automatically and gives it the image
/// list, so nothing needs to be set up in the scene.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class HotspotPopup : MonoBehaviour
{
    [Header("Placement")]
    [Tooltip("How far in front of the player the popup opens, in metres.")]
    public float distanceFromPlayer = 1.5f;

    [Tooltip("Width of the popup in metres.")]
    public float widthInMetres = 0.8f;

    [Header("Look")]
    public Color backgroundColor = new Color(0.07f, 0.09f, 0.15f, 0.95f);
    public Color buttonColor = new Color(0.12f, 0.37f, 0.88f, 1f);
    public Color buttonHoverColor = new Color(0.35f, 0.65f, 1f, 1f);

    // Layout in UI units (the whole panel is scaled down to widthInMetres).
    private const float PanelWidth = 900f;
    private const float Padding = 40f;
    private const float TitleHeight = 80f;
    private const float ImageHeight = 440f;
    private const float ButtonHeight = 90f;
    private const float Gap = 24f;
    private const float DescriptionMinHeight = 50f;
    private const float DescriptionMaxHeight = 380f;
    private const float DescriptionFontSize = 34f;

    private GameObject panelRoot;
    private RectTransform panelRect;
    private Canvas canvas;
    private TextMeshProUGUI titleText;
    private Image contentImage;
    private TextMeshProUGUI descriptionText;
    private GameObject audioControls;
    private TextMeshProUGUI playPauseLabel;
    private Button enterButton;
    private TextMeshProUGUI enterLabel;
    private AudioSource audioSource;
    private bool isPaused;
    private string enterTargetId;

    // Images available to hotspots, looked up by file name.
    private readonly Dictionary<string, Texture2D> imageLibrary = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // plain 2D narration

        BuildUI();
        Hide();
    }

    private void Update()
    {
        // Reset the button label when a clip finishes on its own.
        if (IsOpen && audioControls.activeSelf && !isPaused && !audioSource.isPlaying && playPauseLabel.text != "Play")
        {
            playPauseLabel.text = "Play";
        }
    }

    // ---------------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------------

    /// <summary>
    /// Gives the popup the images it can show (HotspotManager passes in
    /// everything from Assets/Images/Hotspot Popup Images).
    /// </summary>
    public void SetImageLibrary(IEnumerable<Texture2D> images)
    {
        imageLibrary.Clear();
        spriteCache.Clear();
        if (images == null) return;

        foreach (Texture2D image in images)
        {
            if (image != null) imageLibrary[image.name] = image;
        }
    }

    public void Show(LocationData location, HotspotData hotspot)
    {
        if (hotspot == null) return;

        StopAudio();

        string type = string.IsNullOrEmpty(hotspot.overlay_type) ? "" : hotspot.overlay_type.ToLowerInvariant();
        string content = hotspot.content ?? "";

        // ---- Work out what to show ----
        string title = !string.IsNullOrEmpty(hotspot.title)
            ? hotspot.title
            : (location != null ? location.label : "");

        string imageName = !string.IsNullOrEmpty(hotspot.image)
            ? hotspot.image
            : (type == "image" ? content : "");

        string description = hotspot.description ?? "";
        if (string.IsNullOrEmpty(description) && (type == "text" || type == ""))
        {
            description = content;
        }

        Sprite sprite = null;
        if (!string.IsNullOrEmpty(imageName))
        {
            sprite = LoadSprite(imageName);
            if (sprite == null)
            {
                description = AppendLine(description, $"<size=70%><color=#FF8A80>Image not found: {imageName}</color></size>");
                Debug.LogWarning($"HotspotPopup: couldn't find image '{imageName}'. Put it in Assets/Images/Hotspot Popup Images, then on Tour360 > Managers > Hotspot Manager use the ⋮ menu > 'Refresh Hotspot Images' and save the scene. In the JSON use the file name without extension.");
            }
        }

        bool hasAudio = false;
        if (type == "audio")
        {
            AudioClip clip = Resources.Load<AudioClip>(content);
            if (clip != null)
            {
                audioSource.clip = clip;
                hasAudio = true;
            }
            else
            {
                description = AppendLine(description, $"<size=70%><color=#FF8A80>Audio not found: Resources/{content}</color></size>");
                Debug.LogWarning($"HotspotPopup: couldn't load audio '{content}'. Put it in a Resources folder (e.g. Assets/Resources/{content}.mp3) and leave the extension out of the JSON.");
            }
        }
        else if (type == "video")
        {
            description = AppendLine(description, "Video coming soon.");
        }

        enterTargetId = hotspot.target_id;
        bool hasEnter = !string.IsNullOrEmpty(enterTargetId);

        // ---- Fill in the UI ----
        titleText.text = title;
        contentImage.sprite = sprite;
        descriptionText.text = description;
        enterLabel.text = !string.IsNullOrEmpty(hotspot.button_label) ? hotspot.button_label : "Enter";

        LayoutPanel(
            hasTitle: !string.IsNullOrEmpty(title),
            hasImage: sprite != null,
            hasDescription: !string.IsNullOrEmpty(description),
            hasAudio: hasAudio,
            hasEnter: hasEnter);

        PlaceInFrontOfPlayer();
        panelRoot.SetActive(true);

        if (hasAudio)
        {
            audioSource.Play();
            isPaused = false;
            playPauseLabel.text = "Pause";
        }
    }

    public void Hide()
    {
        StopAudio();
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    // ---------------------------------------------------------------------
    // Buttons
    // ---------------------------------------------------------------------

    private void OnEnterClicked()
    {
        string target = enterTargetId;
        Hide();
        SiteNavigator.GoTo(target);
    }

    private void TogglePlayPause()
    {
        if (audioSource.clip == null) return;

        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            isPaused = true;
            playPauseLabel.text = "Play";
        }
        else
        {
            if (isPaused) audioSource.UnPause();
            else audioSource.Play(); // finished or stopped - start again
            isPaused = false;
            playPauseLabel.text = "Pause";
        }
    }

    private void StopAudio()
    {
        if (audioSource == null) return;

        audioSource.Stop();
        isPaused = false;
        if (playPauseLabel != null) playPauseLabel.text = "Play";
    }

    // ---------------------------------------------------------------------
    // Images
    // ---------------------------------------------------------------------

    private Sprite LoadSprite(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        // Accept "alpha-classroom", "alpha-classroom.jpeg" or a folder path.
        string imageName = Path.GetFileNameWithoutExtension(name.Trim());

        if (spriteCache.TryGetValue(imageName, out Sprite cached) && cached != null) return cached;

        // 1. Image list from Assets/Images/Hotspot Popup Images
        imageLibrary.TryGetValue(imageName, out Texture2D texture);

        // 2. Fallback: a Resources folder
        if (texture == null)
        {
            Sprite resourceSprite = Resources.Load<Sprite>(name);
            if (resourceSprite != null) return resourceSprite;
            texture = Resources.Load<Texture2D>(name);
        }

        if (texture == null) return null;

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        spriteCache[imageName] = sprite;
        return sprite;
    }

    private static string AppendLine(string text, string line)
    {
        return string.IsNullOrEmpty(text) ? line : text + "\n" + line;
    }

    // ---------------------------------------------------------------------
    // Layout: stack the parts top to bottom and size the panel to fit
    // ---------------------------------------------------------------------

    private void LayoutPanel(bool hasTitle, bool hasImage, bool hasDescription, bool hasAudio, bool hasEnter)
    {
        float y = Padding;
        float innerWidth = PanelWidth - Padding * 2f;

        // Title row (always reserved - the close button lives there too)
        StackTop(titleText.rectTransform, ref y, TitleHeight, rightInset: 90f);
        titleText.gameObject.SetActive(hasTitle);
        y += Gap;

        contentImage.gameObject.SetActive(hasImage);
        if (hasImage)
        {
            StackTop(contentImage.rectTransform, ref y, ImageHeight);
            y += Gap;
        }

        descriptionText.gameObject.SetActive(hasDescription);
        if (hasDescription)
        {
            float height = descriptionText.GetPreferredValues(descriptionText.text, innerWidth, 0f).y;
            height = Mathf.Clamp(height + 8f, DescriptionMinHeight, DescriptionMaxHeight);
            StackTop(descriptionText.rectTransform, ref y, height);
            y += Gap;
        }

        audioControls.SetActive(hasAudio);
        if (hasAudio)
        {
            StackTop((RectTransform)audioControls.transform, ref y, ButtonHeight);
            y += Gap;
        }

        enterButton.gameObject.SetActive(hasEnter);
        if (hasEnter)
        {
            StackTop((RectTransform)enterButton.transform, ref y, ButtonHeight);
            y += Gap;
        }

        float totalHeight = y - Gap + Padding;
        panelRect.sizeDelta = new Vector2(PanelWidth, totalHeight);
    }

    // Anchors an element to the top of the panel, full width minus padding,
    // at distance y from the top, and moves y down past it.
    private static void StackTop(RectTransform rect, ref float y, float height, float rightInset = 0f)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(Padding, -(y + height));
        rect.offsetMax = new Vector2(-(Padding + rightInset), -y);
        y += height;
    }

    // ---------------------------------------------------------------------
    // Placement
    // ---------------------------------------------------------------------

    private void PlaceInFrontOfPlayer()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        canvas.worldCamera = cam;

        Vector3 forward = cam.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
        forward.Normalize();

        Vector3 position = cam.transform.position + forward * distanceFromPlayer;
        panelRoot.transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, Vector3.up));
    }

    // ---------------------------------------------------------------------
    // Building the UI in code
    // ---------------------------------------------------------------------

    private void BuildUI()
    {
        panelRoot = new GameObject("PopupCanvas", typeof(RectTransform));
        panelRoot.transform.SetParent(transform, false);
        panelRoot.transform.localScale = Vector3.one * (widthInMetres / PanelWidth);

        canvas = panelRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;
        panelRoot.AddComponent<GraphicRaycaster>();              // mouse / touch / Editor
        panelRoot.AddComponent<TrackedDeviceGraphicRaycaster>(); // VR controller rays
        panelRect = (RectTransform)panelRoot.transform;
        panelRect.sizeDelta = new Vector2(PanelWidth, 600f);

        // Background (stretches with the panel)
        GameObject backgroundObject = NewRect("Background", panelRoot.transform);
        Stretch((RectTransform)backgroundObject.transform);
        Image background = backgroundObject.AddComponent<Image>();
        background.color = backgroundColor;

        // Title
        titleText = CreateText("Title", panelRoot.transform, 48f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 28f;
        titleText.fontSizeMax = 48f;

        // Close button (top right, in the title row)
        Button closeButton = CreateButton("CloseButton", panelRoot.transform, "X", Hide);
        RectTransform closeRect = (RectTransform)closeButton.transform;
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(TitleHeight, TitleHeight);
        closeRect.anchoredPosition = new Vector2(-Padding, -Padding);

        // Image
        GameObject imageObject = NewRect("Image", panelRoot.transform);
        contentImage = imageObject.AddComponent<Image>();
        contentImage.preserveAspect = true;
        contentImage.raycastTarget = false;

        // Description
        descriptionText = CreateText("Description", panelRoot.transform, DescriptionFontSize, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        descriptionText.overflowMode = TextOverflowModes.Ellipsis;

        // Audio controls (Play/Pause + Stop side by side)
        audioControls = NewRect("AudioControls", panelRoot.transform);

        Button playPauseButton = CreateButton("PlayPauseButton", audioControls.transform, "Pause", TogglePlayPause);
        SetAnchors((RectTransform)playPauseButton.transform, new Vector2(0f, 0f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(-10f, 0f));
        playPauseLabel = playPauseButton.GetComponentInChildren<TextMeshProUGUI>();

        Button stopButton = CreateButton("StopButton", audioControls.transform, "Stop", StopAudio);
        SetAnchors((RectTransform)stopButton.transform, new Vector2(0.5f, 0f), new Vector2(1f, 1f), new Vector2(10f, 0f), Vector2.zero);

        // Enter button
        enterButton = CreateButton("EnterButton", panelRoot.transform, "Enter", OnEnterClicked);
        enterLabel = enterButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    private static GameObject NewRect(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static void Stretch(RectTransform rect)
    {
        SetAnchors(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }

    private static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, float fontSize, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject obj = NewRect(name, parent);

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private Button CreateButton(string name, Transform parent, string label, UnityAction onClick)
    {
        GameObject obj = NewRect(name, parent);

        Image image = obj.AddComponent<Image>();
        image.color = Color.white;

        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = buttonHoverColor;
        colors.selectedColor = buttonColor;
        colors.pressedColor = buttonHoverColor * 0.8f;
        colors.colorMultiplier = 1f;
        button.colors = colors;
        button.onClick.AddListener(onClick);

        TextMeshProUGUI text = CreateText("Label", obj.transform, 36f, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);
        text.text = label;

        return button;
    }
}
