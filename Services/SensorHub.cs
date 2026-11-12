using AirQualityMonitor.Models;
using AirQualityMonitor.Sensors;

namespace AirQualityMonitor.Services;

// "Сервер" платформи: підключає датчики, приймає від них дані й передає їх інтерфейсу.
public class SensorHub
{
    private readonly Database _db = new();
    private readonly Dictionary<int, ISensor> _connected = new();
    private readonly List<SensorInfo> _sensors = new();

    public IReadOnlyList<SensorInfo> Sensors => _sensors;

    // Нове вимірювання (викликається НЕ з UI-потоку!)
    public event Action<AirReading>? ReadingReceived;

    // Список датчиків змінився (додали / видалили / завантажили)
    public event Action? SensorsChanged;

    public async Task StartAsync()
    {
        await _db.InitAsync();

        var saved = await _db.GetSensorsAsync();

        // Перший запуск: додаємо кілька демо-датчиків, щоб було що показати
        if (saved.Count == 0)
        {
            foreach (var demo in CreateDemoSensors())
            {
                await _db.AddSensorAsync(demo);
                saved.Add(demo);
            }
        }

        foreach (var info in saved)
        {
            _sensors.Add(info);
            Connect(info);
        }

        SensorsChanged?.Invoke();
    }

    public async Task AddAsync(string name, SensorType type, string location)
    {
        var info = new SensorInfo { Name = name, Type = type, Location = location };

        await _db.AddSensorAsync(info);
        _sensors.Add(info);
        Connect(info);

        SensorsChanged?.Invoke();
    }

    public async Task RemoveAsync(SensorInfo info)
    {
        if (_connected.Remove(info.Id, out var sensor))
        {
            sensor.Disconnect();
            sensor.DataSent -= OnDataSent;
        }

        await _db.DeleteSensorAsync(info);
        _sensors.Remove(info);

        SensorsChanged?.Invoke();
    }

    private void Connect(SensorInfo info)
    {
        ISensor sensor = CreateSensor(info);
        sensor.DataSent += OnDataSent;
        sensor.Connect();
        _connected[info.Id] = sensor;
    }

    // Єдине місце, де створюється датчик.
    // Для реального заліза тут був би switch по info.Type:
    //   SensorType.ESP => new EspSensor(info), SensorType.Arduino => new ArduinoSensor(info) ...
    private static ISensor CreateSensor(SensorInfo info) => new RandomSensor(info);

    private void OnDataSent(AirReading reading) => ReadingReceived?.Invoke(reading);

    private static List<SensorInfo> CreateDemoSensors() => new()
    {
        new SensorInfo { Name = "ESP-Кухня", Type = SensorType.ESP, Location = "Кухня" },
        new SensorInfo { Name = "Arduino-Офіс", Type = SensorType.Arduino, Location = "Офіс" },
        new SensorInfo { Name = "RPi-Вулиця", Type = SensorType.RaspberryPi, Location = "Двір" },
    };
}
