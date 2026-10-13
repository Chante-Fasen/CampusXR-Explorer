using System;
using UnityEngine;

/// <summary>
/// Stores the visitor's POPIA analytics consent.
///
/// The answer is saved in PlayerPrefs under "AnalyticsConsentGiven"
/// (1 = accepted, 0 = declined) - the same key AnalyticsLogger already
/// checks before sending anything to Firebase. Declining never blocks the
/// tour; it only switches analytics off.
/// </summary>
public static class ConsentManager
{
    public const string ConsentKey = "AnalyticsConsentGiven";   // read by AnalyticsLogger
    public const string AnsweredKey = "AnalyticsConsentAnswered";

    /// <summary>Raised whenever the visitor accepts or declines.</summary>
    public static event Action<bool> ConsentChanged;

    /// <summary>True if the visitor accepted analytics.</summary>
    public static bool HasConsent => PlayerPrefs.GetInt(ConsentKey, 0) == 1;

    /// <summary>True if the visitor has answered the prompt at least once on this device.</summary>
    public static bool HasAnswered => PlayerPrefs.GetInt(AnsweredKey, 0) == 1;

    public static void SetConsent(bool accepted)
    {
        PlayerPrefs.SetInt(ConsentKey, accepted ? 1 : 0);
        PlayerPrefs.SetInt(AnsweredKey, 1);
        PlayerPrefs.Save();

        Debug.Log("POPIA consent " + (accepted ? "ACCEPTED - analytics on." : "DECLINED - analytics off."));
        ConsentChanged?.Invoke(accepted);
    }

    /// <summary>Forget the answer so the prompt shows again (e.g. from Settings).</summary>
    public static void ResetConsent()
    {
        PlayerPrefs.DeleteKey(ConsentKey);
        PlayerPrefs.DeleteKey(AnsweredKey);
        PlayerPrefs.Save();
    }
}
