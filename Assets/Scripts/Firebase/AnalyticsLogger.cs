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

public class AnalyticsLogger : MonoBehaviour
{
    // Must match the key saved by the POPIA consent screen
    const string ConsentKey = "AnalyticsConsentGiven";

    static AnalyticsLogger instance;
    static string sessionId;

    FirebaseFirestore db;
    bool ready;
    readonly List<Dictionary<string, object>> pending = new List<Dictionary<string, object>>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        // Generate a NEW session ID every time the app launches fresh.
        // Static fields persist if the app is only paused, so we
        // explicitly reset it here during startup.
        sessionId = Guid.NewGuid().ToString();

        var go = new GameObject("AnalyticsLogger");
        instance = go.AddComponent<AnalyticsLogger>();
        DontDestroyOnLoad(go);
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
                foreach (var e in pending) Send(e);
                pending.Clear();
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
            { "sessionId", sessionId }
        };

        if (instance.ready) instance.Send(data);
        else instance.pending.Add(data);
    }

    void Send(Dictionary<string, object> data)
    {
        db.Collection("analytics_events").AddAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
                Debug.Log("AnalyticsLogger: logged " + data["eventType"] +
                          " | session: " + sessionId);
            else
                Debug.LogError("AnalyticsLogger: failed: " + task.Exception);
        });
    }
}