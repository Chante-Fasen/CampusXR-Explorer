using System.IO;
using UnityEngine;

// Reads campus_data.json from StreamingAssets and loads it into
// memory on app startup. Other scripts can then access the data
// via JsonLoader.LoadedData

public class JsonLoader : MonoBehaviour
{
    // Static so any other script can access the loaded data
    // without needing a reference to this specific GameObject
    public static CampusData LoadedData;

    void Awake()
    {
        // Build the full path to campus_data.json inside StreamingAssets
        string filePath = Path.Combine(Application.streamingAssetsPath, "campus_data.json");

        // Check the file actually exists before trying to read it
        if (File.Exists(filePath))
        {
            // Read the raw JSON text from the file
            string jsonText = File.ReadAllText(filePath);

            // Convert the JSON text into our CampusData object
            LoadedData = JsonUtility.FromJson<CampusData>(jsonText);

            Debug.Log("campus_data.json loaded successfully. Campus: " + LoadedData.campus);
        }
        else
        {
            Debug.LogError("campus_data.json not found at: " + filePath);
        }
    }
}