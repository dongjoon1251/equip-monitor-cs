using EquipMonitor.Alarms;
using EquipMonitor.Ingest;
using EquipMonitor.Models;

namespace EquipMonitor.Api;

public static class TelemetryEndpoints
{
    public static void MapTelemetry(this IEndpointRouteBuilder app)
    {
        // JSON 텔레메트리 배치
        app.MapPost("/telemetry", (List<Telemetry> items, AlarmEngine engine)
            => Results.Accepted(null, Process(items.Select(LogParser.Normalize).ToList(), engine))).WithTags("telemetry");

        // raw 로그 라인 배치 (시뮬레이터 --replay, 장비 게이트웨이)
        app.MapPost("/ingest", async (HttpRequest req, AlarmEngine engine) =>
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            try { return Results.Accepted(null, Process(LogParser.ParseLines(body), engine)); }
            catch (Exception e) when (e is FormatException or ArgumentException)
            { return Results.UnprocessableEntity(new { detail = e.Message }); }
        }).Accepts<string>("text/plain").WithTags("telemetry");
    }

    private static object Process(List<Telemetry> items, AlarmEngine engine)
        => new { accepted = items.Count, alarms = items.SelectMany(engine.Process).ToList() };
}
