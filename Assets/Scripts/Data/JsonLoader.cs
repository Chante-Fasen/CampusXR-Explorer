using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// Reads campus_data.json from StreamingAssets and loads it into
// memory on app startup. Uses UnityWebRequest instead of File.ReadAllText
// because on Android/Quest, StreamingAssets files are compressed inside
// the APK and cannot be read with normal file system calls.

public class JsonLoader : MonoBehaviour
{
    public static CampusData LoadedData;

    void Awake()
    {
        StartCoroutine(LoadJsonData());
    }

    IEnumerator LoadJsonData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "campus_data.json");

        // On Android, paths need "jar:file://" handling, which
        // UnityWebRequest manages automatically. On Windows/Mac/Editor,
        // UnityWebRequest also works fine with normal file paths.
        UnityWebRequest request = UnityWebRequest.Get(filePath);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonText = request.downloadHandler.text;
            LoadedData = JsonUtility.FromJson<CampusData>(jsonText);

            Debug.Log("campus_data.json loaded successfully. Campus: " + LoadedData.campus);
        }
        else
        {
            Debug.LogError("Failed to load campus_data.json: " + request.error);
        }
    }
}