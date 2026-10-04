using System;
using System.Collections.Generic;

// Blueprint matching campus_data.json structure.
// Field names must match the JSON exactly (case-sensitive).
//
// One LocationData = one place the player can stand in the tour
// (one 360 photo / site). In Tour360 every site lives in the same Unity
// scene, so locations are identified by their "id", which must match the
// siteId used by SiteArrivalHandler and the map buttons
// (e.g. "ReceptionSite", "AlphaSite", "GammaSite").

[Serializable]
public class CampusData
{
    public string campus;
    public List<LocationData> locations = new List<LocationData>();
}

[Serializable]
public class LocationData
{
    public string id;              // site id, e.g. "ReceptionSite" - must match SiteArrivalHandler's siteId exactly
    public string scene_name;      // Unity scene the site lives in, e.g. "Tour360"
    public string label;           // display name shown to user, e.g. "Reception"
    public string thumbnail_asset; // image used for this location in menus/map
    public List<HotspotData> hotspots = new List<HotspotData>();
    public List<LinkData> links = new List<LinkData>(); // navigation arrows to other sites
}

[Serializable]
public class HotspotData
{
    public string id;
    public Vector3Data world_position;
    public string overlay_type;    // "text" / "image" / "audio" / "video"
    public string content;
}

// A navigation arrow shown at a location that takes the player to another
// location (Street View style "step forward").
[Serializable]
public class LinkData
{
    public string target_id;       // id of the location this arrow leads to, e.g. "AlphaSite"
    public float yaw;              // direction of the arrow in degrees: 0 = straight ahead, 90 = right, 180 = behind, 270 = left
    public string label;           // optional text shown with the arrow, e.g. "To Alpha"
}

[Serializable]
public class Vector3Data
{
    // Custom x/y/z class - JsonUtility struggles with Unity's native Vector3
    public float x;
    public float y;
    public float z;
}
