using System.Collections.ObjectModel;
using AirQualityMonitor.Models;
using AirQualityMonitor.Services;

namespace AirQualityMonitor.ViewModels;

// Логіка клієнтської сторінки: по картці на кожен датчик
public class ClientViewModel
{
    private readonly SensorHub _hub;

    public ObservableCollection<SensorCard> Cards { get; } = new();

    public ClientViewModel(SensorHub hub)
    {
        _hub = hub;

        // Події приходять з інших потоків, а UI можна змінювати лише з головного
        _hub.SensorsChanged += () => MainThread.BeginInvokeOnMainThread(Refresh);
        _hub.ReadingReceived += r => MainThread.BeginInvokeOnMainThread(() => OnReading(r));

        Refresh();
    }

    private void Refresh()
    {
        // Старі картки лишаємо, щоб не втрачати їхні значення
        var old = Cards.ToDictionary(c => c.Info.Id);

        Cards.Clear();
        foreach (var info in _hub.Sensors)
            Cards.Add(old.TryGetValue(info.Id, out var card) ? card : new SensorCard(info));
    }

    private void OnReading(AirReading reading)
    {
        Cards.FirstOrDefault(c => c.Info.Id == reading.SensorId)?.Update(reading);
    }
}
