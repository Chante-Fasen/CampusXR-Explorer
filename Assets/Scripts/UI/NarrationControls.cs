using UnityEngine;
using UnityEngine.UI;

// Pause / resume / stop controls on the hotspot overlay panel.
// Acceptance criterion: controls respond within 1 second.
// Calls into AudioManager, which is owned by Michael.
public class NarrationControls : MonoBehaviour
{
    [SerializeField] private Button pauseResumeButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Image pauseResumeIcon;
    [SerializeField] private Sprite pauseSprite;
    [SerializeField] private Sprite playSprite;

    void OnEnable()
    {
        if (pauseResumeButton != null)
            pauseResumeButton.onClick.AddListener(TogglePauseResume);

        if (stopButton != null)
            stopButton.onClick.AddListener(StopNarration);

        RefreshIcon();
    }

    void OnDisable()
    {
        if (pauseResumeButton != null)
            pauseResumeButton.onClick.RemoveListener(TogglePauseResume);

        if (stopButton != null)
            stopButton.onClick.RemoveListener(StopNarration);

        // Closing the overlay must stop audio, or narration keeps playing
        // over the next hotspot the user opens.
        StopNarration();
    }

    void TogglePauseResume()
    {
        if (AudioManager.Instance == null) return;

        if (AudioManager.Instance.IsPaused)
            AudioManager.Instance.ResumeNarration();
        else
            AudioManager.Instance.PauseNarration();

        RefreshIcon();
    }

    void StopNarration()
    {
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.StopNarration();
        RefreshIcon();
    }

    void RefreshIcon()
    {
        if (pauseResumeIcon == null || AudioManager.Instance == null) return;

        bool showPlay = AudioManager.Instance.IsPaused || !AudioManager.Instance.IsPlaying;
        pauseResumeIcon.sprite = showPlay ? playSprite : pauseSprite;
    }
}
