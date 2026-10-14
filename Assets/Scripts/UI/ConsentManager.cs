using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

/// <summary>
/// Stores the visitor's POPIA analytics consent AND records logins.
///
/// The answer is saved in PlayerPrefs under "AnalyticsConsentGiven"
/// (1 = accepted, 0 = declined) - the same key AnalyticsLogger already
/// checks before sending anything to Firebase. Declining never blocks the
/// tour; it only switches analytics off.
///
/// When the visitor accepts, they are signed in anonymously (no typing) and a
/// user record + session record are written to Firestore.
/// </summary>
public static class ConsentManager
{
    public const string ConsentKey = "AnalyticsConsentGiven";   // read by AnalyticsLogger
    public const string AnsweredKey = "AnalyticsConsentAnswered";

    // Change this whenever your POPIA text changes.
    public const string PolicyVersion = "v1.0";

    // Open Day: many visitors share one headset, so each visitor gets a fresh
    // anonymous ID when the tour is ended ("End Tour" -> ResetConsent).
    public const bool SignOutOnNewVisitor = true;

    /// <summary>Raised whenever the visitor accepts or declines.</summary>
    public static event Action<bool> ConsentChanged;

    /// <summary>True if the visitor accepted analytics.</summary>
    public static bool HasConsent => PlayerPrefs.GetInt(ConsentKey, 0) == 1;

    /// <summary>True if the visitor has answered the prompt at least once on this device.</summary>
    public static bool HasAnswered => PlayerPrefs.GetInt(AnsweredKey, 0) == 1;

    // ---------- Login / session state ----------

    public static string UserId { get; private set; }
    public static string SessionId { get; private set; }
    public static bool IsLoggedIn => _sessionRef != null;

    /// <summary>True while the anonymous login for this visitor is running.</summary>
    public static bool IsLoggingIn => _loggingIn;

    /// <summary>Raised when a login attempt ends (successful or not). AnalyticsLogger
    /// holds events until then so they carry this visitor's ids.</summary>
    public static event Action LoginFinished;

    static DocumentReference _sessionRef;
    static bool _loggingIn;
    static Task _pendingSignOut;   // previous visitor still being signed out

    // ---------- Consent ----------

    public static void SetConsent(bool accepted)
    {
        PlayerPrefs.SetInt(ConsentKey, accepted ? 1 : 0);
        PlayerPrefs.SetInt(AnsweredKey, 1);
        PlayerPrefs.Save();

        Debug.Log("POPIA consent " + (accepted ? "ACCEPTED - analytics on." : "DECLINED - analytics off."));
        ConsentChanged?.Invoke(accepted);

        if (accepted)
            LoginAfterConsent();   // fire and forget; never blocks the tour
        else
            EndSession();          // declined: stop recording
    }

    /// <summary>Forget the answer so the prompt shows again (e.g. from Settings).</summary>
    public static void ResetConsent()
    {
        PlayerPrefs.DeleteKey(ConsentKey);
        PlayerPrefs.DeleteKey(AnsweredKey);
        PlayerPrefs.Save();

        // Withdrawing consent / handing the headset to someone new stops
        // tracking immediately, and the next visitor gets their own ID.
        EndSession(SignOutOnNewVisitor);
    }

    // ---------- Startup hooks (no GameObject needed) ----------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        Application.quitting -= OnQuitting;
        Application.quitting += OnQuitting;

#if !UNITY_EDITOR
        Application.focusChanged -= OnFocusChanged;
        Application.focusChanged += OnFocusChanged;
#endif

