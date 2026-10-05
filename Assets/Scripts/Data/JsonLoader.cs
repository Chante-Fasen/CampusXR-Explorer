using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// Reads campus_data.json from StreamingAssets once and keeps it in memory
// for the whole session (JsonLoader.LoadedData).
//
// Uses UnityWebRequest instead of File.ReadAllText because on Android/Quest
// StreamingAssets is packed inside the APK and can't be read with normal
// file calls.
//
// This creates itself automatically before the first scene loads, so it
// does NOT need to be placed in any scene, and it works no matter which
// scene you press Play from in the Editor. If a JsonLoader component is
// also placed in a scene, that extra copy simply removes itself.
//
// Other scripts can either check JsonLoader.LoadedData != null, or
// subscribe to JsonLoader.DataLoaded to be told when it's ready.

public class JsonLoader : MonoBehaviour
{
    public static CampusData LoadedData { get; private set; }

    // Fired once, when campus_data.json has been loaded successfully.
    public static event Action DataLoaded;

    private static JsonLoader instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateAutomatically()
    {
        if (instance != null) return;

        GameObject loaderObject = new GameObject("[JsonLoader]");
        loaderObject.AddComponent<JsonLoader>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            // Only remove this component, not the whole GameObject, in case
            // it was added to an object that holds other scripts too.
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (LoadedData == null)
        {
            StartCoroutine(LoadJsonData());
        }
    }

    private IEnumerator LoadJsonData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "campus_data.json");

        // Android already gives a "jar:file://..." URL. On Windows/Mac/Editor
        // it's a plain path, so turn it into a proper file:/// URL.
        string url = filePath.Contains("://") ? filePath : new Uri(filePath).AbsoluteUri;

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load campus_data.json: " + request.error);
                yield break;
            }

            CampusData data = JsonUtility.FromJson<CampusData>(request.downloadHandler.text);

            if (data == null || data.locations == null)
            {
                Debug.LogError("campus_data.json loaded but could not be read - check it matches CampusData.cs.");
                yield break;
            }

            LoadedData = data;
            Debug.Log($"campus_data.json loaded successfully. Campus: {data.campus} ({data.locations.Count} locations)");

            DataLoaded?.Invoke();
        }
    }
}
