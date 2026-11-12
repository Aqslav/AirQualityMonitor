using AirQualityMonitor.Models;

namespace AirQualityMonitor.Services;

// Сховище датчиків. У застосунку це SQLite (Database), у тестах — проста підміна в пам'яті.
public interface ISensorStorage
{
    Task InitAsync();
    Task<List<SensorInfo>> GetSensorsAsync();
    Task AddSensorAsync(SensorInfo sensor);   // має заповнити sensor.Id
    Task DeleteSensorAsync(SensorInfo sensor);
}
