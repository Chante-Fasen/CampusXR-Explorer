using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// In-tour Settings panel (Tour360 "CanvasSettings"). Shows/hides like every
/// TourPanel (see TourPanel.cs for the canvas setup).
///
/// Buttons: add SettingsButton to each one
///   Settings button (tour UI) -> Open Settings
///   Return                    -> Return From Settings (or Close Panel)
///   Change Mode               -> Main Menu
///   End Tour                  -> New Visitor
///
/// VOLUME: a Slider inside the panel whose name contains "Volume" is hooked
/// up automatically. It sets the master volume (AudioListener.volume) and
/// remembers it between launches.
/// </summary>
public class SettingsPanel : TourPanel
{
    public const string VolumeKey = "MasterVolume";

    [Header("Volume")]
    [Tooltip("Leave empty to use the first Slider whose name contains 'Volume'.")]
    public Slider volumeSlider;

    /// <summary>Back to full volume, e.g. for the next visitor ("End Tour").</summary>
    public static void ResetVolume()
    {
        PlayerPrefs.SetFloat(VolumeKey, 1f);
        PlayerPrefs.Save();
        AudioListener.volume = 1f;
    }

    // Apply the saved volume as soon as the game starts, in every scene.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplySavedVolume()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
    }

    protected override void OnFirstShow()
    {
        if (volumeSlider == null)
        {
            foreach (Slider slider in GetComponentsInChildren<Slider>(true))
            {
                if (slider.name.ToLowerInvariant().Contains("volume"))
                {
                    volumeSlider = slider;
                    break;
                }
            }
        }

        if (volumeSlider != null)
        {
            float saved = PlayerPrefs.GetFloat(VolumeKey, 1f);
            volumeSlider.SetValueWithoutNotify(Mathf.Lerp(volumeSlider.minValue, volumeSlider.maxValue, saved));
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    private void OnVolumeChanged(float value)
    {
        float volume = Mathf.InverseLerp(volumeSlider.minValue, volumeSlider.maxValue, value);
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }
}
