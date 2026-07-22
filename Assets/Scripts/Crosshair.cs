using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public RectTransform crosshair;
    public float sensitivity = 5.0f;
    public IMUReceiver imuReceiver;

    private Vector2 screenCenter;
    private float rollOffset = 0f;
    private float pitchOffset = 0f;
    private bool imuActive = false;

    void Start()
    {
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        crosshair.position = screenCenter;
        Cursor.visible = false;
    }

    void Update()
    {
        if (imuReceiver != null)
        {
            float pitch = imuReceiver.pitch;
            float roll = imuReceiver.roll;

            imuActive = Mathf.Abs(pitch) > 0.01f || Mathf.Abs(roll) > 0.01f;

            if (imuActive)
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    pitchOffset = pitch;
                    rollOffset = roll;
                }

                float offsetX = (roll - rollOffset) * sensitivity;
                float offsetY = -(pitch - pitchOffset) * sensitivity;

                Vector2 newPos = screenCenter + new Vector2(offsetX, offsetY);
                newPos.x = Mathf.Clamp(newPos.x, 0, Screen.width);
                newPos.y = Mathf.Clamp(newPos.y, 0, Screen.height);

                crosshair.position = newPos;
            }
            else
            {
                // Gunakan mouse hanya jika IMU tidak aktif
                crosshair.position = Input.mousePosition;
            }
        }
        else
        {
            // Jika tidak ada IMU, gunakan mouse
            crosshair.position = Input.mousePosition;
        }

        // Mouse click tetap aktif (terlepas dari IMU)
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("🖱️ Klik Kiri di " + crosshair.position);
        }
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("🖱️ Klik Kanan di " + crosshair.position);
        }
    }

    public Vector2 GetCrosshairPosition()
    {
        return crosshair.position;
    }
}
