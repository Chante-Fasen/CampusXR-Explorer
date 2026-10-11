using UnityEngine;

/// <summary>
/// Keeps the player exactly in the centre of the 360 photo.
///
/// Every frame it moves all the site spheres (from SiteArrivalHandler) to the
/// headset camera's position. Because this script also moves the object it's
/// on (Managers), the arrows, hotspots and popup - which are created as
/// children of Managers - move along with the photo and stay lined up.
///
/// Only POSITION follows the head, never rotation, so looking around still
/// works normally. Leaning or stepping no longer moves you off-centre or out
/// of the sphere, and the photo never looks stretched.
///
/// SETUP (Tour360): add this component to the "Managers" object.
///
/// NOTE for placing hotspots: positions in campus_data.json are measured from
/// the centre of the sphere, which is the player's eyes:
///   x = right(+) / left(-), y = up(+) / down(-) from eye level, z = forward(+) / back(-)
/// In the Editor the spheres sit at (0,0,0), so a temporary camera at (0,0,0)
/// sees exactly what the player sees, and an object's Position can be copied
/// straight into world_position.
/// </summary>
[DefaultExecutionOrder(1000)] // run after other scripts move the camera
public class TourCentreFollower : MonoBehaviour
{
    [Tooltip("Leave empty to find it automatically in this scene.")]
    public SiteArrivalHandler siteArrivalHandler;

    [Tooltip("Also move this object (Managers) so arrows, hotspots and the popup follow the photo.")]
    public bool moveThisObjectToo = true;

    private void Awake()
    {
        if (siteArrivalHandler == null)
        {
            siteArrivalHandler = FindAnyObjectByType<SiteArrivalHandler>();
        }
    }

    private void OnEnable()
    {
        // The headset pose is updated again just before rendering, so follow
        // it there too - this avoids the photo lagging a frame behind the head.
        Application.onBeforeRender += Follow;
        Follow();
    }

    private void OnDisable()
    {
        Application.onBeforeRender -= Follow;
    }

    private void LateUpdate()
    {
        Follow();
    }

    private void Follow()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 eye = cam.transform.position;

        if (siteArrivalHandler != null)
        {
            foreach (SiteArrivalHandler.SiteEntry entry in siteArrivalHandler.sites)
            {
                if (entry.siteObject != null)
                {
                    entry.siteObject.transform.position = eye;
                }
            }
        }

        if (moveThisObjectToo)
        {
            transform.position = eye;
        }
    }
}
