using UnityEngine;

/// <summary>
/// Plays the tour narration (guided-tour voice-overs) through one shared
/// AudioSource, so anything can pause, resume or stop it - e.g.
/// NarrationControls' buttons.
///
/// No setup needed: it creates itself when the game starts and survives
/// scene loads. Volume follows the Settings slider (AudioListener.volume).
///
///   AudioManager.Instance.PlayNarration(clip);
///   AudioManager.Instance.PauseNarration() / ResumeNarration() / StopNarration();
///   AudioManager.Instance.IsPlaying / IsPaused / IsBusy
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource narrationSource;

    /// <summary>A clip is currently audible.</summary>
    public bool IsPlaying => narrationSource != null && narrationSource.isPlaying;

    /// <summary>A clip was paused and can be resumed.</summary>
    public bool IsPaused { get; private set; }

    /// <summary>Playing or paused - i.e. a clip hasn't finished yet.</summary>
    public bool IsBusy => IsPlaying || IsPaused;

    public AudioClip CurrentClip => narrationSource != null ? narrationSource.clip : null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("AudioManager");
        go.AddComponent<AudioManager>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        narrationSource = gameObject.AddComponent<AudioSource>();
        narrationSource.playOnAwake = false;
        narrationSource.loop = false;
        narrationSource.spatialBlend = 0f;   // 2D: same volume wherever the player looks
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void PlayNarration(AudioClip clip)
    {
        if (clip == null) return;

        narrationSource.Stop();              // a new clip replaces any one still playing
        narrationSource.clip = clip;
        narrationSource.Play();
        IsPaused = false;
    }

    public void PauseNarration()
    {
        if (!narrationSource.isPlaying) return;
        narrationSource.Pause();
        IsPaused = true;
    }

    public void ResumeNarration()
    {
        if (!IsPaused) return;
        narrationSource.UnPause();
        IsPaused = false;
    }

    public void StopNarration()
    {
        narrationSource.Stop();
        IsPaused = false;
    }
}
