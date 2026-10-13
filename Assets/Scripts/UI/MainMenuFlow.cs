using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Runs the MainMenu flow:  Start  ->  POPIA consent  ->  Selection
///   - Accept / Decline saves the visitor's choice (see ConsentManager) and
///     moves on to Selection. Declining does NOT block the tour.
///   - Selection: "Guided Tour" (Enrique) or "Free Roam" (building map).
///
/// SETUP (MainMenu scene):
///   1. Empty GameObject (e.g. "MainFlow") with this component.
///   2. Canvases are found by name (CanvasMainMenu, CanvasPOIPA,
///      CanvasSelection) - or drag them into the fields.
///   3. ⋮ menu on this component -> "Apply Text & Buttons To Scene" writes the
///      text and creates any missing buttons in the Editor so you can see and
///      move them. Save the scene afterwards. (The same thing also happens
///      automatically when the game starts, so it works even if you skip it.)
///
/// BUTTONS (found by name; any that are missing are created by copying the
/// Start button from CanvasMainMenu):
///   CanvasMainMenu : ButtonStart
///   CanvasPOIPA    : ButtonAccept, ButtonDecline
///   CanvasSelection: ButtonGuidedTour, ButtonFreeRoam
/// Their old OnClick (MainMenuStartButton -> SplashScreen) is switched off at
/// runtime and replaced with this script's actions - no OnClick setup needed.
///
/// GUIDED TOUR (for Enrique):
///   Hook your start method into "On Guided Tour Selected" in the Inspector,
///   or type your scene name into "Guided Tour Scene Name". Read
///   MainMenuFlow.SelectedMode anywhere to know which option was chosen.
/// </summary>
public class MainMenuFlow : MonoBehaviour
{
    public enum TourMode { None, GuidedTour, FreeRoam }

    /// <summary>What the visitor picked on the Selection screen.</summary>
    public static TourMode SelectedMode { get; private set; } = TourMode.None;

    [Header("Canvases (found by name if left empty)")]
    public GameObject mainMenuCanvas;
    public GameObject consentCanvas;
    public GameObject selectionCanvas;

    [Header("Consent")]
    [Tooltip("Ask every visitor. Keep ON for Open Day - one headset is shared by many people, and each person must give their own consent.")]
    public bool askEveryTime = true;

    [Tooltip("Fill the POPIA canvas text and button labels from this script. Turn off if you want to type them in the Editor yourself.")]
    public bool fillConsentText = true;

    public string consentTitle = "Your Privacy (POPIA)";

    [TextArea(8, 20)]
    public string consentBody =
        "To help us improve the Belgium Campus Open Day tour, CampusXR Explorer can record <b>anonymous</b> usage data:\n" +
        "• which campus locations you visit\n" +
        "• which information points you open\n" +
        "• the time of each action and a random session number\n\n" +
        "We do <b>not</b> collect your name, email, contact details, photos, location or any device identifiers. " +
        "The data is stored securely in Google Firebase and is only used by the project team for the PRJ381 usability report.\n\n" +
        "Taking part is optional. If you decline, you can still use the full tour - nothing will be recorded.";

    public string acceptLabel = "Accept";
    public string declineLabel = "Decline";

    [Header("Selection")]
    public string guidedTourLabel = "Guided Tour";
    public string freeRoamLabel = "Free Roam";

    [Tooltip("Scene loaded for Free Roam (the splash/loading screen, which then opens the building map).")]
    public string freeRoamSceneName = "SplashScreen";

    [Tooltip("Optional: scene loaded for the Guided Tour. Leave empty if Enrique hooks in via the event below.")]
    public string guidedTourSceneName = "";

    [Tooltip("Enrique: hook the guided tour start here.")]
    public UnityEvent onGuidedTourSelected = new UnityEvent();

    // Where newly created buttons go, as anchor boxes inside the canvas
    // (0,0 = bottom-left, 1,1 = top-right). The right half of the panel is
    // where the Start button and the consent text already sit.
    private static readonly Rect AcceptArea   = new Rect(0.715f, 0.13f, 0.15f, 0.17f);
    private static readonly Rect DeclineArea  = new Rect(0.557f, 0.13f, 0.15f, 0.17f);
    private static readonly Rect GuidedArea   = new Rect(0.58f, 0.52f, 0.254f, 0.2f);
    private static readonly Rect FreeRoamArea = new Rect(0.58f, 0.26f, 0.254f, 0.2f);

    private static readonly HashSet<string> RoleNames = new HashSet<string>
    {
        "ButtonStart", "ButtonAccept", "ButtonDecline", "ButtonGuidedTour", "ButtonFreeRoam"
    };

