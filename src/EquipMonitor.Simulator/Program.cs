using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EquipMonitor.Models;

// 가짜 장비 3대. 주기적으로 POST /telemetry 하거나(--scenario), 로그 파일을 POST /ingest 로 재생한다(--replay).
// 사용법:
//   dotnet run --project src/EquipMonitor.Simulator -- --scenario overheat --interval 1 --count 30   (--count 기본 60)
//   dotnet run --project src/EquipMonitor.Simulator -- --replay data/samples/overheat.log

const string Usage = "사용법: --scenario normal|overheat [--interval 초] [--count N] [--url http://...] | --replay <로그 파일>";
var url = "http://localhost:8000";
var scenario = "normal";
string? replay = null;
var interval = 2;
var count = 60;

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--url": url = args[++i]; break;
        case "--scenario": scenario = args[++i]; break;
        case "--replay": replay = args[++i]; break;
        case "--interval": interval = int.Parse(args[++i]); break;
        case "--count": count = int.Parse(args[++i]); break;
        default:
            Console.Error.WriteLine($"알 수 없는 옵션: {args[i]}");
            Console.Error.WriteLine(Usage);
            return 2;
    }
}

if (replay is not null && !File.Exists(replay))
{
    Console.Error.WriteLine($"파일을 찾을 수 없습니다: {replay}");
    return 2;
}

string[] devices = ["DEV-01", "DEV-02", "DEV-03"];
var overheat = scenario == "overheat";
var rnd = Random.Shared;
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    DictionaryKeyPolicy = null,
    Converters = { new JsonStringEnumConverter() },
};

Telemetry Sample(string device, int tick)
{
    var temp = 60 + rnd.NextDouble() * 4 - 2; // uniform(-2, 2)
    if (overheat && device == "DEV-02" && tick >= 3)
        temp = Math.Min(86 + (tick - 3) * 1.5, 97); // 86°C 부터 상승, tick 9 부터 95°C 이상
    var metrics = new Dictionary<string, double>
    {
        ["temp"] = Math.Round(temp, 1),
        ["pressure"] = Math.Round(1.0 + (rnd.NextDouble() * 0.2 - 0.1), 2),
        ["vibration"] = Math.Round(rnd.NextDouble(), 2),
    };
    return new Telemetry(device, DateTimeOffset.UtcNow, metrics, DeviceState.RUN);
}

static (int Accepted, List<string?> Rules) ReadResult(string body)
{
    using var doc = JsonDocument.Parse(body);
    var accepted = doc.RootElement.GetProperty("accepted").GetInt32();
    var rules = doc.RootElement.GetProperty("alarms").EnumerateArray()
        .Select(e => e.GetProperty("rule").GetString()).ToList();
    return (accepted, rules);
}

using var http = new HttpClient { BaseAddress = new Uri(url), Timeout = TimeSpan.FromSeconds(5) };

try
{
    if (replay is not null)
    {
        var text = File.ReadAllText(replay);
        var resp = await http.PostAsync("/ingest", new StringContent(text, Encoding.UTF8, "text/plain"));
        var body = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
        {
            Console.Error.WriteLine($"HTTP {(int)resp.StatusCode} {body}");
            return 1;
        }
        var (accepted, rules) = ReadResult(body);
        Console.WriteLine($"accepted={accepted} alarms: {string.Join(",", rules)}");
        return 0;
    }

    for (var tick = 0; tick < count; tick++)
    {
        var items = devices.Select(d => Sample(d, tick)).ToList();
        var json = JsonSerializer.Serialize(items, jsonOptions);
        var resp = await http.PostAsync("/telemetry", new StringContent(json, Encoding.UTF8, "application/json"));
        var body = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            Console.Error.WriteLine($"tick {tick,3}: HTTP {(int)resp.StatusCode} {body}");
        else
        {
            var (accepted, rules) = ReadResult(body);
            Console.WriteLine($"tick {tick,3}: accepted={accepted} alarms=[{string.Join(",", rules)}]");
        }
        await Task.Delay(TimeSpan.FromSeconds(interval));
    }
    return 0;
}
catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
{
    // HttpClient.Timeout 만료는 HttpRequestException 이 아니라 TaskCanceledException(내부 TimeoutException) 으로 던져진다.
    // Python 의 httpx.HTTPError 는 연결 실패와 타임아웃을 모두 포괄하므로 동일하게 처리한다.
    Console.Error.WriteLine($"서버에 연결할 수 없습니다 ({url}). 먼저 실행하세요: dotnet run --project src/EquipMonitor");
    return 1;
}
