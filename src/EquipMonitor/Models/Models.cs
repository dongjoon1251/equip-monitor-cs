namespace EquipMonitor.Models;

// 도메인 모델. 모든 시각은 DateTimeOffset(UTC).
public enum DeviceState { RUN, IDLE, DOWN, MAINT }
public enum Severity { INFO, WARNING, CRITICAL }

public record Device(string Id, string Name = "", string Type = "generic", string Location = "")
{
    public DeviceState State { get; set; } = DeviceState.IDLE;
}

public record Telemetry(string DeviceId, DateTimeOffset Ts, Dictionary<string, double>? Metrics, DeviceState State = DeviceState.IDLE)
{
    // JSON 에 metrics 가 없어도 null 대신 빈 dict — 소비자(Normalize·룰)는 non-null 로 본다.
    public Dictionary<string, double> Metrics { get; init; } = Metrics ?? new();
}

public class Alarm
{
    public int Id { get; init; }
    public string DeviceId { get; init; } = "";
    public string Rule { get; init; } = "";
    public Severity Severity { get; init; }
    public string Message { get; init; } = "";
    public DateTimeOffset Ts { get; init; }
    public bool Acked { get; set; }
}

public static class TimeUtil
{
    /// <summary>쿼리 파라미터용: 어떤 offset 이든 UTC 로 변환.</summary>
    public static DateTimeOffset? EnsureUtc(DateTimeOffset? dt) => dt?.ToUniversalTime();
}
