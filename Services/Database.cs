using AirQualityMonitor.Models;
using SQLite;

namespace AirQualityMonitor.Services;

// Проста обгортка над SQLite: зберігає список датчиків між запусками.
// Запити в sqlite-net параметризовані, тому SQL-ін'єкції неможливі.
public class Database
{
    private SQLiteAsyncConnection _db = null!;

    public async Task InitAsync()
    {
        string path = Path.Combine(FileSystem.AppDataDirectory, "sensors.db3");
        _db = new SQLiteAsyncConnection(path);
        await _db.CreateTableAsync<SensorInfo>();
    }

    public Task<List<SensorInfo>> GetSensorsAsync() => _db.Table<SensorInfo>().ToListAsync();

    public Task AddSensorAsync(SensorInfo sensor) => _db.InsertAsync(sensor); // заповнює sensor.Id

    public Task DeleteSensorAsync(SensorInfo sensor) => _db.DeleteAsync(sensor);
}
