using EquipMonitor.Models;
using EquipMonitor.Store;

namespace EquipMonitor.Alarms;

/// <summary>텔레메트리 1건을 저장하고 모든 룰로 평가해 Alarm 을 만든다.</summary>
public class AlarmEngine(InMemoryStore store, IEnumerable<IRule>? rules = null)
{
    public InMemoryStore Store { get; } = store;
    public IReadOnlyList<IRule> Rules { get; } = (rules ?? DefaultRules.All).ToList();

    public List<Alarm> Process(Telemetry t)
    {
        var history = Store.TelemetryFor(t.DeviceId); // current 이전 이력 (복사본)
        Store.AddTelemetry(t);
        var raised = new List<Alarm>();
        foreach (var rule in Rules)
        {
            var finding = rule.Evaluate(t, history);
            if (finding is not null)
                raised.Add(Store.AddAlarm(t.DeviceId, rule.Name, finding.Severity, finding.Message, t.Ts));
        }
        return raised;
    }
}
