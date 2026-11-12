using AirQualityMonitor.Models;
using AirQualityMonitor.Sensors;

namespace AirQualityMonitor.Tests;

public class RandomSensorTests
{
    private static RandomSensor CreateSensor(int id = 1) =>
        new(new SensorInfo { Id = id, Name = "Test", Type = SensorType.ESP, Location = "Lab" }, intervalMs: 20);

    // Збирає перші N вимірювань і зупиняє датчик
    private static async Task<List<AirReading>> CollectAsync(RandomSensor sensor, int count)
    {
        var readings = new List<AirReading>();

        sensor.DataSent += r =>
        {
            lock (readings)
            {
                if (readings.Count < count)
                    readings.Add(r);
            }
        };

        sensor.Connect();
        await TestHelpers.WaitUntilAsync(() => { lock (readings) return readings.Count >= count; });
        sensor.Disconnect();

        lock (readings) return readings.ToList();
    }

    [Fact]
    public async Task Connect_SendsReadingWithSensorId()
    {
        var readings = await CollectAsync(CreateSensor(id: 42), 1);

        Assert.Equal(42, readings[0].SensorId);
    }

    [Fact]
    public async Task Readings_AreInRealisticRanges()
    {
        var readings = await CollectAsync(CreateSensor(), 50);

        Assert.All(readings, r =>
        {
            Assert.InRange(r.Pm25, 1, 150);
            Assert.InRange(r.Co2, 400, 2000);
            Assert.InRange(r.Temperature, 10, 35);
            Assert.InRange(r.Humidity, 20, 80);
        });
    }

    [Fact]
    public async Task Pm10_IsNotLessThanPm25()
    {
        var readings = await CollectAsync(CreateSensor(), 50);

        Assert.All(readings, r => Assert.True(r.Pm10 >= r.Pm25));
    }

    [Fact]
    public async Task Values_ChangeSmoothly()
    {
        var readings = await CollectAsync(CreateSensor(), 50);

        // Крок PM2.5 — не більше 3 (+0.1 на округлення)
        for (int i = 1; i < readings.Count; i++)
            Assert.True(Math.Abs(readings[i].Pm25 - readings[i - 1].Pm25) <= 3.1);
    }

    [Fact]
    public async Task Disconnect_StopsSendingData()
    {
        var sensor = CreateSensor();
        int count = 0;
        sensor.DataSent += _ => Interlocked.Increment(ref count);

        sensor.Connect();
        await TestHelpers.WaitUntilAsync(() => Volatile.Read(ref count) >= 2);
        sensor.Disconnect();

        await Task.Delay(100);                   // дати завершитися вимірюванню, що вже йшло
        int afterStop = Volatile.Read(ref count);
        await Task.Delay(200);

        Assert.Equal(afterStop, Volatile.Read(ref count));
    }
}
