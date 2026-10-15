using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// Plays the "voiceover" clip of a location when the player arrives there
/// during a guided tour. Each clip plays once per tour.
/// Clips go through AudioManager, so they can be paused/stopped from anywhere.
public class TourVoiceover : MonoBehaviour
{
    [Tooltip("Folder inside a Resources folder that holds the clips.")]
    public string resourcesFolder = "Voiceovers";

    [Tooltip("Stop the narration as soon as the player moves to another site.")]
    public bool stopWhenLeaving = false;

    [Tooltip("Clip (in the same Resources folder) played after the last stop's voiceover.")]
    public string conclusionClip = "tour-conclusion";

    private NavigationArrowManager manager;
    private readonly HashSet<string> played = new HashSet<string>();

    private void Awake()
    {
        manager = FindAnyObjectByType<NavigationArrowManager>();
    }

    private static AudioManager Audio => AudioManager.Instance;

    private void OnEnable()
    {
        SceneDataLoader.LocationChanged += OnLocationChanged;
        NavigationArrowManager.TourStarted += OnTourStarted;
        NavigationArrowManager.TourStopped += OnTourStopped;
        NavigationArrowManager.TourFinished += OnTourFinished;
    }

    private void OnDisable()
    {
        SceneDataLoader.LocationChanged -= OnLocationChanged;
        NavigationArrowManager.TourStarted -= OnTourStarted;
        NavigationArrowManager.TourStopped -= OnTourStopped;
        NavigationArrowManager.TourFinished -= OnTourFinished;

        // Leaving the tour scene (Change Mode / End Tour): AudioManager lives
        // on between scenes, so stop the narration here.
        if (Audio != null) Audio.StopNarration();
    }

    private void OnTourStarted()
    {
        StopAllCoroutines();
        played.Clear();
        TryPlay(SceneDataLoader.CurrentLocation);   // the player may already be at the first stop
    }

    private void OnTourStopped()
    {
        StopAllCoroutines();
        if (Audio != null) Audio.StopNarration();
    }

    private void OnLocationChanged(LocationData location)
    {
        if (manager == null || !manager.tourGuide) return;

        if (stopWhenLeaving && Audio != null) Audio.StopNarration();
        TryPlay(location);
    }

    private void TryPlay(LocationData location)
    {
        if (location == null || string.IsNullOrEmpty(location.voiceover)) return;
        if (played.Contains(location.id)) return;

        AudioClip clip = Resources.Load<AudioClip>(resourcesFolder + "/" + location.voiceover);
        if (clip == null)
        {
            Debug.LogWarning($"TourVoiceover: no clip '{location.voiceover}' in Resources/{resourcesFolder}.");
            return;
        }

        if (Audio == null) return;

        played.Add(location.id);
        Audio.PlayNarration(clip);   // a new clip replaces any one still playing
    }

    private void OnTourFinished()
    {
        StartCoroutine(PlayConclusion());
    }

    private IEnumerator PlayConclusion()
    {
        yield return null;                              // let the last stop's own clip start first
        while (Audio != null && Audio.IsBusy) yield return null;   // then wait for it to finish (or be stopped)

        AudioClip clip = Resources.Load<AudioClip>(resourcesFolder + "/" + conclusionClip);
        if (clip == null)
        {
            Debug.LogWarning($"TourVoiceover: no clip '{conclusionClip}' in Resources/{resourcesFolder}.");
            yield break;
        }

        if (Audio != null) Audio.PlayNarration(clip);
    }
}