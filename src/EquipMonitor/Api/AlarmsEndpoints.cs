using EquipMonitor.Models;
using EquipMonitor.Store;
using Microsoft.AspNetCore.Mvc;

namespace EquipMonitor.Api;

public static class AlarmsEndpoints
{
    public static void MapAlarms(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/alarms").WithTags("alarms");
        g.MapGet("", ([FromQuery(Name = "device_id")] string? deviceId, DateTimeOffset? since, InMemoryStore store)
            => store.ListAlarms(deviceId, TimeUtil.EnsureUtc(since)));
        g.MapPost("/{alarmId:int}/ack", (int alarmId, InMemoryStore store)
            => store.AckAlarm(alarmId) is { } a ? Results.Ok(a) : Results.NotFound(new { detail = "alarm not found" }));
    }
}
