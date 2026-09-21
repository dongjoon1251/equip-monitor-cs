using EquipMonitor.Models;

namespace EquipMonitor.Report;

/// <summary>상태별 체류 시간과 가동률. 각 샘플의 상태는 다음 샘플(또는 end)까지 유지된다고 본다.</summary>
public static class Uptime
{
    public static Dictionary<string, double> StateDurations(IEnumerable<Telemetry> telemetry, DateTimeOffset start, DateTimeOffset end)
    {
        var totals = Enum.GetNames<DeviceState>().ToDictionary(n => n, _ => 0.0);
        var samples = telemetry.Where(t => t.Ts < end).OrderBy(t => t.Ts).ToList();
        for (var i = 0; i < samples.Count; i++)
        {
            var segStart = samples[i].Ts > start ? samples[i].Ts : start;
            var next = i + 1 < samples.Count ? samples[i + 1].Ts : end;
            var segEnd = next < end ? next : end;
            if (segEnd > segStart) totals[samples[i].State.ToString()] += (segEnd - segStart).TotalSeconds;
        }
        return totals;
    }

    public static double UptimeRatio(IEnumerable<Telemetry> telemetry, DateTimeOffset start, DateTimeOffset end)
    {
        var total = (end - start).TotalSeconds;
        return total > 0 ? StateDurations(telemetry, start, end)["RUN"] / total : 0.0;
    }

    /// <summary>하루 구간 [00:00, 다음날 00:00). 실행 머신의 로컬 시간대 기준.</summary>
    public static (DateTimeOffset Start, DateTimeOffset End) DayRange(DateOnly day)
    {
        var start = new DateTimeOffset(day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local)).ToUniversalTime();
        return (start, start.AddDays(1));
    }
}
