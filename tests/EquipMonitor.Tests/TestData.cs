using EquipMonitor.Models;

namespace EquipMonitor.Tests;

public static class TestData
{
    public static readonly DateTimeOffset T0 = new(2026, 9, 21, 9, 0, 0, TimeSpan.Zero);
    public static DateTimeOffset At(double minutes) => T0.AddMinutes(minutes);
    public static Telemetry Tele(string deviceId = "DEV-01", double minutes = 0, DeviceState state = DeviceState.RUN,
        params (string Key, double Value)[] metrics)
        => new(deviceId, At(minutes), metrics.ToDictionary(m => m.Key, m => m.Value), state);
}
