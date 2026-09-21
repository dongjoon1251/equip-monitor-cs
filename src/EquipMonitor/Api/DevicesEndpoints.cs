using EquipMonitor.Models;
using EquipMonitor.Store;

namespace EquipMonitor.Api;

public static class DevicesEndpoints
{
    public static void MapDevices(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/devices").WithTags("devices");
        g.MapPost("", (Device device, InMemoryStore store) => Results.Created($"/devices/{device.Id}", store.UpsertDevice(device)));
        g.MapGet("", (InMemoryStore store) => store.ListDevices());
        g.MapGet("/{deviceId}", (string deviceId, InMemoryStore store)
            => store.GetDevice(deviceId) is { } d ? Results.Ok(d) : Results.NotFound(new { detail = "device not found" }));
        g.MapGet("/{deviceId}/telemetry", (string deviceId, DateTimeOffset? since, InMemoryStore store)
            => store.TelemetryFor(deviceId, TimeUtil.EnsureUtc(since)));
    }
}
