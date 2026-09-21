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

public static class DefaultRules
{
    public static IReadOnlyList<IRule> All { get; } =
    [
        new ThresholdRule("temp-critical", "temp", 95.0, Severity.CRITICAL),
        new ThresholdRule("pressure-high", "pressure", 3.0),
    ];
}
