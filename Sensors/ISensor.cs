using AirQualityMonitor.Models;

namespace AirQualityMonitor.Sensors;

// Спільний інтерфейс для всіх датчиків (ESP, Raspberry Pi, Arduino...).
// Датчик сам відправляє дані через подію DataSent.
public interface ISensor
{
    SensorInfo Info { get; }

    event Action<AirReading>? DataSent;

    void Connect();     // почати відправляти дані
    void Disconnect();  // припинити
}
