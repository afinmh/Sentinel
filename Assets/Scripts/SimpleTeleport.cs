using UnityEngine;

public class SimpleTeleport : MonoBehaviour
{
    public Transform player;       // Drag player ke sini
    public Transform pointA;       // Titik awal
    public Transform pointB;       // Titik tujuan
    [SerializeField] private ZoomCamera zoomCamera; // Tambahkan referensi ke ZoomCamera

    private bool atPointA = true;  // Status posisi sekarang

    void Update()
    {
        // Cek jika sedang zoom, tidak bisa teleport
        if (zoomCamera != null && zoomCamera.IsZoomingOrZoomed())
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (atPointA)
            {
                player.position = pointB.position;
                Vector3 rot = player.eulerAngles;
                rot.y = 180f;
                player.eulerAngles = rot;
            }
            else
            {
                player.position = pointA.position;
                Vector3 rot = player.eulerAngles;
                rot.y = 0f;
                player.eulerAngles = rot;
            }

            atPointA = !atPointA; // toggle status
            Debug.Log("Teleported to " + (atPointA ? "Point A" : "Point B"));
        }
    }
}
