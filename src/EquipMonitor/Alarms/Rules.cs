using System.Globalization;
using EquipMonitor.Models;

namespace EquipMonitor.Alarms;

// 알람 룰. 새 룰은 IRule 을 구현하고 DefaultRules.All 에 등록한다.
public record Finding(Severity Severity, string Message);

public interface IRule
{
    string Name { get; }
    /// <summary>history 는 같은 장비의 이전 텔레메트리(오름차순, current 미포함).</summary>
    Finding? Evaluate(Telemetry current, IReadOnlyList<Telemetry> history);
}

/// <summary>metric 값이 limit 이상이면 즉시 알람.</summary>
public record ThresholdRule(string Name, string Metric, double Limit, Severity Severity = Severity.WARNING) : IRule
{
    public Finding? Evaluate(Telemetry current, IReadOnlyList<Telemetry> history)
    {
        if (!current.Metrics.TryGetValue(Metric, out var value) || value < Limit) return null;
        return new Finding(Severity, $"{Metric}={G(value)} >= {G(Limit)}");
    }

    // Python f"{v:g}" 와 동일: 유효숫자 6, 소문자 e
    internal static string G(double v) => v.ToString("G6", CultureInfo.InvariantCulture).ToLowerInvariant();
}

/// <summary>metric 이 limit 이상인 상태가 DurationS 이상 지속되면 1회 알람. 회복 후 재발하면 다시 알람.</summary>
public record SustainedThresholdRule(string Name, string Metric, double Limit, double DurationS, Severity Severity = Severity.WARNING) : IRule
{
    private bool Over(Telemetry t) => t.Metrics.TryGetValue(Metric, out var v) && v >= Limit;

    public Finding? Evaluate(Telemetry current, IReadOnlyList<Telemetry> history)
    {
        if (!Over(current)) return null;
        var streak = history.Reverse().TakeWhile(Over).Reverse().ToList(); // 직전까지 이어진 초과 구간 (오름차순)
        var start = streak.Count > 0 ? streak[0].Ts : current.Ts;
        var spanNow = (current.Ts - start).TotalSeconds;
        var spanPrev = streak.Count > 0 ? (streak[^1].Ts - start).TotalSeconds : 0.0;
        // ponytail: DurationS == 0 이면 절대 발화하지 않음 — 즉시 알람은 ThresholdRule 을 쓴다
        return spanNow >= DurationS && spanPrev < DurationS
            ? new Finding(Severity, $"{Metric} >= {ThresholdRule.G(Limit)} for {ThresholdRule.G(spanNow / 60)} min")
            : null;
    }
}

public static class DefaultRules
{
    public static IReadOnlyList<IRule> All { get; } =
    [
        new ThresholdRule("temp-critical", "temp", 95.0, Severity.CRITICAL),
        new ThresholdRule("pressure-high", "pressure", 3.0),
        new SustainedThresholdRule("temp-sustained", "temp", 85.0, 300),
    ];
}
