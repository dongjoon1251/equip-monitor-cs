---
applyTo: "src/EquipMonitor/Api/**"
---
# API 규약
- 도메인별 파일 하나에 `Map*` 확장 메서드 하나(예: `DevicesEndpoints.MapDevices`). `Program.cs` 에 등록.
- 경로·쿼리의 `DateTimeOffset` 은 `TimeUtil.EnsureUtc` 를 거친다.
- snake_case 쿼리 이름은 `[FromQuery(Name = "device_id")]` 로 매핑.
- 없는 리소스는 `Results.NotFound(new { detail = "<resource> not found" })`, 입력 오류는 `Results.UnprocessableEntity(new { detail })`.
- JSON 은 snake_case + 문자열 enum(`Program.cs` 의 `ConfigureHttpJsonOptions`)으로 이미 직렬화된다. 응답 타입은 `Models/Models.cs` 의 레코드(또는 그 목록)를 그대로 반환한다.
- 계산 로직은 `Report/`·`Alarms/` 로 위임하고 엔드포인트는 조립만 한다.
