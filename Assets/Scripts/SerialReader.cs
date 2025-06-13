using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System.Globalization;

public class SerialReaderThreaded : MonoBehaviour
{
    SerialPort serial;
    Thread thread;
    bool running = false;

    public float roll, pitch, yaw;
    public bool shoot, reload, scope;

    private readonly object dataLock = new object();

    void Start()
    {
        serial = new SerialPort("COM9", 38400);
        serial.ReadTimeout = 1000; // Timeout supaya ReadLine ga hang forever

        try
        {
            //serial.Open();
            //running = true;
            //thread = new Thread(ReadSerial);
            //thread.Start();
        }
        catch
        {
            Debug.LogError("Failed to open serial port");
        }
    }

    void ReadSerial()
    {
        while (running)
        {
            try
            {
                string line = serial.ReadLine();
                string[] parts = line.Split(',');

                if (parts.Length >= 6)
                {
                    // Pastikan parsing pakai InvariantCulture agar titik desimal dikenali
                    float parsedRoll = float.Parse(parts[0], CultureInfo.InvariantCulture);
                    float parsedPitch = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float parsedYaw = float.Parse(parts[2], CultureInfo.InvariantCulture);

                    bool parsedShoot = parts[3].Trim() == "1";
                    bool parsedReload = parts[4].Trim() == "1";
                    bool parsedScope = parts[5].Trim() == "1";

                    lock (dataLock)
                    {
                        roll = parsedRoll;
                        pitch = parsedPitch;
                        yaw = parsedYaw;

                        shoot = parsedShoot;
                        reload = parsedReload;
                        scope = parsedScope;
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Opsional: log error untuk debug
                // Debug.LogWarning("Serial read error: " + ex.Message);
            }
        }
    }

    // Method untuk dapatkan data sensor dan tombol dengan aman dari thread utama
    public void GetSensorData(out float outRoll, out float outPitch, out float outYaw,
                              out bool outShoot, out bool outReload, out bool outScope)
    {
        lock (dataLock)
        {
            outRoll = roll;
            outPitch = pitch;
            outYaw = yaw;
            outShoot = shoot;
            outReload = reload;
            outScope = scope;
        }
    }

    void OnDestroy()
    {
        running = false;
        if (thread != null && thread.IsAlive)
            thread.Join();
        if (serial != null && serial.IsOpen)
            serial.Close();
    }

    public bool IsRunning()
    {
        return running;
    }
    
}
