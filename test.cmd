@echo off
dotnet test AirQualityMonitor.Tests --collect:"XPlat Code Coverage" --settings AirQualityMonitor.Tests/coverlet.runsettings
pause