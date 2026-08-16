using System;
using System.Collections.Generic;

// Blueprint matching campus_data.json structure.
// Field names must match the JSON exactly (case-sensitive).

[Serializable]
public class CampusData
{
    public string campus;
    public List<LocationData> locations;
}

[Serializable]
public class LocationData
{
    public string id;              // e.g. "computer_lab"
    public string scene_name;      // must match actual Unity scene name
    public string label;           // display name shown to user
    public string thumbnail_asset;
    public List<HotspotData> hotspots;
}

[Serializable]
public class HotspotData
{
    public string id;
    public Vector3Data world_position;
    public string overlay_type;    // "text" / "image" / "audio" / "video"
    public string content;
}

[Serializable]
public class Vector3Data
{
    // Custom x/y/z class - JsonUtility struggles with Unity's native Vector3
    public float x;
    public float y;
    public float z;
}