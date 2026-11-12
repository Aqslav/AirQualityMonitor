using System.Collections.Concurrent;
using AirQualityMonitor.Models;
using AirQualityMonitor.Services;

namespace AirQualityMonitor.Tests;

// Підміна БД: зберігає датчики в списку в пам'яті
public class FakeStorage : ISensorStorage
{
    public List<SensorInfo> Items { get; } = new();
    private int _nextId = 1;

    public Task InitAsync() => Task.CompletedTask;
    public Task<List<SensorInfo>> GetSensorsAsync() => Task.FromResult(Items.ToList());

    public Task AddSensorAsync(SensorInfo sensor)
    {
        sensor.Id = _nextId++;
        Items.Add(sensor);
        return Task.CompletedTask;
    }

    public Task DeleteSensorAsync(SensorInfo sensor)
    {
        Items.RemoveAll(s => s.Id == sensor.Id);
        return Task.CompletedTask;
    }
}

public class SensorHubTests
{
    private static async Task<(SensorHub hub, FakeStorage storage)> StartHubAsync()
    {
        var storage = new FakeStorage();
        var hub = new SensorHub(storage);
        await hub.StartAsync();
        return (hub, storage);
    }

    [Fact]
    public async Task StartAsync_EmptyStorage_AddsThreeDemoSensors()
    {
        var storage = new FakeStorage();
        var hub = new SensorHub(storage);
        int changed = 0;
        hub.SensorsChanged += () => changed++;

        await hub.StartAsync();

        Assert.Equal(3, hub.Sensors.Count);
        Assert.Equal(3, storage.Items.Count);
        Assert.Equal(1, changed);
    }

    [Fact]
    public async Task StartAsync_WithSavedSensors_DoesNotAddDemo()
    {
        var storage = new FakeStorage();
        storage.Items.Add(new SensorInfo { Id = 7, Name = "Saved", Type = SensorType.RaspberryPi, Location = "Hall" });
        var hub = new SensorHub(storage);

        await hub.StartAsync();

        Assert.Single(hub.Sensors);
        Assert.Equal("Saved", hub.Sensors[0].Name);
    }

    [Fact]
    public async Task AddAsync_SavesSensorAndRaisesSensorsChanged()
    {
        var (hub, storage) = await StartHubAsync();
        int changed = 0;
        hub.SensorsChanged += () => changed++;

        await hub.AddAsync("ESP-Test", SensorType.ESP, "Лабораторія");

        var added = hub.Sensors.Last();
        Assert.Equal("ESP-Test", added.Name);
        Assert.Equal(SensorType.ESP, added.Type);
        Assert.Equal("Лабораторія", added.Location);
        Assert.True(added.Id > 0);
        Assert.Contains(storage.Items, s => s.Id == added.Id);
        Assert.Equal(1, changed);
    }

    [Fact]
    public async Task AddAsync_ConnectedSensorSendsReadings()
    {
        var (hub, _) = await StartHubAsync();
        var ids = new ConcurrentBag<int>();
        hub.ReadingReceived += r => ids.Add(r.SensorId);

        await hub.AddAsync("ESP-Test", SensorType.ESP, "Lab");
        int newId = hub.Sensors.Last().Id;

        await TestHelpers.WaitUntilAsync(() => ids.Contains(newId));
    }

    [Fact]
    public async Task RemoveAsync_DeletesSensorAndRaisesSensorsChanged()
    {
        var (hub, storage) = await StartHubAsync();
        await hub.AddAsync("ESP-Test", SensorType.ESP, "Lab");
        var sensor = hub.Sensors.Last();
        int changed = 0;
        hub.SensorsChanged += () => changed++;

        await hub.RemoveAsync(sensor);

        Assert.DoesNotContain(hub.Sensors, s => s.Id == sensor.Id);
        Assert.DoesNotContain(storage.Items, s => s.Id == sensor.Id);
        Assert.Equal(1, changed);
    }

    [Fact]
    public async Task RemoveAsync_StopsReadingsFromSensor()
    {
        var (hub, _) = await StartHubAsync();
        await hub.AddAsync("ESP-Test", SensorType.ESP, "Lab");
        var sensor = hub.Sensors.Last();

        var ids = new ConcurrentBag<int>();
        hub.ReadingReceived += r => ids.Add(r.SensorId);
        await TestHelpers.WaitUntilAsync(() => ids.Contains(sensor.Id));

        await hub.RemoveAsync(sensor);
        await Task.Delay(100);                                   // запізніле вимірювання, що вже йшло
        int before = ids.Count(id => id == sensor.Id);

        await Task.Delay(2500);                                  // інтервал датчика за замовчуванням — 2 с

        Assert.Equal(before, ids.Count(id => id == sensor.Id));
    }
}
