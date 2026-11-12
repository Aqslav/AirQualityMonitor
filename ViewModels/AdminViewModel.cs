using System.Collections.ObjectModel;
using AirQualityMonitor.Models;
using AirQualityMonitor.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AirQualityMonitor.ViewModels;

// Логіка адмін-сторінки: додати / видалити датчик
public partial class AdminViewModel : ObservableObject
{
    private readonly SensorHub _hub;

    public ObservableCollection<SensorInfo> Sensors { get; } = new();
    public List<SensorType> SensorTypes { get; } = Enum.GetValues<SensorType>().ToList();

    // Поля форми (CommunityToolkit сам створює властивості Name, Location, ...)
    [ObservableProperty] private string name = "";
    [ObservableProperty] private string location = "";
    [ObservableProperty] private SensorType selectedType = SensorType.ESP;
    [ObservableProperty] private string error = "";

    public AdminViewModel(SensorHub hub)
    {
        _hub = hub;
        _hub.SensorsChanged += () => MainThread.BeginInvokeOnMainThread(Refresh);
        Refresh();
    }

    private void Refresh()
    {
        Sensors.Clear();
        foreach (var sensor in _hub.Sensors)
            Sensors.Add(sensor);
    }

    [RelayCommand]  // створює команду AddCommand
    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Error = "Введіть назву датчика";
            return;
        }

        Error = "";
        await _hub.AddAsync(Name.Trim(), SelectedType, Location.Trim());

        Name = "";
        Location = "";
    }

    [RelayCommand]  // створює команду RemoveCommand
    private Task RemoveAsync(SensorInfo sensor) => _hub.RemoveAsync(sensor);
}
