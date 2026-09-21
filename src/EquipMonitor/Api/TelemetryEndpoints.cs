using EquipMonitor.Alarms;
using EquipMonitor.Ingest;
using EquipMonitor.Models;

namespace EquipMonitor.Api;

public static class TelemetryEndpoints
{
    // ponytail: Telemetry 레코드는 주 생성자(4-param) 외에 보조 생성자(2-param)가 있어
    // System.Text.Json 이 "단일 매개변수 생성자" 규칙에 걸려 역직렬화하지 못한다
    // (Models.cs 는 Task 1-4 산출물이라 여기서 수정하지 않음). 요청 바인딩 전용 DTO 로 우회.
    private record TelemetryDto(string DeviceId, DateTimeOffset Ts, Dictionary<string, double> Metrics, DeviceState State = DeviceState.IDLE)
    {
        public Telemetry ToTelemetry() => new(DeviceId, Ts, Metrics, State);
    }

    public static void MapTelemetry(this IEndpointRouteBuilder app)
    {
        // JSON 텔레메트리 배치
        app.MapPost("/telemetry", (List<TelemetryDto> items, AlarmEngine engine)
            => Results.Accepted(null, Process(items.Select(d => LogParser.Normalize(d.ToTelemetry())).ToList(), engine))).WithTags("telemetry");

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
