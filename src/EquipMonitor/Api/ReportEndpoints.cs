using EquipMonitor.Models;
using EquipMonitor.Report;
using EquipMonitor.Store;
using Microsoft.AspNetCore.Mvc;

namespace EquipMonitor.Api;

public static class ReportEndpoints
{
    public static void MapReport(this IEndpointRouteBuilder app)
    {
        // [from, to] 날짜(양 끝 포함) 구간의 장비별 상태 체류시간과 가동률
        app.MapGet("/report/uptime", ([FromQuery(Name = "from")] DateOnly from, [FromQuery] DateOnly to,
            [FromQuery(Name = "device_id")] string? deviceId, InMemoryStore store) =>
        {
            var (start, _) = Uptime.DayRange(from);
            var (_, end) = Uptime.DayRange(to);
            List<Device> devices;
            if (deviceId is not null)
            {
                var d = store.GetDevice(deviceId);
                if (d is null) return Results.NotFound(new { detail = "device not found" });
                devices = [d];
            }
            else devices = store.ListDevices();
            var rows = devices.Select(d =>
            {
                var telemetry = store.TelemetryFor(d.Id);
                return new { device_id = d.Id, from = start, to = end, durations_s = Uptime.StateDurations(telemetry, start, end),
                             uptime_ratio = Math.Round(Uptime.UptimeRatio(telemetry, start, end), 4) };
            }).ToList();
            return Results.Ok(rows);
        }).WithTags("report");
    }
}
