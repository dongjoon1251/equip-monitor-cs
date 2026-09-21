using EquipMonitor.Alarms;
using EquipMonitor.Models;
using EquipMonitor.Store;
using static EquipMonitor.Tests.TestData;

namespace EquipMonitor.Tests;

public class RulesTests
{
    private static SustainedThresholdRule Sustained() => new("temp-sustained", "temp", 85.0, 300);

    /// <summary>(minute, temp) 샘플을 엔진에 흘려 발화한 minute 목록을 돌려준다.</summary>
    private static List<double> Feed(IRule rule, params (double Minute, double Temp)[] samples)
    {
        var engine = new AlarmEngine(new InMemoryStore(), [rule]);
        var fired = new List<double>();
        foreach (var (minute, temp) in samples)
            if (engine.Process(Tele(minutes: minute, metrics: ("temp", temp))).Count > 0) fired.Add(minute);
        return fired;
    }

    [Fact]
    public void FiresOnceWhenDurationReachedExactly()
        => Assert.Equal([5], Feed(Sustained(), (0, 86), (1, 86), (2, 86), (3, 86), (4, 86), (5, 86), (6, 86)));

    [Fact]
    public void DoesNotFireBelowDuration()
        => Assert.Empty(Feed(Sustained(), (0, 86), (4 + 59.0 / 60.0, 86)));

    [Fact]
    public void ExactlyAtLimitCountsAsOver()
        => Assert.Equal([5], Feed(Sustained(), (0, 85.0), (1, 85.0), (2, 85.0), (3, 85.0), (4, 85.0), (5, 85.0)));

    [Fact]
    public void BelowLimitResetsStreak()
    {
        Assert.Empty(Feed(Sustained(), (0, 86), (2, 84.9), (3, 86), (7, 86)));
        Assert.Equal([8], Feed(Sustained(), (0, 86), (2, 84.9), (3, 86), (8, 86)));
    }

    [Fact]
    public void RealarmAfterRecovery()
        => Assert.Equal([5, 12], Feed(Sustained(), (0, 86), (5, 86), (6, 70), (7, 86), (12, 86)));

    [Fact]
    public void AlarmContent()
    {
        var store = new InMemoryStore();
        var engine = new AlarmEngine(store, [Sustained()]);
        engine.Process(Tele(minutes: 0, metrics: ("temp", 90)));
        var raised = engine.Process(Tele(minutes: 5, metrics: ("temp", 90)));
        var a = Assert.Single(raised);
        Assert.Equal(("temp-sustained", Severity.WARNING, "temp >= 85 for 5 min"), (a.Rule, a.Severity, a.Message));
        Assert.Equal(At(5), a.Ts);
    }

    [Fact]
    public void DefaultRulesIncludeSustained()
        => Assert.Equal(["temp-critical", "pressure-high", "temp-sustained"], DefaultRules.All.Select(r => r.Name));
}
