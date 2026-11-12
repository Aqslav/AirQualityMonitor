using AirQualityMonitor.Models;

namespace AirQualityMonitor.Tests;

public class SensorInfoTests
{
    [Fact]
    public void Description_CombinesTypeAndLocation()
    {
        var info = new SensorInfo { Name = "ESP-Кухня", Type = SensorType.ESP, Location = "Кухня" };

        Assert.Equal("ESP • Кухня", info.Description);
    }
}