        // No automatic login at launch: every visitor is asked on the POPIA
        // screen (MainMenuFlow "Ask Every Time"), and the login only starts
        // when THIS visitor presses Accept. Otherwise the previous visitor's
        // answer would create a session before the new one has agreed.
    }

    static void OnQuitting()
    {
        EndSession();
    }

    // On Quest, taking the headset off usually loses focus rather than quitting.
    static void OnFocusChanged(bool hasFocus)
    {
        if (!hasFocus)
            EndSession();
        else if (HasConsent && !IsLoggedIn)
            LoginAfterConsent();   // headset back on: start a fresh session
    }

    // ---------- Firebase login ----------

    static async void LoginAfterConsent()
    {
        if (!HasConsent) return;                 // safety net: never track without consent
        if (IsLoggedIn || _loggingIn) return;    // avoid duplicate sessions
        _loggingIn = true;

        try
        {
            // "End Tour" may still be closing the previous visitor's session -
            // wait so this visitor doesn't reuse their anonymous ID.
            if (_pendingSignOut != null) await _pendingSignOut;

            // Make sure Firebase is ready
            var status = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (status != DependencyStatus.Available)
            {
                Debug.LogError("Firebase dependency error: " + status);
                return;
            }

            var auth = FirebaseAuth.DefaultInstance;
            var db = FirebaseFirestore.DefaultInstance;

            // 1. Anonymous sign-in (reuses the existing user on this device)
            FirebaseUser user = auth.CurrentUser;
            if (user == null)
            {
                var result = await auth.SignInAnonymouslyAsync();
                user = result.User;
            }
            UserId = user.UserId;

            // 2. Create or update the user record
            var userRef = db.Collection("users").Document(UserId);
            var snapshot = await userRef.GetSnapshotAsync();

            var userData = new Dictionary<string, object>
            {
                { "lastLogin", FieldValue.ServerTimestamp },
                { "loginCount", FieldValue.Increment(1) },
                { "consentVersion", PolicyVersion },
                { "consentTimestamp", FieldValue.ServerTimestamp }
            };
            if (!snapshot.Exists)
                userData["firstLogin"] = FieldValue.ServerTimestamp;

            await userRef.SetAsync(userData, SetOptions.MergeAll);

            // 3. Create the session record (the login event for this visit)
            var newSession = db.Collection("sessions").Document();
            await newSession.SetAsync(new Dictionary<string, object>
            {
                { "userId", UserId },
                { "startTime", FieldValue.ServerTimestamp },
                { "device", SystemInfo.deviceModel },
                { "appVersion", Application.version },
                { "consentVersion", PolicyVersion }
            });

            // If consent was withdrawn while we were logging in, close it straight away
            _sessionRef = newSession;
            SessionId = newSession.Id;

            if (!HasConsent)
            {
                EndSession();
                return;
            }

            Debug.Log($"Logged in. User: {UserId}, Session: {SessionId}");
        }
        catch (Exception e)
        {
            Debug.LogError("Login failed (tour continues without tracking): " + e);
        }
        finally
        {
            _loggingIn = false;
            LoginFinished?.Invoke();
        }
    }

    /// <param name="signOut">True when a new visitor takes over: forget this
    /// visitor's anonymous ID so the next one gets a new user record.</param>
    public static void EndSession(bool signOut = false)
    {
        Task closing = Task.CompletedTask;

        if (_sessionRef != null)
        {
            closing = _sessionRef.UpdateAsync("endTime", FieldValue.ServerTimestamp);
            _sessionRef = null;
            SessionId = null;
        }

        if (signOut && UserId != null)   // only if this visitor actually logged in
        {
            UserId = null;
            _pendingSignOut = SignOutAfter(closing);
        }
    }

    // Sign out only AFTER the endTime write has gone through: Firestore sends
    // writes as the signed-in user, so signing out first would leave the
    // session without an end time (the rules only let its owner update it).
    static async Task SignOutAfter(Task closing)
    {
        try
        {
            // Don't hold the next visitor up if the headset is offline.
            Task first = await Task.WhenAny(closing, Task.Delay(5000));
            if (first == closing) await closing;
        }
        catch (Exception e) { Debug.LogWarning("Could not save session end time: " + e.Message); }

        try { FirebaseAuth.DefaultInstance.SignOut(); }
        catch (Exception e) { Debug.LogWarning("Sign-out failed: " + e.Message); }
        finally { _pendingSignOut = null; }
    }
}
