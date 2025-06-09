using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public RectTransform crosshair;
    public float sensitivity = 5.0f; // Sesuaikan sensitivitas
    public SerialReaderThreaded serialReader;

    private Vector2 screenCenter;
    private float yawOffset = 0f;

    void Start()
    {
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        crosshair.position = screenCenter;
    }

    void Update()
    {
        // Cek serialReader aktif dan running
        if (serialReader != null && serialReader.IsRunning())
        {
            float pitch, roll, yaw;
            bool shoot, reload, scope;

            serialReader.GetSensorData(out pitch, out roll, out yaw, out shoot, out reload, out scope);

            if (Input.GetKeyDown(KeyCode.Z))
            {
                yawOffset = yaw;
            }

            float offsetX = -(yaw - yawOffset) * sensitivity;
            float offsetY = pitch * sensitivity;

            Vector2 newPos = screenCenter + new Vector2(offsetX, offsetY);

            newPos.x = Mathf.Clamp(newPos.x, 0, Screen.width);
            newPos.y = Mathf.Clamp(newPos.y, 0, Screen.height);

            crosshair.position = newPos;
        }
        else
        {
            // Fallback pakai mouse biasa kalau serialReader tidak aktif/running
            Vector2 mousePos = Input.mousePosition;

            mousePos.x = Mathf.Clamp(mousePos.x, 0, Screen.width);
            mousePos.y = Mathf.Clamp(mousePos.y, 0, Screen.height);

            crosshair.position = mousePos;
        }
    }

    public Vector2 GetCrosshairPosition()
    {
        return crosshair.position;
    }
}
