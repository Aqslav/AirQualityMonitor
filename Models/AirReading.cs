namespace AirQualityMonitor.Models;

// Одне вимірювання, яке датчик відправляє на сервер
public record AirReading(
    int SensorId,
    DateTime Time,
    double Pm25,         // дрібний пил, µg/m³
    double Pm10,         // грубий пил, µg/m³
    double Co2,          // ppm
    double Temperature,  // °C
    double Humidity);    // %
