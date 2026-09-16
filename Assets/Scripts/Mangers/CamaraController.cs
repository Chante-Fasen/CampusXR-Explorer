using UnityEngine;
using UnityEngine.UI; // Required for interacting with UI elements

public class CameraController : MonoBehaviour
{
    private float rotateSped = 300.0f;

    private float zoomSpeed = 600.0f;
    private float zoomAmount = 0.0f;

    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x + Input.GetAxis("Mouse Y") * rotateSped * Time.deltaTime, transform.localEulerAngles.y + Input.GetAxis("Mouse X") * rotateSped * Time.deltaTime, 0);
        }

        if(Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            zoomAmount = Mathf.Clamp(zoomAmount + Input.GetAxis("Mouse Y") * zoomSpeed * Time.deltaTime, -5.0f, 5.0f);
            Camera.main.transform.localPosition = new Vector3(0, 0, zoomAmount);
        }
    }
}
