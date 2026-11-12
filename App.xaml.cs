using AirQualityMonitor.Services;

namespace AirQualityMonitor;

public partial class App : Application
{
    public App(SensorHub hub)
    {
        InitializeComponent();

        // Завантажуємо датчики з БД і запускаємо їх
        _ = hub.StartAsync();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
