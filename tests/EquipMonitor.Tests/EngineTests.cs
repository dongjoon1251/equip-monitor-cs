using EquipMonitor.Alarms;
using EquipMonitor.Models;
using EquipMonitor.Store;
using static EquipMonitor.Tests.TestData;

namespace EquipMonitor.Tests;

public class EngineTests
{
    private readonly InMemoryStore _store = new();

    [Fact]
    public void ThresholdRule_FiresAtLimitNotBelow()
    {
        var rule = new ThresholdRule("temp-critical", "temp", 95.0, Severity.CRITICAL);
        Assert.Null(rule.Evaluate(Tele(metrics: ("temp", 94.9)), []));
        var f = rule.Evaluate(Tele(metrics: ("temp", 95.0)), []);
        Assert.Equal(new Finding(Severity.CRITICAL, "temp=95 >= 95"), f);
    }

    [Fact]
    public void MissingMetric_IsIgnored()
        => Assert.Null(new ThresholdRule("p", "pressure", 3.0).Evaluate(Tele(metrics: ("temp", 200)), []));

    private sealed class Spy : IRule
    {
        public string Name => "spy";
        public List<int> Seen { get; } = new();
        public Finding? Evaluate(Telemetry current, IReadOnlyList<Telemetry> history) { Seen.Add(history.Count); return null; }
    }

    [Fact]
    public void Engine_GivesRulePriorHistoryOnly()
    {
        var spy = new Spy();
        var engine = new AlarmEngine(_store, [spy]);
        for (var i = 0; i < 3; i++) engine.Process(Tele(minutes: i));
        Assert.Equal([0, 1, 2], spy.Seen);
    }

    [Fact]
    public void Alarm_IsPersistedInStore()
    {
        var engine = new AlarmEngine(_store);
        var raised = engine.Process(Tele(metrics: ("temp", 96.0)));
        Assert.Single(raised);
        Assert.Equal("temp-critical", raised[0].Rule);
        Assert.Equal(_store.ListAlarms("DEV-01").Select(a => a.Id), raised.Select(a => a.Id));
    }

    [Fact]
    public void DefaultRules_Names()
        => Assert.Equal(["temp-critical", "pressure-high"], DefaultRules.All.Select(r => r.Name));
}
