using EquipMonitor.Models;
using EquipMonitor.Store;
using static EquipMonitor.Tests.TestData;

namespace EquipMonitor.Tests;

public class StoreTests
{
    private readonly InMemoryStore _store = new();

    [Fact]
    public void AddTelemetry_AutoregistersDeviceAndUpdatesState()
    {
        _store.AddTelemetry(Tele("DEV-07", 0, DeviceState.RUN, ("temp", 70)));
        _store.AddTelemetry(Tele("DEV-07", 1, DeviceState.DOWN));
        var d = _store.GetDevice("DEV-07");
        Assert.NotNull(d);
        Assert.Equal(DeviceState.DOWN, d!.State);
        Assert.Equal(2, _store.TelemetryFor("DEV-07").Count);
        Assert.Equal(At(1), _store.TelemetryFor("DEV-07", since: At(1))[0].Ts);
    }

    [Fact]
    public void AlarmIdsIncrementAndAck()
    {
        var a1 = _store.AddAlarm("DEV-01", "r", Severity.WARNING, "m", T0);
        var a2 = _store.AddAlarm("DEV-02", "r", Severity.CRITICAL, "m", At(1));
        Assert.Equal((1, 2), (a1.Id, a2.Id));
        Assert.Single(_store.ListAlarms("DEV-02"));
        Assert.Single(_store.ListAlarms(since: At(1)));
        Assert.True(_store.AckAlarm(1)!.Acked);
        Assert.Null(_store.AckAlarm(99));
    }

    [Fact]
    public void EnsureUtc_ConvertsOffsetToUtc()
    {
        var kst = new DateTimeOffset(2026, 9, 21, 18, 0, 0, TimeSpan.FromHours(9));
        Assert.Equal(T0, TimeUtil.EnsureUtc(kst));
        Assert.Equal(TimeSpan.Zero, TimeUtil.EnsureUtc(kst)!.Value.Offset);
        Assert.Null(TimeUtil.EnsureUtc(null));
    }
}
