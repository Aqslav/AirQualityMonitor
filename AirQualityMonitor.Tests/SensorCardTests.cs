using System.Globalization;
using AirQualityMonitor.Models;
using AirQualityMonitor.ViewModels;

namespace AirQualityMonitor.Tests;

public class SensorCardTests
{
    private static SensorCard CreateCard() =>
        new(new SensorInfo { Id = 1, Name = "Test", Type = SensorType.Arduino, Location = "Lab" });

    private static AirReading Reading(double pm25) =>
        new(1, new DateTime(2026, 11, 12, 10, 30, 15), pm25, pm25 * 1.5, 650, 21.5, 45);

    [Fact]
    public void NewCard_ShowsPlaceholders()
    {
        var card = CreateCard();

        Assert.Equal("—", card.Pm25);
        Assert.Equal("Очікування даних…", card.Updated);
        Assert.Equal("", card.Quality);
    }

    [Fact]
    public void Update_FormatsValues()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;   // щоб крапка, а не кома
        var card = CreateCard();

        card.Update(new AirReading(1, new DateTime(2026, 11, 12, 10, 30, 15), 15.5, 24.0, 650, 21.5, 45));

        Assert.Equal("15.5", card.Pm25);
        Assert.Equal("24.0", card.Pm10);
        Assert.Equal("650", card.Co2);
        Assert.Equal("21.5", card.Temperature);
        Assert.Equal("45", card.Humidity);
        Assert.Equal("Оновлено: 10:30:15", card.Updated);
    }

    // Межі за шкалою US AQI: ≤12, ≤35, ≤55, далі — небезпечне
    [Theory]
    [InlineData(5.0, "Добре")]
    [InlineData(12.0, "Добре")]
    [InlineData(12.1, "Помірне")]
    [InlineData(35.0, "Помірне")]
    [InlineData(40.0, "Шкідливе")]
    [InlineData(55.0, "Шкідливе")]
    [InlineData(80.0, "Небезпечне")]
    public void Update_SetsQualityByPm25(double pm25, string expected)
    {
        var card = CreateCard();

        card.Update(Reading(pm25));

        Assert.Equal(expected, card.Quality);
    }
}
