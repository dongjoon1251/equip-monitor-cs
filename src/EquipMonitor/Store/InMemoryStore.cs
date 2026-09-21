using EquipMonitor.Models;

namespace EquipMonitor.Store;

/// <summary>인메모리 저장소. ponytail: Dictionary 셋. 영속화가 필요해지면 SQLite 로 교체.</summary>
public class InMemoryStore
{
    private readonly Dictionary<string, Device> _devices = new();
    private readonly Dictionary<string, List<Telemetry>> _telemetry = new();
    private readonly List<Alarm> _alarms = new();
    private int _seq;

    public void Reset() { _devices.Clear(); _telemetry.Clear(); _alarms.Clear(); _seq = 0; }

    public Device UpsertDevice(Device device) { _devices[device.Id] = device; return device; }
    public Device? GetDevice(string deviceId) => _devices.GetValueOrDefault(deviceId);
    public List<Device> ListDevices() => _devices.Values.ToList();

    public void AddTelemetry(Telemetry t)
    {
        var device = GetDevice(t.DeviceId) ?? UpsertDevice(new Device(t.DeviceId, t.DeviceId));
        device.State = t.State;
        if (!_telemetry.TryGetValue(t.DeviceId, out var list)) _telemetry[t.DeviceId] = list = new();
        list.Add(t);
    }

    public List<Telemetry> TelemetryFor(string deviceId, DateTimeOffset? since = null)
        => (_telemetry.GetValueOrDefault(deviceId) ?? new()).Where(t => since is null || t.Ts >= since).ToList();

    public Alarm AddAlarm(string deviceId, string rule, Severity severity, string message, DateTimeOffset ts)
    {
        var alarm = new Alarm { Id = ++_seq, DeviceId = deviceId, Rule = rule, Severity = severity, Message = message, Ts = ts };
        _alarms.Add(alarm);
        return alarm;
    }

    public List<Alarm> ListAlarms(string? deviceId = null, DateTimeOffset? since = null)
        => _alarms.Where(a => (deviceId is null || a.DeviceId == deviceId) && (since is null || a.Ts >= since)).ToList();

    public Alarm? AckAlarm(int alarmId)
    {
        var a = _alarms.FirstOrDefault(x => x.Id == alarmId);
        if (a is not null) a.Acked = true;
        return a;
    }
}
