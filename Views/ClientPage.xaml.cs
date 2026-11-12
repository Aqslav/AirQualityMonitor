using AirQualityMonitor.ViewModels;

namespace AirQualityMonitor.Views;

public partial class ClientPage : ContentPage
{
    public ClientPage(ClientViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
