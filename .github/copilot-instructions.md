# equip-monitor 작업 지침

## 프로젝트
장비 텔레메트리 수집 → 알람 룰 평가 → 가동률 리포트를 제공하는 ASP.NET Core Minimal API 백엔드(`dotnet run --project src/EquipMonitor`, http://localhost:8000). 저장은 인메모리(`Store/InMemoryStore.cs`).
- `Api/*Endpoints.cs` 는 라우팅만. 비즈니스 로직은 넣지 않는다.
- `Ingest/LogParser.cs` raw 로그 라인·JSON → `Telemetry` 정규화의 단일 진입점.
- `Alarms/Rules.cs` 룰 정의(`IRule`, `DefaultRules.All`), `Alarms/AlarmEngine.cs` 평가·저장.
- `Report/Uptime.cs` 상태별 체류시간·가동률·`DayRange`.

## 규약
- 모든 시각은 `DateTimeOffset`. `DateTime.Now` 금지. 쿼리 파라미터는 `TimeUtil.EnsureUtc` 로 UTC 변환.
- 새 알람 룰: `Alarms/Rules.cs` 에 `IRule` 구현 → `DefaultRules.All` 등록 → `tests/EquipMonitor.Tests/RulesTests.cs` 경계 테스트(없으면 새로 만든다) → `dotnet run --project src/EquipMonitor.CheckRules -- data/samples/overheat.log` 로 확인.
- 저장소 접근은 `InMemoryStore` 의 public 메서드로만.
- 요청/응답 모델은 `Models/Models.cs` 의 레코드를 재사용.
- 새 NuGet 패키지 추가 금지. 시크릿·토큰 하드코딩 금지.

## 작업 방식
- 변경 전 관련 테스트를 먼저 실행하고, 변경 후 `dotnet test` 전체를 돌린다.
- 요구가 모호하면 확인 질문 대신 가정을 한 줄로 밝히고 진행한다.
- 한 응답에 변경 파일 목록과 테스트 결과 요약을 포함한다.
- 알람 관련 작업은 `src/EquipMonitor/Alarms/` 부터 읽는다. 전체 검색은 위치를 모를 때 한 번만.
