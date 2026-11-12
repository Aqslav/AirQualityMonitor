using AirQualityMonitor.Models;

namespace AirQualityMonitor.Sensors;

// Єдина реалізація ISensor: генерує псевдовипадкові, але правдоподібні дані.
// Значення змінюються плавно (невеликий крок від попереднього), як у справжніх датчиків.
public class RandomSensor : ISensor
{
    private readonly Random _random = new();
    private System.Threading.Timer? _timer;

    private double _pm25;
    private double _co2;
    private double _temperature;
    private double _humidity;

    public SensorInfo Info { get; }
    public event Action<AirReading>? DataSent;

    public RandomSensor(SensorInfo info)
    {
        Info = info;

        // У кожного датчика свої початкові значення
        _pm25 = _random.Next(5, 30);
        _co2 = _random.Next(450, 900);
        _temperature = _random.Next(18, 25);
        _humidity = _random.Next(35, 60);
    }

    public void Connect()
    {
        // Кожні 2 секунди відправляємо нове вимірювання
        _timer = new System.Threading.Timer(_ => Send(), null, 0, 2000);
    }

    public void Disconnect()
    {
        _timer?.Dispose();
        _timer = null;
    }

    private void Send()
    {
        _pm25 = Step(_pm25, 3, 1, 150);
        _co2 = Step(_co2, 40, 400, 2000);
        _temperature = Step(_temperature, 0.3, 10, 35);
        _humidity = Step(_humidity, 1, 20, 80);

        // PM10 завжди трохи більше за PM2.5
        double pm10 = _pm25 * (1.3 + _random.NextDouble() * 0.5);

        DataSent?.Invoke(new AirReading(
            Info.Id,
            DateTime.Now,
            Math.Round(_pm25, 1),
            Math.Round(pm10, 1),
            Math.Round(_co2),
            Math.Round(_temperature, 1),
            Math.Round(_humidity)));
    }

    // Зсуває значення на випадкову величину в межах ±maxDelta і тримає в діапазоні [min, max]
    private double Step(double value, double maxDelta, double min, double max)
    {
        double delta = (_random.NextDouble() * 2 - 1) * maxDelta;
        return Math.Clamp(value + delta, min, max);
    }
}
