using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// Reads campus_data.json from StreamingAssets and loads it into
// memory on app startup. Other scripts access the data via
// JsonLoader.LoadedData once IsLoaded is true.
public class JsonLoader : MonoBehaviour
{
    // Static so any other script can access the loaded data
    // without needing a reference to this specific GameObject
    public static CampusData LoadedData;
    public static bool IsLoaded { get { return LoadedData != null; } }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(LoadCampusData());
    }

    IEnumerator LoadCampusData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "campus_data.json");
        string jsonText = null;

        // On Android, StreamingAssets is compressed inside the APK and is
        // only reachable through a jar:file:// URL, so System.IO file access
        // fails there. That path contains "://" and needs UnityWebRequest.
        if (filePath.Contains("://"))
        {
            using (UnityWebRequest req = UnityWebRequest.Get(filePath))
            {
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to read campus_data.json: " + req.error);
                    yield break;
                }
                jsonText = req.downloadHandler.text;
            }
        }
        else
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError("campus_data.json not found at: " + filePath);
                yield break;
            }
            jsonText = File.ReadAllText(filePath);
        }

        LoadedData = JsonUtility.FromJson<CampusData>(jsonText);
        Debug.Log("campus_data.json loaded successfully. Campus: " + LoadedData.campus);
    }
}