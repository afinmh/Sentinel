using System;
using UnityEngine;
using SocketIOClient;

public class IMUReceiver : MonoBehaviour
{
    public string serverUrl = "http://localhost:3000"; // Ganti dengan IP server kamu
    public string eventName = "message";

    public float yaw;
    public float pitch;
    public float roll;

    private SocketIO socket;

    [Serializable]
    public class ImuData
    {
        public float yaw;
        public float pitch;
        public float roll;
    }

    async void Start()
    {
        socket = new SocketIO(serverUrl, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        socket.OnConnected += (_, _) =>
        {
            Debug.Log("✅ WebSocket Connected!");
        };

        socket.OnDisconnected += (_, _) =>
        {
            Debug.LogWarning("🔌 WebSocket Disconnected!");
        };

        socket.On(eventName, response =>
        {
            try
            {
                string json = response.GetValue<string>();
                ImuData data = JsonUtility.FromJson<ImuData>(json);

                yaw = data.yaw;
                pitch = data.pitch;
                roll = data.roll;

                Debug.Log($"Yaw: {yaw}, Pitch: {pitch}, Roll: {roll}");
            }
            catch (Exception ex)
            {
                Debug.LogError("❌ Gagal parsing: " + ex.Message);
            }
        });

        await socket.ConnectAsync();
    }

    async void OnDestroy()
    {
        if (socket != null)
        {
            await socket.DisconnectAsync();
            socket.Dispose();
        }
    }
}
