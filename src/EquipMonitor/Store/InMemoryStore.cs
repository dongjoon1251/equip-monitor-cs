using EquipMonitor.Models;

namespace EquipMonitor.Store;

/// <summary>
/// 인메모리 저장소. ponytail: Dictionary 셋. 영속화가 필요해지면 SQLite 로 교체.
/// ponytail: lock 한 개로 동시 요청 보호 — 처리량이 문제 되면 ConcurrentDictionary 로
/// </summary>
public class InMemoryStore
{
    private readonly object _gate = new();
    private readonly Dictionary<string, Device> _devices = new();
    private readonly Dictionary<string, List<Telemetry>> _telemetry = new();
    private readonly List<Alarm> _alarms = new();
    private int _seq;

    public void Reset() { lock (_gate) { _devices.Clear(); _telemetry.Clear(); _alarms.Clear(); _seq = 0; } }

    public Device UpsertDevice(Device device) { lock (_gate) { _devices[device.Id] = device; return device; } }
    public Device? GetDevice(string deviceId) { lock (_gate) return _devices.GetValueOrDefault(deviceId); }
    public List<Device> ListDevices() { lock (_gate) return _devices.Values.ToList(); }

    public void AddTelemetry(Telemetry t)
    {
        lock (_gate)
        {
            var device = GetDevice(t.DeviceId) ?? UpsertDevice(new Device(t.DeviceId, t.DeviceId));  // Monitor 는 재진입 가능
            device.State = t.State;
            if (!_telemetry.TryGetValue(t.DeviceId, out var list)) _telemetry[t.DeviceId] = list = new();
            list.Add(t);
        }
    }

    public List<Telemetry> TelemetryFor(string deviceId, DateTimeOffset? since = null)
    {
        lock (_gate) return (_telemetry.GetValueOrDefault(deviceId) ?? new()).Where(t => since is null || t.Ts >= since).ToList();
    }

    public Alarm AddAlarm(string deviceId, string rule, Severity severity, string message, DateTimeOffset ts)
    {
        lock (_gate)
        {
            var alarm = new Alarm { Id = ++_seq, DeviceId = deviceId, Rule = rule, Severity = severity, Message = message, Ts = ts };
            _alarms.Add(alarm);
            return alarm;
        }
    }

    public List<Alarm> ListAlarms(string? deviceId = null, DateTimeOffset? since = null)
    {
        lock (_gate) return _alarms.Where(a => (deviceId is null || a.DeviceId == deviceId) && (since is null || a.Ts >= since)).ToList();
    }

    public Alarm? AckAlarm(int alarmId)
    {
        lock (_gate)
        {
            var a = _alarms.FirstOrDefault(x => x.Id == alarmId);
            if (a is not null) a.Acked = true;
            return a;
        }
    }
}
