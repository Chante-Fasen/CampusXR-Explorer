using UnityEngine;

// TEMPORARY placeholder so NarrationControls compiles.
// Michael owns the real implementation - DELETE THIS FILE when his lands,
// or you will get a duplicate class definition error.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource narrationSource;

    public bool IsPlaying { get { return narrationSource != null && narrationSource.isPlaying; } }
    public bool IsPaused { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (narrationSource == null)
            narrationSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayNarration(AudioClip clip)
    {
        if (clip == null) return;
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