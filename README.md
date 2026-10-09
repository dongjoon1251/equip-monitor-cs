# equip-monitor-cs

장비 텔레메트리(온도·압력·진동·상태)를 받아 알람 룰을 평가하고 가동률을 계산하는 모니터링 백엔드. GitHub Copilot 실습용 (.NET 10 판).

## 시작
```bash
dotnet restore
dotnet build
powershell -ExecutionPolicy Bypass -File scripts\check_env.ps1   # 사전 과제: 모두 OK 캡처 (mac/Linux: pwsh scripts/check_env.ps1)
dotnet test
dotnet run --project src/EquipMonitor                      # http://localhost:8000  # 터미널을 점유함 — 이후 명령은 새 터미널에서
dotnet run --project src/EquipMonitor.Simulator -- --replay data/samples/overheat.log
dotnet run --project src/EquipMonitor.Simulator -- --scenario overheat --interval 1 --count 30   # --count 기본 60
dotnet run --project src/EquipMonitor.CheckRules -- data/samples/overheat.log   # 룰 검증 (알람 표 출력)
```

## 구조
```
src/EquipMonitor/
  Api/       DevicesEndpoints · TelemetryEndpoints(/telemetry, /ingest) · AlarmsEndpoints
  Ingest/    LogParser.cs — raw 로그 라인 + JSON → Telemetry 정규화
  Alarms/    Rules.cs(IRule, ThresholdRule, DefaultRules.All) · AlarmEngine.cs
  Report/    Uptime.cs — 상태별 체류시간 · 가동률 · DayRange
  Store/     InMemoryStore.cs
src/EquipMonitor.Simulator/   가짜 장비 3대
src/EquipMonitor.CheckRules/  로그 → 알람 표
scripts/     check_env.ps1 · seed.ps1(이슈 생성)
data/samples 정상 · 과열 · 음수 로그
```
로그 라인: `2026-09-21T09:00:00Z DEV-01 state=RUN temp=82.5 pressure=1.20`

## 실습 이슈
`powershell -ExecutionPolicy Bypass -File scripts\seed.ps1` 로 생성 (repo 루트에서 한 번만 실행 — 재실행 시 이슈가 중복 생성됨; mac/Linux: `pwsh scripts/seed.ps1`). 본문은 `docs/issues/`.
강사용: `powershell -ExecutionPolicy Bypass -File scripts\seed.ps1 -WithInjection` (이슈 #4 본문은 강사용 solution repo 에만 있음 → -InjectionFile 로 경로 지정)

## 체크포인트
> **Use this template** 로 자기 repo 를 만들 때 **Include all branches** 를 반드시 체크하세요. 체크하지 않으면 아래 체크포인트 브랜치가 생기지 않습니다.

뒤처지면 `git checkout m3-start` … `m6-start` 로 합류.
브랜치를 바꾼 뒤에는 `dotnet restore` 를 다시 실행하세요 (m6-start 부터 패키지가 늘어납니다).
