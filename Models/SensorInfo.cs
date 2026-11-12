using SQLite;

namespace AirQualityMonitor.Models;

// Опис датчика. Саме це зберігається в БД (таблиця SensorInfo).
public class SensorInfo
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public SensorType Type { get; set; }
    public string Location { get; set; } = "";

    [Ignore]
    public string Description => $"{Type} • {Location}";
}
