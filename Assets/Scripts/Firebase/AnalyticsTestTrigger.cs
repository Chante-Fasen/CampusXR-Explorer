using UnityEngine;

// TEMPORARY test script - manually triggers analytics events
// so we can confirm Firestore is receiving data correctly.
// Delete this once real hotspot/scene scripts call AnalyticsLogger
// directly.

public class AnalyticsTestTrigger : MonoBehaviour
{
    private AnalyticsLogger logger;

    void Start()
    {
        logger = GetComponent<AnalyticsLogger>();

        // TEMPORARY: manually give consent for testing purposes only.
        // Real consent will come from Thierry's POPIA consent screen.
        PlayerPrefs.SetInt("AnalyticsConsentGiven", 1);

        // Fire a test facility_view event immediately on Play
        logger.LogFacilityView("TestScene_ComputerLab");

        // Fire a test hotspot_trigger event 2 seconds later
        Invoke(nameof(TriggerTestHotspot), 2f);
    }

    void TriggerTestHotspot()
    {
        logger.LogHotspotTrigger("TestScene_ComputerLab", "hs_001");
    }
}