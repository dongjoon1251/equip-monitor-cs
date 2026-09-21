using EquipMonitor.Alarms;
using EquipMonitor.Ingest;
using EquipMonitor.Models;
using EquipMonitor.Store;

// 로그 파일을 파서 → 룰 엔진에 흘려 알람 표를 출력한다. Skill `alarm-rule` 의 검증 도구.
// 사용법: dotnet run --project src/EquipMonitor.CheckRules -- data/samples/overheat.log
if (args.Length != 1) { Console.Error.WriteLine("사용법: dotnet run --project src/EquipMonitor.CheckRules -- <log 파일>"); return 2; }
if (!File.Exists(args[0])) { Console.Error.WriteLine($"파일을 찾을 수 없습니다: {args[0]}"); return 2; }

var engine = new AlarmEngine(new InMemoryStore());
List<Alarm> alarms;
try { alarms = LogParser.ParseLines(File.ReadAllText(args[0])).SelectMany(engine.Process).ToList(); }
catch (Exception e) when (e is FormatException or ArgumentException) { Console.Error.WriteLine($"{e.GetType().Name}: {e.Message}"); return 1; }

Console.WriteLine($"{"ts",-20} {"device",-8} {"rule",-20} {"severity",-9} message");
foreach (var a in alarms)
    Console.WriteLine($"{a.Ts:yyyy-MM-dd'T'HH:mm:ss'Z'} {a.DeviceId,-8} {a.Rule,-20} {a.Severity,-9} {a.Message}");
Console.WriteLine($"-- {alarms.Count} alarm(s)");
return 0;
