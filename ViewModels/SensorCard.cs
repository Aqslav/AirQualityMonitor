using AirQualityMonitor.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AirQualityMonitor.ViewModels;

// Картка одного датчика на клієнтській сторінці. Оновлюється при кожному новому вимірюванні.
public partial class SensorCard : ObservableObject
{
    public SensorInfo Info { get; }

    [ObservableProperty] private string pm25 = "—";
    [ObservableProperty] private string pm10 = "—";
    [ObservableProperty] private string co2 = "—";
    [ObservableProperty] private string temperature = "—";
    [ObservableProperty] private string humidity = "—";
    [ObservableProperty] private string updated = "Очікування даних…";
    [ObservableProperty] private string quality = "";
    [ObservableProperty] private Color qualityColor = Colors.Gray;

    public SensorCard(SensorInfo info) => Info = info;

    public void Update(AirReading r)
    {
        Pm25 = r.Pm25.ToString("F1");
        Pm10 = r.Pm10.ToString("F1");
        Co2 = r.Co2.ToString("F0");
        Temperature = r.Temperature.ToString("F1");
        Humidity = r.Humidity.ToString("F0");
        Updated = $"Оновлено: {r.Time:HH:mm:ss}";

        // Оцінка якості повітря за PM2.5 (спрощено, за шкалою US AQI)
        (Quality, QualityColor) = r.Pm25 switch
        {
            <= 12 => ("Добре", Colors.Green),
            <= 35 => ("Помірне", Colors.Orange),
            <= 55 => ("Шкідливе", Colors.OrangeRed),
            _ => ("Небезпечне", Colors.Red)
        };
    }
}
