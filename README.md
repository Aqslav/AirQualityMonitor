# Edge-платформа моніторингу якості повітря

Застосунок .NET MAUI: датчики (Raspberry Pi / ESP / Arduino) відправляють дані на "сервер",
клієнт бачить показники в реальному часі, адмін підключає та видаляє датчики.

> Реальні драйвери ESP/Raspberry/Arduino не реалізовані — усі датчики працюють через
> `RandomSensor`, який генерує правдоподібні випадкові дані (PM2.5, PM10, CO₂, температура, вологість).

## Як запустити

Потрібно: .NET 8 SDK (або новіший) + workload MAUI (`dotnet workload install maui`),
Visual Studio 2022 / VS Code з розширенням .NET MAUI.

```bash
dotnet restore
dotnet build -t:Run -f net8.0-windows10.0.19041.0     # Windows
dotnet build -t:Run -f net8.0-android                 # Android (емулятор або телефон)
```
(замініть `net8.0` на версію, вказану в `.csproj`). У Visual Studio — просто F5 і вибір платформи.

## Як користуватись

* Вкладка **Клієнт** — картки датчиків: PM2.5, PM10, CO₂, температура, вологість, оцінка якості повітря.
* Вкладка **Адмін** — додати датчик (назва, розташування, тип) і видалити його.
* При першому запуску створюються 3 демо-датчики. Список датчиків зберігається в SQLite.

## Структура

| Папка | Що там |
|---|---|
| `Models/` | `SensorInfo` (зберігається в БД), `AirReading` (одне вимірювання), `SensorType` |
| `Sensors/` | `ISensor` — спільний інтерфейс, `RandomSensor` — єдина реалізація |
| `Services/` | `SensorHub` — "сервер": підключає датчики й приймає дані; `ISensorStorage` + `Database` — сховище (SQLite) |
| `ViewModels/` | `AdminViewModel`, `ClientViewModel`, `SensorCard` |
| `Views/` | `AdminPage`, `ClientPage` (XAML) |

## Потік даних

```
RandomSensor --(DataSent, кожні 2 с)--> SensorHub --(ReadingReceived)--> ClientViewModel --> ClientPage
AdminPage --> AdminViewModel --> SensorHub.AddAsync / RemoveAsync --> Database (SQLite)
```

## Як додати справжній датчик

1. Створити клас, що реалізує `ISensor` (наприклад, `EspSensor`).
2. У `SensorHub.CreateSensor` додати `switch` по `info.Type`.
Більше нічого змінювати не потрібно.

## Автотести та покриття

Тести лежать у `AirQualityMonitor.Tests/` (xUnit): `RandomSensor`, `SensorCard`, `SensorHub`, `SensorInfo`.
`SensorHub` тестується з підміною сховища (`FakeStorage`), тому реальна БД не потрібна.

```bash
dotnet test AirQualityMonitor.Tests --collect:"XPlat Code Coverage" --settings AirQualityMonitor.Tests/coverlet.runsettings
```

GitHub Actions (`.github/workflows/ci.yml`) запускає тести та будує звіт про покриття на кожен push/PR.
