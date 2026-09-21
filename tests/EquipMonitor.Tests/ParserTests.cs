using EquipMonitor.Ingest;
using EquipMonitor.Models;
using static EquipMonitor.Tests.TestData;

namespace EquipMonitor.Tests;

public class ParserTests
{
    [Fact]
    public void ParseLine_Basic()
    {
        var t = LogParser.ParseLine("2026-09-21T09:00:00Z DEV-01 state=RUN temp=82.5 pressure=1.20");
        Assert.Equal(("DEV-01", T0, DeviceState.RUN), (t.DeviceId, t.Ts, t.State));
        Assert.Equal(new Dictionary<string, double> { ["temp"] = 82.5, ["pressure"] = 1.2 }, t.Metrics);
    }

    [Fact]
    public void ParseLine_DefaultsStateToIdleAndLowercasesKeys()
    {
        var t = LogParser.ParseLine("2026-09-21T09:00:00Z DEV-01 Temp=70");
        Assert.Equal(DeviceState.IDLE, t.State);
        Assert.Equal(70, t.Metrics["temp"]);
    }

    [Fact]
    public void ParseLines_SkipsBlankAndCommentLines()
    {
        var items = LogParser.ParseLines("# header\n\n2026-09-21T09:00:00Z DEV-01 temp=1\n   \n2026-09-21T09:01:00Z DEV-02 temp=2\n");
        Assert.Equal(["DEV-01", "DEV-02"], items.Select(t => t.DeviceId));
    }

    [Theory]
    [InlineData("garbage")]
    [InlineData("2026-09-21T09:00:00 DEV-01 temp=1")]          // offset 없음
    [InlineData("2026-09-21T09:00:00Z DEV-01 temp=hot")]
    [InlineData("2026-09-21T09:00:00Z DEV-01 state=FLYING")]
    [InlineData("2026-09-21T09:00:00Z DEV-01 state=0")]
    public void ParseLine_RejectsMalformed(string line) => Assert.ThrowsAny<Exception>(() => LogParser.ParseLine(line));

    [Fact]
    public void KstTimestamp_IsConvertedToUtc()
    {
        var t = LogParser.ParseLine("2026-09-21T18:00:00+09:00 DEV-01 temp=1");
        Assert.Equal(T0, t.Ts);
        Assert.Equal(TimeSpan.Zero, t.Ts.Offset);
    }

    [Fact]
    public void Normalize_LowercasesMetricKeys()
    {
        var n = LogParser.Normalize(new Telemetry("D", T0, new() { ["TEMP"] = 1 }));
        Assert.Equal(new[] { "temp" }, n.Metrics.Keys);
    }
}
