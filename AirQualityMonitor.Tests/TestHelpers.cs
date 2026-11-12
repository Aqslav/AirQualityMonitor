namespace AirQualityMonitor.Tests;

public static class TestHelpers
{
    // Чекає, доки умова стане true (датчики працюють у фонових потоках)
    public static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 5000)
    {
        var start = DateTime.UtcNow;
        while (!condition())
        {
            if ((DateTime.UtcNow - start).TotalMilliseconds > timeoutMs)
                throw new TimeoutException("Умова не виконалась за відведений час");

            await Task.Delay(20);
        }
    }
}
