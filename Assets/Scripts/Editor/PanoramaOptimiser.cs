using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only helper. Menu: CampusXR > Optimise 360 Photos for Quest
///
/// Sets every image in Assets/Images/Panoramic 360 Photos to settings that
/// load much faster on the Quest:
///   - Android max size 4096 (was 8192)
///   - ASTC 6x6 compression
///   - No mipmaps (a 360 photo is always seen up close from the centre, so
///     the smaller mipmap levels are never used - they only cost memory)
///
/// That's roughly 5x less image data to load when Tour360 opens. Run it
/// again after adding new 360 photos. Unity re-imports the images, which
/// can take a few minutes.
/// </summary>
public static class PanoramaOptimiser
{
    private const string Folder = "Assets/Images/Panoramic 360 Photos";
    private const int AndroidMaxSize = 4096;

    [MenuItem("CampusXR/Optimise 360 Photos for Quest")]
    public static void Optimise()
    {
        if (!AssetDatabase.IsValidFolder(Folder))
        {
            EditorUtility.DisplayDialog("Optimise 360 Photos", $"Folder not found: {Folder}", "OK");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { Folder });
        if (!EditorUtility.DisplayDialog("Optimise 360 Photos",
                $"Update import settings for {guids.Length} photos in\n{Folder}?\n\nAndroid: max {AndroidMaxSize}, ASTC 6x6, no mipmaps.\nRe-importing can take a few minutes.",
                "Optimise", "Cancel"))
        {
            return;
        }

        int changed = 0;
        try
        {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar("Optimise 360 Photos", path, (float)i / guids.Length);

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                importer.mipmapEnabled = false;

                TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
                android.overridden = true;
                android.maxTextureSize = AndroidMaxSize;
                android.format = TextureImporterFormat.ASTC_6x6;
                android.compressionQuality = 50;
                importer.SetPlatformTextureSettings(android);

                importer.SaveAndReimport();
                changed++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }

        Debug.Log($"Optimised {changed} 360 photos for Quest (Android max {AndroidMaxSize}, ASTC 6x6, no mipmaps).");
        EditorUtility.DisplayDialog("Optimise 360 Photos", $"Done - {changed} photos updated.\n\nRebuild the APK to see the faster loading on the Quest.", "OK");
    }
}
