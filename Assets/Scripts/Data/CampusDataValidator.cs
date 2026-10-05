using System.Collections.Generic;
using UnityEngine;

public static class CampusDataValidator
{
    private const int MaxHotspotsPerLocation = 8;
 
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Hook()
    {
        JsonLoader.DataLoaded -= OnDataLoaded;
        JsonLoader.DataLoaded += OnDataLoaded;
    }
 
    private static void OnDataLoaded()
    {
        int problems = Validate(JsonLoader.LoadedData);
        if (problems == 0)
        {
            Debug.Log("CampusDataValidator: campus_data.json looks good.");
        }
    }
 
    /// <summary>Returns the number of problems found (each one is logged).</summary>
    public static int Validate(CampusData data)
    {
        if (data == null || data.locations == null)
        {
            Debug.LogError("CampusDataValidator: no data to check.");
            return 1;
        }
 
        int problems = 0;
        HashSet<string> ids = new HashSet<string>();
 
        foreach (LocationData location in data.locations)
        {
            if (location == null) { problems++; Debug.LogError("CampusDataValidator: empty entry in 'locations'."); continue; }
 
            if (string.IsNullOrEmpty(location.id))
            {
                problems++;
                Debug.LogError($"CampusDataValidator: a location ('{location.label}') has no id.");
            }
            else if (!ids.Add(location.id))
            {
                problems++;
                Debug.LogError($"CampusDataValidator: duplicate location id '{location.id}'.");
            }
        }
 
        foreach (LocationData location in data.locations)
        {
            if (location == null) continue;
 
            int hotspotCount = location.hotspots != null ? location.hotspots.Count : 0;
            if (hotspotCount > MaxHotspotsPerLocation)
            {
                problems++;
                Debug.LogWarning($"CampusDataValidator: '{location.id}' has {hotspotCount} hotspots (limit is {MaxHotspotsPerLocation}).");
            }
 
            if (location.hotspots != null)
            {
                foreach (HotspotData hotspot in location.hotspots)
                {
                    if (hotspot == null) continue;
 
                    if (string.IsNullOrEmpty(hotspot.title) && string.IsNullOrEmpty(hotspot.content))
                    {
                        problems++;
                        Debug.LogWarning($"CampusDataValidator: hotspot '{hotspot.id}' in '{location.id}' has no title or content.");
                    }
 
                    if (!string.IsNullOrEmpty(hotspot.target_id) && !ids.Contains(hotspot.target_id))
                    {
                        problems++;
                        Debug.LogError($"CampusDataValidator: hotspot '{hotspot.id}' in '{location.id}' has target_id '{hotspot.target_id}' which doesn't exist.");
                    }
                }
            }
 
            if (location.links != null)
            {
                foreach (LinkData link in location.links)
                {
                    if (link == null) continue;
 
                    if (string.IsNullOrEmpty(link.target_id) || !ids.Contains(link.target_id))
                    {
                        problems++;
                        Debug.LogError($"CampusDataValidator: a link in '{location.id}' points to '{link.target_id}' which doesn't exist.");
                    }
                }
            }
        }
 
        return problems;
    }
}
 