    private Button startButton, acceptButton, declineButton, guidedButton, freeRoamButton;

    private void Awake()
    {
        SelectedMode = TourMode.None;
        BuildScreens(false);

        Hook(startButton, OnStartPressed);
        Hook(acceptButton, OnAccept);
        Hook(declineButton, OnDecline);
        Hook(guidedButton, OnGuidedTour);
        Hook(freeRoamButton, OnFreeRoam);
    }

    private void Start()
    {
        ShowOnly(mainMenuCanvas);
    }

    // ---------------------------------------------------------------------
    // Screens
    // ---------------------------------------------------------------------

    public void OnStartPressed()
    {
        bool skipConsent = !askEveryTime && ConsentManager.HasAnswered;
        ShowOnly(skipConsent || consentCanvas == null ? selectionCanvas : consentCanvas);
    }

    public void OnAccept()
    {
        ConsentManager.SetConsent(true);
        ShowOnly(selectionCanvas);
    }

    public void OnDecline()
    {
        ConsentManager.SetConsent(false);
        ShowOnly(selectionCanvas);
    }

    public void OnGuidedTour()
    {
        SelectedMode = TourMode.GuidedTour;

        bool hooked = onGuidedTourSelected.GetPersistentEventCount() > 0;
        onGuidedTourSelected.Invoke();

        if (!string.IsNullOrEmpty(guidedTourSceneName))
        {
            SceneManager.LoadScene(guidedTourSceneName);
        }
        else if (!hooked)
        {
            Debug.LogWarning("MainMenuFlow: Guided Tour isn't connected yet - add Enrique's start method to 'On Guided Tour Selected' or set 'Guided Tour Scene Name'.");
        }
    }

    public void OnFreeRoam()
    {
        SelectedMode = TourMode.FreeRoam;
        SceneManager.LoadScene(freeRoamSceneName);
    }

