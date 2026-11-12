using AirQualityMonitor.Services;
using AirQualityMonitor.ViewModels;
using AirQualityMonitor.Views;
using Microsoft.Extensions.Logging;

namespace AirQualityMonitor;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Один "сервер" і одні ViewModel на весь застосунок
        builder.Services.AddSingleton<SensorHub>();
        builder.Services.AddSingleton<AdminViewModel>();
        builder.Services.AddSingleton<ClientViewModel>();

        builder.Services.AddTransient<AdminPage>();
        builder.Services.AddTransient<ClientPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
