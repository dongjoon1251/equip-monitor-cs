using System.Globalization;
using System.Text.RegularExpressions;
using EquipMonitor.Models;

namespace EquipMonitor.Ingest;

/// <summary>
/// raw 로그 라인과 JSON 텔레메트리를 하나의 Telemetry 로 정규화하는 단일 진입점.
/// 라인 포맷: &lt;ISO-8601 ts with offset&gt; &lt;device_id&gt; key=value ...   (state=RUN|IDLE|DOWN|MAINT, 나머지는 숫자)
/// </summary>
public static class LogParser
{
    private static readonly Regex Line = new(@"^(?<ts>\S+)\s+(?<device>\S+)\s*(?<kv>.*)$", RegexOptions.Compiled);
    private static readonly Regex Kv = new(@"(\w+)=(\S+)", RegexOptions.Compiled);
    private static readonly Regex Number = new(@"^\d+(\.\d+)?$", RegexOptions.Compiled);  // 숫자 토큰
    private static readonly Regex HasOffset = new(@"(Z|[+-]\d{2}:?\d{2})$", RegexOptions.Compiled);

    public static Telemetry ParseLine(string line)
    {
        var m = Line.Match(line.Trim());
        if (!m.Success) throw new FormatException($"malformed line: '{line}'");
        var metrics = new Dictionary<string, double>();
        var state = DeviceState.IDLE;
        foreach (Match kv in Kv.Matches(m.Groups["kv"].Value))
        {
            var key = kv.Groups[1].Value;
            var raw = kv.Groups[2].Value;
            if (key.Equals("state", StringComparison.OrdinalIgnoreCase))
                state = Enum.Parse<DeviceState>(raw);
            else if (Number.IsMatch(raw))
                metrics[key] = double.Parse(raw, CultureInfo.InvariantCulture);
            else
                throw new FormatException($"bad value {key}='{raw}' in '{line}'");
        }
        return Normalize(new Telemetry(m.Groups["device"].Value, ParseTs(m.Groups["ts"].Value), metrics, state));
    }

    public static List<Telemetry> ParseLines(string text)
        => text.Split('\n').Select(l => l.TrimEnd('\r'))
               .Where(l => l.Trim().Length > 0 && !l.TrimStart().StartsWith('#'))
               .Select(ParseLine).ToList();

    /// <summary>JSON·raw 공통: ts 는 UTC 로, metric 키는 소문자로.</summary>
    public static Telemetry Normalize(Telemetry t)
        => t with { Ts = t.Ts.ToUniversalTime(), Metrics = t.Metrics.ToDictionary(kv => kv.Key.ToLowerInvariant(), kv => kv.Value) };

    private static DateTimeOffset ParseTs(string raw)
    {
        if (!HasOffset.IsMatch(raw)) throw new FormatException($"timestamp must carry an offset: '{raw}'");
        return DateTimeOffset.Parse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
    }
}
