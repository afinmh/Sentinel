using UnityEngine;

public class ZoomCamera : MonoBehaviour
{
    public Camera cam;
    public Crosshair crosshairController;

    public float minFOV = 5f;
    public float maxFOV = 60f;
    public float zoomSpeed = 10f;
    public float rotationSpeed = 10f;

    private bool isZoomed = false;
    private bool isZooming = false;

    private float targetFOV;
    private Quaternion targetRotation;

    private float originalFOV;
    private Quaternion originalRotation;

    // Referensi ke serial reader
    private SerialReaderThreaded serialReader;

    void Start()
    {
        originalFOV = cam.fieldOfView;
        originalRotation = cam.transform.rotation;

        // Cari SerialReaderThreaded di scene (pastikan ada)
        serialReader = FindObjectOfType<SerialReaderThreaded>();
    }

void Update()
{
    bool scopePressed = false;

    if (serialReader != null && serialReader.IsRunning())
    {
        bool s;
        lock (serialReader)
        {
            s = serialReader.scope;
        }
        scopePressed = s;
    }
    else
    {
        scopePressed = Input.GetMouseButton(1);
    }

    if (scopePressed && !isZoomed && !isZooming)
    {
        ZoomIn();
    }
    else if (!scopePressed && isZoomed && !isZooming)
    {
        ZoomOut();
    }

    if (isZooming)
    {
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        if (Mathf.Abs(cam.fieldOfView - targetFOV) < 0.1f &&
            Quaternion.Angle(cam.transform.rotation, targetRotation) < 0.5f)
        {
            cam.fieldOfView = targetFOV;
            cam.transform.rotation = targetRotation;
            isZooming = false;
        }
    }
}


    void ZoomIn()
    {
        originalFOV = cam.fieldOfView;
        originalRotation = cam.transform.rotation;

        Vector2 crosshairPos = crosshairController.GetCrosshairPosition();
        Ray ray = cam.ScreenPointToRay(crosshairPos);
        targetRotation = Quaternion.LookRotation(ray.direction);

        targetFOV = minFOV;

        isZoomed = true;
        isZooming = true;
    }

    void ZoomOut()
    {
        targetRotation = originalRotation;
        targetFOV = originalFOV;

        isZoomed = false;
        isZooming = true;
    }

    public bool IsZoomingOrZoomed()
    {
        return isZoomed || isZooming;
    }
}
