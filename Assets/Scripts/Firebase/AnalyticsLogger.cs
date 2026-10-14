using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;

// Logs facility_view and hotspot_trigger events to Firestore.
// Self-creating: no need to attach to a GameObject.
// Usage: AnalyticsLogger.LogFacilityView(siteId);
//        AnalyticsLogger.LogHotspotTrigger(siteId, hotspotId);
//
// Each event carries the visitor's ids from the POPIA login (ConsentManager):
//   userId    - the visitor's anonymous Firebase user (users/{userId})
//   sessionId - this visit (sessions/{sessionId}); a new one per visitor
//   launchId  - the app launch, used as sessionId if the login failed
//               (e.g. no internet or Anonymous sign-in disabled)

public class AnalyticsLogger : MonoBehaviour
{
    // Must match the key saved by the POPIA consent screen
    const string ConsentKey = "AnalyticsConsentGiven";

    static AnalyticsLogger instance;
    static string launchId;

    FirebaseFirestore db;
    bool ready;
    readonly List<Dictionary<string, object>> pending = new List<Dictionary<string, object>>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        // Generate a NEW launch ID every time the app launches fresh.
        // Static fields persist if the app is only paused, so we
        // explicitly reset it here during startup.
        launchId = Guid.NewGuid().ToString();

        var go = new GameObject("AnalyticsLogger");
        instance = go.AddComponent<AnalyticsLogger>();
        DontDestroyOnLoad(go);
    }

    void OnEnable()
    {
        ConsentManager.LoginFinished += Flush;
    }

    void OnDisable()
    {
        ConsentManager.LoginFinished -= Flush;
    }

    void Start()
    {
        // Check Firebase dependencies (required on Android/Quest) before use
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                ready = true;
                Debug.Log("AnalyticsLogger: Firebase ready");
                Flush();
            }
            else
            {
                Debug.LogError("AnalyticsLogger: Firebase unavailable: " + task.Result);
            }
        });
    }

    public static void LogFacilityView(string siteId)
    {
        Queue("facility_view", siteId, null);
    }

    public static void LogHotspotTrigger(string siteId, string hotspotId)
    {
        Queue("hotspot_trigger", siteId, hotspotId);
    }

    static void Queue(string eventType, string sceneId, string hotspotId)
    {
        if (instance == null) return;

        if (PlayerPrefs.GetInt(ConsentKey, 0) != 1)
        {
            Debug.Log("AnalyticsLogger: no consent, event skipped");
            return;
        }

        var data = new Dictionary<string, object>
        {
            { "eventType", eventType },
            { "timestamp", Timestamp.GetCurrentTimestamp() },
            { "sceneId", sceneId },
            { "hotspotId", hotspotId },
            { "launchId", launchId }
        };

        instance.pending.Add(data);
        instance.Flush();
    }

    // Sends waiting events once Firebase is ready and the visitor's login has
    // finished, so the first events of a visit get the right ids too.
    void Flush()
    {
        if (!ready || ConsentManager.IsLoggingIn) return;

        foreach (var data in pending)
        {
            // Ids are added at send time: the login may have finished after
            // the event was queued.
            data["userId"] = ConsentManager.UserId;
            data["sessionId"] = ConsentManager.SessionId ?? launchId;
            Send(data);
        }
        pending.Clear();
    }

    void Send(Dictionary<string, object> data)
    {
        db.Collection("analytics_events").AddAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
                Debug.Log("AnalyticsLogger: logged " + data["eventType"] +
                          " | session: " + data["sessionId"]);
            else
                Debug.LogError("AnalyticsLogger: failed: " + task.Exception);
        });
    }
}
