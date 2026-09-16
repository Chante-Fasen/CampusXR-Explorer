using UnityEngine;
using Firebase.Firestore;
using System;

// Logs two types of events to Firestore: facility_view (when a
// scene loads) and hotspot_trigger (when a hotspot is activated).
// Only logs if the user has given POPIA consent (checked via
// PlayerPrefs, set by Thierry's consent screen).

public class AnalyticsLogger : MonoBehaviour
{
    // Reference to the Firestore database
    private FirebaseFirestore db;

    // Unique ID for this app session, generated once per launch
    private static string sessionId;

    void Awake()
    {
        // Get a reference to the Firestore database
        db = FirebaseFirestore.DefaultInstance;

        // Generate one random session ID per app launch (anonymous,
        // no personal info - matches POPIA-compliant design from
        // the project plan)
        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
        }
    }

    // Call this when a user enters a 360 environment
    public void LogFacilityView(string sceneId)
    {
        if (!HasConsent()) return;

        LogEvent("facility_view", sceneId, null);
    }

    // Call this when a user activates a hotspot
    public void LogHotspotTrigger(string sceneId, string hotspotId)
    {
        if (!HasConsent()) return;

        LogEvent("hotspot_trigger", sceneId, hotspotId);
    }

    // Shared logic for writing an event document to Firestore
    private void LogEvent(string eventType, string sceneId, string hotspotId)
    {
        // Build the event data matching the schema from the
        // project plan: eventType, timestamp, sceneId, hotspotId, sessionId
        var eventData = new System.Collections.Generic.Dictionary<string, object>
        {
            { "eventType", eventType },
            { "timestamp", Timestamp.GetCurrentTimestamp() },
            { "sceneId", sceneId },
            { "hotspotId", hotspotId },
            { "sessionId", sessionId }
        };

        // Write it to the "analytics_events" collection in Firestore.
        // Add() auto-generates a unique document ID for each event.
        db.Collection("analytics_events").AddAsync(eventData).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Analytics event logged: " + eventType);
            }
            else
            {
                Debug.LogError("Failed to log analytics event: " + task.Exception);
            }
        });
    }

    // Checks whether the user has given POPIA consent.
    // Assumes Thierry's consent screen saves this key.
    private bool HasConsent()
    {
        return PlayerPrefs.GetInt("AnalyticsConsentGiven", 0) == 1;
    }
}
