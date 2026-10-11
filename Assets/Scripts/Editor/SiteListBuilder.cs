using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor-only helper. Menu: CampusXR > Rebuild Site List (Tour360)
///
/// Finds every 360 photo sphere in the open scene and fills
/// Managers > Site Arrival Handler > Sites with one entry per sphere:
///   Site Id     = the sphere's name without spaces ("Alpha Entrance" -> "AlphaEntrance")
///   Site Object = the sphere
///
/// "Reception Area" is put first, so it's the default site when Tour360 is
/// opened without a site picked on the map. Run it again whenever spheres are
/// added, removed or renamed, then save the scene.
/// </summary>
public static class SiteListBuilder
{
    [MenuItem("CampusXR/Rebuild Site List (Tour360)")]
    public static void Rebuild()
    {
        SiteArrivalHandler handler = Object.FindAnyObjectByType<SiteArrivalHandler>(FindObjectsInactive.Include);
        if (handler == null)
        {
            EditorUtility.DisplayDialog("Rebuild Site List", "No Site Arrival Handler found. Open the Tour360 scene first.", "OK");
            return;
        }

        var scene = handler.gameObject.scene;

        // Every object with a built-in Sphere mesh = a 360 photo site (inactive ones included).
        var spheres = new List<GameObject>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (MeshFilter meshFilter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (meshFilter.sharedMesh != null
                    && meshFilter.sharedMesh.name == "Sphere"
                    && meshFilter.GetComponent<MeshRenderer>() != null)
                {
                    spheres.Add(meshFilter.gameObject);
                }
            }
        }

        if (spheres.Count == 0)
        {
            EditorUtility.DisplayDialog("Rebuild Site List", "No photo spheres found in this scene.", "OK");
            return;
        }

        // Fix the known typo so the id comes out right.
        foreach (GameObject sphere in spheres)
        {
            if (sphere.name.StartsWith("Gamme "))
            {
                Undo.RecordObject(sphere, "Rename sphere");
                sphere.name = "Gamma " + sphere.name.Substring("Gamme ".Length);
            }
        }

        // Reception Area first (default site), the rest alphabetical.
        spheres = spheres
            .OrderBy(s => s.name == "Reception Area" ? 0 : 1)
            .ThenBy(s => s.name)
            .ToList();

        Undo.RecordObject(handler, "Rebuild Site List");
        handler.sites = spheres
            .Select(s => new SiteArrivalHandler.SiteEntry { siteId = MakeId(s.name), siteObject = s })
            .ToList();
        EditorUtility.SetDirty(handler);
        EditorSceneManager.MarkSceneDirty(scene);

        // Warn about duplicate ids (two spheres that only differ by spaces/symbols).
        var duplicates = handler.sites.GroupBy(e => e.siteId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

        string list = string.Join("\n", handler.sites.Select(e => $"{e.siteId}  ←  {e.siteObject.name}"));
        Debug.Log($"Site list rebuilt with {handler.sites.Count} sites:\n{list}");

        string message = $"Added {handler.sites.Count} sites to the Site Arrival Handler.\n\n"
                         + "The full list (id ← sphere) is in the Console.\n\nSave the scene to keep it.";
        if (duplicates.Count > 0)
        {
            message += "\n\nWARNING - duplicate ids, rename these spheres: " + string.Join(", ", duplicates);
        }
        EditorUtility.DisplayDialog("Rebuild Site List", message, "OK");
    }

    // "Alpha Entrance" -> "AlphaEntrance", "Cafeteria 1" -> "Cafeteria1"
    private static string MakeId(string name)
    {
        return new string(name.Where(char.IsLetterOrDigit).ToArray());
    }
}