    private void ShowOnly(GameObject canvas)
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(canvas == mainMenuCanvas);
        if (consentCanvas != null) consentCanvas.SetActive(canvas == consentCanvas);
        if (selectionCanvas != null) selectionCanvas.SetActive(canvas == selectionCanvas);
    }

    // ---------------------------------------------------------------------
    // Building the screens (used at runtime AND by the Editor menu)
    // ---------------------------------------------------------------------

    private void BuildScreens(bool inEditor)
    {
        if (mainMenuCanvas == null) mainMenuCanvas = FindRoot("CanvasMainMenu");
        if (consentCanvas == null) consentCanvas = FindRoot("CanvasPOIPA", "CanvasPOPIA", "CanvasConsent");
        if (selectionCanvas == null) selectionCanvas = FindRoot("CanvasSelection");

        if (mainMenuCanvas == null)
        {
            Debug.LogWarning("MainMenuFlow: CanvasMainMenu not found.");
            return;
        }

        startButton = FindButton(mainMenuCanvas, "ButtonStart", 0);
        if (startButton == null)
        {
            Debug.LogWarning("MainMenuFlow: no Start button in CanvasMainMenu - it's needed as the template for the other buttons.");
            return;
        }
        Rename(startButton, "ButtonStart", inEditor);

        // --- POPIA ---
        if (consentCanvas != null)
        {
            acceptButton = FindButton(consentCanvas, "ButtonAccept", 0)
                           ?? CreateButton(consentCanvas, AcceptArea, inEditor);
            declineButton = FindButton(consentCanvas, "ButtonDecline", 1)
                            ?? CreateButton(consentCanvas, DeclineArea, inEditor);
            Rename(acceptButton, "ButtonAccept", inEditor);
            Rename(declineButton, "ButtonDecline", inEditor);

            if (fillConsentText)
            {
                SetLabel(acceptButton, acceptLabel, inEditor);
                SetLabel(declineButton, declineLabel, inEditor);
                FillConsentBody(inEditor);
            }
        }
        else
        {
            Debug.LogWarning("MainMenuFlow: POPIA canvas not found - Start will go straight to Selection.");
        }

        // --- Selection ---
        if (selectionCanvas != null)
        {
            guidedButton = FindButton(selectionCanvas, "ButtonGuidedTour", 0)
                           ?? CreateButton(selectionCanvas, GuidedArea, inEditor);
            freeRoamButton = FindButton(selectionCanvas, "ButtonFreeRoam", 1)
                             ?? CreateButton(selectionCanvas, FreeRoamArea, inEditor);
            Rename(guidedButton, "ButtonGuidedTour", inEditor);
            Rename(freeRoamButton, "ButtonFreeRoam", inEditor);
            SetLabel(guidedButton, guidedTourLabel, inEditor);
            SetLabel(freeRoamButton, freeRoamLabel, inEditor);
        }
        else
        {
            Debug.LogWarning("MainMenuFlow: CanvasSelection not found.");
        }
    }

    private void FillConsentBody(bool inEditor)
    {
        // Body = a text named "TextBody", otherwise the first text that isn't
        // a button label and isn't the CAMPUSXR logo title.
        TMP_Text body = FindText(consentCanvas, "TextBody");
        if (body == null)
        {
            foreach (TMP_Text text in consentCanvas.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.GetComponentInParent<Button>() != null) continue;
                if (text.text.Contains("CAMPUS")) continue;
                body = text;
                break;
            }
        }

        if (body == null)
        {
            Debug.LogWarning("MainMenuFlow: no body text on the POPIA canvas - add a TextMeshPro text named 'TextBody'.");
            return;
        }

        RecordUndo(body, inEditor);
        RecordUndo(body.gameObject, inEditor);
        body.gameObject.name = "TextBody";
        body.text = "<b>" + consentTitle + "</b>\n\n" + consentBody;
        body.enableAutoSizing = true;
        body.fontSizeMin = 10f;
        body.fontSizeMax = 28f;
        body.alignment = TextAlignmentOptions.TopLeft;
        SetDirty(body, inEditor);
    }

    // Copy the main menu's Start button into another canvas, filling the given
    // anchor box (so it scales with the panel like the original).
    private Button CreateButton(GameObject canvas, Rect area, bool inEditor)
    {
        GameObject copy = Instantiate(startButton.gameObject, canvas.transform, false);
        copy.SetActive(true);

        RectTransform rect = (RectTransform)copy.transform;
        rect.anchorMin = new Vector2(area.xMin, area.yMin);
        rect.anchorMax = new Vector2(area.xMax, area.yMax);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.anchoredPosition3D = Vector3.zero;

#if UNITY_EDITOR
        if (inEditor) UnityEditor.Undo.RegisterCreatedObjectUndo(copy, "Create menu button");
#endif
        return copy.GetComponent<Button>();
    }

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------

    // Replace whatever the button did before (e.g. MainMenuStartButton ->
    // load SplashScreen) with our own action.
    private static void Hook(Button button, UnityAction action)
    {
        if (button == null) return;

        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            button.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private static Button FindButton(GameObject canvas, string preferredName, int fallbackIndex)
    {
        Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.name == preferredName) return button;
        }

        // Fall back to Hierarchy order, skipping buttons already named for
        // another role (so a renamed "ButtonDecline" isn't used as Accept).
        List<Button> unnamed = new List<Button>();
        foreach (Button button in buttons)
        {
            if (!RoleNames.Contains(button.name)) unnamed.Add(button);
        }

        return fallbackIndex < unnamed.Count ? unnamed[fallbackIndex] : null;
    }

    private static TMP_Text FindText(GameObject canvas, string name)
    {
        foreach (TMP_Text text in canvas.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.name == name) return text;
        }
        return null;
    }

    private static void Rename(Button button, string newName, bool inEditor)
    {
        if (button == null || button.name == newName) return;
        RecordUndo(button.gameObject, inEditor);
        button.gameObject.name = newName;
    }

    private static void SetLabel(Button button, string label, bool inEditor)
    {
        if (button == null || string.IsNullOrEmpty(label)) return;
        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text == null) return;
        RecordUndo(text, inEditor);
        text.text = label;
        SetDirty(text, inEditor);
    }

    private static void RecordUndo(Object target, bool inEditor)
    {
#if UNITY_EDITOR
        if (inEditor) UnityEditor.Undo.RecordObject(target, "Apply menu text");
#endif
    }

    private static void SetDirty(Object target, bool inEditor)
    {
#if UNITY_EDITOR
        if (inEditor) UnityEditor.EditorUtility.SetDirty(target);
#endif
    }

    private static GameObject FindRoot(params string[] names)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (string name in names)
            {
                if (root.name == name) return root;
            }
        }
        return null;
    }

#if UNITY_EDITOR
    [ContextMenu("Apply Text & Buttons To Scene")]
    private void ApplyToSceneInEditor()
    {
        BuildScreens(true);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        Debug.Log("MainMenuFlow: text and buttons applied to the scene. Turn a canvas on to see it, and save the scene (Ctrl+S) to keep the changes.");
    }
#endif
}
