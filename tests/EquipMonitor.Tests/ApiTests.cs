using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EquipMonitor.Store;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace EquipMonitor.Tests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        factory.Services.GetRequiredService<InMemoryStore>().Reset(); // 테스트마다 새 인스턴스 → 매번 리셋
    }

    private static async Task<JsonElement> Json(HttpResponseMessage r) => await r.Content.ReadFromJsonAsync<JsonElement>();

    [Fact]
    public async Task Health()
    {
        var r = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal("ok", (await Json(r)).GetProperty("status").GetString());
    }

    [Fact]
    public async Task PostTelemetry_RaisesAlarmAndListsIt()
    {
        var r = await _client.PostAsJsonAsync("/telemetry", new[] { new { device_id = "DEV-01", ts = "2026-09-21T09:00:00Z", metrics = new { TEMP = 96.0 }, state = "RUN" } });
        Assert.Equal(HttpStatusCode.Accepted, r.StatusCode);
        var body = await Json(r);
        Assert.Equal(1, body.GetProperty("accepted").GetInt32());
        Assert.Equal("temp-critical", body.GetProperty("alarms")[0].GetProperty("rule").GetString());
        var alarms = await Json(await _client.GetAsync("/alarms?device_id=DEV-01"));
        Assert.Equal(1, alarms.GetArrayLength());
        Assert.Equal("CRITICAL", alarms[0].GetProperty("severity").GetString());
        var ack = await _client.PostAsync($"/alarms/{alarms[0].GetProperty("id").GetInt32()}/ack", null);
        Assert.True((await Json(ack)).GetProperty("acked").GetBoolean());
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PostAsync("/alarms/999/ack", null)).StatusCode);
    }

    [Fact]
    public async Task IngestTextLines_AutoregistersDevice()
    {
        var r = await _client.PostAsync("/ingest", new StringContent("2026-09-21T09:00:00Z DEV-09 state=RUN temp=70\n2026-09-21T09:01:00Z DEV-09 state=RUN temp=71\n", System.Text.Encoding.UTF8, "text/plain"));
        Assert.Equal(HttpStatusCode.Accepted, r.StatusCode);
        Assert.Equal(2, (await Json(r)).GetProperty("accepted").GetInt32());
        var tele = await Json(await _client.GetAsync("/devices/DEV-09/telemetry?since=2026-09-21T09:01:00Z"));
        Assert.Equal(1, tele.GetArrayLength());
        Assert.Equal("RUN", (await Json(await _client.GetAsync("/devices/DEV-09"))).GetProperty("state").GetString());
    }

    [Fact]
    public async Task IngestMalformed_Returns422()
    {
        var r = await _client.PostAsync("/ingest", new StringContent("2026-09-21T09:00:00Z DEV-01 temp=hot\n", System.Text.Encoding.UTF8, "text/plain"));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, r.StatusCode);
    }

    [Fact]
    public async Task DevicesCrud()
    {
        var r = await _client.PostAsJsonAsync("/devices", new { id = "DEV-01", name = "Press #1", type = "press", location = "A-1" });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var list = await Json(await _client.GetAsync("/devices"));
        Assert.Equal(["DEV-01"], list.EnumerateArray().Select(d => d.GetProperty("id").GetString()));
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/devices/NOPE")).StatusCode);
    }

    [Fact]
    public async Task UptimeReportPerDevice()
    {
        await _client.PostAsJsonAsync("/telemetry", new[]
        {
            new { device_id = "DEV-01", ts = "2026-09-21T09:00:00Z", metrics = new { temp = 70.0 }, state = "RUN" },
            new { device_id = "DEV-01", ts = "2026-09-21T15:00:00Z", metrics = new { temp = 70.0 }, state = "DOWN" },
        });
        var r = await _client.GetAsync("/report/uptime?from=2026-09-21&to=2026-09-21");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var rows = await Json(r);
        Assert.Equal(1, rows.GetArrayLength());
        var row = rows[0];
        Assert.Equal("DEV-01", row.GetProperty("device_id").GetString());
        Assert.Equal(6 * 3600, row.GetProperty("durations_s").GetProperty("RUN").GetDouble()); // KST/UTC 어느 창에도 09:00Z~15:00Z 가 들어감
        var ratio = row.GetProperty("uptime_ratio").GetDouble();
        Assert.True(ratio > 0 && ratio < 1);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/report/uptime?from=2026-09-21&to=2026-09-21&device_id=NOPE")).StatusCode);
    }
}
