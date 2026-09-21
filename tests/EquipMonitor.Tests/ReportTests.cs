using EquipMonitor.Models;
using EquipMonitor.Report;
using static EquipMonitor.Tests.TestData;

namespace EquipMonitor.Tests;

public class ReportTests
{
    [Fact]
    public void StateCarriesUntilNextSample()
    {
        var d = Uptime.StateDurations([Tele(minutes: 0, state: DeviceState.RUN), Tele(minutes: 30, state: DeviceState.IDLE)], T0, At(60));
        Assert.Equal(new Dictionary<string, double> { ["RUN"] = 1800, ["IDLE"] = 1800, ["DOWN"] = 0, ["MAINT"] = 0 }, d);
    }

    [Fact]
    public void WindowClipsSamplesOutsideRange()
    {
        var d = Uptime.StateDurations([Tele(minutes: -30, state: DeviceState.DOWN), Tele(minutes: 10, state: DeviceState.RUN), Tele(minutes: 70, state: DeviceState.IDLE)], T0, At(60));
        Assert.Equal((600.0, 3000.0, 0.0), (d["DOWN"], d["RUN"], d["IDLE"]));
    }

    [Fact]
    public void UptimeRatio_RunOverWindow()
    {
        Assert.Equal(0.25, Uptime.UptimeRatio([Tele(minutes: 0, state: DeviceState.RUN), Tele(minutes: 15, state: DeviceState.DOWN)], T0, At(60)));
        Assert.Equal(0.0, Uptime.UptimeRatio([], T0, T0));
    }

    [Fact]
    public void DayRange_IncludesEarlyMorningKst()
    {
        // 2026-09-21 05:00 KST 는 "9월 21일" 리포트에 들어가야 한다.
        var (start, end) = Uptime.DayRange(new DateOnly(2026, 9, 21));
        var earlyMorning = new DateTimeOffset(2026, 9, 21, 5, 0, 0, TimeSpan.FromHours(9));
        Assert.True(start <= earlyMorning && earlyMorning < end, $"{start:o} .. {end:o}");
    }
}
