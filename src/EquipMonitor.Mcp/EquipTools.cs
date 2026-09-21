using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace EquipMonitor.Mcp;

/// <summary>읽기 전용 도구 4개. 전부 실행 중인 API 에 HTTP GET 만 보낸다 — 쓰기 호출 없음.</summary>
[McpServerToolType]
public static class EquipTools
{
    [McpServerTool(Name = "list_devices"), Description("등록된 장비와 현재 상태 목록.")]
    public static Task<string> ListDevices(HttpClient http) => http.GetStringAsync("/devices");

    [McpServerTool(Name = "get_alarms"), Description("알람 목록. since 는 ISO-8601 UTC (예: 2026-09-21T09:00:00Z).")]
    public static Task<string> GetAlarms(HttpClient http,
        [Description("장비 ID")] string? device_id = null,
        [Description("ISO-8601 UTC, 예: 2026-09-21T09:00:00Z")] string? since = null)
        => http.GetStringAsync("/alarms" + Query(("device_id", device_id), ("since", since)));

    [McpServerTool(Name = "get_recent_telemetry"), Description("장비의 최근 텔레메트리 limit 개 (오래된 것부터).")]
    public static async Task<string> GetRecentTelemetry(HttpClient http, [Description("장비 ID")] string device_id, int limit = 20)
    {
        using var doc = JsonDocument.Parse(await http.GetStringAsync($"/devices/{Uri.EscapeDataString(device_id)}/telemetry"));
        var all = doc.RootElement.EnumerateArray().ToList();
        return JsonSerializer.Serialize(all.Skip(Math.Max(0, all.Count - limit)));
    }

    [McpServerTool(Name = "get_uptime"), Description("기간(YYYY-MM-DD, 양 끝 포함)의 장비별 상태 체류시간과 가동률.")]
    public static Task<string> GetUptime(HttpClient http,
        [Description("시작일 YYYY-MM-DD")] string from_date,
        [Description("종료일 YYYY-MM-DD")] string to_date,
        [Description("장비 ID (생략 시 전체)")] string? device_id = null)
        => http.GetStringAsync("/report/uptime" + Query(("from", from_date), ("to", to_date), ("device_id", device_id)));

    // null 값은 쿼리에서 생략 (py: {k: v for k, v in params.items() if v is not None})
    private static string Query(params (string Key, string? Value)[] kv)
    {
        var parts = kv.Where(p => p.Value is not null).Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}").ToList();
        return parts.Count == 0 ? "" : "?" + string.Join("&", parts);
    }
}
